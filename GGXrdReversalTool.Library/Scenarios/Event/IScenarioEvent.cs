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
    bool NeedLockDummyUntilEvent() => false;
    // Returns a string containing inputs in the form of "5H*2,5" to be inserted at the beginning of the action to perform.
    void Init() { }
    void Finish() { }
    public int ApplySuperFreezeSlowdown(int framesRemaining, int dummySide, int playerSide, int inputReversalFrame)
    {
        int result = framesRemaining;
        if (result >= int.MaxValue || MemoryReader == null) return result;

        var slowdownFrames = MemoryReader.GetSlowdownFrames(playerSide);
        var freezeFrames = MemoryReader.GetSuperflashFreezeFrames(dummySide);

        result += Math.Min(result, slowdownFrames / 2 + slowdownFrames % 1);
        result -= inputReversalFrame;

        if (freezeFrames <= 0) return result;

        // Avoid trying a reversal directly out of a super freeze, where everything but blocking and throwing gets dropped
        if (result + freezeFrames == 0)
            result = int.MaxValue;
        else
            result += freezeFrames;

        return result;
    }
}