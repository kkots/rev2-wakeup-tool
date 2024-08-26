﻿using GGXrdReversalTool.Library.Memory;
using GGXrdReversalTool.Library.Scenarios.Action;

namespace GGXrdReversalTool.Library.Scenarios.Event.Implementations;

public class SimulatedRoundstartEvent : IScenarioEvent
{
    public IMemoryReader? MemoryReader { get; set; }
    public bool IsValid => true;

    public int FramesUntilEvent(int inputReversalFrame)
    {
        if (MemoryReader is null)
            return int.MaxValue;

        var playerSide = MemoryReader.GetPlayerSide();
        var animationString = MemoryReader.ReadAnimationString(playerSide);
        if (animationString != "CounterGuardStand") return int.MaxValue;
        var animFrame = MemoryReader.GetAnimFrame(playerSide);

        var result = 50 - animFrame - inputReversalFrame;

        return result;
    }
    public bool CanEnable(IScenarioAction action, int slotNumber)
    {
        return action.Inputs[slotNumber - 1].IsReversalValid && IsValid;
    }
    public bool DependsOnReversalFrame() => true;
    public bool NeedLockDummyUntilEvent() => true;

}