using System.Windows;
using System.Windows.Controls;

namespace GGXrdReversalTool.Controls
{
    public partial class SlotSelectionControl : NotifiedUserControl
    {
        public SlotSelectionControl()
        {
            InitializeComponent();
        }
        
        public string GroupName
        {
            get => (string)GetValue(GroupNameProperty);
            set => SetValue(GroupNameProperty, value);
        }
        public static readonly DependencyProperty GroupNameProperty =
            DependencyProperty.Register(nameof(GroupName), typeof(string), typeof(SlotSelectionControl),
                new PropertyMetadata(string.Empty));
        
        private bool[] _isChecked = new bool[3] { true, false, false };
        public bool[] IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked == value) return;
                _isChecked = value;
                OnPropertyChanged();
            }
        }
        
        private void OnRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            RadioButton radioBtn = (RadioButton)sender;
            SlotNumber = int.Parse((string)radioBtn.Tag);
        }
        
        public int SlotNumber
        {
            get => (int)GetValue(SlotNumberProperty);
            set => SetValue(SlotNumberProperty, value);
        }
        public static readonly DependencyProperty SlotNumberProperty =
            DependencyProperty.Register(nameof(SlotNumber), typeof(int), typeof(SlotSelectionControl),
                new PropertyMetadata(1));
        
    }
}
