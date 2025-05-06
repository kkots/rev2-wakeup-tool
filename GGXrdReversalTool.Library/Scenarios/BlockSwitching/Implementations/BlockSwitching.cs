
using GGXrdReversalTool.Library.Memory;
using GGXrdReversalTool.Library.Models;
using GGXrdReversalTool.Library.Scenarios.Event;
using System.Xml.Linq;

namespace GGXrdReversalTool.Library.Scenarios.BlockSwitching.Implementations
{
    public class BlockSwitching : IScenarioBlockSwitching
    {
        public IMemoryReader? MemoryReader { get; set; }
        public BlockSwitchingElement[] Elements { get; set; } = Array.Empty<BlockSwitchingElement>();
        
        private readonly Random _random = new();
        private int _elementIndex = 0;
        private int _elementSubindex = 0;
        private bool _isHitReversal = false;
        public int BlockedHitTimer { get; set; } = 30;
        private int _timeSinceLastBlock = int.MaxValue;
        private bool _enumeratorEnd = false;
        public bool IsLooping { get; set; } = false;
        private BlockTypeValues? _pendingBlockWrite = null;
        
        public void Init()
        {
            OnStageReset();
        }
        
        public void OnStageReset()
        {
            if (MemoryReader is null)
                return;
            
            _timeSinceLastBlock = int.MaxValue;
            ResetEnumerator();
            var playerSide = MemoryReader.GetPlayerSide();
            var dummySide = 1 - playerSide;
            ApplyCurrentElement(dummySide);
            ApplyPendingBlockWrite();
        }
        
        public void Tick(bool isUserControllingDummy)
        {
            if (MemoryReader is null)
                return;
            
            var playerSide = MemoryReader.GetPlayerSide();
            var dummySide = 1 - playerSide;
            if (isUserControllingDummy) dummySide = playerSide;
            
            ApplyPendingBlockWrite();
            
            if (MemoryReader.GetWillBeInHitstunNextFrame(dummySide) && !MemoryReader.GetIsCurrentlyInHitstun(dummySide))
            {
                // in case the first element has a random stance on it,
                // make the new random choice as soon as dummy enters hitstun
                ResetEnumerator();
                ApplyCurrentElement(dummySide);
                _timeSinceLastBlock = int.MaxValue;
                return;
            }
            
            bool blockedHitOnThisFrame = MemoryReader.GetWillBeInBlockstunNextFrame(dummySide);
            if (blockedHitOnThisFrame)
            {
                _isHitReversal = !_enumeratorEnd && Elements[_elementIndex].IsReversal;
                IncrementSubindex();
                ApplyCurrentElement(dummySide);
                _timeSinceLastBlock = 0;
            }
                
            int blockstun = MemoryReader.GetBlockstun(dummySide);
            if (_timeSinceLastBlock < BlockedHitTimer && blockstun == 0 && MemoryReader.GetSuperflashFreezeFrames(dummySide) == 0)
            {
                ++_timeSinceLastBlock;
                if (_timeSinceLastBlock == BlockedHitTimer)
                {
                    ResetEnumerator();
                    ApplyCurrentElement(dummySide);
                }
            }
            
        }
        public bool IsHitReversal() => _isHitReversal;
        
        private void ResetEnumerator()
        {
            _elementIndex = 0;
            _elementSubindex = 0;
            _enumeratorEnd = _elementIndex >= Elements.Length;
            SkipZeroLengthElements();
        }
        
        private void SkipZeroLengthElements()
        {
            while (!_enumeratorEnd)
            {
                BlockSwitchingElement element = Elements[_elementIndex];
                if (element.Multiplier > 0) break;
                _enumeratorEnd = ++_elementIndex >= Elements.Length;
            }
        }
        
        private void IncrementSubindex()
        {
            if (_enumeratorEnd) return;
            
            BlockSwitchingElement element = Elements[_elementIndex];
            
            if (_elementSubindex >= element.Multiplier - 1)
            {
                _enumeratorEnd = ++_elementIndex >= Elements.Length;
                if (_enumeratorEnd && Elements.Length > 0 && IsLooping)
                {
                    _elementIndex = 0;
                    _enumeratorEnd = false;
                }
                _elementSubindex = 0;
                SkipZeroLengthElements();
            }
            else
            {
                ++_elementSubindex;
            }
        }
        
        private void ApplyCurrentElement(int dummySide)
        {
            if (_enumeratorEnd) return;
            BlockSwitchingElement element = Elements[_elementIndex];
            ApplyCurrentElementStance(element.Stance, dummySide);
            ApplyCurrentElementBlock(element.Block, dummySide);
            ApplyCurrentElementSwitching(element.BlockSwitching, dummySide);
            ApplyCurrentElementSettings(element.BlockSettings, dummySide);
        }
        
        private void ApplyCurrentElementStance(BlockInputStanceType stance, int dummySide)
        {
            if (MemoryReader is null || _enumeratorEnd)
                return;
            
            switch (stance)
            {
                case BlockInputStanceType.Standing:
                    MemoryReader.WriteStanceSetting(StanceValues.Standing);
                    break;
                case BlockInputStanceType.Crouching:
                    MemoryReader.WriteStanceSetting(StanceValues.Crouching);
                    break;
                case BlockInputStanceType.Jumping:
                    MemoryReader.WriteStanceSetting(StanceValues.Jumping);
                    break;
                case BlockInputStanceType.Pin:
                {
                    if (MemoryReader.GetIsAirborne(dummySide))
                    {
                        MemoryReader.WriteStanceSetting(StanceValues.Jumping);
                        return;
                    }
                    ForcedBlockStance forcedStance = MemoryReader.GetForcedBlockStance(dummySide);
                    if (forcedStance == ForcedBlockStance.Standing) MemoryReader.WriteStanceSetting(StanceValues.Standing);
                    else if (forcedStance == ForcedBlockStance.Crouching) MemoryReader.WriteStanceSetting(StanceValues.Crouching);
                    break;
                }
                case BlockInputStanceType.Opposite:
                {
                    ForcedBlockStance forcedStance = MemoryReader.GetForcedBlockStance(dummySide);
                    StanceValues? currentStance = null;
                    if (forcedStance == ForcedBlockStance.Standing) currentStance = StanceValues.Standing;
                    else if (forcedStance == ForcedBlockStance.Crouching) currentStance = StanceValues.Crouching;
                    else if (!MemoryReader.GetIsAirborne(dummySide))
                    {
                        currentStance = MemoryReader.GetStanceSetting();
                    }
                    MemoryReader.WriteStanceSetting(currentStance == StanceValues.Standing
                            ? StanceValues.Crouching
                            : StanceValues.Standing);
                    break;
                }
                case BlockInputStanceType.Random:
                {
                    int r = _random.Next(1, 3);
                    if (r == 1) MemoryReader.WriteStanceSetting(StanceValues.Standing);
                    else MemoryReader.WriteStanceSetting(StanceValues.Crouching);
                    break;
                }
            }
        }
        
        private void ApplyCurrentElementBlock(BlockInputBlockType block, int dummySide)
        {
            _pendingBlockWrite = null;
            
            if (MemoryReader is null || _enumeratorEnd)
                return;
            
            switch (block)
            {
                case BlockInputBlockType.Normal:
                    _pendingBlockWrite = BlockTypeValues.NormalBlock;
                    break;
                case BlockInputBlockType.IB:
                    _pendingBlockWrite = BlockTypeValues.InstantBlock;
                    break;
                case BlockInputBlockType.FD:
                    _pendingBlockWrite = BlockTypeValues.FaultlessDefense;
                    break;
                case BlockInputBlockType.Pin:
                {
                    BlockTypes currentBlock = MemoryReader.DetermineBlockType(dummySide);
                    _pendingBlockWrite = currentBlock switch {
                        BlockTypes.Normal => BlockTypeValues.NormalBlock,
                        BlockTypes.InstantBlock => BlockTypeValues.InstantBlock,
                        BlockTypes.FaultlessDefense => BlockTypeValues.FaultlessDefense,
                        _ => BlockTypeValues.NormalBlock
                    };
                    break;
                }
                case BlockInputBlockType.GamesOwnImplementationOfRandom:
                    _pendingBlockWrite = BlockTypeValues.Random;
                    break;
                case BlockInputBlockType.Random:
                    int r = _random.Next(1,4);
                    _pendingBlockWrite = r switch
                    {
                        1 => BlockTypeValues.NormalBlock,
                        2 => BlockTypeValues.InstantBlock,
                        _ => BlockTypeValues.FaultlessDefense
                    };
                    break;
            }
        }
        
        private void ApplyCurrentElementSwitching(BlockInputSwitchingType block, int dummySide)
        {
            if (MemoryReader is null || _enumeratorEnd)
                return;
            
            switch (block)
            {
                case BlockInputSwitchingType.On:
                    MemoryReader.WriteBlockSwitching(BlockSwitchingValues.Enabled);
                    break;
                case BlockInputSwitchingType.Off:
                    MemoryReader.WriteBlockSwitching(BlockSwitchingValues.Disabled);
                    break;
                case BlockInputSwitchingType.OnSecond:
                    MemoryReader.WriteBlockSwitching(BlockSwitchingValues.SwitchOnThe2nd);
                    break;
            }
        }
        
        private void ApplyCurrentElementSettings(BlockInputBlockSettingsType block, int dummySide)
        {
            if (MemoryReader is null || _enumeratorEnd)
                return;
            
            switch (block)
            {
                case BlockInputBlockSettingsType.NotBlock:
                    MemoryReader.WriteBlockSettings(BlockSettingsValues.None);
                    break;
                case BlockInputBlockSettingsType.Everything:
                    MemoryReader.WriteBlockSettings(BlockSettingsValues.Everything);
                    break;
                case BlockInputBlockSettingsType.AfterFirst:
                    MemoryReader.WriteBlockSettings(BlockSettingsValues.AfterFirstHit);
                    break;
                case BlockInputBlockSettingsType.FirstOnly:
                    MemoryReader.WriteBlockSettings(BlockSettingsValues.FirstHitOnly);
                    break;
                case BlockInputBlockSettingsType.Random:
                    MemoryReader.WriteBlockSettings(BlockSettingsValues.Random);
                    break;
                case BlockInputBlockSettingsType.Pin:
                    MemoryReader.WriteBlockSettings(MemoryReader.IsHoldingBlock(dummySide)
                            ? BlockSettingsValues.Everything
                            : BlockSettingsValues.None);
                    break;
            }
        }
        private void ApplyPendingBlockWrite()
        {
            if (_pendingBlockWrite is null || MemoryReader is null) return;
            MemoryReader.WriteBlockTypeSetting((BlockTypeValues)_pendingBlockWrite);
            _pendingBlockWrite = null;
        }
    }
}
