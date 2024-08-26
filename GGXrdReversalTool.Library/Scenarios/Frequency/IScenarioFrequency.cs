using GGXrdReversalTool.Library.Memory;

namespace GGXrdReversalTool.Library.Scenarios.Frequency;

public interface IScenarioFrequency
{
    /// <summary>
    /// Should the scenario be played back.
    /// </summary>
    /// <param name="slotNumber">Out variable denoting the slot number that
    /// must be used. 1, 2 or 3. Specify -1 to use the slot currently selected in the interface.</param>
    /// <returns>Returns true if the scenario should be played, false otherwise.</returns>
    bool ShouldHappen(out int slotNumber);
    IMemoryReader? MemoryReader { get; internal set; }
    int[] UsedSlotNumbers();
}