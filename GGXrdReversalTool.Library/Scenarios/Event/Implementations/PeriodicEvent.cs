using GGXrdReversalTool.Library.Memory;
using GGXrdReversalTool.Library.Scenarios.Action;

namespace GGXrdReversalTool.Library.Scenarios.Event.Implementations;

public class PeriodicEvent : IScenarioEvent
{
    private int _counter = 0;
    public int MaxDelay { get; set; } = 180;
    public int MinDelay { get; set; } = 180;
    public bool OnlyWhenIdle { get; set; } = true;
    private readonly Random _random = new();
    public IMemoryReader? MemoryReader { get; set; }
    public bool IsValid => MinDelay <= MaxDelay;
    public int FramesUntilEvent(int inputReversalFrame, bool isUserControllingDummy)
    {
        if (MemoryReader is null)
            return int.MaxValue;
        
        if (OnlyWhenIdle && !MemoryReader.ReadAnimationString(1 - MemoryReader.GetPlayerSide()).Equals("CmnActStand") || isUserControllingDummy) {
            _counter = _random.Next(MinDelay, MaxDelay + 1) - 1;
            return int.MaxValue;
        }
        
        int counterCurrent = _counter;
        if (_counter == 0)
        {
            _counter = _random.Next(MinDelay, MaxDelay + 1);
        }
        --_counter;
        return Math.Max(0, counterCurrent - inputReversalFrame);
    }
    public bool CanEnable(IScenarioAction action, int slotNumber)
    {
        return action.Inputs[slotNumber - 1].IsReversalValid && IsValid;
    }
    public bool DependsOnReversalFrame() => true;
    public void OnStageReset()
    {
        _counter = _random.Next(MinDelay, MaxDelay + 1) - 1;
    }

}