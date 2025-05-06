using GGXrdReversalTool.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GGXrdReversalTool.Controls
{
    public partial class SlotSelectionDialog : Window, INotifyPropertyChanged
    {
        public SlotSelectionDialog()
        {
            InitializeComponent();
        }
        
        private bool _isImport = false;
        public bool IsImport {
            get => _isImport;
            set
            {
                if (_isImport == value) return;
                _isImport = value;
                EmitPropertyChanged("UserPromptMessage");
            }
        }
        
        public string UserPromptMessage
        {
            get => $"Please select an in-game slot to {(_isImport ? "import from" : "export to")}.";
        }
        
        public int SlotNumber { get; set; } = 1;
        
        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        private void OnOKClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;
        private void EmitPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
