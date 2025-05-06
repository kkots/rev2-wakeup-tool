using System.Collections.Generic;
using System.Windows.Data;
using GGXrdReversalTool.Library.Scenarios.Event;

namespace GGXrdReversalTool.Converters;

[ValueConversion(typeof(ScenarioEventTypes), typeof(string))]
[ValueConversion(typeof(IEnumerable<ScenarioEventTypes>), typeof(IEnumerable<string>))]
public class ScenarioEventTypeToStringConverter : EnumToStringConverter<ScenarioEventTypes> { }