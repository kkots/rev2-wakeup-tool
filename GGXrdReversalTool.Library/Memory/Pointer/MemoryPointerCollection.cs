using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GGXrdReversalTool.Library.Memory.Pointer;

public class MemoryPointerCollection
{
    public MemoryPointer P1AnimStringPtr { get; private set; } = null!;
    public MemoryPointer P2AnimStringPtr { get;private set; } = null!;
    public MemoryPointer FrameCountPtr { get;private set; } = null!;
    public MemoryPointer DummyIdPtr { get; private set;} = null!;
    public MemoryPointer RecordingSlotPtr  { get; private set;} = null!;
    public MemoryPointer P1ComboCountPtr  { get; private set;} = null!;
    public MemoryPointer P2ComboCountPtr  { get; private set;} = null!;
    public MemoryPointer P1ReplayKeyPtr  { get; private set;} = null!;
    public MemoryPointer P2ReplayKeyPtr  { get; private set;} = null!;
    public MemoryPointer P1BlockStunPtr  { get; private set;} = null!;
    public MemoryPointer P2BlockStunPtr  { get; private set;} = null!;
    
    private readonly Process _process;
    private readonly IMemoryReader _memoryReader;

    public MemoryPointerCollection(Process process, IMemoryReader memoryReader)
    {
        _process = process;
        _memoryReader = memoryReader;
        BuildCollection();
    }

    private void BuildCollection()
    {
        var textAddr = _process.MainModule!.BaseAddress + 0x1000;
        
        if (VirtualQueryEx(_process.Handle, textAddr, out var memoryBasicInformation64, (uint)Marshal.SizeOf(typeof(MemoryBasicInformation64))) == 0)
        {
            throw new Exception("Failed to retrieve information about .text allocation");
        }

        var text = _memoryReader.ReadBytes(textAddr, (int)memoryBasicInformation64.RegionSize);

        const string playerPatternNeedle = "i4Ew5iIAi4B8AwAAM9s7w3QPi4DIAQAAg+ABiUQkKOs=";
        var matchPtrAddr = _memoryReader.Read<int>(textAddr - 4 + FindPatternOffset(text, playerPatternNeedle));
        
        const int playerOffset = 0x169814;
        const int playerSize = 0x2d198;
        
        P1AnimStringPtr = new MemoryPointer(matchPtrAddr, playerOffset + 0x2444);
        P2AnimStringPtr = new MemoryPointer(matchPtrAddr, playerOffset + playerSize + 0x2444);
        P2ComboCountPtr = new MemoryPointer(matchPtrAddr, playerOffset + 0x9f28);
        P1ComboCountPtr = new MemoryPointer(matchPtrAddr, playerOffset + playerSize + 0x9f28);
        P1BlockStunPtr = new MemoryPointer(matchPtrAddr, playerOffset + 0x4d54);
        P2BlockStunPtr = new MemoryPointer(matchPtrAddr, playerOffset + playerSize + 0x4d54);


        const string recordingSlotPatternNeedle = "i1QkBGnSyBIAAIHC";
        RecordingSlotPtr = new MemoryPointer(_memoryReader.Read<int>(textAddr + 12 + FindPatternOffset(text, recordingSlotPatternNeedle)));

        const string frameCountPatternNeedle = "0o1U0AhBiYioAAAAxwIEAAAAiV8Mx0cUAwAAAF9eiR0=";
        FrameCountPtr = new MemoryPointer(_memoryReader.Read<int>(textAddr + 32 + FindPatternOffset(text, frameCountPatternNeedle)));

        const string dummyIdPatternNeedle = "VmYP1kZoajjHRgQ4AAAA6A==";
        DummyIdPtr = new MemoryPointer(_memoryReader.Read<int>(textAddr - 0x70 + FindPatternOffset(text, dummyIdPatternNeedle)) + 0x200);

        const string keyBindingPatternNeedle = "h0EBAACLRgiNPIDB5wSBxw==";
        var keyBindingRelAddr = _memoryReader.Read<int>(textAddr + 16 + FindPatternOffset(text, keyBindingPatternNeedle));
        
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

    private int FindPatternOffset(in byte[] haystack, in string needle) => FindPatternOffset(haystack, Convert.FromBase64String(needle));


    #region DLL Imports

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern int VirtualQueryEx(
        IntPtr hProcess,
        IntPtr lpBaseAddress,
        out MemoryBasicInformation64 lpBuffer,
        uint dwSize);

    [StructLayout(LayoutKind.Sequential)]
    private struct MemoryBasicInformation64 {
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