using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GGXrdReversalTool.Commands;
using GGXrdReversalTool.Library.Logging;
using GGXrdReversalTool.Library.Models.Inputs;
using GGXrdReversalTool.Library.Scenarios.Action;
using GGXrdReversalTool.Library.Scenarios.Action.Implementations;
using GGXrdReversalTool.Library.Scenarios.Event;
using GGXrdReversalTool.ViewModels;
using Microsoft.Win32;

namespace GGXrdReversalTool.Controls;

public sealed partial class ActionControl
{
    public ActionControl()
    {
        InitializeComponent();
    }
    
    public EventTabElement? TabElement
    {
        get => (EventTabElement?)GetValue(TabElementProperty);
        set => SetValue(TabElementProperty, value);
    }
    public static readonly DependencyProperty TabElementProperty =
        DependencyProperty.Register(nameof(TabElement), typeof(EventTabElement), typeof(ActionControl),
            new PropertyMetadata(default(EventTabElement), OnTabElementPropertyChanged));
    
    public bool IsRunning
    {
        get => (bool)GetValue(IsRunningProperty);
        set => SetValue(IsRunningProperty, value);
    }
    public static readonly DependencyProperty IsRunningProperty =
        DependencyProperty.Register(nameof(IsRunning), typeof(bool), typeof(ActionControl),
            new PropertyMetadata(false));
    
    private EventTabElement? _lastSubscribedTabElement = null;
    public static void OnTabElementPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ActionControl control = (ActionControl)d;
        control.OnPropertyChanged("RawInputText");
        control.OnPropertyChanged("GuaranteeChargeInput");
        control.UpdateWarnings();
        
        if (control._lastSubscribedTabElement != null)
        {
            control._lastSubscribedTabElement.SlotsData.PropertyChanged -= control.OnSubscribedTabElementSlotsDataPropertyChanged;
        }
        control._lastSubscribedTabElement = control.TabElement;
        if (control._lastSubscribedTabElement != null)
        {
            control._lastSubscribedTabElement.SlotsData.PropertyChanged += control.OnSubscribedTabElementSlotsDataPropertyChanged;
        }
        control.ResubscribeToSlot();
    }
    
    public void OnSubscribedTabElementSlotsDataPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != null && e.PropertyName.Equals("SlotNumber"))
        {
            ResubscribeToSlot();
            OnPropertyChanged("RawInputText");
            OnPropertyChanged("GuaranteeChargeInput");
            CreateScenario();
        }
    }
    
    private SlotsControlSlotData? _lastSubscribedSlot = null;
    private void ResubscribeToSlot()
    {
        if (_lastSubscribedSlot != null)
        {
            _lastSubscribedSlot.PropertyChanged -= OnSubscribedSlotPropertyChanged;
        }
        _lastSubscribedSlot = TabElement?.SlotsData.CurrentSlot;
        if (_lastSubscribedSlot != null)
        {
            _lastSubscribedSlot.PropertyChanged += OnSubscribedSlotPropertyChanged;
        }
    }
    public void OnSubscribedSlotPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != null && e.PropertyName.Equals("Text"))
        {
            OnPropertyChanged("RawInputText");
            CreateScenario();
        }
    }
    
    public string RawInputText
    {
        get => TabElement?.SlotsData.CurrentSlot.Text ?? string.Empty;
        set
        {
            if (TabElement == null || value == TabElement.SlotsData.CurrentSlot.Text) return;
            TabElement.SlotsData.CurrentSlot.Text = value;
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
    
    public bool GuaranteeChargeInput
    {
        get => TabElement?.SlotsData.CurrentSlot.GuaranteeChargeInput ?? false;
        set
        {
            if (TabElement == null || value == TabElement.SlotsData.CurrentSlot.GuaranteeChargeInput) return;
            TabElement.SlotsData.CurrentSlot.GuaranteeChargeInput = value;
            OnPropertyChanged();
            CreateScenario();
        }
    }
    
    public IScenarioEvent? ScenarioEvent
    {
        get => (IScenarioEvent)GetValue(ScenarioEventProperty);
        set => SetValue(ScenarioEventProperty, value);
    }

    public static readonly DependencyProperty ScenarioEventProperty =
        DependencyProperty.Register(nameof(ScenarioEvent), typeof(IScenarioEvent), typeof(ActionControl),
            new PropertyMetadata(null, OnScenarioEventPropertyChanged));
    
    public static void OnScenarioEventPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        ((ActionControl)d).UpdateWarnings();
    }
    
    public RelayCommand ImportCommand => new(Import);
    public RelayCommand ImportFromInGameSlotCommand => new(ImportFromInGameSlot);
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
            RawInputText = string.Join(",", slotInput.CondensedInputListText);
        }
        else
        {
            MessageBox.Show("Failed to import inputs!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    private void ImportFromInGameSlot()
    {
        var slotSelectionWindow = new SlotSelectionDialog();
        slotSelectionWindow.IsImport = true;
        if (slotSelectionWindow.ShowDialog() == true)
        {
            int selectedSlot = slotSelectionWindow.SlotNumber;
            ImportExportSlotEventArgs routedEventArgs = new(ImportExportSlotEvent, RawInputText, true, selectedSlot);
            RaiseEvent(routedEventArgs);
        }
    }
    
    public static readonly RoutedEvent ImportExportSlotEvent = EventManager.RegisterRoutedEvent(
        name: "ImportExportSlot",
        routingStrategy: RoutingStrategy.Bubble,
        handlerType: typeof(ImportExportSlotEventHandler),
        ownerType: typeof(ActionControl));
    
    public event ImportExportSlotEventHandler ImportExportSlot
    {
        add { AddHandler(ImportExportSlotEvent, value); }
        remove { RemoveHandler(ImportExportSlotEvent, value); }
    }

    public RelayCommand ExportCommand => new(Export, CanExport);
    public RelayCommand ExportToInGameSlotCommand => new(ExportToInGameSlot, CanExportToInGameSlot);

    private void Export()
    {
        var saveFileDialog = new SaveFileDialog
        {
            Filter = "Reversal Tool Replay Slot file (*.ggrs)|*.ggrs"
        };

        var dialogResult = saveFileDialog.ShowDialog();

        if (!dialogResult.HasValue || !dialogResult.Value) return;

        var condensedInputText = string.Join(",", new SlotInput(RawInputText).CondensedInputListText);

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
        return TabElement != null && _scenarioAction != null && _scenarioAction.Inputs[TabElement.SlotsData.SlotNumber - 1].IsValid;
    }
    
    private bool CanExportToInGameSlot()
    {
        return CanExport() && !IsRunning;
    }
    private void ExportToInGameSlot()
    {
        var slotSelectionWindow = new SlotSelectionDialog();
        slotSelectionWindow.IsImport = false;
        if (slotSelectionWindow.ShowDialog() == true)
        {
            int selectedSlot = slotSelectionWindow.SlotNumber;
            ImportExportSlotEventArgs routedEventArgs = new(ImportExportSlotEvent, RawInputText, false, selectedSlot);
            RaiseEvent(routedEventArgs);
        }
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
    
    private IScenarioAction? _scenarioAction
    {
        get => TabElement?.ScenarioAction;
        set => TabElement!.ScenarioAction = value;
    }

    private void CreateScenario()
    {
        if (TabElement == null) return;
        var inputs = TabElement.SlotsData.Slots.Select(slot => new SlotInput(slot.Text)).ToArray();
        
        _scenarioAction = new PlayReversalAction
        {
            Inputs = inputs,
            SlotNumber = TabElement.SlotsData.SlotNumber,
            GuaranteeChargeInputArray = TabElement.SlotsData.Slots.Select(slot => slot.GuaranteeChargeInput).ToArray()
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
        if (_scenarioAction == null || TabElement == null || RawInputText.Length == 0) {
            TooShortInfoVisible = Visibility.Collapsed;
            return;
        }
        int tooShortLength = 4;
        int count = 0;
        foreach (string input in _scenarioAction.Inputs[TabElement.SlotsData.SlotNumber - 1].ExpandedInputList) {
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
        if (ScenarioEvent == null
                || ScenarioEvent.DependsOnReversalFrame()
                || TabElement == null
                || RawInputText.Length == 0
                || _scenarioAction == null
                || _scenarioAction.Inputs[TabElement.SlotsData.SlotNumber - 1].ReversalFrameIndex <= 0
                || !RawInputText.Contains('!')) {
            NoNeedStartMarkerInfoVisible = Visibility.Collapsed;
            return;
        }
        NoNeedStartMarkerInfoVisible = Visibility.Visible;
    }
}

public class ImportExportSlotEventArgs : RoutedEventArgs
{
    public ImportExportSlotEventArgs(RoutedEvent id, string RawInputText, bool IsImport, int SlotNumber) : base(id)
    {
        this.RawInputText = RawInputText;
        this.IsImport = IsImport;
        this.SlotNumber = SlotNumber;
    }
    public string RawInputText { get; set; }
    public bool IsImport { get; set; }
    public int SlotNumber { get; set; }
}

public delegate void ImportExportSlotEventHandler(object sender, ImportExportSlotEventArgs e);
