using GGXrdReversalTool.Library.Memory;
using GGXrdReversalTool.Library.Memory.Implementations;
using GGXrdReversalTool.Library.Scenarios.Action;

namespace GGXrdReversalTool.Library.Scenarios.Event.Implementations;

public class AnimationEvent : IScenarioEvent
{
    private const string FaceDownAnimation = "CmnActFDown2Stand";
    private const string FaceUpAnimation = "CmnActBDown2Stand";
    private const string WallSplatAnimation = "CmnActWallHaritsukiGetUp";
    private const string TechAnimation = "CmnActUkemi";
    private const string CrouchBlockingAnimation = "CmnActCrouchGuardLoop";
    private const string StandBlockingAnimation = "CmnActMidGuardLoop";
    private const string HighBlockingAnimation = "CmnActHighGuardLoop";

    public IMemoryReader? MemoryReader { get; set; }

    public bool ShouldCheckWakingUp { get; set; } = true;
    public bool ShouldCheckWallSplat { get; set; } = true;
    public bool ShouldCheckAirTech { get; set; } = false;
    public bool ShouldCheckStartBlocking { get; set; } = false;
    public bool ShouldCheckBlockstunEnding { get; set; } = false;

    public bool IsValid =>
        ShouldCheckWakingUp || ShouldCheckWallSplat || ShouldCheckAirTech || ShouldCheckStartBlocking || ShouldCheckBlockstunEnding;

    private int _lastBlockstun;

    public int FramesUntilEvent(int inputReversalFrame, bool isUserControllingDummy)
    {
        if (MemoryReader is null)
            return int.MaxValue;

        var playerSide = MemoryReader.GetPlayerSide();
        var dummySide = 1 - playerSide;
        var blockstun = MemoryReader.GetBlockstun(dummySide);
        var lastBlockstun = _lastBlockstun;
        _lastBlockstun = blockstun;
        
        if (isUserControllingDummy)
        {
            return int.MaxValue;
        }
        var currentDummy = MemoryReader.GetCurrentDummy();
        var animationString = MemoryReader.ReadAnimationString(dummySide);
        var animFrame = MemoryReader.GetAnimFrame(dummySide);
        var hitstop = MemoryReader.GetHitstop(dummySide);

        var result = animationString switch
        {
            FaceDownAnimation when ShouldCheckWakingUp => currentDummy.FaceDownFrames - animFrame,
            FaceUpAnimation when ShouldCheckWakingUp => currentDummy.FaceUpFrames - animFrame,
            WallSplatAnimation when ShouldCheckWallSplat => currentDummy.WallSplatWakeupTiming - animFrame,
            TechAnimation when ShouldCheckAirTech => 9 - animFrame,
            _ => int.MaxValue,
        };

        if (result == int.MaxValue && ShouldCheckStartBlocking && blockstun > 0 && lastBlockstun == 0)
            result = 0;
        if (result == int.MaxValue && ShouldCheckBlockstunEnding && blockstun > 0)
            result = blockstun + hitstop - 1;

        IScenarioEvent thisButEvent = this;
        result = thisButEvent.ApplySuperFreezeSlowdown(result, dummySide, playerSide, inputReversalFrame);
        return result;
    }
    public bool CanEnable(IScenarioAction action, int slotNumber)
    {
        return action.Inputs[slotNumber - 1].IsReversalValid && IsValid;
    }
    public bool DependsOnReversalFrame() => true;
    
}