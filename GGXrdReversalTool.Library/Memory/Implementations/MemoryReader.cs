using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using GGXrdReversalTool.Library.Characters;
using GGXrdReversalTool.Library.Memory.Pointer;
using GGXrdReversalTool.Library.Models.Inputs;

namespace GGXrdReversalTool.Library.Memory.Implementations;

//TODO Lint
public class MemoryReader : IMemoryReader
{
    public MemoryReader(Process process)
    {
        Process = process;
        _pointerCollection = new MemoryPointerCollection(process, this);
    }

    private const int RecordingSlotSize = 4808;

    private readonly MemoryPointerCollection _pointerCollection;

    public Process Process { get; }

    public string ReadAnimationString(int player)
    {
        const int length = 32;

        return player switch
        {
            1 => ReadString(_pointerCollection.P1AnimStringPtr, length),
            2 => ReadString(_pointerCollection.P2AnimStringPtr, length),
            _ => string.Empty
        };
    }

    public int FrameCount()
    {
        return Read<int>(_pointerCollection.FrameCountPtr);
    }

    public Character GetCurrentDummy()
    {
        var index = Read<int>(_pointerCollection.DummyIdPtr);
        var result = Character.Characters[index];

        return result;
    }

    public bool WriteInputInSlot(int slotNumber, SlotInput slotInput)
    {
        if (slotNumber is < 1 or > 3)
        {
            throw new ArgumentException("Invalid Slot number", nameof(slotNumber));
        }
        var baseAddress = GetAddressWithOffsets(_pointerCollection.RecordingSlotPtr);
        var slotAddress = IntPtr.Add(baseAddress, RecordingSlotSize * (slotNumber - 1));

        return Write(slotAddress, slotInput.Header.Concat(slotInput.Content));
    }

    public int GetComboCount(int player)
    {
        return player switch
        {
            1 => Read<int>(_pointerCollection.P1ComboCountPtr),
            2 => Read<int>(_pointerCollection.P2ComboCountPtr),
            _ => throw new ArgumentException($"Player index is invalid : {player}")
        };
    }

    public int GetReplayKeyCode(int player)
    {
        return player switch
        {
            1 => Read<int>(_pointerCollection.P1ReplayKeyPtr),
            2 => Read<int>(_pointerCollection.P2ReplayKeyPtr),
            _ => throw new ArgumentException($"Player index is invalid : {player}")
        };
    }

    public int GetBlockstun(int player)
    {
        return player switch
        {
            1 => throw new NotImplementedException("GetBlockstun not implemented for player 1"),
            2 => Read<int>(_pointerCollection.P2BlockStunPtr),
            _ => throw new ArgumentException($"Player index is invalid : {player}")
        };
    }


    #region DLL Imports

    [DllImport("kernel32.dll")]
    private static extern bool ReadProcessMemory(
        IntPtr hProcess,
        IntPtr lpBaseAddress,
        byte[] lpBuffer,
        int dwSize,
        ref int lpNumberOfBytesRead);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool WriteProcessMemory(
        IntPtr hProcess,
        IntPtr lpBaseAddress,
        byte[] lpBuffer,
        int dwSize,
        ref int lpNumberOfBytesRead);

    

    #endregion

    private IntPtr GetAddressWithOffsets(MemoryPointer memoryPointer)
    {
        var address = memoryPointer.Pointer;
        foreach (var offset in memoryPointer.Offsets)
        {
            address = ReadPtr(address) + offset;
        }

        return address;
    }

    private string ReadString(MemoryPointer memoryPointer, int length)
    {
        var value = ReadBytes(GetAddressWithOffsets(memoryPointer), length);
        var result = Encoding.Default.GetString(value);
        return result.Replace("\0", "");
    }

    public T Read<T>(IntPtr address)
    {
        var outputType = typeof(T).IsEnum ? Enum.GetUnderlyingType(typeof(T)) : typeof(T);

        var length = Marshal.SizeOf(outputType);

        var value = this.ReadBytes(address, length);


        T result;

        var handle = GCHandle.Alloc(value, GCHandleType.Pinned);

        try
        {
            result = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), outputType);
        }
        finally
        {
            handle.Free();
        }

        return result;
    }

    private IntPtr ReadPtr(IntPtr address)
    {
        return new IntPtr(Read<int>(address));
    }

    public byte[] ReadBytes(IntPtr address, int length)
    {
        var handle = Process.Handle;
        var bytes = new byte[length];
        var lpNumberOfBytesRead = 0;
        ReadProcessMemory(handle, address, bytes, bytes.Length, ref lpNumberOfBytesRead);

        return bytes;
    }

    private T Read<T>(MemoryPointer memoryPointer)
    {
        return Read<T>(GetAddressWithOffsets(memoryPointer));
    }

    private bool Write(IntPtr address, IEnumerable<ushort> shorts)
    {
        var bytes = new List<byte>();

        foreach (var @ushort in shorts)
        {
            bytes.Add((byte)(@ushort & 0xFF));
            bytes.Add((byte)((@ushort >> 8) & 0xFF));
        }

        return Write(address, bytes.ToArray());

    }
    private bool Write(IntPtr address, IEnumerable<byte> bytes)
    {
        var byteList = bytes.ToList();
        var handle = Process.Handle;
        var lpNumberOfBytesRead = 0;
        return WriteProcessMemory(handle, address, byteList.ToArray(), byteList.Count(), ref lpNumberOfBytesRead);
    }
}