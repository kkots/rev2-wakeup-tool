using GGXrdReversalTool.Library.Memory;
using GGXrdReversalTool.Library.Memory.Implementations;
using GGXrdReversalTool.Library.Scenarios.Action;
using System.Numerics;

namespace GGXrdReversalTool.Library.Scenarios.Event.Implementations;

public class AnimationEvent : IScenarioEvent
{
    private const string FaceDownAnimation = "CmnActFDown2Stand";
    private const string FaceUpAnimation = "CmnActBDown2Stand";
    private const string WallSplatAnimation = "CmnActWallHaritsukiGetUp";
    private const string TechAnimation = "CmnActUkemi";

    public IMemoryReader? MemoryReader { get; set; }

    public bool ShouldCheckWakingUp { get; set; } = true;
    public bool ShouldCheckWallSplat { get; set; } = true;
    public bool ShouldCheckAirTech { get; set; } = false;
    public bool ShouldCheckStartBlocking { get; set; } = false;
    public bool ShouldCheckBlockstunEnding { get; set; } = false;
    public bool ShouldCheckHitstunStarting { get; set; } = false;
    public bool ShouldCheckHitstunEnding { get; set; } = false;

    public bool IsValid =>
        ShouldCheckWakingUp
        || ShouldCheckWallSplat
        || ShouldCheckAirTech
        || ShouldCheckStartBlocking
        || ShouldCheckBlockstunEnding
        || ShouldCheckHitstunStarting
        || ShouldCheckHitstunEnding;

    private int _prevFrameBBScrVar = 0;
    private int _prevFrameBBScrVar5 = 0;
    private int _staggerMax = 0;

    public int FramesUntilEvent(bool isUserControllingDummy)
    {
        if (MemoryReader is null)
            return int.MaxValue;

        var playerSide = MemoryReader.GetPlayerSide();
        var dummySide = 1 - playerSide;
        
        if (isUserControllingDummy)
        {
            return int.MaxValue;
        }
        IScenarioEvent thisButEvent = this;
        var blockstun = MemoryReader.GetBlockstun(dummySide);
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
        if (result == int.MaxValue && ShouldCheckHitstunStarting)
        {
            if (!MemoryReader.GetIsCurrentlyInHitstun(dummySide)
                    && MemoryReader.GetWillBeInHitstunNextFrame(dummySide))
            {
                return 0;  // do not apply slowdown
            }
        }
        if (result == int.MaxValue && ShouldCheckHitstunEnding)
        {
            int cmnActIndex = MemoryReader.GetCmnActIndex(dummySide);
            if (cmnActIndex == 57 || MemoryReader.GetDizzyMashAmountLeft(dummySide) > 0)
            {
                // remaining dizziness is too hard to calculate, because as soon as you tell the dummy to play a recording it will stop automashing through it,
                // making it actually dependent on the reversal frame index and on the contents of text in the Action to perform panel
                return int.MaxValue;
            }
            var hitstun = CalculateHitstun(dummySide, cmnActIndex, animFrame, hitstop);
            if (hitstun > 0)
            {
                result = hitstun + hitstop - 1;
            }
        }

        if (result == int.MaxValue && ShouldCheckStartBlocking && blockstun == 0 && MemoryReader.GetWillBeInBlockstunNextFrame(dummySide))
            return 0;  // do not apply slowdown
            
        if (result == int.MaxValue && ShouldCheckBlockstunEnding && blockstun > 0)
            result = blockstun + hitstop - 1;

        return thisButEvent.ApplySuperFreezeSlowdown(result, dummySide, playerSide);
    }
    public bool CanEnable(IScenarioAction action, int slotNumber)
    {
        return action.Inputs[slotNumber - 1].IsReversalValid && IsValid;
    }
    public bool DependsOnReversalFrame() => ShouldCheckWakingUp
        || ShouldCheckWallSplat
        || ShouldCheckAirTech
        || ShouldCheckBlockstunEnding
        || ShouldCheckHitstunEnding;
    
    private int CalculateHitstun(int dummySide, int cmnActIndex, int animFrame, int hitstop)
    {
        if (MemoryReader is null)
            return 0;
        
        return cmnActIndex switch
        {
            36 or 42 or 48 or 54 => 0,  // knockdown loops and wallslump
            56 => CalculateStaggerHitstun(dummySide, animFrame, hitstop),
            74 or 75 or 76 => CalculateRejectHitstun(dummySide, animFrame),
            _ => MemoryReader.GetIsAirtechEnabled(dummySide) ? 0  // airteching is handled by another event
                : MemoryReader.GetHitstun(dummySide)
        };
    }
    private int CalculateStaggerHitstun(int dummySide, int animFrame, int hitstop)
    {
        if (MemoryReader is null)
            return 0;
        
        int oldBBScrVar = _prevFrameBBScrVar;
        int oldBBScrVar5 = _prevFrameBBScrVar5;
        var newBBScrVar = MemoryReader.GetBBScrVar(dummySide);
        _prevFrameBBScrVar = newBBScrVar;
        _prevFrameBBScrVar5 = MemoryReader.GetBBScrVar5(dummySide);
        if (animFrame == 1)
        {
            return 0;  // we need _prevFrameBBScrVar5 to be ready
        }
        
        if (newBBScrVar == 0 || oldBBScrVar == 0)
        {
            int staggerMax = oldBBScrVar5 / 10 + MemoryReader.GetStaggerRelatedValue(dummySide);
            
            if (newBBScrVar == 0)
            {
                _staggerMax = staggerMax;
            }
            else if (oldBBScrVar == 0)
            {
                _staggerMax = Math.Max(staggerMax, animFrame + 4);
            }
        }
        if (_staggerMax == 0) return 0;
        return _staggerMax - (animFrame - 1) - (hitstop > 0 ? 1 : 0);
    }
    private int CalculateRejectHitstun(int dummySide, int animFrame)
    {
        if (MemoryReader is null)
            return 0;
        
        int rejectionMax;
        int currentHitEffect = MemoryReader.GetCurrentHitEffect(dummySide);
        if (currentHitEffect >= 22 && currentHitEffect <= 25)  // these basically mean rejection due to throw clash
        {
            rejectionMax = 30;
        }
        else
        {
            rejectionMax = 60;
        }
        return rejectionMax - animFrame + 1;
    }
}
