using GGXrdReversalTool.Library.Memory;
using GGXrdReversalTool.Library.Scenarios.Action;

namespace GGXrdReversalTool.Library.Scenarios.Event;

public interface IScenarioEvent
{
    IMemoryReader? MemoryReader { get; internal set; }
    bool IsValid { get; }
    int FramesUntilEvent(int inputReversalFrame);
    bool CanEnable(IScenarioAction action, int slotNumber);
    bool CanEnable(IScenarioAction action) => CanEnable(action, action.SlotNumber);
    bool DependsOnReversalFrame();
    public bool NeedLockDummyUntilEvent() => false;
}