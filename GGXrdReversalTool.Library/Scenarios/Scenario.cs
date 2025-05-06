using GGXrdReversalTool.Library.Logging;
using GGXrdReversalTool.Library.Memory;
using GGXrdReversalTool.Library.Models.Inputs;
using GGXrdReversalTool.Library.Scenarios.Action;
using GGXrdReversalTool.Library.Scenarios.BlockSwitching;
using GGXrdReversalTool.Library.Scenarios.Event;
using GGXrdReversalTool.Library.Scenarios.Frequency;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace GGXrdReversalTool.Library.Scenarios;

public class ScenarioElement
{
    public IScenarioEvent ScenarioEvent;
    public IScenarioAction ScenarioAction;
    public IScenarioFrequency ScenarioFrequency;
    // contains slot numbers (1,2,3,etc) from a frequency control,
    // including when there is only one slot with 100% probability,
    // with exact slot number.
    // -1 is not allowed here
    public int[] UsedSlots;
    
    public ScenarioElement(
            IScenarioEvent ScenarioEvent,
            IScenarioAction ScenarioAction,
            IScenarioFrequency ScenarioFrequency,
            int[] UsedSlots) {
        this.ScenarioEvent = ScenarioEvent;
        this.ScenarioAction = ScenarioAction;
        this.ScenarioFrequency = ScenarioFrequency;
        this.UsedSlots = UsedSlots;
    }
    
    public bool MustIgnoreEvent = false;
    public int PickedSlot = -1;  // the number of the last picked slot: 1,2,3,etc. -1 means no slot is picked
    public int MaxReversalFrame = 0;  // out of all used slots (see UsedSlots), how far to the right the '!' symbol is, measured in frames? 0 means it's on the left
}

public class Scenario : IDisposable
{
    private readonly IMemoryReader _memoryReader;
    
    private ScenarioElement[] _scenarioElements;
    private IScenarioBlockSwitching? _scenarioBlockSwitching = null;
    private int _blockedHitTimer = 30;

    private static bool _runThread;
    private static readonly object RunThreadLock = new object();

    public bool IsRunning => _runThread;



    public Scenario(
        IMemoryReader memoryReader,
        ScenarioElement[] scenarioElements,
        IScenarioBlockSwitching? ScenarioBlockSwitching,
        int BlockedHitTimer)
    {
        _memoryReader = memoryReader;
        _scenarioElements = scenarioElements;
        _scenarioBlockSwitching = ScenarioBlockSwitching;
        _blockedHitTimer = BlockedHitTimer;
    }


    private void Init()
    {
        
        if (_scenarioBlockSwitching != null)
        {
            _scenarioBlockSwitching.MemoryReader = _memoryReader;
            _scenarioBlockSwitching.Init();
        }
        //TODO Inject via factory
        foreach (ScenarioElement element in _scenarioElements)
        {
            element.ScenarioEvent.MemoryReader = _memoryReader;
            element.ScenarioEvent.BlockedHitTimer = _blockedHitTimer;
            element.ScenarioAction.MemoryReader = _memoryReader;
            element.ScenarioFrequency.MemoryReader = _memoryReader;
            
            if (_scenarioElements.Length == 1 && element.UsedSlots.Length == 1)
            {
                element.ScenarioAction.Init(element.UsedSlots[0]);
            }
            element.ScenarioEvent.Init();
        }

    }

    public void Run()
    {
        lock (RunThreadLock)
        {
            _runThread = true;
        }
        
        Init();
        
        

        var scenarioThread = new Thread(() =>
        {
            LogManager.Instance.WriteLine("Scenario Thread start");
            var localRunThread = true;

            uint oldWhatCanDoFlags = 0;
            ScenarioElement? dummyLocked = null;
            
            var engineTicks = _memoryReader.GetEngineTickCount();
            var prevEngineTicks = engineTicks;
            var aswEngineTicks = _memoryReader.GetAswEngineTickCount();
            var prevAswEngineTicks = aswEngineTicks;
            foreach (ScenarioElement element in _scenarioElements)
            {
                element.MustIgnoreEvent = false;
                element.PickedSlot = -1;
                element.MaxReversalFrame = 0;
                if (element.ScenarioEvent.DependsOnReversalFrame())
                {
                    foreach (int slotNumber in element.UsedSlots)
                    {
                        int reversalFIndex = Math.Max(0, element.ScenarioAction.Inputs[slotNumber - 1].ReversalFrameIndex);
                        if (reversalFIndex > element.MaxReversalFrame)
                        {
                            element.MaxReversalFrame = reversalFIndex;
                        }
                    }
                }
            }

            timeBeginPeriod(1);

            while (localRunThread)
            {
                Thread.Sleep(1);

                lock (RunThreadLock)
                {
                    localRunThread = _runThread;
                }
                
                // Approximately synchronise with the game's main loop finishing game state updates
                // In practice this leaves >13ms for our work before the next tick
                prevEngineTicks = engineTicks;
                engineTicks = _memoryReader.GetEngineTickCount();
                if (engineTicks == prevEngineTicks) continue;
                
                var worldInTick = _memoryReader.IsWorldInTick();
                if (engineTicks - prevEngineTicks > 1 || worldInTick)
                {
                    LogManager.Instance.WriteLine("Overslept through tick");
                }
                // Very unlikely, but skip this tick if somehow we overslept into the middle of a new tick
                if (worldInTick) continue;
                
                if (!_memoryReader.IsBattle()) continue;  // not in-game or switching characters?
                
                // Check if the match frame counter advanced.
                // It will not advance if the Pause Menu is open or another mod paused the game via other means.
                prevAswEngineTicks = aswEngineTicks;
                aswEngineTicks = _memoryReader.GetAswEngineTickCount();
                if (aswEngineTicks == prevAswEngineTicks) continue;
                if (!_memoryReader.MatchRunning()) {  // stage reset?
                    foreach (ScenarioElement element in _scenarioElements)
                    {
                        element.ScenarioFrequency.OnStageReset();
                        element.ScenarioEvent.OnStageReset();
                    }
                    _scenarioBlockSwitching?.OnStageReset();
                }
                
                bool isUserControllingDummy = _memoryReader.IsUserControllingDummy();
                _scenarioBlockSwitching?.Tick(isUserControllingDummy);
                bool isHitReversal = _scenarioBlockSwitching?.IsHitReversal() ?? false;
                
                int playerSide = _memoryReader.GetPlayerSide();
                int dummySide = 1 - playerSide;
                
                foreach (ScenarioElement element in _scenarioElements)
                {
                    if (element.ScenarioAction.IsRunning)
                    {
                        element.ScenarioAction.Tick();
                        // TODO (low priority?): Logic for cancelling actions
                    }
                    // Only execute a reversal on the exact frame, skipping if we miss it
                    // Potentially configurable later, e.g. executing one frame early or on the exact frame if missed
                    element.ScenarioEvent.IsHitReversal = isHitReversal;
                    int framesUntilEvent = element.ScenarioEvent.FramesUntilEvent(isUserControllingDummy);
                    
                    if (framesUntilEvent == int.MaxValue || dummyLocked != null && dummyLocked != element)
                    {
                        element.PickedSlot = -1;
                    }
                    else if (0 == framesUntilEvent - element.MaxReversalFrame)
                    {
                        LogManager.Instance.WriteLine("Event Occured");
    
                        int slotNumber = 0;
                        element.PickedSlot = -1;
                        element.MustIgnoreEvent = !element.ScenarioFrequency.ShouldHappen(out slotNumber);
                        
                        if (!element.MustIgnoreEvent)
                        {
                            if (slotNumber == -1)
                                slotNumber = element.UsedSlots[0];
                            
                            element.PickedSlot = slotNumber;
                        }
                    }
                    
                    if (dummyLocked == element && 0 == framesUntilEvent)
                    {
                        dummyLocked = null;
                        _memoryReader.UnlockDummy(dummySide, oldWhatCanDoFlags);
                    }
                    
                    int reversalFrameIndex = -1;
                    if (!element.ScenarioEvent.DependsOnReversalFrame())
                    {
                        reversalFrameIndex = 0;
                    }
                    else if (element.PickedSlot != -1)
                    {
                        reversalFrameIndex = element.ScenarioAction.Inputs[element.PickedSlot - 1].ReversalFrameIndex;
                    }
                    
                    
                    if (element.PickedSlot != -1
                        && !element.MustIgnoreEvent
                        && framesUntilEvent < int.MaxValue
                        && 0 == framesUntilEvent - Math.Max(0, reversalFrameIndex))
                    {
                        bool actionValid = element.ScenarioEvent.CanEnable(element.ScenarioAction, element.PickedSlot);
                        if (!actionValid)
                        {
                            element.ScenarioAction.Execute(element.PickedSlot, element.PickedSlot);
                        }
                        else 
                        {
                            if (element.UsedSlots.Length > 1 || _scenarioElements.Length > 1)
                            {
                                element.ScenarioAction.Init(element.PickedSlot);
                            }
                            if (reversalFrameIndex > 0
                                    && element.ScenarioEvent.NeedLockDummyUntilEvent()
                                    && dummyLocked == null)
                            {
                                dummyLocked = element;
                                _memoryReader.LockDummy(dummySide, out oldWhatCanDoFlags);
                            }
                            element.ScenarioAction.Execute(element.ScenarioAction.SlotNumber, element.PickedSlot);
                        }
                        element.PickedSlot = -1;
    
                        LogManager.Instance.WriteLine("Action Executed");
                    }
                }
            }

            timeEndPeriod(1);
            
            if (dummyLocked != null)
            {
                dummyLocked = null;
                _memoryReader.UnlockDummy(1 - _memoryReader.GetPlayerSide(), oldWhatCanDoFlags);
            }
            foreach (ScenarioElement element in _scenarioElements)
            {
                element.ScenarioEvent.Finish();
            }
            LogManager.Instance.WriteLine("Scenario Thread ended");
        });

        scenarioThread.Start();

    }

    public void Stop()
    {
        lock (RunThreadLock)
        {
            _runThread = false;
        }
    }

    public void Dispose()
    {
        Stop();
    }

    #region DLL Imports
    [DllImport("winmm.dll")]
    private static extern int timeBeginPeriod(uint uPeriod);

    [DllImport("winmm.dll")]
    private static extern int timeEndPeriod(uint uPeriod);

    #endregion
}
