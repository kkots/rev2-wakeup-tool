using GGXrdReversalTool.Library.Extensions;
using GGXrdReversalTool.Library.Memory;

namespace GGXrdReversalTool.Library.Scenarios.Frequency.Implementations;

public class RandomSlotFrequencySlot
{
    public int Percentage = 100;
    public int Index = 0;
}

public class RandomSlotFrequency : IScenarioFrequency
{
    private readonly Random _random = new();
    
    private RandomSlotFrequencySlot[] _slots = new RandomSlotFrequencySlot[0];
    public RandomSlotFrequencySlot[] Slots
    {
        get => _slots;
        set => _slots = value;
    }


    public bool ShouldHappen(out int slotNumber)
    {
        slotNumber = -1;
        
        int slotCount = _slots.Length;
        if (slotCount == 0) return false;
        
        int summaryChance = _slots.Sum(slot => slot.Percentage);
        if (summaryChance < 100)
        {
            summaryChance = 100;
        }
        
        int randomValue = _random.Next(1, summaryChance + 1);
        
        int nextSlotStartingPercentage = 1;
        foreach (RandomSlotFrequencySlot slot in _slots)
        {
            
            int currentSlotLowBound = nextSlotStartingPercentage;
            int currentSlotHighBound = nextSlotStartingPercentage + slot.Percentage;
            
            if (randomValue >= currentSlotLowBound && randomValue < currentSlotHighBound)
            {
                slotNumber = slot.Index + 1;
                return true;
            }
            
            nextSlotStartingPercentage = currentSlotHighBound;
        }
        
        return false;
        
    }

    public IMemoryReader? MemoryReader { get; set; }
    public int[] UsedSlotNumbers()
    {
        return _slots.Select(slot => slot.Index + 1).ToArray();
    }
    
    public void OnStageReset() { }
}