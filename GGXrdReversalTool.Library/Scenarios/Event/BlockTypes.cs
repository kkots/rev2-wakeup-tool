using System.ComponentModel;

namespace GGXrdReversalTool.Library.Scenarios.Event
{
    public enum BlockTypes
    {
        Normal,
        [Description("Instant Block")]
        InstantBlock,
        [Description("Faultless Defense")]
        FaultlessDefense,
        Any
    }
}
