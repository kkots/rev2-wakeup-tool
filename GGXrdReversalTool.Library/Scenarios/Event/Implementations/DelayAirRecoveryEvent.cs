using GGXrdReversalTool.Library.Memory;
using GGXrdReversalTool.Library.Scenarios.Action;

namespace GGXrdReversalTool.Library.Scenarios.Event.Implementations;

public class DelayAirRecoveryEvent : IScenarioEvent
{
    private int _oldAirRecoverySetting = -1;
    public int MaxDelay { get; set; } = 20;
    public int MinDelay { get; set; } = 5;
    public AirRecoveryTypes AirRecoveryType { get; set; } = AirRecoveryTypes.Neutral;
    private readonly Random _random = new();
    private const string TechAnimation = "CmnActUkemi";
    private const int TechDuration = 9;
    public IMemoryReader? MemoryReader { get; set; }
    public bool IsValid => MinDelay <= MaxDelay;
    private int _framesSinceTechPossible = -1;
    private bool _recoveryEnabled = true;
    private int _delayBy = -1;
    private int _prevAirRecoverySetting = -1;
    public int FramesUntilEvent(int inputReversalFrame, bool isUserControllingDummy)
    {
        if (MemoryReader is null)
            return int.MaxValue;
        
        IScenarioEvent thisButEvent = this;
        
        int currentAirRecoverySetting = MemoryReader.GetAirRecoverySetting();
        if (currentAirRecoverySetting != _prevAirRecoverySetting)
        {
            _oldAirRecoverySetting = currentAirRecoverySetting;
            _recoveryEnabled = (currentAirRecoverySetting != 0);
        }

        if (isUserControllingDummy)
        {
            _framesSinceTechPossible = -1;
            return int.MaxValue;
        }

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
                _prevAirRecoverySetting = 0;
                _recoveryEnabled = false;
            }
        }
        
        if (_framesSinceTechPossible != -1)
        {
            if (_framesSinceTechPossible == _delayBy)
            {
                MemoryReader.StopDummyPlayback();
                if (!_recoveryEnabled)
                {
                    _prevAirRecoverySetting = AirRecoveryType switch {
                        AirRecoveryTypes.Backward => 1,
                        AirRecoveryTypes.Neutral => 2,
                        AirRecoveryTypes.Forward => 3,
                        AirRecoveryTypes.Random => 4,
                        _ => 2
                    };
                    MemoryReader.WriteAirRecoverySetting(_prevAirRecoverySetting);
                    _recoveryEnabled = true;
                }
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
        
        _oldAirRecoverySetting = MemoryReader.GetAirRecoverySetting();
        _prevAirRecoverySetting = _oldAirRecoverySetting;
        _recoveryEnabled = (_oldAirRecoverySetting != 0);
    }
    public void Finish()
    {
        if (MemoryReader == null)
            return;
        
        if (_oldAirRecoverySetting != -1)
        {
            MemoryReader.WriteAirRecoverySetting(_oldAirRecoverySetting);
        }
    }

}