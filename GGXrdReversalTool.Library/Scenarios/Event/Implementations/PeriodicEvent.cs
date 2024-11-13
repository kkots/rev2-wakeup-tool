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
    private void resetCounter() => _counter = _random.Next(MinDelay, MaxDelay + 1) - 1;
    public int FramesUntilEvent(int inputReversalFrame, bool isUserControllingDummy)
    {
        if (MemoryReader is null)
            return int.MaxValue;
        
        if (isUserControllingDummy)
        {
            resetCounter();
            return int.MaxValue;
        }
        
        if (OnlyWhenIdle)
        {
            string anim = MemoryReader.ReadAnimationString(1 - MemoryReader.GetPlayerSide());
            if (!anim.Equals("CmnActStand") && !anim.Equals("CmnActCrouch"))
            {
                resetCounter();
                return int.MaxValue;
            }
        }
        
        int counterCurrent = _counter;
        if (_counter == 0)
        {
            resetCounter();
        }
        else
        {
            --_counter;
        }
        return Math.Max(0, counterCurrent - inputReversalFrame);
    }
    public bool CanEnable(IScenarioAction action, int slotNumber)
    {
        return action.Inputs[slotNumber - 1].IsReversalValid && IsValid;
    }
    public bool DependsOnReversalFrame() => true;
    public void OnStageReset() => resetCounter();

}