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

        return Write(address, bytes);

    }

    private bool Write(IntPtr address, IEnumerable<byte> bytes)
    {
        var byteArray = bytes.ToArray();
        var handle = Process.Handle;
        var lpNumberOfBytesRead = 0;
        return WriteProcessMemory(handle, address, byteArray, byteArray.Length, ref lpNumberOfBytesRead);
    }


    private class MemoryPointerCollection
    {
        public MemoryPointer P1AnimStringPtr { get; private set; } = null!;
        public MemoryPointer P2AnimStringPtr { get; private set; } = null!;
        public MemoryPointer FrameCountPtr { get; private set; } = null!;
        public MemoryPointer DummyIdPtr { get; private set; } = null!;
        public MemoryPointer RecordingSlotPtr { get; private set; } = null!;
        public MemoryPointer P1ComboCountPtr { get; private set; } = null!;
        public MemoryPointer P2ComboCountPtr { get; private set; } = null!;
        public MemoryPointer P1ReplayKeyPtr { get; private set; } = null!;
        public MemoryPointer P2ReplayKeyPtr { get; private set; } = null!;
        public MemoryPointer P1BlockStunPtr { get; private set; } = null!;
        public MemoryPointer P2BlockStunPtr { get; private set; } = null!;

        private readonly Process _process;
        private readonly MemoryReader _memoryReader;

        public MemoryPointerCollection(Process process, MemoryReader memoryReader)
        {
            _process = process;
            _memoryReader = memoryReader;
            BuildCollection();
        }

        private void BuildCollection()
        {
            var textAddr = _process.MainModule!.BaseAddress + 0x1000;

            if (VirtualQueryEx(_process.Handle, textAddr, out var memoryBasicInformation64,
                    (uint)Marshal.SizeOf(typeof(MemoryBasicInformation64))) == 0)
            {
                throw new Exception("Failed to retrieve information about .text allocation");
            }

            var text = _memoryReader.ReadBytes(textAddr, (int)memoryBasicInformation64.RegionSize);

            const string playerPattern = "i4Ew5iIAi4B8AwAAM9s7w3QPi4DIAQAAg+ABiUQkKOs=";
            var matchPtrAddr = _memoryReader.Read<int>(textAddr - 4 + FindPatternOffset(text, playerPattern));

            const int playerOffset = 0x169814;
            const int playerSize = 0x2d198;

            P1AnimStringPtr = new MemoryPointer(matchPtrAddr, playerOffset + 0x2444);
            P2AnimStringPtr = new MemoryPointer(matchPtrAddr, playerOffset + playerSize + 0x2444);
            P2ComboCountPtr = new MemoryPointer(matchPtrAddr, playerOffset + 0x9f28);
            P1ComboCountPtr = new MemoryPointer(matchPtrAddr, playerOffset + playerSize + 0x9f28);
            P1BlockStunPtr = new MemoryPointer(matchPtrAddr, playerOffset + 0x4d54);
            P2BlockStunPtr = new MemoryPointer(matchPtrAddr, playerOffset + playerSize + 0x4d54);


            const string recordingSlotPattern = "i1QkBGnSyBIAAIHC";
            RecordingSlotPtr =
                new MemoryPointer(
                    _memoryReader.Read<int>(textAddr + 12 + FindPatternOffset(text, recordingSlotPattern)));

            const string frameCountPattern = "0o1U0AhBiYioAAAAxwIEAAAAiV8Mx0cUAwAAAF9eiR0=";
            FrameCountPtr =
                new MemoryPointer(_memoryReader.Read<int>(textAddr + 32 + FindPatternOffset(text, frameCountPattern)));

            const string dummyIdPattern = "VmYP1kZoajjHRgQ4AAAA6A==";
            DummyIdPtr =
                new MemoryPointer(_memoryReader.Read<int>(textAddr - 0x70 + FindPatternOffset(text, dummyIdPattern)) +
                                  0x200);

            const string keyBindingPattern = "h0EBAACLRgiNPIDB5wSBxw==";
            var keyBindingRelAddr = _memoryReader.Read<int>(textAddr + 16 + FindPatternOffset(text, keyBindingPattern));

            P1ReplayKeyPtr = new MemoryPointer(keyBindingRelAddr + 0x40);
            P2ReplayKeyPtr = new MemoryPointer(keyBindingRelAddr + 0x90);
        }

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

        private int FindPatternOffset(in byte[] haystack, in string needle) =>
            FindPatternOffset(haystack, Convert.FromBase64String(needle));


        #region DLL Imports

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int VirtualQueryEx(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            out MemoryBasicInformation64 lpBuffer,
            uint dwSize);

        [StructLayout(LayoutKind.Sequential)]
        private struct MemoryBasicInformation64
        {
            private readonly ulong BaseAddress;
            private readonly ulong AllocationBase;
            private readonly uint AllocationProtect;
            private readonly uint Alignment1;
            public readonly ulong RegionSize;
            private readonly uint State;
            private readonly uint Protect;
            private readonly uint Type;
            private readonly uint Alignment2;
        }

        #endregion

    }
}