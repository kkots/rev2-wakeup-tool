using GGXrdReversalTool.Library.Scenarios.Action;
using GGXrdReversalTool.Library.Scenarios.Event;
using GGXrdReversalTool.Library.Scenarios.Frequency;
using GGXrdReversalTool.Library.Scenarios.Frequency.Implementations;

namespace GGXrdReversalTool.ViewModels;

public class EventTabElement : ViewModelBase
{
    
    private int _index = 0;
    public int Index {
        get => _index;
        set
        {
            if (_index == value) return;
            _index = value;
            OnPropertyChanged();
            OnPropertyChanged("TabName");
        }
    }
    
    public string TabName {
        get => _index == -1 ? "New Event" : $"Event {_index + 1}";
    }
    
    private bool _showCrossmark = true;
    public bool ShowCrossmark {
        get => _showCrossmark;
        set
        {
            if (_showCrossmark == value) return;
            _showCrossmark = value;
            OnPropertyChanged();
        }
    }
    
    private EventControlData _controlData;
    public EventControlData ControlData {
        get => _controlData;
        set
        {
            if (_controlData == value) return;
            _controlData = value;
            OnPropertyChanged();
        }
    }
    
    private IScenarioEvent? _scenarioEvent = null;
    public IScenarioEvent? ScenarioEvent
    {
        get => _scenarioEvent;
        set
        {
            if (_scenarioEvent == value) return;
            _scenarioEvent = value;
            OnPropertyChanged();
        }
    }
    
    private IScenarioAction? _scenarioAction = null;
    public IScenarioAction? ScenarioAction
    {
        get => _scenarioAction;
        set
        {
            if (_scenarioAction == value) return;
            _scenarioAction = value;
            OnPropertyChanged();
        }
    }
    
    private IScenarioFrequency? _scenarioFrequency = null;
    public IScenarioFrequency? ScenarioFrequency
    {
        get => _scenarioFrequency;
        set
        {
            if (_scenarioFrequency == value) return;
            _scenarioFrequency = value;
            OnPropertyChanged();
        }
    }
    
    public EventTabElement(int Index, bool ShowCrossmark, IEventControlDataParent Parent, bool IsFirstEvent) {
        this.Index = Index;
        this.ShowCrossmark = ShowCrossmark;
        _controlData = new EventControlData(Parent);
        _frequencyData = new FrequencyControlData();
        _slotsData = new SlotsControlData(_frequencyData, IsFirstEvent);
        _scenarioFrequency = new SingleSlotFrequency();
    }
    
    private FrequencyControlData _frequencyData;
    public FrequencyControlData FrequencyData
    {
        get => _frequencyData;
        set
        {
            if (_frequencyData == value) return;
            _frequencyData = value;
            OnPropertyChanged();
        }
    }
    
    private SlotsControlData _slotsData;
    public SlotsControlData SlotsData
    {
        get => _slotsData;
        set
        {
            if (_slotsData == value) return;
            _slotsData = value;
            OnPropertyChanged();
        }
    }
    
}
