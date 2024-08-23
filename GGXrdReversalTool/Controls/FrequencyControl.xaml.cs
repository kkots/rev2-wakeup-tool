using System;
using System.Windows;
using GGXrdReversalTool.Library.Scenarios.Frequency;
using GGXrdReversalTool.Library.Scenarios.Frequency.Implementations;

namespace GGXrdReversalTool.Controls;

public sealed partial class FrequencyControl : NotifiedUserControl
{
    public FrequencyControl()
    {
        InitializeComponent();
    }

    private int _percentage = 100;
    public int Percentage
    {
        get => _percentage;
        set
        {
            var coercedValue = Math.Clamp(value, 0, 100);
            if (coercedValue == _percentage) return;
            _percentage = coercedValue;
            OnPropertyChanged();
            CreateScenario();
        }
    }

    private bool _playRandomSlot = false;
    public bool PlayRandomSlot
    {
        get => _playRandomSlot;
        set
        {
            if (value == _playRandomSlot) return;
            _playRandomSlot = value;
            OnPropertyChanged();
            CreateScenario();
        }
    }

    private int _slot1Percentage = 100;
    public int Slot1Percentage
    {
        get => _slot1Percentage;
        set
        {
            var coercedValue = Math.Clamp(value, 0, 100);
            if (coercedValue == _slot1Percentage) return;
            _slot1Percentage = coercedValue;
            SetLastChangedSlider(1);
            LimitOtherPercentages(1);
            OnPropertyChanged();
            CreateScenario();
        }
    }

    private int _slot2Percentage = 100;
    public int Slot2Percentage
    {
        get => _slot2Percentage;
        set
        {
            var coercedValue = Math.Clamp(value, 0, 100);
            if (coercedValue == _slot2Percentage) return;
            _slot2Percentage = coercedValue;
            SetLastChangedSlider(2);
            LimitOtherPercentages(2);
            OnPropertyChanged();
            CreateScenario();
        }
    }

    private int _slot3Percentage = 100;
    public int Slot3Percentage
    {
        get => _slot3Percentage;
        set
        {
            var coercedValue = Math.Clamp(value, 0, 100);
            if (coercedValue == _slot3Percentage) return;
            _slot3Percentage = coercedValue;
            SetLastChangedSlider(3);
            LimitOtherPercentages(3);
            OnPropertyChanged();
            CreateScenario();
        }
    }

    private bool _useSlot1 = false;
    public bool UseSlot1
    {
        get => _useSlot1;
        set
        {
            if (value == _useSlot1) return;
            _useSlot1 = value;
            ResetLastChangedSlider();
            if (value)
            {
                LimitThisPercentage(1);
            }
            OnPropertyChanged();
            CreateScenario();
        }
    }

    private bool _useSlot2 = false;
    public bool UseSlot2
    {
        get => _useSlot2;
        set
        {
            if (value == _useSlot2) return;
            _useSlot2 = value;
            ResetLastChangedSlider();
            if (value)
            {
                LimitThisPercentage(2);
            }
            OnPropertyChanged();
            CreateScenario();
        }
    }

    private bool _useSlot3 = false;
    public bool UseSlot3
    {
        get => _useSlot3;
        set
        {
            if (value == _useSlot3) return;
            _useSlot3 = value;
            ResetLastChangedSlider();
            if (value)
            {
                LimitThisPercentage(3);
            }
            OnPropertyChanged();
            CreateScenario();
        }
    }

    private int[] _otherSlidersStartingValues = new int[2];
    private int _lastChangedSlider = -1;

    public IScenarioFrequency? ScenarioFrequency
    {
        get => (IScenarioFrequency)GetValue(ScenarioFrequencyProperty);
        set => SetValue(ScenarioFrequencyProperty, value);
    }

    public static readonly DependencyProperty ScenarioFrequencyProperty =
        DependencyProperty.Register(nameof(ScenarioFrequency), typeof(IScenarioFrequency), typeof(FrequencyControl),
            new PropertyMetadata(new PercentageFrequency { Percentage = 100 }));

    private void CreateScenario()
    {
        ScenarioFrequency = new PercentageFrequency()
        {
            Percentage = Percentage,
            PlayRandomSlot = PlayRandomSlot,
            UseSlot1 = UseSlot1,
            UseSlot2 = UseSlot2,
            UseSlot3 = UseSlot3,
            Slot1Percentage = Slot1Percentage,
            Slot2Percentage = Slot2Percentage,
            Slot3Percentage = Slot3Percentage
        };
    }
    
    private int SumPercentage()
    {
        int summary = 0;
        if (_useSlot1)
        {
            summary += _slot1Percentage;
        }
        if (_useSlot2)
        {
            summary += _slot2Percentage;
        }
        if (_useSlot3)
        {
            summary += _slot3Percentage;
        }
        return summary;
    }
    
    private int TotalUsedSlots()
    {
        int count = 0;
        if (_useSlot1)
        {
            ++count;
        }
        if (_useSlot2)
        {
            ++count;
        }
        if (_useSlot3)
        {
            ++count;
        }
        return count;
    }
    
    private int GetPercentage(int slotNumber)
    {
        switch (slotNumber)
        {
            case 1:
                return _slot1Percentage;
            case 2:
                return _slot2Percentage;
            case 3:
                return _slot3Percentage;
        }
        return 0;
    }
    
    private void SetPercentage(int slotNumber, int percentage)
    {
        switch (slotNumber)
        {
            case 1:
                _slot1Percentage = percentage;
                break;
            case 2:
                _slot2Percentage = percentage;
                break;
            case 3:
                _slot3Percentage = percentage;
                break;
        }
    }
    
    private bool GetUseSlot(int slotNumber)
    {
        switch (slotNumber)
        {
            case 1:
                return _useSlot1;
            case 2:
                return _useSlot2;
            case 3:
                return _useSlot3;
        }
        return false;
    }
    
    private void NotifyPercentageChanged(int slotNumber)
    {
        switch (slotNumber)
        {
            case 1:
                OnPropertyChanged("Slot1Percentage");
                break;
            case 2:
                OnPropertyChanged("Slot2Percentage");
                break;
            case 3:
                OnPropertyChanged("Slot3Percentage");
                break;
        }
    }
    
    private void LimitOtherPercentages(int initiatorNumber)
    {
        int summary = SumPercentage();
        int count = TotalUsedSlots();
        if (count <= 1 || summary <= 100) return;
        int diff = summary - 100;
        if (count == 2)
        {
            for (int i = 1; i <= 3; ++i)
            {
                if (GetUseSlot(i) && initiatorNumber != i)
                {
                    SetPercentage(i, Math.Max(0, GetPercentage(i) - diff));
                    NotifyPercentageChanged(i);
                    break;
                }
            }
            return;
        }
        
        int[] theOtherNumbers = new int[count - 1];
        float[] otherRatios = new float[count - 1];
        count = 0;
        int theOtherSum = 0;
        for (int i = 1; i <= 3; ++i)
        {
            if (GetUseSlot(i) && initiatorNumber != i)
            {
                int startingValue = _otherSlidersStartingValues[count];
                theOtherSum += startingValue;
                theOtherNumbers[count] = i;
                ++count;
            }
        }
        if (theOtherSum == 0) return;
        float diffFloat = diff;
        for (int i = 0; i < count; ++i)
        {
            int startingValue = _otherSlidersStartingValues[i];
            otherRatios[i] = (float)startingValue / (float)theOtherSum;
            int thisDiffInt = (int)(otherRatios[i] * diffFloat);
            int slotNumber = theOtherNumbers[i];
            int percentageValue = GetPercentage(slotNumber);
            if (percentageValue < thisDiffInt)
            {
                thisDiffInt = percentageValue;
            }
            SetPercentage(slotNumber, percentageValue - thisDiffInt);
            diff -= thisDiffInt;
        }
        while (diff > 0)
        {
            int nonZeroCount = 0;
            for (int i = 0; i < count; ++i)
            {
                int slotNumber = theOtherNumbers[i];
                int percentageValue = GetPercentage(slotNumber);
                if (percentageValue != 0)
                {
                    ++nonZeroCount;
                }
            }
            if (nonZeroCount == 0)
            {
                break;
            }
            if (nonZeroCount == 1)
            {
                for (int i = 0; i < count; ++i)
                {
                    int slotNumber = theOtherNumbers[i];
                    int percentageValue = GetPercentage(slotNumber);
                    if (percentageValue != 0)
                    {
                        SetPercentage(slotNumber, Math.Max(0, percentageValue - diff));
                        break;
                    }
                }
                break;
            }
            
            bool isFirstHypothetical = true;
            float lowestHypotheticalPercentageChange = 0.0F;
            int indexOfLowestHypothetical = 0;
            for (int i = 0; i < count; ++i)
            {
                int startingValue = _otherSlidersStartingValues[i];
                int slotNumber = theOtherNumbers[i];
                int percentageValue = GetPercentage(slotNumber);
                if (percentageValue == 0 || startingValue == 0) continue;
                int hypotheticalValue = percentageValue - 1;
                float hypotheticalPercentageChange = (float)(startingValue - hypotheticalValue) / (float)startingValue;
                if (isFirstHypothetical
                        || hypotheticalPercentageChange < lowestHypotheticalPercentageChange)
                {
                    lowestHypotheticalPercentageChange = hypotheticalPercentageChange;
                    indexOfLowestHypothetical = i;
                }
                isFirstHypothetical = false;
            }
            
            if (isFirstHypothetical) break;
            else
            {
                int slotNumber = theOtherNumbers[indexOfLowestHypothetical];
                SetPercentage(slotNumber, Math.Max(0, GetPercentage(slotNumber) - 1));
            }
            
            --diff;
        }
        
        for (int i = 0; i < count; ++i)
        {
            int slotNumber = theOtherNumbers[i];
            NotifyPercentageChanged(slotNumber);
        }
    }
    private void LimitThisPercentage(int initiatorNumber)
    {
        
        int summary = SumPercentage();
        int count = TotalUsedSlots();
        if (count == 1) return;
        if (summary > 100)
        {
            int diff = summary - 100;
            switch (initiatorNumber)
            {
                case 1:
                    _slot1Percentage -= diff;
                    OnPropertyChanged("Slot1Percentage");
                    break;
                case 2:
                    _slot2Percentage -= diff;
                    OnPropertyChanged("Slot2Percentage");
                    break;
                case 3:
                    _slot3Percentage -= diff;
                    OnPropertyChanged("Slot3Percentage");
                    break;
            }
        }
    }
    private void ResetLastChangedSlider()
    {
        _lastChangedSlider = -1;
    }
    private void SetLastChangedSlider(int initiatorNumber)
    {
        if (_lastChangedSlider == initiatorNumber) return;
        _lastChangedSlider = initiatorNumber;
        int[] theOtherNumbers = new int[3];
        int count = 0;
        if (_useSlot1 && initiatorNumber != 1) theOtherNumbers[count++] = 1;
        if (_useSlot2 && initiatorNumber != 2) theOtherNumbers[count++] = 2;
        if (_useSlot3 && initiatorNumber != 3) theOtherNumbers[count++] = 3;
        for (int i = 0; i < count; ++i)
        {
            int otherValue = 0;
            switch (theOtherNumbers[i])
            {
                case 1:
                    otherValue = _slot1Percentage;
                    break;
                case 2:
                    otherValue = _slot2Percentage;
                    break;
                case 3:
                    otherValue = _slot3Percentage;
                    break;
            }
            _otherSlidersStartingValues[i] = otherValue;
        }
    }
}