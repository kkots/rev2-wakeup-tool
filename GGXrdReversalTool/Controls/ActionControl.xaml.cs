using System;
using System.IO;
using System.Linq;
using System.Windows;
using GGXrdReversalTool.Commands;
using GGXrdReversalTool.Library.Logging;
using GGXrdReversalTool.Library.Models.Inputs;
using GGXrdReversalTool.Library.Scenarios.Action;
using GGXrdReversalTool.Library.Scenarios.Action.Implementations;
using GGXrdReversalTool.Library.Scenarios.Event;
using Microsoft.Win32;

namespace GGXrdReversalTool.Controls;

public sealed partial class ActionControl
{
    public ActionControl()
    {
        InitializeComponent();
    }
    

    private readonly string[] _rawInputTexts = {string.Empty, string.Empty, string.Empty};
    public string RawInputText
    {
        get => _rawInputTexts[_slotNumber - 1];
        set
        {
            if (value == _rawInputTexts[_slotNumber - 1]) return;
            _rawInputTexts[_slotNumber - 1] = value;
            OnPropertyChanged();
            CreateScenario();
        }
    }
    
    private Visibility _tooShortInfoVisible = Visibility.Collapsed;
    public Visibility TooShortInfoVisible
    {
        get => _tooShortInfoVisible;
        set
        {
            if (value == _tooShortInfoVisible) return;
            _tooShortInfoVisible = value;
            OnPropertyChanged();
        }
    }
    
    private Visibility _noStartMarkerWarningVisible = Visibility.Collapsed;
    public Visibility NoStartMarkerWarningVisible
    {
        get => _noStartMarkerWarningVisible;
        set
        {
            if (value == _noStartMarkerWarningVisible) return;
            _noStartMarkerWarningVisible = value;
            OnPropertyChanged();
        }
    }
    
    private Visibility _tooLongMarkerWarningVisible = Visibility.Collapsed;
    public Visibility TooLongMarkerWarningVisible
    {
        get => _tooLongMarkerWarningVisible;
        set
        {
            if (value == _tooLongMarkerWarningVisible) return;
            _tooLongMarkerWarningVisible = value;
            OnPropertyChanged();
        }
    }
    
    private Visibility _noNeedStartMarkerInfoVisible = Visibility.Collapsed;
    public Visibility NoNeedStartMarkerInfoVisible
    {
        get => _noNeedStartMarkerInfoVisible;
        set
        {
            if (value == _noNeedStartMarkerInfoVisible) return;
            _noNeedStartMarkerInfoVisible = value;
            OnPropertyChanged();
        }
    }
    
    private bool[] _guaranteeChargeInputArray = { false, false, false };
    public bool GuaranteeChargeInput
    {
        get => _guaranteeChargeInputArray[_slotNumber - 1];
        set
        {
            if (value == _guaranteeChargeInputArray[_slotNumber - 1]) return;
            _guaranteeChargeInputArray[_slotNumber - 1] = value;
            OnPropertyChanged();
            CreateScenario();
        }
    }
    
    private IScenarioEvent? _scenarioEvent;
    public IScenarioEvent? ScenarioEvent
    {
        get => (IScenarioEvent)GetValue(ScenarioEventProperty);
        set
        {
            if (_scenarioEvent == value) return;
            _scenarioEvent = value;
            UpdateWarnings();
        }
    }

    public static readonly DependencyProperty ScenarioEventProperty =
        DependencyProperty.Register(nameof(ScenarioEvent), typeof(IScenarioEvent), typeof(ActionControl),
            new FrameworkPropertyMetadata(null, OnScenarioEventPropertyChanged)
            {
                BindsTwoWayByDefault = false
            });

    private static void OnScenarioEventPropertyChanged(DependencyObject source,
        DependencyPropertyChangedEventArgs eventArgs)
    {
        if (source is not ActionControl control)
        {
            return;
        }
        
        var value = eventArgs.NewValue;
        control.ScenarioEvent = (IScenarioEvent)value;
    }
    
    private int _slotNumber = 1;
    public int SlotNumber
    {
        get => (int)GetValue(SlotNumberProperty);
        set
        {
            int coercedValue = Math.Clamp(value, 1, 3);
            if (_slotNumber == coercedValue) return;
            _slotNumber = coercedValue;
            OnPropertyChanged("RawInputText");
            OnPropertyChanged("GuaranteeChargeInput");
            CreateScenario();
        }
    }

    public static readonly DependencyProperty SlotNumberProperty =
        DependencyProperty.Register(nameof(SlotNumber), typeof(int), typeof(ActionControl),
            new FrameworkPropertyMetadata(1, OnSlotNumberPropertyChanged, OnCoerceSlotNumberProperty)
            {
                BindsTwoWayByDefault = false
            });

    private static object OnCoerceSlotNumberProperty(DependencyObject source, object baseValue)
    {
        if (baseValue is not int value)
        {
            return SlotNumberProperty.DefaultMetadata.DefaultValue;
        }

        switch (value)
        {
            case 1:
            case 2:
            case 3:
                return value;

        }

        return SlotNumberProperty.DefaultMetadata.DefaultValue;
    }
    private static void OnSlotNumberPropertyChanged(DependencyObject source,
        DependencyPropertyChangedEventArgs eventArgs)
    {
        if (source is not ActionControl control)
        {
            return;
        }

        var value = eventArgs.NewValue;
        control.SlotNumber = (int)value;
    }

    public RelayCommand ImportCommand => new(Import);
    private void Import()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Reversal Tool Replay Slot file (*.ggrs)|*.ggrs"
        };
        var dialogResult = openFileDialog.ShowDialog();

        if (!dialogResult.HasValue || !dialogResult.Value) return;
        
        using var streamReader = new StreamReader(openFileDialog.FileName);

        var content = streamReader.ReadToEnd();

        var slotInput = new SlotInput(content);


        if (slotInput.IsValid)
        {
            RawInputText = slotInput.CondensedInputListText.Aggregate((a, b) => $"{a},{b}");
        }
        else
        {
            MessageBox.Show("Failed to import inputs!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public RelayCommand ExportCommand => new(Export, CanExport);

    private void Export()
    {
        var saveFileDialog = new SaveFileDialog
        {
            Filter = "Reversal Tool Replay Slot file (*.ggrs)|*.ggrs"
        };

        var dialogResult = saveFileDialog.ShowDialog();

        if (!dialogResult.HasValue || !dialogResult.Value) return;

        var condensedInputText = new SlotInput(RawInputText).CondensedInputListText.Aggregate((a, b) => $"{a},{b}");

        try
        {
            using var streamWriter = new StreamWriter(saveFileDialog.FileName);
            streamWriter.Write(condensedInputText);

            MessageBox.Show("Inputs Exported!");
        }
        catch (Exception e)
        {
            LogManager.Instance.WriteException(e);

            MessageBox.Show("Failed to export inputs!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    private bool CanExport()
    {
        return ScenarioAction != null && ScenarioAction.Inputs[_slotNumber - 1].IsValid;
    }

    #region InsertPresetInputCommand

    public RelayCommand<string> InsertPresetInputCommand => new(InsertPresetInput, CanInsertPresetInput);

    

    private void InsertPresetInput(string input)
    {
        RawInputText = RawInputText +
                       $"{(!RawInputText.EndsWith(",") && !string.IsNullOrWhiteSpace(RawInputText)  ? "," : "")}" +
                       input;
    }

    private bool CanInsertPresetInput(string input)
    {
        return IsEnabled;
    }

    #endregion
    
    public IScenarioAction? ScenarioAction
    {
        get => (IScenarioAction?)GetValue(ScenarioActionProperty);
        set => SetValue(ScenarioActionProperty, value);
    }

    public static readonly DependencyProperty ScenarioActionProperty =
        DependencyProperty.Register(nameof(ScenarioAction), typeof(IScenarioAction), typeof(ActionControl),
            new PropertyMetadata(default(IScenarioAction?)));


    private void CreateScenario()
    {
        var inputs = _rawInputTexts.Select(rawInputText => new SlotInput(rawInputText)).ToArray();
        
        ScenarioAction = new PlayReversalAction
        {
            Inputs = inputs,
            SlotNumber = _slotNumber,
            GuaranteeChargeInputArray = _guaranteeChargeInputArray
        };
        UpdateWarnings();
    }
    
    private void UpdateWarnings()
    {
        UpdateTooShort();
        UpdateMissingStart();
        UpdateNoNeedStart();
        UpdateTooLong();
    }
    
    private void UpdateTooShort()
    {
        if (ScenarioAction == null || RawInputText.Length == 0) {
            TooShortInfoVisible = Visibility.Collapsed;
            return;
        }
        int tooShortLength = 4;
        int count = 0;
        foreach (string input in ScenarioAction.Inputs[_slotNumber - 1].ExpandedInputList) {
            ++count;
            if (count > tooShortLength) break;
        }
        if (count != 0 && count <= tooShortLength) {
            TooShortInfoVisible = Visibility.Visible;
        } else {
            TooShortInfoVisible = Visibility.Collapsed;
        }
    }
    
    private void UpdateMissingStart()
    {
        if (ScenarioEvent != null && !ScenarioEvent.DependsOnReversalFrame() || RawInputText.Length == 0) {
            NoStartMarkerWarningVisible = Visibility.Collapsed;
            return;
        }
        if (!RawInputText.Contains('!')) {
            NoStartMarkerWarningVisible = Visibility.Visible;
        } else {
            NoStartMarkerWarningVisible = Visibility.Collapsed;
        }
    }
    
    private void UpdateTooLong()
    {
        if (RawInputText.Length == 0) {
            TooLongMarkerWarningVisible = Visibility.Collapsed;
            return;
        }
        SlotInput testSlot = new SlotInput(RawInputText);
        if (testSlot.Header.Count() + testSlot.Content.Count() > SlotInput.RecordingSlotSize) {
            TooLongMarkerWarningVisible = Visibility.Visible;
        } else {
            TooLongMarkerWarningVisible = Visibility.Collapsed;
        }
    }
    
    private void UpdateNoNeedStart()
    {
        if (ScenarioEvent != null && ScenarioEvent.DependsOnReversalFrame()
                || RawInputText.Length == 0
                || ScenarioAction != null && ScenarioAction.Inputs[_slotNumber - 1].ReversalFrameIndex <= 0
                || !RawInputText.Contains('!')) {
            NoNeedStartMarkerInfoVisible = Visibility.Collapsed;
            return;
        }
        NoNeedStartMarkerInfoVisible = Visibility.Visible;
    }
}