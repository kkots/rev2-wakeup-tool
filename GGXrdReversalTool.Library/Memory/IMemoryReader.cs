using System.Diagnostics;
using GGXrdReversalTool.Library.Domain.Characters;
using GGXrdReversalTool.Library.Memory.Implementations;
using GGXrdReversalTool.Library.Memory.Pointer;
using GGXrdReversalTool.Library.Models;
using GGXrdReversalTool.Library.Models.Inputs;
using GGXrdReversalTool.Library.Scenarios.Event;

namespace GGXrdReversalTool.Library.Memory;

public interface IMemoryReader
{
    string ReadAnimationString(int player);
    int FrameCount();
    Character GetCurrentDummy();
    bool SetDummyPlayback(int slotNumber, int inputIndex, int startingSide);
    bool StopDummyPlayback();
    bool SetDummyRecordingSlot(int slotNumber);
    TrainingDummyRecordingMode GetDummyMode();
    int GetTrainingRecordingSlot();
    bool WriteInputInSlot(int slotNumber, SlotInput slotInput);
    int GetComboCount(int player);
    int GetBlockstun(int player);
    int GetHitstop(int player);
    int GetFacing(int player);
    int GetAnimFrame(int player);
    int GetSlowdownFrames(int player);
    int GetSuperflashFreezeFrames(int player);
    int GetPlayerSide();
    bool IsUserControllingDummy();
    bool IsTrainingMode();
    public bool IsWorldInTick();
    public uint GetEngineTickCount();
    public bool MatchRunning();
    public uint GetAswEngineTickCount();

    Process Process { get; }
    SlotInput ReadInputFromSlot(int slotNumber);
    void LockDummy(int player, out uint oldFlags);
    void UnlockDummy(int player, uint oldFlags);
    bool GetIsCurrentlyInHitstun(int player);
    bool GetWillBeInHitstunNextFrame(int player);
    bool GetAreNormalsEnabled(int player);
    int GetHitstun(int player);
    bool GetIsAirtechEnabled(int player);
    RecoveryValues GetAirRecoverySetting();
    StunRecoveryValues GetStunRecoverySetting();
    bool WriteAirRecoverySetting(RecoveryValues setting);
    bool GuaranteeChargeInput(int player);
    int GetLifetimeCounter(int player);
    int GetDizzyMashAmountLeft(int player);
    int GetBBScrVar(int player);
    int GetBBScrVar4(int player);
    int GetBBScrVar5(int player);
    int GetCmnActIndex(int player);
    string GetGotoLabelRequest(int player);
    int GetCurrentHitEffect(int player);
    int GetStaggerRelatedValue(int player);
    bool IsBattle();
    bool GetWillBeInBlockstunNextFrame(int player);
    BlockTypes GetDummyBlockType(int player);
    bool GetIsSuccessfulIB(int player);
    StanceValues GetStanceSetting();
    void WriteStanceSetting(StanceValues setting);
    BlockSettingsValues GetBlockSettings();
    void WriteBlockSettings(BlockSettingsValues setting);
    BlockSwitchingValues GetBlockSwitching();
    void WriteBlockSwitching(BlockSwitchingValues setting);
    BlockTypeValues GetBlockTypeSetting();
    void WriteBlockTypeSetting(BlockTypeValues setting);
    ForcedBlockStance GetForcedBlockStance(int player);
    bool GetIsAirborne(int player);
    BlockTypes DetermineBlockType(int player);  // works on players, idle dummies and dummies that are not idle. The result only valid on the frame on which a hit got blocked
    bool IsHoldingFD(int player);
    bool IsHoldingBlock(int player);
    bool IsIdleDummy(int player);
    
}
