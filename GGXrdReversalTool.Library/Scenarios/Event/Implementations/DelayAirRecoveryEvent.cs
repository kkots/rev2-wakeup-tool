using GGXrdReversalTool.Library.Memory;
using GGXrdReversalTool.Library.Models;
using GGXrdReversalTool.Library.Scenarios.Action;

namespace GGXrdReversalTool.Library.Scenarios.Event.Implementations;

public class DelayAirRecoveryEvent : IScenarioEvent
{
    private RecoveryValues? _oldAirRecoverySetting = null;
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
    private RecoveryValues? _prevAirRecoverySetting = null;
    public int Probability { get; set; } = 100;
    public int MinHitCount { get; set; } = 1;
    public int MaxHitCount { get; set; } = 100;
    
    public int FramesUntilEvent(bool isUserControllingDummy)
    {
        if (MemoryReader is null)
            return int.MaxValue;
        
        IScenarioEvent thisButEvent = this;
        
        RecoveryValues currentAirRecoverySetting = MemoryReader.GetAirRecoverySetting();
        if (currentAirRecoverySetting != _prevAirRecoverySetting)
        {
            _oldAirRecoverySetting = currentAirRecoverySetting;
            _recoveryEnabled = (currentAirRecoverySetting != RecoveryValues.Disabled);
        }

        if (isUserControllingDummy)
        {
            _framesSinceTechPossible = -1;
            return int.MaxValue;
        }
        
        var playerSide = MemoryReader.GetPlayerSide();
        var dummySide = 1 - playerSide;
        
        var timeUntilTech = MemoryReader.GetHitstun(dummySide);
        bool airtechEnabled = MemoryReader.GetIsAirtechEnabled(dummySide);
        
        bool techOnNextFrame = timeUntilTech == 1 && airtechEnabled && !MemoryReader.GetWillBeInHitstunNextFrame(dummySide);
        
        if (techOnNextFrame)
        {
            _framesSinceTechPossible = 0;
            
            int comboCount = MemoryReader.GetComboCount(dummySide);
            if (_random.Next(1, 101) <= Probability
                    && comboCount >= MinHitCount && comboCount <= MaxHitCount)
            {
                _delayBy = _random.Next(MinDelay, MaxDelay + 1);
            }
            else
            {
                _delayBy = int.MaxValue;
            }
            
            if (_delayBy != 0 && _recoveryEnabled)
            {
                MemoryReader.WriteAirRecoverySetting(RecoveryValues.Disabled);
                _prevAirRecoverySetting = RecoveryValues.Disabled;
                _recoveryEnabled = false;
            }
        }
        
        if (_framesSinceTechPossible != -1)
        {
            if (_framesSinceTechPossible == _delayBy)
            {
                MemoryReader.StopDummyPlayback();
                if (!_recoveryEnabled || AirRecoveryType == AirRecoveryTypes.Random)
                {
                    _prevAirRecoverySetting = AirRecoveryType switch {
                        AirRecoveryTypes.Backward => RecoveryValues.Backward,
                        AirRecoveryTypes.Neutral => RecoveryValues.Neutral,
                        AirRecoveryTypes.Forward => RecoveryValues.Forward,
                        // RecoveryValues.Random has a chance of not teching at all.
                        // We have a separate control for probability of teching (see int Probability) so we don't want it.
                        // Instead, we'll randomly set a particular type of recovery
                        AirRecoveryTypes.Random => RandomRecoveryType(),
                        _ => RecoveryValues.Neutral
                    };
                    MemoryReader.WriteAirRecoverySetting((RecoveryValues)_prevAirRecoverySetting);
                    _recoveryEnabled = true;
                }
                _framesSinceTechPossible = -1;
                return thisButEvent.ApplySuperFreezeSlowdown(TechDuration, dummySide, playerSide);
            }
            if (_framesSinceTechPossible < int.MaxValue)
                ++_framesSinceTechPossible;
        }
        
        var animationString = MemoryReader.ReadAnimationString(dummySide);
        if (animationString == TechAnimation)
        {
            var animFrame = MemoryReader.GetAnimFrame(dummySide);
            return thisButEvent.ApplySuperFreezeSlowdown(TechDuration - animFrame, dummySide, playerSide);
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
        _recoveryEnabled = (_oldAirRecoverySetting != RecoveryValues.Disabled);
        _framesSinceTechPossible = -1;
    }
    public void OnStageReset() => _framesSinceTechPossible = -1;
    public void Finish()
    {
        if (MemoryReader == null)
            return;
        
        if (_oldAirRecoverySetting != null)
        {
            MemoryReader.WriteAirRecoverySetting((RecoveryValues)_oldAirRecoverySetting);
        }
    }
    private RecoveryValues RandomRecoveryType()
    {
        int r = _random.Next(1, 4);
        return r switch
        {
            1 => RecoveryValues.Neutral,
            2 => RecoveryValues.Backward,
            _ => RecoveryValues.Forward
        };
    }

}