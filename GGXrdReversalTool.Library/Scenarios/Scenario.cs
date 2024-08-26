﻿using GGXrdReversalTool.Library.Logging;
using GGXrdReversalTool.Library.Memory;
using GGXrdReversalTool.Library.Models.Inputs;
using GGXrdReversalTool.Library.Scenarios.Action;
using GGXrdReversalTool.Library.Scenarios.Event;
using GGXrdReversalTool.Library.Scenarios.Frequency;
using System.Runtime.InteropServices;

namespace GGXrdReversalTool.Library.Scenarios;

public class Scenario : IDisposable
{
    private readonly IMemoryReader _memoryReader;
    
    private readonly IScenarioEvent _scenarioEvent;
    private readonly IScenarioAction _scenarioAction;
    private readonly IScenarioFrequency _scenarioFrequency;
    private readonly int _selectedSlot;
    private readonly int[] _usedSlots;


    private static bool _runThread;
    private static readonly object RunThreadLock = new object();

    public bool IsRunning => _runThread;



    public Scenario(
        IMemoryReader memoryReader, 
        IScenarioEvent scenarioEvent, 
        IScenarioAction scenarioAction, 
        int selectedSlot, 
        int[] usedSlots, 
        IScenarioFrequency scenarioFrequency)
    {
        _memoryReader = memoryReader; 
        _scenarioEvent = scenarioEvent;
        _scenarioAction = scenarioAction;
        _selectedSlot = selectedSlot;
        _usedSlots = usedSlots;
        _scenarioFrequency = scenarioFrequency;
    }


    private void Init()
    {
        
        //TODO Inject via factory
        _scenarioEvent.MemoryReader = _memoryReader;
        _scenarioAction.MemoryReader = _memoryReader;
        _scenarioFrequency.MemoryReader = _memoryReader;
        
        _scenarioAction.Init(_selectedSlot);

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
            bool dummyLocked = false;
            var engineTicks = _memoryReader.GetEngineTickCount();
            var prevEngineTicks = engineTicks;
            int pickedSlot = -1;
            bool mustIgnoreEvent = false;
            int maxReversalFIndex = 0;
            bool dependsOnReversalFrame = _scenarioEvent.DependsOnReversalFrame();
            if (dependsOnReversalFrame)
            {
                foreach (int slotNumber in _usedSlots)
                {
                    int reversalFIndex = Math.Max(0, _scenarioAction.Inputs[slotNumber - 1].ReversalFrameIndex);
                    if (reversalFIndex > maxReversalFIndex)
                    {
                        maxReversalFIndex = reversalFIndex;
                    }
                }
            }

            timeBeginPeriod(1);

            while (localRunThread)
            {
                // Approximately synchronise with the game's main loop finishing game state updates
                // In practice this leaves >13ms for our work before the next tick
                engineTicks = _memoryReader.GetEngineTickCount();
                if (engineTicks != prevEngineTicks)
                {
                    var worldInTick = _memoryReader.IsWorldInTick();
                    if (engineTicks - prevEngineTicks > 1 || worldInTick)
                    {
                        LogManager.Instance.WriteLine("Overslept through tick");
                    }
                    // Very unlikely, but skip this tick if somehow we overslept into the middle of a new tick
                    if (!worldInTick)
                    {
                        if (_scenarioAction.IsRunning)
                        {
                            _scenarioAction.Tick();
                            // TODO (low priority?): Logic for cancelling actions
                        }
                        // Only execute a reversal on the exact frame, skipping if we miss it
                        // Potentially configurable later, e.g. executing one frame early or on the exact frame if missed
                        int framesUntilEvent = _scenarioEvent.FramesUntilEvent(0);
                        
                        if (framesUntilEvent == int.MaxValue)
                        {
                            pickedSlot = -1;
                        }
                        else if (0 == framesUntilEvent - maxReversalFIndex)
                        {
                            LogManager.Instance.WriteLine("Event Occured");

                            int slotNumber = 0;
                            pickedSlot = -1;
                            mustIgnoreEvent = !_scenarioFrequency.ShouldHappen(out slotNumber);
                            
                            if (!mustIgnoreEvent)
                            {
                                pickedSlot = slotNumber;
                                if (pickedSlot == -1)
                                {
                                    pickedSlot = _selectedSlot;
                                }
                            }
                        }
                        
                        if (dummyLocked && 0 == framesUntilEvent)
                        {
                            dummyLocked = false;
                            _memoryReader.UnlockDummy(1 - _memoryReader.GetPlayerSide(), oldWhatCanDoFlags);
                        }
                        
                        if (pickedSlot != -1
                            && !mustIgnoreEvent
                            && framesUntilEvent < int.MaxValue
                            && (
                                dependsOnReversalFrame
                                && 0 == framesUntilEvent - Math.Max(0, _scenarioAction.Inputs[pickedSlot - 1].ReversalFrameIndex)
                                || !dependsOnReversalFrame
                                && 0 == framesUntilEvent))
                        {
                            bool actionValid = _scenarioEvent.CanEnable(_scenarioAction, pickedSlot);
                            if (!actionValid)
                            {
                                _scenarioAction.Execute(pickedSlot);
                            }
                            else 
                            {
                                if (_usedSlots.Length > 1)
                                {
                                    _scenarioAction.Init(pickedSlot);
                                }
                                if (dependsOnReversalFrame
                                        && _scenarioAction.Inputs[pickedSlot - 1].ReversalFrameIndex > 0
                                        && _scenarioEvent.NeedLockDummyUntilEvent()
                                        && !dummyLocked)
                                {
                                    dummyLocked = true;
                                    _memoryReader.LockDummy(1 - _memoryReader.GetPlayerSide(), out oldWhatCanDoFlags);
                                }
                                _scenarioAction.Execute();
                            }
                            pickedSlot = -1;

                            LogManager.Instance.WriteLine("Action Executed");
                        }
                    }
                }

                lock (RunThreadLock)
                {
                    localRunThread = _runThread;
                }

                prevEngineTicks = engineTicks;
                Thread.Sleep(1);
            }

            timeEndPeriod(1);
            
            if (dummyLocked)
            {
                dummyLocked = false;
                _memoryReader.UnlockDummy(1 - _memoryReader.GetPlayerSide(), oldWhatCanDoFlags);
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
