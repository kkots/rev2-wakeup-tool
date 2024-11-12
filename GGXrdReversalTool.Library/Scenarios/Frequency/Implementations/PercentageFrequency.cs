using GGXrdReversalTool.Library.Memory;

namespace GGXrdReversalTool.Library.Scenarios.Frequency.Implementations;

public class PercentageFrequency : IScenarioFrequency
{
    private readonly Random _random = new();
    private int _percentage = 100;
    private bool _playRandomSlot = false;
    private bool _playSlotsInOrder = false;
    private bool _resetOnStageReset = false;
    private int _playSlotsInOrderNext = 1;
    private bool _useSlot1 = false;
    private bool _useSlot2 = false;
    private bool _useSlot3 = false;
    private int _slot1Percentage = 100;
    private int _slot2Percentage = 100;
    private int _slot3Percentage = 100;

    public int Percentage
    {
        get => _percentage;
        set => _percentage = Math.Clamp(value, 0, 100);
    }
    
    public int Slot1Percentage
    {
        get => _slot1Percentage;
        set => _slot1Percentage = Math.Clamp(value, 0, 100);
    }
    
    public int Slot2Percentage
    {
        get => _slot2Percentage;
        set => _slot2Percentage = Math.Clamp(value, 0, 100);
    }
    
    public int Slot3Percentage
    {
        get => _slot3Percentage;
        set => _slot3Percentage = Math.Clamp(value, 0, 100);
    }
    
    public bool PlayRandomSlot
    {
        get => _playRandomSlot;
        set => _playRandomSlot = value;
    }
    
    public bool PlaySlotsInOrder
    {
        get => _playSlotsInOrder;
        set => _playSlotsInOrder = value;
    }
    
    public bool ResetOnStageReset
    {
        get => _resetOnStageReset;
        set => _resetOnStageReset = value;
    }
    
    public bool UseSlot1
    {
        get => _useSlot1;
        set => _useSlot1 = value;
    }
    
    public bool UseSlot2
    {
        get => _useSlot2;
        set => _useSlot2 = value;
    }
    
    public bool UseSlot3
    {
        get => _useSlot3;
        set => _useSlot3 = value;
    }


    public bool ShouldHappen(out int slotNumber)
    {
        slotNumber = -1;
        bool mainChanceOk = _random.Next(1, 101) <= _percentage;
        if (!_playRandomSlot && !mainChanceOk)
        {
            return false;
        }
        if (!_playRandomSlot && !_playSlotsInOrder)
        {
            return true;
        }
        
        int[] slotNumbers = new int[3];
        int[] slotChances = new int[3];
        int slotCount = 0;
        int summaryChance = 0;
        
        if (_useSlot1)
        {
            summaryChance += _slot1Percentage;
            slotNumbers[slotCount] = 1;
            slotChances[slotCount] = _slot1Percentage;
            ++slotCount;
        }
        
        if (_useSlot2)
        {
            summaryChance += _slot2Percentage;
            slotNumbers[slotCount] = 2;
            slotChances[slotCount] = _slot2Percentage;
            ++slotCount;
        }
        
        if (_useSlot3)
        {
            summaryChance += _slot3Percentage;
            slotNumbers[slotCount] = 3;
            slotChances[slotCount] = _slot3Percentage;
            ++slotCount;
        }
        
        if (_playSlotsInOrder)
        {
            for (int i = 0; i < slotCount; ++i)
            {
                if (slotNumbers[i] == _playSlotsInOrderNext)
                {
                    slotNumber = _playSlotsInOrderNext;
                    if (i < slotCount - 1)
                    {
                        _playSlotsInOrderNext = slotNumbers[i + 1];
                    }
                    else
                    {
                        _playSlotsInOrderNext = slotNumbers[0];
                    }
                    return true;
                }
            }
            if (slotCount == 0)
            {
                return false;
            }
            slotNumber = slotNumbers[0];
            if (slotCount == 1)
            {
                _playSlotsInOrderNext = slotNumber;
            }
            else
            {
                _playSlotsInOrderNext = slotNumbers[1];
            }
            return true;
        }
        
        if (summaryChance < 100)
        {
            summaryChance = 100;
        }
        
        int randomValue = _random.Next(1, summaryChance + 1);
        
        int nextSlotStartingPercentage = 1;
        for (int i = 0; i < slotCount; ++i)
        {
            int currentSlotNumber = slotNumbers[i];
            int currentSlotChance = slotChances[i];
            
            int currentSlotLowBound = nextSlotStartingPercentage;
            int currentSlotHighBound = nextSlotStartingPercentage + currentSlotChance;
            
            if (randomValue >= currentSlotLowBound && randomValue < currentSlotHighBound)
            {
                slotNumber = currentSlotNumber;
                return true;
            }
            
            nextSlotStartingPercentage = currentSlotHighBound;
        }
        
        return false;
        
    }

    public IMemoryReader? MemoryReader { get; set; }
    public int[] UsedSlotNumbers()
    {
        int[] array;
        if (!_playRandomSlot && !_playSlotsInOrder) {
            array = new int[1];
            array[0] = -1;
            return array;
        }
        
        int count = 0;
        if (_useSlot1) ++count;
        if (_useSlot2) ++count;
        if (_useSlot3) ++count;
        
        array = new int[count];
        count = 0;
        if (_useSlot1) array[count++] = 1;
        if (_useSlot2) array[count++] = 2;
        if (_useSlot3) array[count++] = 3;
        return array;
    }
    
    public void onStageReset()
    {
        if (_resetOnStageReset)
        {
            _playSlotsInOrderNext = 1;
        }
    }
}