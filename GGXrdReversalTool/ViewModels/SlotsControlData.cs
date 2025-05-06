using GGXrdReversalTool.Controls;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace GGXrdReversalTool.ViewModels
{
    
    public class SlotsControlSlotData : ViewModelBase
    {
        
        public SlotsControlSlotData(FrequencyControlData Parent, int Index, bool IsChecked, bool Use)
        {
            _index = Index;
            _isChecked = IsChecked;
            _use = Use;
            this.Parent = Parent;
        }
        
        // For use in Slots Control
        public string Name
        {
            get => $"Slot {_index + 1}";
        }
        
        private int _index;
        public int Index
        {
            get => _index;
            set
            {
                if (_index == value) return;
                _index = value;
                OnPropertyChanged();
                OnPropertyChanged("Number");
                OnPropertyChanged("Name");
                OnPropertyChanged("TitleForUseCheckbox");
            }
        }
        
        public int Number
        {
            get => _index + 1;
            set => _index = value - 1;
        }
        
        // For use in Slots Control
        private bool _isChecked;
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked == value) return;
                _isChecked = value;
                OnPropertyChanged();
            }
        }
        
        // Action text
        private string _text = string.Empty;
        public string Text {
            get => _text;
            set
            {
                if (_text == value) return;
                _text = value;
                OnPropertyChanged();
            }
        }
        
        private bool _guaranteeChargeInput = false;
        public bool GuaranteeChargeInput
        {
            get => _guaranteeChargeInput;
            set
            {
                if (value == _guaranteeChargeInput) return;
                _guaranteeChargeInput = value;
                OnPropertyChanged();
            }
        }
        
        // The slot is used by the Frequency Control
        private bool _use = false;
        public bool Use
        {
            get => _use;
            set
            {
                if (_use == value) return;
                _use = value;
                OnPropertyChanged();
                Parent.SlotPropertyChanged(this, Index, SlotChangedAction.Use);
            }
        }
        
        // Percentage in Frequency Control for this slot
        private int _percentage = 100;
        public int Percentage
        {
            get => _percentage;
            set
            {
                var coercedValue = Math.Clamp(value, 0, 100);
                if (_percentage == coercedValue) return;
                _percentage = coercedValue;
                OnPropertyChanged();
                Parent.SlotPropertyChanged(this, Index, SlotChangedAction.Percentage);
            }
        }
        
        // "Use slot #" title for the checkbox in Frequency Control that corresponds to this slot
        public string TitleForUseCheckbox
        {
            get => $"Use slot {_index + 1}";
        }
        
        // For use in FrequencyControl algorithms
        public int StartingValue;
        
        // Needed to deliver updates to slot's data to the Frequency Control more easily
        public FrequencyControlData Parent { get; set; }
        
    }
    
    public class SlotsControlData : DependencyObject, INotifyPropertyChanged
    {
        
        private ObservableCollection<SlotsControlSlotData> _slots;
        public ObservableCollection<SlotsControlSlotData> Slots
        {
            get => _slots;
            set
            {
                if (_slots == value) return;
                _slots = value;
                OnPropertyChanged();
            }
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        private FrequencyControlData _parent;
        
        public void AddSlotAt(int Index)
        {
            foreach (SlotsControlSlotData slot in Slots)
            {
                slot.IsChecked = false;
            }
            Slots.Insert(Index, new SlotsControlSlotData(_parent, Index, true, false));
            for (int i = Index + 1; i < Slots.Count; ++i)
            {
                SlotsControlSlotData slot = Slots[i];
                slot.Index = i;
            }
            SlotNumber = Index + 1;
        }
        
        public void RemoveSlotAt(int Index)
        {
            Slots.RemoveAt(Index);
            for (int i = Index; i < Slots.Count; ++i)
            {
                SlotsControlSlotData slot = Slots[i];
                slot.Index = i;
            }
            
            int newSlotNumber = SlotNumber;
            if (newSlotNumber >= Slots.Count + 1)
            {
                --newSlotNumber;
            }
            
            Slots[newSlotNumber - 1].IsChecked = true;
            SlotNumber = newSlotNumber;
        }
        
        public SlotsControlData(FrequencyControlData Parent, bool IsFirstEvent)
        {
            _parent = Parent;
            _slots = new ObservableCollection<SlotsControlSlotData>() {
                new SlotsControlSlotData(Parent, 0, true, IsFirstEvent),
                new SlotsControlSlotData(Parent, 1, false, false),
                new SlotsControlSlotData(Parent, 2, false, false)
            };
        }
        
        public SlotsControlSlotData this[int index]
        {
            get => _slots[index];
            set
            {
                if (_slots[index] == value) return;
                _slots[index] = value;
            }
        }
        
        public int SlotNumber
        {
            get => (int)GetValue(SlotNumberProperty);
            set => SetValue(SlotNumberProperty, value);
        }
        public static readonly DependencyProperty SlotNumberProperty =
            DependencyProperty.Register(nameof(SlotNumber), typeof(int), typeof(SlotsControlData),
                new PropertyMetadata(1, OnSlotNumberPropertyChanged));
        
        public static void OnSlotNumberPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SlotsControlData control = (SlotsControlData)d;
            control.OnPropertyChanged("SlotNumber");
        }
        
        public SlotsControlSlotData CurrentSlot
        {
            get => _slots[SlotNumber - 1];
        }
        
        
    }
    
}
