using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace GGXrdReversalTool.Controls
{
    public partial class NumericTextInput : NotifiedUserControl
    {
        public NumericTextInput()
        {
            InitializeComponent();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox box = (TextBox)sender;
            int intValue;
            if (!int.TryParse(box.Text, out intValue))
            {
                int textCaretPos = box.SelectionStart;
                int shift = 0;
                StringBuilder builder = new StringBuilder();
                builder.EnsureCapacity(box.Text.Length);
                for (int i = 0; i < box.Text.Length; ++i)
                {
                    char c = box.Text[i];
                    if (c >= '0' && c <= '9')
                    {
                        builder.Append(c);
                        continue;
                    }
                    
                    if (textCaretPos > i) ++shift;
                }
                box.Text = builder.ToString();
                box.SelectionStart = textCaretPos - shift;
            }
        }
        
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(NumericTextInput),
                new PropertyMetadata(string.Empty));
        
    }
}
