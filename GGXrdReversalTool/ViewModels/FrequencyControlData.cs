using System;
using System.Collections.ObjectModel;

namespace GGXrdReversalTool.ViewModels
{
    public class FrequencyControlData : ViewModelBase
    {
        
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
                if (_playRandomSlot && _playSlotsInOrder) {
                    _playSlotsInOrder = false;
                    OnPropertyChanged("PlaySlotsInOrder");
                }
            }
        }
        
        private bool _playSlotsInOrder = false;
        public bool PlaySlotsInOrder
        {
            get => _playSlotsInOrder;
            set
            {
                if (value == _playSlotsInOrder) return;
                _playSlotsInOrder = value;
                OnPropertyChanged();
                if (_playSlotsInOrder && _playRandomSlot) {
                    _playRandomSlot = false;
                    OnPropertyChanged("PlayRandomSlot");
                }
            }
        }
        
        private bool _resetOnStageReset = false;
        public bool ResetOnStageReset
        {
            get => _resetOnStageReset;
            set
            {
                if (value == _resetOnStageReset) return;
                _resetOnStageReset = value;
                OnPropertyChanged();
            }
        }
        
        public delegate void SlotChangedEventHandler(object? sender, SlotChangedEventArgs e);
        public event SlotChangedEventHandler? SlotChanged;
        
        public void SlotPropertyChanged(SlotsControlSlotData Slot, int Index, SlotChangedAction Action)
        {
            SlotChanged?.Invoke(this, new SlotChangedEventArgs(Action, Index, Slot));
        }
        
    }
    
    public enum SlotChangedAction
    {
        Use,
        Percentage,
        Everything
    }
    
    public class SlotChangedEventArgs
    {
        public SlotChangedAction Action;
        public int Index;
        public SlotsControlSlotData Slot;
        public SlotChangedEventArgs(SlotChangedAction Action, int Index, SlotsControlSlotData Slot)
        {
            this.Action = Action;
            this.Index = Index;
            this.Slot = Slot;
        }
    }
}
