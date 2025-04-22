using GGXrdReversalTool.Library.Extensions;
using GGXrdReversalTool.Library.Memory;

namespace GGXrdReversalTool.Library.Scenarios.Frequency.Implementations;

public class SlotsInOrderFrequency : IScenarioFrequency
{
    private readonly Random _random = new();

    private int _percentage = 100;
    public int Percentage
    {
        get => _percentage;
        set => _percentage = Math.Clamp(value, 0, 100);
    }
    
    private bool _resetOnStageReset = true;
    public bool ResetOnStageReset
    {
        get => _resetOnStageReset;
        set => _resetOnStageReset = value;
    }
    
    private int[] _usedSlotIndices = new int[0];
    public int[] UsedSlotIndices
    {
        get => _usedSlotIndices;
        set
        {
            _usedSlotIndices = value;
        }
    }
    
    private int _playSlotsInOrderNext = 0;
    
    public bool ShouldHappen(out int slotNumber)
    {
        slotNumber = -1;
        bool mainChanceOk = _random.Next(1, 101) <= _percentage;
        if (!mainChanceOk)
        {
            return false;
        }
        
        slotNumber = _usedSlotIndices[_playSlotsInOrderNext++] + 1;
        if (_playSlotsInOrderNext >= _usedSlotIndices.Length)
        {
            _playSlotsInOrderNext = 0;
        }
        return true;
    }

    public IMemoryReader? MemoryReader { get; set; }
    public int[] UsedSlotNumbers()
    {
        return _usedSlotIndices.Select(index => index + 1).ToArray();
    }
    
    public void OnStageReset()
    {
        if (_resetOnStageReset)
        {
            _playSlotsInOrderNext = 0;
        }
    }
}