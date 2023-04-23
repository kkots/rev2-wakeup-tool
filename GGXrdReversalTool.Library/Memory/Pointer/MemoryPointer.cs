using GGXrdReversalTool.Library.Configuration;

namespace GGXrdReversalTool.Library.Memory.Pointer;

//TODO Lint
public class MemoryPointer
{
    public IntPtr Pointer { get; }
    public IEnumerable<int> Offsets { get; }


    public MemoryPointer(params int[] offsets)
    {
        Pointer = new IntPtr(offsets.First());
        Offsets = new List<int>(offsets.Skip(1));
    }
}