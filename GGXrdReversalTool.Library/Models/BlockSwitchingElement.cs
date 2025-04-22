using System.ComponentModel;

namespace GGXrdReversalTool.Library.Models
{
    
    public enum BlockInputBlockSettingsType
    {
        Unchanged,
        NotBlock,
        Everything,
        AfterFirst,
        FirstOnly,
        Random,
        Pin
    }
    
    public enum BlockInputStanceType
    {
        Unchanged,
        Standing,
        Crouching,
        Opposite,
        Pin,
        Random,
        Jumping
    }
    
    public enum BlockInputBlockType
    {
        Unchanged,
        Normal,
        FD,
        IB,
        Pin,
        Random,
        GamesOwnImplementationOfRandom
    }
    
    public enum BlockInputSwitchingType
    {
        Unchanged,
        Off,
        On,
        OnSecond
    }
    
    public class BlockSwitchingElement : INotifyPropertyChanged
    {
        public BlockSwitchingElement(
                BlockInputStanceType Stance,
                BlockInputBlockType Block,
                BlockInputSwitchingType BlockSwitching,
                BlockInputBlockSettingsType BlockSettings,
                bool IsReversal,
                bool HasMultiplier,
                int Multiplier)
        {
            this.Stance = Stance;
            this.Block = Block;
            this.BlockSwitching = BlockSwitching;
            this.BlockSettings = BlockSettings;
            this.IsReversal = IsReversal;
            this.HasMultiplier = HasMultiplier;
            this.Multiplier = HasMultiplier ? Multiplier : 1;
        }
        public BlockInputStanceType Stance { get; set; }
        public BlockInputBlockType Block { get; set; }
        public BlockInputSwitchingType BlockSwitching { get; set; }
        public BlockInputBlockSettingsType BlockSettings { get; set; }
        public bool IsReversal { get; set; }
        public bool HasMultiplier { get; set; }
        public int Multiplier { get; set; }
        private bool _isSelected = false;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsSelected"));
            }
        }
        public int Start { get; set; } = 0;
        public int End { get; set; } = 0;
        public int Index { get; set; } = 0;
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
