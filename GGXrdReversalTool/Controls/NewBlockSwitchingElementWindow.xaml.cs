using GGXrdReversalTool.Library.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace GGXrdReversalTool.Controls
{
    public partial class NewBlockSwitchingElementWindow : Window, INotifyPropertyChanged
    {
        public NewBlockSwitchingElementWindow()
        {
            InitializeComponent();
            
            object emptyIcon = Resources["EmptyIcon"];
            CurrentStanceIcon = emptyIcon;
            OnPropertyChanged("CurrentStanceIcon");
            CurrentBlockTypeIcon = emptyIcon;
            OnPropertyChanged("CurrentBlockTypeIcon");
            CurrentBlockSwitchingIcon = emptyIcon;
            OnPropertyChanged("CurrentBlockSwitchingIcon");
            CurrentBlockSettingsIcon = emptyIcon;
            OnPropertyChanged("CurrentBlockSettingsIcon");
        }
        
        public void SetInitialValues(BlockSwitchingElement data, bool updateTextRepresentation = true)
        {
            RepresentedBlockSwitching = new BlockSwitchingElement(
                data.Stance,
                data.Block,
                data.BlockSwitching,
                data.BlockSettings,
                data.IsReversal,
                data.HasMultiplier,
                data.Multiplier
            );
            OnPropertyChanged("IsReversal");
            
            _multiplier = data.HasMultiplier ? data.Multiplier : 1;
            _multiplierText = data.HasMultiplier ? $"{data.Multiplier}" : "1";
            OnPropertyChanged("MultiplierText");
            
            object? emptyIcon = Resources["EmptyIcon"];
            
            CurrentStanceIcon = data.Stance switch
            {
                BlockInputStanceType.Unchanged => emptyIcon,
                BlockInputStanceType.Pin or BlockInputStanceType.Random => Resources[$"Block{Enum.GetName(data.Stance)}StanceIcon"],
                _ => Resources[$"Block{Enum.GetName(data.Stance)}Icon"]
            };
            OnPropertyChanged("CurrentStanceIcon");
            
            CurrentBlockTypeIcon = data.Block switch
            {
                BlockInputBlockType.Unchanged => emptyIcon,
                BlockInputBlockType.Pin or BlockInputBlockType.Random => Resources[$"Block{Enum.GetName(data.Block)}BlockIcon"],
                _ => Resources[$"Block{Enum.GetName(data.Block)}Icon"]
            };
            OnPropertyChanged("CurrentBlockTypeIcon");
            
            CurrentBlockSwitchingIcon = data.BlockSwitching switch
            {
                BlockInputSwitchingType.Unchanged => emptyIcon,
                _ => Resources[$"Block{Enum.GetName(data.BlockSwitching)}Icon"]
            };
            OnPropertyChanged("CurrentBlockSwitchingIcon");
            
            CurrentBlockSettingsIcon = data.BlockSettings switch
            {
                BlockInputBlockSettingsType.Unchanged => emptyIcon,
                BlockInputBlockSettingsType.Pin or BlockInputBlockSettingsType.Random => Resources[$"Block{Enum.GetName(data.BlockSettings)}SettingsIcon"],
                _ => Resources[$"Block{Enum.GetName(data.BlockSettings)}Icon"]
            };
            OnPropertyChanged("CurrentBlockSettingsIcon");
            
            if (updateTextRepresentation) UpdateTextRepresentation();
            
        }
        
        private BlockSwitchingElement _representedBlockSwitching = new BlockSwitchingElement(
            BlockInputStanceType.Unchanged,
            BlockInputBlockType.Unchanged,
            BlockInputSwitchingType.Unchanged,
            BlockInputBlockSettingsType.Unchanged,
            false,
            false,
            1
        );
        public BlockSwitchingElement RepresentedBlockSwitching
        {
            get => _representedBlockSwitching;
            set
            {
                if (_representedBlockSwitching == value) return;
                _representedBlockSwitching = value;
                OnPropertyChanged();
            }
        }
        public object CurrentStanceIcon { get; set; }
        public object CurrentBlockTypeIcon { get; set; }
        public object CurrentBlockSwitchingIcon { get; set; }
        public object CurrentBlockSettingsIcon { get; set; }
        
        private int _multiplier = 1;
        private string _multiplierText = "1";
        public string MultiplierText
        {
            get => _multiplierText;
            set
            {
                if (_multiplierText.Equals(value)) return;
                _multiplierText = value;
                if (value.Length == 0 || !int.TryParse(value, out _multiplier)) _multiplier = 1;
                else if (_multiplier < 1) _multiplier = 1;
                OnPropertyChanged();
                
                RepresentedBlockSwitching = new (
                    RepresentedBlockSwitching.Stance,
                    RepresentedBlockSwitching.Block,
                    RepresentedBlockSwitching.BlockSwitching,
                    RepresentedBlockSwitching.BlockSettings,
                    RepresentedBlockSwitching.IsReversal,
                    _multiplier != 1,
                    _multiplier
                );
                
                UpdateTextRepresentation();
            }
        }
        public bool IsReversal
        {
            get => RepresentedBlockSwitching.IsReversal;
            set
            {
                if (RepresentedBlockSwitching.IsReversal == value) return;
                
                RepresentedBlockSwitching = new (
                    RepresentedBlockSwitching.Stance,
                    RepresentedBlockSwitching.Block,
                    RepresentedBlockSwitching.BlockSwitching,
                    RepresentedBlockSwitching.BlockSettings,
                    value,
                    RepresentedBlockSwitching.HasMultiplier,
                    RepresentedBlockSwitching.Multiplier
                );
                
                UpdateTextRepresentation();
            }
        }
        
        private string _textRepresentationText = "____";
        public string TextRepresentationText
        {
            get => _textRepresentationText;
            set
            {
                if (_textRepresentationText.Equals(value)) return;
                _textRepresentationText = value;
                OnPropertyChanged();
                IEnumerable<BlockSwitchingElement> parseResult = BlockSwitchingControl.ParseInputText(_textRepresentationText);
                var enumerator = parseResult.GetEnumerator();
                if (!enumerator.MoveNext())
                {
                    SetInitialValues(new BlockSwitchingElement(
                            BlockInputStanceType.Unchanged,
                            BlockInputBlockType.Unchanged,
                            BlockInputSwitchingType.Unchanged,
                            BlockInputBlockSettingsType.Unchanged,
                            false,
                            false,
                            1
                        ),
                        false);
                }
                else
                {
                    SetInitialValues(enumerator.Current, false);
                }
            }
        }
        
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;
            if (menuItem.Tag is not string tag) return;
            int index = tag.IndexOf('=');
            if (index == -1) return;
            string field = tag.Substring(0, index);
            string value = tag.Substring(index + 1);
            object icon = menuItem.Icon is Image menuItemIconImage && menuItemIconImage != null
                ? Resources[menuItemIconImage.Tag]
                : Resources["EmptyIcon"];
            
            if (field.Equals("Stance"))
            {
                CurrentStanceIcon = icon;
                OnPropertyChanged("CurrentStanceIcon");
                RepresentedBlockSwitching.Stance = Enum.Parse<BlockInputStanceType>(value);
            }
            else if (field.Equals("Block"))
            {
                CurrentBlockTypeIcon = icon;
                OnPropertyChanged("CurrentBlockTypeIcon");
                RepresentedBlockSwitching.Block = Enum.Parse<BlockInputBlockType>(value);
            }
            else if (field.Equals("BlockSwitching"))
            {
                CurrentBlockSwitchingIcon = icon;
                OnPropertyChanged("CurrentBlockSwitchingIcon");
                RepresentedBlockSwitching.BlockSwitching = Enum.Parse<BlockInputSwitchingType>(value);
            }
            else if (field.Equals("BlockSettings"))
            {
                CurrentBlockSettingsIcon = icon;
                OnPropertyChanged("CurrentBlockSettingsIcon");
                RepresentedBlockSwitching.BlockSettings = Enum.Parse<BlockInputBlockSettingsType>(value);
            }
            
            RepresentedBlockSwitching = new(
                RepresentedBlockSwitching.Stance,
                RepresentedBlockSwitching.Block,
                RepresentedBlockSwitching.BlockSwitching,
                RepresentedBlockSwitching.BlockSettings,
                RepresentedBlockSwitching.IsReversal,
                RepresentedBlockSwitching.HasMultiplier,
                RepresentedBlockSwitching.Multiplier
            );
            
            UpdateTextRepresentation();
        }
        
        private void UpdateTextRepresentation()
        {
            _textRepresentationText =
                (RepresentedBlockSwitching.IsReversal ? "!" : string.Empty)
                + BlockSwitchingControl.TranslateStanceToNotation(RepresentedBlockSwitching.Stance)
                + BlockSwitchingControl.TranslateBlockStanceToNotation(RepresentedBlockSwitching.Block)
                + BlockSwitchingControl.TranslateBlockSwitchingToNotation(RepresentedBlockSwitching.BlockSwitching)
                + BlockSwitchingControl.TranslateBlockSettingsToNotation(RepresentedBlockSwitching.BlockSettings)
                + (RepresentedBlockSwitching.HasMultiplier ? $"*{_multiplierText}" : string.Empty);
            OnPropertyChanged("TextRepresentationText");
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;
    
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OKClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
