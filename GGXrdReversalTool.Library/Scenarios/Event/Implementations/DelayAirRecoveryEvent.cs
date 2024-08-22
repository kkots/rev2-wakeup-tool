using GGXrdReversalTool.Library.Memory;
using GGXrdReversalTool.Library.Scenarios.Action;

namespace GGXrdReversalTool.Library.Scenarios.Event.Implementations;

public class DelayAirRecoveryEvent : IScenarioEvent
{
    public IMemoryReader? MemoryReader { get; set; }
    public bool IsValid => true;

    public int FramesUntilEvent(int inputReversalFrame)
    {
        if (MemoryReader is null)
            return int.MaxValue;

        var playerSide = MemoryReader.GetPlayerSide();
        var dummySide = 1 - playerSide;
        var timeUntilTech = MemoryReader.GetTimeUntilTech(dummySide);
        var yPos = MemoryReader.GetYPos(dummySide);

        return timeUntilTech == 1 && yPos > 0 ? 0 : int.MaxValue;
    }
    public bool CanEnable(IScenarioAction action, int slotNumber)
    {
        return action.Inputs[slotNumber - 1].IsValid;
    }
    public bool DependsOnReversalFrame()
    {
        return false;
    }

}