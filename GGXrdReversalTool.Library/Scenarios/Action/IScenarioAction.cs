using GGXrdReversalTool.Library.Memory;
using GGXrdReversalTool.Library.Models;
using GGXrdReversalTool.Library.Models.Inputs;

namespace GGXrdReversalTool.Library.Scenarios.Action;

public interface IScenarioAction
{
    void Execute(int slotNumber);
    void Execute()
    {
        Execute(SlotNumber);
    }
    void Tick();
    IMemoryReader? MemoryReader { get; internal set; }
    bool IsRunning { get; }
    
    SlotInput[] Inputs { get; set; }
    void Init(int inputIndex);
    
    int SlotNumber { get; set; }
}
