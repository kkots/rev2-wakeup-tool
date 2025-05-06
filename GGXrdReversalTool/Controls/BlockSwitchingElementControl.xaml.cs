using GGXrdReversalTool.Library.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using GGXrdReversalTool.ViewModels;
using GGXrdReversalTool.Commands;
using System.Windows.Input;

namespace GGXrdReversalTool.Controls {
    public partial class BlockSwitchingElementControl : NotifiedUserControl
    {
        public BlockSwitchingElementControl()
        {
            InitializeComponent();
        }
        
        public BlockSwitchingElement? BlockSwitchingElementData
        {
            get => (BlockSwitchingElement?)GetValue(BlockSwitchingElementDataProperty);
            set => SetValue(BlockSwitchingElementDataProperty, value);
        }
        public static readonly DependencyProperty BlockSwitchingElementDataProperty =
            DependencyProperty.Register(nameof(BlockSwitchingElementData), typeof(BlockSwitchingElement), typeof(BlockSwitchingElementControl),
                new PropertyMetadata(default(BlockSwitchingElement), OnBlockSwitchingElementDataPropertyChanged));
        
        public static void OnBlockSwitchingElementDataPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BlockSwitchingElementControl control = (BlockSwitchingElementControl)d;
            control.OnBlockSwitchingElementDataPropertyChanged(e);
        }
        
        private void OnBlockSwitchingElementDataPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            
            DataForTemplateSelector = new BlockSwitchingElementTemplateSelectorData();
            DataForTemplateSelector.Element = BlockSwitchingElementData;
            DataForTemplateSelector.Interactive = Interactive;  // interactive is false here even if set to true in XAML
            
            if (BlockSwitchingElementData == null)
            {
                DataForTemplateSelector.ImagesCount = 0;
                DataForTemplateSelector.Tooltip = string.Empty;
                OnPropertyChanged("DataForTemplateSelector");
                return;
            }
            BlockSwitchingElement elem = BlockSwitchingElementData;
            
            string[] builder = new string[5];
            int partCount = 0;
            
            if (elem.Stance != BlockInputStanceType.Unchanged && elem.Block != BlockInputBlockType.Unchanged)
                DataForTemplateSelector.AddImage((Image)Resources[$"Block{Enum.GetName(elem.Block)}{Enum.GetName(elem.Stance)}Icon"]);
            else if (elem.Stance != BlockInputStanceType.Unchanged)
                if (elem.Stance == BlockInputStanceType.Pin || elem.Stance == BlockInputStanceType.Random)
                    DataForTemplateSelector.AddImage((Image)Resources[$"Block{Enum.GetName(elem.Stance)}StanceIcon"]);
                else
                    DataForTemplateSelector.AddImage((Image)Resources[$"Block{Enum.GetName(elem.Stance)}Icon"]);
            else if (elem.Block != BlockInputBlockType.Unchanged)
                if (elem.Block == BlockInputBlockType.Pin || elem.Block == BlockInputBlockType.Random)
                    DataForTemplateSelector.AddImage((Image)Resources[$"Block{Enum.GetName(elem.Block)}BlockIcon"]);
                else
                    DataForTemplateSelector.AddImage((Image)Resources[$"Block{Enum.GetName(elem.Block)}Icon"]);
            
            switch (elem.Stance)
            {
                case BlockInputStanceType.Standing: builder[partCount++] = "Change stance to standing in preparation for this hit."; break;
                case BlockInputStanceType.Crouching: builder[partCount++] = "Change stance to crouching in preparation for this hit."; break;
                case BlockInputStanceType.Jumping: builder[partCount++] = "Change stance to jumping in preparation for this hit."; break;
                case BlockInputStanceType.Opposite: builder[partCount++] = "Change stance to the opposite of what it currently is (including the result of block switching) in preparation for this hit."; break;
                case BlockInputStanceType.Pin: builder[partCount++] = "Change stance to what it currently is (including the result of block switching) in preparation for this hit."; break;
                case BlockInputStanceType.Random: builder[partCount++] = "In preparation for this hit, change stance a single time (so only for this hit) to a random choice between standing and crouching."; break;
            }
            
            switch (elem.Block)
            {
                case BlockInputBlockType.Normal: builder[partCount++] = "Normal block."; break;
                case BlockInputBlockType.IB: builder[partCount++] = "Instant block."; break;
                case BlockInputBlockType.FD: builder[partCount++] = "Faultless Defense."; break;
                case BlockInputBlockType.Random: builder[partCount++] = "In preparation for this hit, make a single-time random choice between Normal block, IB or FD."; break;
                case BlockInputBlockType.GamesOwnImplementationOfRandom: builder[partCount++] = "Set block type to game's built-in 'Random' option. Be wary, the pushback from such block might not match the actual type of block that was used (it's bugged)."; break;
                case BlockInputBlockType.Pin: builder[partCount++] = "In preparation for this hit, change block type to the last actually used type of block, removing randomness from it."; break;
            }
            
            switch (elem.BlockSwitching)
            {
                case BlockInputSwitchingType.Off: builder[partCount++] = "Change 'Block Switching' to 'Disabled' in preparation for this hit."; break;
                case BlockInputSwitchingType.On: builder[partCount++] = "Change 'Block Switching' to 'Enabled' in preparation for this hit."; break;
                case BlockInputSwitchingType.OnSecond: builder[partCount++] = "Change 'Block Switching' to 'Switch on the 2nd' in preparation for this hit."; break;
            }
            
            switch (elem.BlockSettings)
            {
                case BlockInputBlockSettingsType.NotBlock: builder[partCount++] = "Change 'Block Settings' to 'None' in preparation for this hit."; break;
                case BlockInputBlockSettingsType.Everything: builder[partCount++] = "Change 'Block Settings' to 'Everything' in preparation for this hit."; break;
                case BlockInputBlockSettingsType.AfterFirst: builder[partCount++] = "Change 'Block Settings' to 'After First Hit' in preparation for this hit."; break;
                case BlockInputBlockSettingsType.FirstOnly: builder[partCount++] = "Change 'Block Settings' to 'First Hit Only' in preparation for this hit."; break;
                case BlockInputBlockSettingsType.Random: builder[partCount++] = "Change 'Block Settings' to 'Random' in preparation for this hit."; break;
                case BlockInputBlockSettingsType.Pin: builder[partCount++] = "In preparation for this hit, change 'Block Settings' to the last choice made by the 'Random' setting, removing randomness from it."; break;
            }
            
            if (elem.IsReversal)
            {
                builder[partCount++] = "On this hit play a reversal, if the specified Event on the 'Scenario' tab is 'Blocked a certain hit'"
                    + " and the 'Only on hits from Block Switching tab marked with !' checkbox is checked.";
            }
            
            if (partCount > 0) DataForTemplateSelector.Tooltip = string.Join("\n", builder.Take(partCount));
            else DataForTemplateSelector.Tooltip = "Don't change any settings on this hit.";
            
            if (elem.BlockSettings != BlockInputBlockSettingsType.Unchanged)
                if (elem.BlockSettings == BlockInputBlockSettingsType.Pin || elem.BlockSettings == BlockInputBlockSettingsType.Random)
                    DataForTemplateSelector.AddImage((Image)Resources[$"Block{Enum.GetName(elem.BlockSettings)}SettingsIcon"]);
                else
                    DataForTemplateSelector.AddImage((Image)Resources[$"Block{Enum.GetName(elem.BlockSettings)}Icon"]);
                    
            if (elem.BlockSwitching != BlockInputSwitchingType.Unchanged)
                DataForTemplateSelector.AddImage((Image)Resources[$"Block{Enum.GetName(elem.BlockSwitching)}Icon"]);
            
            if (elem.HasMultiplier) DataForTemplateSelector.Multiplier = $"*{elem.Multiplier}";
            else DataForTemplateSelector.Multiplier = string.Empty;
            
            OnPropertyChanged("DataForTemplateSelector");
        }
        
        public BlockSwitchingElementTemplateSelectorData DataForTemplateSelector { get; set; } = null!;
        
        public RelayCommand<BlockSwitchingElement>? ElementClickCommand
        {
            get => (RelayCommand<BlockSwitchingElement>?)GetValue(ElementClickCommandProperty);
            set => SetValue(ElementClickCommandProperty, value);
        }
        public static readonly DependencyProperty ElementClickCommandProperty =
            DependencyProperty.Register(nameof(ElementClickCommand), typeof(RelayCommand<BlockSwitchingElement>), typeof(BlockSwitchingElementControl),
                new PropertyMetadata(null));
        
        public RelayCommand<BlockSwitchingElement>? ElementDoubleClickCommand
        {
            get => (RelayCommand<BlockSwitchingElement>?)GetValue(ElementDoubleClickCommandProperty);
            set => SetValue(ElementDoubleClickCommandProperty, value);
        }
        public static readonly DependencyProperty ElementDoubleClickCommandProperty =
            DependencyProperty.Register(nameof(ElementDoubleClickCommand), typeof(RelayCommand<BlockSwitchingElement>), typeof(BlockSwitchingElementControl),
                new PropertyMetadata(null));
        
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left
                && e.ChangedButton != MouseButton.Right) return;
            
            if (e.ClickCount == 2 && e.ChangedButton == MouseButton.Left)
            {
                ElementDoubleClickCommand?.Execute(BlockSwitchingElementData);
                return;
            }
            ElementClickCommand?.Execute(BlockSwitchingElementData);
        }

        public bool Interactive
        {
            get => (bool)GetValue(InteractiveProperty);
            set => SetValue(InteractiveProperty, value);
        }
        public static readonly DependencyProperty InteractiveProperty =
            DependencyProperty.Register(nameof(Interactive), typeof(bool), typeof(BlockSwitchingElementControl),
                new PropertyMetadata(false, InteractivePropertyChanged));
        
        public static void InteractivePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BlockSwitchingElementControl control = (BlockSwitchingElementControl)d;
            control.DataForTemplateSelector.Interactive = control.Interactive;
        }

        public RelayCommand<BlockSwitchingElement>? ElementEditCommand
        {
            get => (RelayCommand<BlockSwitchingElement>?)GetValue(ElementEditCommandProperty);
            set => SetValue(ElementEditCommandProperty, value);
        }
        public static readonly DependencyProperty ElementEditCommandProperty =
            DependencyProperty.Register(nameof(ElementEditCommand), typeof(RelayCommand<BlockSwitchingElement>), typeof(BlockSwitchingElementControl),
                new PropertyMetadata(null));
        
        public RelayCommand<BlockSwitchingElement>? ElementDeleteCommand
        {
            get => (RelayCommand<BlockSwitchingElement>?)GetValue(ElementDeleteCommandProperty);
            set => SetValue(ElementDeleteCommandProperty, value);
        }
        public static readonly DependencyProperty ElementDeleteCommandProperty =
            DependencyProperty.Register(nameof(ElementDeleteCommand), typeof(RelayCommand<BlockSwitchingElement>), typeof(BlockSwitchingElementControl),
                new PropertyMetadata(null));
        
        public RelayCommand<BlockSwitchingElement>? ElementInsertNewAfterCommand
        {
            get => (RelayCommand<BlockSwitchingElement>?)GetValue(ElementInsertNewAfterCommandProperty);
            set => SetValue(ElementInsertNewAfterCommandProperty, value);
        }
        public static readonly DependencyProperty ElementInsertNewAfterCommandProperty =
            DependencyProperty.Register(nameof(ElementInsertNewAfterCommand), typeof(RelayCommand<BlockSwitchingElement>), typeof(BlockSwitchingElementControl),
                new PropertyMetadata(null));
        
        public RelayCommand<BlockSwitchingElement>? ElementInsertNewBeforeCommand
        {
            get => (RelayCommand<BlockSwitchingElement>?)GetValue(ElementInsertNewBeforeCommandProperty);
            set => SetValue(ElementInsertNewBeforeCommandProperty, value);
        }
        public static readonly DependencyProperty ElementInsertNewBeforeCommandProperty =
            DependencyProperty.Register(nameof(ElementInsertNewBeforeCommand), typeof(RelayCommand<BlockSwitchingElement>), typeof(BlockSwitchingElementControl),
                new PropertyMetadata(null));
        
        public RelayCommand<BlockSwitchingElement>? ElementMoveLeftCommand
        {
            get => (RelayCommand<BlockSwitchingElement>?)GetValue(ElementMoveLeftCommandProperty);
            set => SetValue(ElementMoveLeftCommandProperty, value);
        }
        public static readonly DependencyProperty ElementMoveLeftCommandProperty =
            DependencyProperty.Register(nameof(ElementMoveLeftCommand), typeof(RelayCommand<BlockSwitchingElement>), typeof(BlockSwitchingElementControl),
                new PropertyMetadata(null));
        
        public RelayCommand<BlockSwitchingElement>? ElementMoveRightCommand
        {
            get => (RelayCommand<BlockSwitchingElement>?)GetValue(ElementMoveRightCommandProperty);
            set => SetValue(ElementMoveRightCommandProperty, value);
        }
        public static readonly DependencyProperty ElementMoveRightCommandProperty =
            DependencyProperty.Register(nameof(ElementMoveRightCommand), typeof(RelayCommand<BlockSwitchingElement>), typeof(BlockSwitchingElementControl),
                new PropertyMetadata(null));
        
        private void OnEditClick(object sender, RoutedEventArgs e)
        {
            ElementEditCommand?.Execute(BlockSwitchingElementData);
        }

        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            ElementDeleteCommand?.Execute(BlockSwitchingElementData);
        }

        private void OnInsertNewAfterClick(object sender, RoutedEventArgs e)
        {
            ElementInsertNewAfterCommand?.Execute(BlockSwitchingElementData);
        }

        private void OnInsertNewBeforeClick(object sender, RoutedEventArgs e)
        {
            ElementInsertNewBeforeCommand?.Execute(BlockSwitchingElementData);
        }

        private void OnMoveLeftClick(object sender, RoutedEventArgs e)
        {
            ElementMoveLeftCommand?.Execute(BlockSwitchingElementData);
        }

        private void OnMoveRightClick(object sender, RoutedEventArgs e)
        {
            ElementMoveRightCommand?.Execute(BlockSwitchingElementData);
        }
    }

    public class BlockSwitchingElementTemplateSelectorData : ViewModelBase
    {
        private BlockSwitchingElement? _element = null;
        public BlockSwitchingElement? Element
        {
            get => _element;
            set
            {
                if (_element == value) return;
                _element = value;
                OnPropertyChanged();
            }
        }
        
        private int _imagesCount = 0;
        public int ImagesCount
        {
            get => _imagesCount;
            set
            {
                if (_imagesCount == value) return;
                _imagesCount = value;
                OnPropertyChanged();
            }
        }
        
        private Image?[] _images = { null, null, null };
        public Image?[] Images
        {
            get => _images;
            set
            {
                if (_images == value) return;
                _images = Images;
                OnPropertyChanged();
            }
        }
        
        private string _multiplier = string.Empty;
        public string Multiplier
        {
            get => _multiplier;
            set
            {
                if (_multiplier.Equals(value)) return;
                _multiplier = value;
                OnPropertyChanged();
            }
        }
        
        private string _tooltip = "Don't change any settings on this hit.";
        public string Tooltip
        {
            get => _tooltip;
            set
            {
                if (_tooltip.Equals(value)) return;
                _tooltip = value;
                OnPropertyChanged();
            }
        }
        
        private bool _interactive = false;
        public bool Interactive
        {
            get => _interactive;
            set
            {
                if (_interactive == value) return;
                _interactive = value;
                OnPropertyChanged();
            }
        }
        
        public void AddImage(Image img)
        {
            Images[ImagesCount++] = img;  // will throw index out of range exception for me, no need to write own throw
        }
    }
    
    public class BlockSwitchingElementTemplateSelector : DataTemplateSelector
    {
        public DataTemplate JustExclamationMark { get; set; } = null!;
        public DataTemplate JustMultiplication { get; set; } = null!;
        public DataTemplate JustExclamationMarkAndMultiplication { get; set; } = null!;
        public DataTemplate WithoutExclamationMarkOrMultiplication1 { get; set; } = null!;
        public DataTemplate WithoutExclamationMarkOrMultiplication2 { get; set; } = null!;
        public DataTemplate WithoutExclamationMarkOrMultiplication3 { get; set; } = null!;
        public DataTemplate WithoutExclamationMarkButWithMultiplication1 { get; set; } = null!;
        public DataTemplate WithoutExclamationMarkButWithMultiplication2 { get; set; } = null!;
        public DataTemplate WithoutExclamationMarkButWithMultiplication3 { get; set; } = null!;
        public DataTemplate WithExclamationMarkButWithoutMultiplication1 { get; set; } = null!;
        public DataTemplate WithExclamationMarkButWithoutMultiplication2 { get; set; } = null!;
        public DataTemplate WithExclamationMarkButWithoutMultiplication3 { get; set; } = null!;
        public DataTemplate WithExclamationMarkAndMultiplication1 { get; set; } = null!;
        public DataTemplate WithExclamationMarkAndMultiplication2 { get; set; } = null!;
        public DataTemplate WithExclamationMarkAndMultiplication3 { get; set; } = null!;
        public DataTemplate EmptyTemplate { get; set; } = null!;
    
        public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
        {
            if (item is not BlockSwitchingElementTemplateSelectorData data || data.Element == null)
                return EmptyTemplate;
            
            if (data.ImagesCount == 0)
                if (!data.Element.HasMultiplier)
                    return data.Element.IsReversal ? JustExclamationMark : EmptyTemplate;
                else
                    return data.Element.IsReversal ? JustExclamationMarkAndMultiplication : JustMultiplication;
            else if (!data.Element.HasMultiplier)
                if (data.Element.IsReversal)
                    return data.ImagesCount switch
                    {
                        1 => WithExclamationMarkButWithoutMultiplication1,
                        2 => WithExclamationMarkButWithoutMultiplication2,
                        _ => WithExclamationMarkButWithoutMultiplication3
                    };
                else
                    return data.ImagesCount switch
                    {
                        1 => WithoutExclamationMarkOrMultiplication1,
                        2 => WithoutExclamationMarkOrMultiplication2,
                        _ => WithoutExclamationMarkOrMultiplication3
                    };
            else if (data.Element.IsReversal)
                return data.ImagesCount switch
                {
                    1 => WithExclamationMarkAndMultiplication1,
                    2 => WithExclamationMarkAndMultiplication2,
                    _ => WithExclamationMarkAndMultiplication3
                };
            else
                return data.ImagesCount switch
                {
                    1 => WithoutExclamationMarkButWithMultiplication1,
                    2 => WithoutExclamationMarkButWithMultiplication2,
                    _ => WithoutExclamationMarkButWithMultiplication3
                };
        }
    } 
}
