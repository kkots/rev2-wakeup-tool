using System.Collections.Generic;
using System.Windows.Data;
using GGXrdReversalTool.Library.Scenarios.Event;

namespace GGXrdReversalTool.Converters;

[ValueConversion(typeof(BlockTypes), typeof(string))]
[ValueConversion(typeof(IEnumerable<BlockTypes>), typeof(IEnumerable<string>))]
public class BlockTypeToStringConverter : EnumToStringConverter<BlockTypes> { }