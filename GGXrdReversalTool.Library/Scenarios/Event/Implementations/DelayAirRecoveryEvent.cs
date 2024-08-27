using GGXrdReversalTool.Library.Memory;
using GGXrdReversalTool.Library.Scenarios.Action;

namespace GGXrdReversalTool.Library.Scenarios.Event.Implementations;

public class DelayAirRecoveryEvent : IScenarioEvent
{
    public int oldAirRecoverySetting = -1;
    public int MaxDelay;
    public int MinDelay;
    public AirRecoveryTypes AirRecoveryType;
    private readonly Random _random = new();
    private const string TechAnimation = "CmnActUkemi";
    private const int TechDuration = 9;
    public IMemoryReader? MemoryReader { get; set; }
    public bool IsValid => true;
    private int _framesSinceTechPossible = -1;
    private bool _recoveryEnabled = false;
    private int _delayBy = -1;
    public int FramesUntilEvent(int inputReversalFrame)
    {
        if (MemoryReader is null)
            return int.MaxValue;
        
        IScenarioEvent thisButEvent = this;

        var playerSide = MemoryReader.GetPlayerSide();
        var dummySide = 1 - playerSide;
        var timeUntilTech = MemoryReader.GetTimeUntilTech(dummySide);
        bool hasTechRelatedFlag = MemoryReader.GetTechRelatedFlag(dummySide);
        
        bool techOnNextFrame = timeUntilTech == 1 && hasTechRelatedFlag;
        
        if (techOnNextFrame)
        {
            _framesSinceTechPossible = 0;
            _delayBy = _random.Next(MinDelay, MaxDelay + 1);
            if (_delayBy != 0 && _recoveryEnabled)
            {
                MemoryReader.WriteAirRecoverySetting(0);
                _recoveryEnabled = false;
            }
        }
        
        if (_framesSinceTechPossible != -1)
        {
            if (_framesSinceTechPossible == _delayBy)
            {
                MemoryReader.StopDummyPlayback();
                MemoryReader.WriteAirRecoverySetting(AirRecoveryType switch
                    {
                        AirRecoveryTypes.Backward => 1,
                        AirRecoveryTypes.Neutral => 2,
                        AirRecoveryTypes.Forward => 3,
                        AirRecoveryTypes.Random => 4,
                        _ => 2
                    });
                _recoveryEnabled = true;
                _framesSinceTechPossible = -1;
                return thisButEvent.ApplySuperFreezeSlowdown(TechDuration, dummySide, playerSide, inputReversalFrame);;
            }
            if (_framesSinceTechPossible < int.MaxValue)
                ++_framesSinceTechPossible;
        }
        
        var animationString = MemoryReader.ReadAnimationString(dummySide);
        if (animationString == TechAnimation)
        {
            var animFrame = MemoryReader.GetAnimFrame(dummySide);
            return thisButEvent.ApplySuperFreezeSlowdown(TechDuration - animFrame, dummySide, playerSide, inputReversalFrame);
        }
        return int.MaxValue;
    }
    public bool CanEnable(IScenarioAction action, int slotNumber)
    {
        return action.Inputs[slotNumber - 1].IsReversalValid && IsValid;
    }
    public bool DependsOnReversalFrame() => true;
    public void Init()
    {
        if (MemoryReader == null)
            return;
        
        oldAirRecoverySetting = MemoryReader.GetAirRecoverySetting();
        MemoryReader.WriteAirRecoverySetting(0);
        _recoveryEnabled = false;
    }
    public void Finish()
    {
        if (MemoryReader == null)
            return;
        
        if (oldAirRecoverySetting != -1)
        {
            MemoryReader.WriteAirRecoverySetting(oldAirRecoverySetting);
        }
    }

}