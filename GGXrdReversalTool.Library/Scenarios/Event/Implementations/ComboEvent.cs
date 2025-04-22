using GGXrdReversalTool.Library.Memory;
using GGXrdReversalTool.Library.Models.Inputs;
using GGXrdReversalTool.Library.Scenarios.Action;

namespace GGXrdReversalTool.Library.Scenarios.Event.Implementations;

public class ComboEvent : IScenarioEvent
{
    public int MinComboCount { get; set; } = 1;
    public int MaxComboCount { get; set; } = 5;
    public EndsStartsTypes HitstunStartsEnds { get; set; } = EndsStartsTypes.Starts;

    public IMemoryReader? MemoryReader { get; set; }

    private int _oldComboCount;
    private AnimationEvent? animationEvent = null;
    
    public bool IsValid => MinComboCount <= MaxComboCount;
    
    public void OnStageReset() => Init();
    
    public void Init()
    {
        _oldComboCount = 0;
        if (HitstunStartsEnds == EndsStartsTypes.Starts) return;
        animationEvent = new AnimationEvent()
        {
            MemoryReader = MemoryReader,
            ShouldCheckHitstunEnding = true
        };
    }
    
    public int FramesUntilEvent(bool isUserControllingDummy)
    {
        if (MemoryReader is null)
            return int.MaxValue;

        var comboCount = MemoryReader.GetComboCount(1 - MemoryReader.GetPlayerSide());
        
        int result = int.MaxValue;
        
        if (HitstunStartsEnds == EndsStartsTypes.Ends)
        {
            int animationEventResult = animationEvent!.FramesUntilEvent(isUserControllingDummy);
            if (comboCount >= MinComboCount && comboCount <= MaxComboCount) result = animationEventResult;
        }
        else
        {
            if (comboCount >= MinComboCount && comboCount <= MaxComboCount && _oldComboCount != comboCount)
                result = 0;
        }

        _oldComboCount = comboCount;

        return isUserControllingDummy ? int.MaxValue : result;
    }
    public bool CanEnable(IScenarioAction action, int slotNumber)
    {
        SlotInput slotInput = action.Inputs[slotNumber - 1];
        
        return (DependsOnReversalFrame() ? slotInput.IsReversalValid : slotInput.IsValid)
            && IsValid;
    }
    public bool DependsOnReversalFrame() => HitstunStartsEnds == EndsStartsTypes.Ends;

}