using GGXrdReversalTool.Library.Memory;
using GGXrdReversalTool.Library.Scenarios.Action;

namespace GGXrdReversalTool.Library.Scenarios.Event;

public interface IScenarioEvent
{
    IMemoryReader? MemoryReader { get; internal set; }
    bool IsValid { get; }
    bool IsHitReversal { internal get => false; set { } }
    int BlockedHitTimer { internal get => 30; set { } }
    int FramesUntilEvent(bool isUserControllingDummy);
    bool CanEnable(IScenarioAction action, int slotNumber);
    bool CanEnable(IScenarioAction action) => CanEnable(action, action.SlotNumber);
    bool DependsOnReversalFrame();
    bool NeedLockDummyUntilEvent() => false;
    void Init() { }
    void Finish() { }
    int ApplySuperFreezeSlowdown(int framesRemaining, int dummySide, int playerSide)
    {
        int result = framesRemaining;
        if (result >= int.MaxValue || MemoryReader == null) return result;
        
        var freezeFrames = MemoryReader.GetSuperflashFreezeFrames(dummySide);
        if (result == 0 && freezeFrames > 0)
        {
            // On this frame you can't do anything other than block, throw or nothing.
            // However, we don't have information about what the user
            // has entered into the Action to perform panel. It could be one of those exact two things, or delayed or preemptive somehow.
            // Therefore we should let the dummy perform whatever reversal action on this frame.
            // Also, there can be no slowdown on the frame immediately after a superfreeze
            return freezeFrames - 1;
        }
        
        int slowdownFrames;
        if (freezeFrames > 0)
        {
            bool playerIsDoingRC = MemoryReader.GetCmnActIndex(playerSide) == 0x54;
            slowdownFrames = playerIsDoingRC ? MemoryReader.GetBBScrVar4(playerSide) - 1 : 0;
        }
        else
        {
            slowdownFrames = MemoryReader.GetSlowdownFrames(playerSide);
        }
        
        if (freezeFrames > 0) --result;
        
        return freezeFrames + result + Math.Min(result, slowdownFrames / 2) + slowdownFrames % 2;
    }
    void OnStageReset() { }
}