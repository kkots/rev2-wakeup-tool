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
    private readonly Process _process;

    public MemoryReader(Process process)
    {
        _process = process;

        _MEMORY_BASIC_INFORMATION64 mbi;
        var textAddr = _process.MainModule.BaseAddress + 0x1000;
        if (0 == VirtualQueryEx(_process.Handle, textAddr, out mbi, (uint)Marshal.SizeOf(typeof(_MEMORY_BASIC_INFORMATION64))))
        {
            throw new Exception($"Failed to retrieve information about .text allocation");
        }

        var text = ReadBytes(textAddr, (int)mbi.RegionSize);

        var matchPtrAddr = Read<int>(textAddr - 4 + FindPatternOffset(text, "i4Ew5iIAi4B8AwAAM9s7w3QPi4DIAQAAg+ABiUQkKOs="));
        const int playerOffset = 0x169814;
        const int playerSize = 0x2d198;
        _p1AnimStringPtr = new MemoryPointer("P1AnimStringPtr", new[] { matchPtrAddr, playerOffset + 0x2444 });
        _p2AnimStringPtr = new MemoryPointer("P2AnimStringPtr", new[] { matchPtrAddr, playerOffset + playerSize + 0x2444 });
        _p2ComboCountPtr = new MemoryPointer("P2ComboCountPtr", new[] { matchPtrAddr, playerOffset + 0x9f28 });
        _p1ComboCountPtr = new MemoryPointer("P1ComboCountPtr", new[] { matchPtrAddr, playerOffset + playerSize + 0x9f28 });
        _p1BlockStunPtr = new MemoryPointer("P1BlockStunPtr", new[] { matchPtrAddr, playerOffset + 0x4d54 });
        _p2BlockStunPtr = new MemoryPointer("P2BlockStunPtr", new[] { matchPtrAddr, playerOffset + playerSize + 0x4d54 });
        _recordingSlotPtr = new MemoryPointer("RecordingSlotPtr",  new[] { Read<int>(textAddr + 12 + FindPatternOffset(text, "i1QkBGnSyBIAAIHC")) });
        _frameCountPtr = new MemoryPointer("FrameCountPtr", new[] { Read<int>(textAddr + 32 + FindPatternOffset(text, "0o1U0AhBiYioAAAAxwIEAAAAiV8Mx0cUAwAAAF9eiR0=")) });
        _dummyIdPtr = new MemoryPointer("DummyIdPtr", new[] { Read<int>(textAddr - 0x70 + FindPatternOffset(text, "VmYP1kZoajjHRgQ4AAAA6A==")) + 0x200 });
        var keyBindingRelAddr = Read<int>(textAddr + 16 + FindPatternOffset(text, "h0EBAACLRgiNPIDB5wSBxw=="));
        _p1ReplayKeyPtr = new MemoryPointer("P1ReplaykeyPtr", new[] { keyBindingRelAddr + 0x40 });
        _p2ReplayKeyPtr = new MemoryPointer("P2ReplaykeyPtr", new[] { keyBindingRelAddr + 0x90 });
    }

    private readonly MemoryPointer _p1AnimStringPtr;
    private readonly MemoryPointer _p2AnimStringPtr;
    private readonly MemoryPointer _frameCountPtr;
    private readonly MemoryPointer _dummyIdPtr;
    private readonly MemoryPointer _recordingSlotPtr;
    private readonly MemoryPointer _p1ComboCountPtr;
    private readonly MemoryPointer _p2ComboCountPtr;
    private readonly MemoryPointer _p1ReplayKeyPtr;
    private readonly MemoryPointer _p2ReplayKeyPtr;
    private readonly MemoryPointer _p1BlockStunPtr;
    private readonly MemoryPointer _p2BlockStunPtr;
    private const int RecordingSlotSize = 4808;


    public Process Process => _process;

    public string ReadAnimationString(int player)
    {
        const int length = 32;

        switch (player)
        {
            case 1:
                return ReadString(_p1AnimStringPtr, length);
            case 2:
                return ReadString(_p2AnimStringPtr, length);
            default:
                return string.Empty;
        }
    }

    public int FrameCount()
    {
        return Read<int>(_frameCountPtr);
    }

    public Character GetCurrentDummy()
    {
        var index = Read<int>(_dummyIdPtr);
        var result = Character.Characters[index];

        return result;
    }


    public bool WriteInputInSlot(int slotNumber, SlotInput slotInput)
    {
        if (slotNumber is < 1 or > 3)
        {
            throw new ArgumentException("Invalid Slot number", nameof(slotNumber));
        }
        var baseAddress = GetAddressWithOffsets(_recordingSlotPtr);
        var slotAddress = IntPtr.Add(baseAddress, RecordingSlotSize * (slotNumber - 1));

        return Write(slotAddress, slotInput.Header.Concat(slotInput.Content));
    }

    public int GetComboCount(int player)
    {
        return player switch
        {
            1 => Read<int>(_p1ComboCountPtr),
            2 => Read<int>(_p2ComboCountPtr),
            _ => throw new ArgumentException($"Player index is invalid : {player}")
        };
    }

    public int GetReplayKeyCode(int player)
    {
        return player switch
        {
            1 => Read<int>(_p1ReplayKeyPtr),
            2 => Read<int>(_p2ReplayKeyPtr),
            _ => throw new ArgumentException($"Player index is invalid : {player}")
        };
    }

    public int GetBlockstun(int player)
    {
        return player switch
        {
            1 => throw new NotImplementedException("GetBlockstun not implemented for player 1"),
            2 => Read<int>(_p2BlockStunPtr),
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

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern int VirtualQueryEx(
        IntPtr hProcess,
        IntPtr lpBaseAddress,
        out _MEMORY_BASIC_INFORMATION64 lpBuffer,
        uint dwSize);

    [StructLayout(LayoutKind.Sequential)]
    private struct _MEMORY_BASIC_INFORMATION64 {
        public ulong BaseAddress;
        public ulong AllocationBase;
        public uint AllocationProtect;
        uint _alignment1;
        public ulong RegionSize;
        public uint State;
        public uint Protect;
        public uint Type;
        uint _alignment2;
    }

    #endregion

    private int FindPatternOffset(in byte[] haystack, in byte[] needle)
    {
        // Boyer-Moore-Horspool substring search
        var needleLen = needle.Length;
        var step = Enumerable.Repeat(needleLen, 256).ToArray();
        for (var i = 0; i < needleLen - 1; i++)
        {
            step[needle[i]] = needleLen - 1 - i;
        }

        var end = haystack.Length - needleLen;
        for (var p = 0; p <= end; p += step[haystack[p + needleLen - 1]])
        {
            int j;
            for (j = needleLen; --j >= 0;)
            {
                if (needle[j] != haystack[p + j])
                {
                    break;
                }
            }

            if (j < 0)
            {
                return p;
            }
        }
        throw new Exception("Could not find offset in process");
    }

    private int FindPatternOffset(in byte[] haystack, in string needle) => FindPatternOffset(haystack, Convert.FromBase64String(needle));

    //TODO Remove?
    private T UnmanagedConvert<T>(object value)
    {
        var outputType = typeof(T).IsEnum ? Enum.GetUnderlyingType(typeof(T)) : typeof(T);

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

    private T Read<T>(IntPtr address)
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

    private byte[] ReadBytes(IntPtr address, int length)
    {
        var handle = _process.Handle;
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
        var handle = _process.Handle;
        var lpNumberOfBytesRead = 0;
        return WriteProcessMemory(handle, address, bytes.ToArray(), bytes.Count(), ref lpNumberOfBytesRead);
    }
}