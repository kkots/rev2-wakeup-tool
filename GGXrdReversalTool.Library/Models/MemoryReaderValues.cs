
namespace GGXrdReversalTool.Library.Models
{
    
    public enum StanceValues
    {
        Standing,
        Crouching,
        Jumping
    }
    
    public enum BlockSettingsValues
    {
        None,
        Everything,
        AfterFirstHit,
        FirstHitOnly,
        Random
    }
    
    public enum BlockSwitchingValues
    {
        Disabled,
        Enabled,
        SwitchOnThe2nd
    }
    
    public enum BlockTypeValues
    {
        NormalBlock,
        InstantBlock,
        FaultlessDefense,
        Random
    }
    
    public enum RecoveryValues
    {
        Disabled,
        Backward,
        Neutral,
        Forward,
        Random
    }
    
    public enum StunRecoveryValues
    {
        Disabled,
        Lv1,
        Lv2,
        Lv3,
        QuickestPossible
    }
    
    public enum TrainingDummyRecordingMode
    {
        Idle,
        Controlling,
        Recording,
        PlayingBack,
        SettingController,  // "Press Enter/Start on the controller you wish to use."
        Controller,
        COM,
        ContinuousAttacks
    }
    
    public enum ForcedBlockStance
    {
        None,
        Standing,
        Crouching
    }
    
}
