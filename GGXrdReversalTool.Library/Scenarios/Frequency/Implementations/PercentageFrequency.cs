using GGXrdReversalTool.Library.Extensions;
using GGXrdReversalTool.Library.Memory;

namespace GGXrdReversalTool.Library.Scenarios.Frequency.Implementations;

public class PercentageFrequency : IScenarioFrequency
{
    private readonly Random _random = new();

    private int _percentage = 100;
    public int Percentage
    {
        get => _percentage;
        set => _percentage = Math.Clamp(value, 0, 100);
    }
    
    public bool ShouldHappen(out int slotNumber)
    {
        slotNumber = -1;
        return _random.Next(1, 101) <= _percentage;
    }

    public IMemoryReader? MemoryReader { get; set; }
    public int[] UsedSlotNumbers()
    {
        return new int[1] { -1 };
    }
    
    public void OnStageReset() { }
}