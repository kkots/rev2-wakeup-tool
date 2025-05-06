using GGXrdReversalTool.Library.Extensions;
using GGXrdReversalTool.Library.Memory;

namespace GGXrdReversalTool.Library.Scenarios.Frequency.Implementations;

public class SingleSlotFrequency : IScenarioFrequency
{
    public bool ShouldHappen(out int slotNumber)
    {
        slotNumber = -1;
        return true;
    }

    public IMemoryReader? MemoryReader { get; set; }
    public int[] UsedSlotNumbers()
    {
        return new int[1] { -1 };
    }
    
    public void OnStageReset() { }
}