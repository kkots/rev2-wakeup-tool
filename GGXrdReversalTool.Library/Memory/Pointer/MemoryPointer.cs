using GGXrdReversalTool.Library.Configuration;

namespace GGXrdReversalTool.Library.Memory.Pointer;

public class MemoryPointer
{
    private const char SeparatorChar = '|';
    
    public IntPtr Pointer { get; }
    public IEnumerable<int> Offsets { get; }


    public MemoryPointer(params int[] offsets)
    {
        Pointer = new IntPtr(offsets.First());
        Offsets = new List<int>(offsets.Skip(1));
    }

    public MemoryPointer(IntPtr pointer, IEnumerable<int> offsets)
    {
        Pointer = pointer;
        Offsets = offsets;
    }
    
    public MemoryPointer OffsetBy(int offset)
    {
        var newOffsets = new List<int>(Offsets.SkipLast(1))
        {
            Offsets.Last() + offset
        };
        return new MemoryPointer(Pointer, newOffsets);
    }

    public static MemoryPointer FromConfigName(string configName, IntPtr baseAddress)
    {
        var value = ReversalToolConfiguration.Get(configName);

        return FromValue(value, baseAddress);
    }

    public static MemoryPointer FromValue(string value, IntPtr baseAddress)
    {
        var values = value.Split(SeparatorChar);

        if (!values.Any())
        {
            throw new ArgumentException("Memory pointer value should not be null", nameof(value));
        }
        
        int pointerValue;

        try
        {
            pointerValue = Convert.ToInt32(values[0], 16);
        }
        catch (Exception)
        {
            throw new ArgumentException("Pointer value is invalid", nameof(value));
        }
        
        var pointer = baseAddress + pointerValue;
        
        var offsets = values.Skip(1).Select(offset =>
        {
            int offsetValue;
            try
            {
                offsetValue = Convert.ToInt32(offset, 16);
            }
            catch (Exception)
            {
                throw new ArgumentException($"Pointer value is invalid", nameof(value));
            }

            return offsetValue;
        });

        return new MemoryPointer(pointer, offsets);
    }
    
  
}