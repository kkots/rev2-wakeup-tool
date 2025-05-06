using System.ComponentModel;

namespace GGXrdReversalTool.Library.Scenarios.Event;

public enum ScenarioEventTypes
{
    Combo,
    Animation,
    [Description("Simulated roundstart")]
    SimulatedRoundstart,
    [Description("Delay air recovery")]
    DelayAirRecovery,
    Periodically,
    [Description("Blocked a certain hit")]
    BlockedACertainHit
}