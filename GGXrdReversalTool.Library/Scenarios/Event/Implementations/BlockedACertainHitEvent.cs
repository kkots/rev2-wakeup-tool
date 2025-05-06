using GGXrdReversalTool.Library.Memory;
using GGXrdReversalTool.Library.Scenarios.Action;
using GGXrdReversalTool.Library.Models;
using GGXrdReversalTool.Library.Models.Inputs;

namespace GGXrdReversalTool.Library.Scenarios.Event.Implementations;

public class BlockedACertainHitEvent : IScenarioEvent
{
    public int MinHitNumber { get; set; } = 1;
    public int MaxHitNumber { get; set; } = 100;
    public bool IsHitReversal { get; set; } = false;
    public EndsStartsTypes BlockstunEndsStartsFilter { get; set; } = EndsStartsTypes.Ends;
    public BlockTypes BlockTypeFilter { get; set; } = BlockTypes.Any;
    public IMemoryReader? MemoryReader { get; set; }
    public bool IsValid => MinHitNumber <= MaxHitNumber;
    public bool UseBlockSwitching { get; set; } = false;
    public int BlockedHitTimer { get; set; }
    
    private bool _lastBlockOk = false;
    private int _timeSinceLastBlock = int.MaxValue;
    private int _hitNumber = 0;
    
    public void Init() => OnStageReset();
    
    public int FramesUntilEvent(bool isUserControllingDummy)
    {
        if (UseBlockSwitching)
        {
            return FramesUntilEvent_UseBlockSwitching(isUserControllingDummy);
        }
        else
        {
            return FramesUntilEvent_OwnHitCounter(isUserControllingDummy);
        }
    }
    
    private int FramesUntilEvent_UseBlockSwitching(bool isUserControllingDummy)
    {
        if (MemoryReader is null)
            return int.MaxValue;
        
        if (isUserControllingDummy)
        {
            return int.MaxValue;
        }
        
        var playerSide = MemoryReader.GetPlayerSide();
        var dummySide = 1 - playerSide;
        
        if (MemoryReader.GetWillBeInBlockstunNextFrame(dummySide))
        {
            _lastBlockOk = false;
            if ((BlockTypeFilter == BlockTypes.Any || BlockTypeFilter == MemoryReader.DetermineBlockType(dummySide))
                    && IsHitReversal)
            {
                if (BlockstunEndsStartsFilter == EndsStartsTypes.Starts)
                    return 0;
                
                _lastBlockOk = true;
            }
        }
        
        if (BlockstunEndsStartsFilter == EndsStartsTypes.Ends && _lastBlockOk)
        {
            int blockstun = MemoryReader.GetBlockstun(dummySide);
            if (blockstun <= 0) return int.MaxValue;
            int hitstop = MemoryReader.GetHitstop(dummySide);
            
            IScenarioEvent thisButEvent = this;
            return thisButEvent.ApplySuperFreezeSlowdown(hitstop + blockstun - 1, dummySide, playerSide);
        }
        
        return int.MaxValue;
    }
    
    private int FramesUntilEvent_OwnHitCounter(bool isUserControllingDummy)
    {
        if (MemoryReader is null)
            return int.MaxValue;
        
        var playerSide = MemoryReader.GetPlayerSide();
        var dummySide = 1 - playerSide;
        
        if (MemoryReader.GetWillBeInHitstunNextFrame(dummySide) && !MemoryReader.GetIsCurrentlyInHitstun(dummySide))
        {
            _timeSinceLastBlock = int.MaxValue;
            _hitNumber = 0;
            _lastBlockOk = false;
            return int.MaxValue;
        }
        
        bool blockedHitOnThisFrame = MemoryReader.GetWillBeInBlockstunNextFrame(dummySide);
        if (blockedHitOnThisFrame)
        {
            _lastBlockOk = false;
            if (_timeSinceLastBlock >= BlockedHitTimer) _hitNumber = 0;
            ++_hitNumber;
            _timeSinceLastBlock = 0;
        }
        
        int blockstun = MemoryReader.GetBlockstun(dummySide);
        if (_timeSinceLastBlock < BlockedHitTimer
                && blockstun == 0
                && MemoryReader.GetSuperflashFreezeFrames(dummySide) == 0)
        {
            ++_timeSinceLastBlock;
        }
        
        if (isUserControllingDummy)
        {
            return int.MaxValue;
        }
        
        if (blockedHitOnThisFrame)
        {
            if ((BlockTypeFilter == BlockTypes.Any || BlockTypeFilter == MemoryReader.DetermineBlockType(dummySide))
                    && _hitNumber >= MinHitNumber && _hitNumber <= MaxHitNumber)
            {
                if (BlockstunEndsStartsFilter == EndsStartsTypes.Starts)
                    return 0;
                _lastBlockOk = true;
            }
        }
        
        if (BlockstunEndsStartsFilter == EndsStartsTypes.Ends && _lastBlockOk)
        {
            if (blockstun <= 0) return int.MaxValue;
            int hitstop = MemoryReader.GetHitstop(dummySide);
            
            IScenarioEvent thisButEvent = this;
            return thisButEvent.ApplySuperFreezeSlowdown(hitstop + blockstun - 1, dummySide, playerSide);
        }
        
        return int.MaxValue;
    }
    
    public bool CanEnable(IScenarioAction action, int slotNumber)
    {
        SlotInput slotInput = action.Inputs[slotNumber - 1];
        return (
            DependsOnReversalFrame() ? slotInput.IsReversalValid : slotInput.IsValid
        ) && IsValid;
    }
    public bool DependsOnReversalFrame() => BlockstunEndsStartsFilter == EndsStartsTypes.Ends;
    public void OnStageReset()
    {
        _timeSinceLastBlock = BlockedHitTimer;
        _hitNumber = 0;
    }
}
