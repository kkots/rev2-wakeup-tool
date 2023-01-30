﻿using GGXrdWakeupDPUtil.Library.Enums;
using GGXrdWakeupDPUtil.Library.Memory;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using GGXrdWakeupDPUtil.Library.Replay;
using GGXrdWakeupDPUtil.Library.Replay.AsmInjection;
using GGXrdWakeupDPUtil.Library.Replay.Keyboard;

namespace GGXrdWakeupDPUtil.Library
{
    [Obsolete]
    public class ReversalTool : IDisposable
    {
        private readonly string _ggProcName = ConfigurationManager.AppSettings.Get("GGProcessName");


        private readonly List<NameWakeupData> _nameWakeupDataList = new List<NameWakeupData>
        {
            new NameWakeupData("Sol", 25, 21),
            new NameWakeupData("Ky", 23, 21),
            new NameWakeupData("May", 25, 22),
            new NameWakeupData("Millia", 25, 23),
            new NameWakeupData("Zato", 25, 22),
            new NameWakeupData("Potemkin", 24, 22),
            new NameWakeupData("Chipp", 30, 24),
            new NameWakeupData("Faust", 25, 29),
            new NameWakeupData("Axl", 25, 21),
            new NameWakeupData("Venom", 21, 26),
            new NameWakeupData("Slayer", 26, 20),
            new NameWakeupData("I-No", 24, 20),
            new NameWakeupData("Bedman", 24, 30),
            new NameWakeupData("Ramlethal", 25, 23),
            new NameWakeupData("Sin", 30, 21),
            new NameWakeupData("Elphelt", 27, 27),
            new NameWakeupData("Leo", 28, 26),
            new NameWakeupData("Johnny", 25, 24),
            new NameWakeupData("Jack-O'", 25, 23),
            new NameWakeupData("Jam", 26, 25),
            new NameWakeupData("Haehyun", 22, 27),
            new NameWakeupData("Raven", 25, 24),
            new NameWakeupData("Dizzy", 25, 24),
            new NameWakeupData("Baiken", 25, 21),
            new NameWakeupData("Answer", 25, 25)
        };

        private const int WallSplatWakeupTiming = 15;


        #region Offsets
        private readonly MemoryPointer _recordingSlotPtr = new MemoryPointer("RecordingSlotPtr");
        private readonly MemoryPointer _p1AnimStringPtr = new MemoryPointer("P1AnimStringPtr");
        private readonly MemoryPointer _p2AnimStringPtr = new MemoryPointer("P2AnimStringPtr");
        private readonly MemoryPointer _frameCountPtr = new MemoryPointer("FrameCountOffset");
        private readonly MemoryPointer _p1ComboCountPtr = new MemoryPointer("P1ComboCountPtr");
        private readonly MemoryPointer _p2ComboCountPtr = new MemoryPointer("P2ComboCountPtr");
        private readonly MemoryPointer _dummyIdPtr = new MemoryPointer("DummyIdPtr");
        private readonly MemoryPointer _replayKeyOffset = new MemoryPointer("ReplayKeyOffset");
        private readonly MemoryPointer _p2BlockStunPtr = new MemoryPointer("P2BlockStunPtr");
        
        #endregion

        private const string FaceDownAnimation = "CmnActFDown2Stand";
        private const string FaceUpAnimation = "CmnActBDown2Stand";
        private const string WallSplatAnimation = "CmnActWallHaritsukiGetUp";
        private const string TechAnimation = "CmnActUkemi";

        private const int RecordingSlotSize = 4808;

        private Process _process;

        private MemoryReader _memoryReader;
        private ReplayTrigger _replayTrigger;
        
        public ReplayTriggerTypes ReplayTriggerType
        {
            get
            {
                if (!Enum.TryParse(ConfigurationManager.AppSettings.Get("ReplayTriggerType"), false, out ReplayTriggerTypes replayTriggerType))
                {
                    replayTriggerType = ReplayTriggerTypes.AsmInjection;
                }

                return replayTriggerType;
            }
            private set => ConfigManager.Set("ReplayTriggerType", value.ToString());
        }

        #region Reversal Loop
        private static bool _runReversalThread;
        private static readonly object RunReversalThreadLock = new object();
        public delegate void ReversalLoopErrorHandler(Exception ex);

        public event ReversalLoopErrorHandler ReversalLoopErrorOccured;
        #endregion

        #region Dummy Loop
        private static bool _runDummyThread;
        private static readonly object RunDummyThreadLock = new object();

        public delegate void DummyChangedHandler(NameWakeupData dummy);

        public event DummyChangedHandler DummyChanged;


        public delegate void DummyLoopErrorHandler(Exception ex);

        public event DummyLoopErrorHandler DummyLoopErrorOccured;
        #endregion

        #region Random Burst Loop
        private static bool _runRandomBurstThread;
        private static readonly object RunRandomBurstThreadLock = new object();
        public delegate void RandomBurstLoopErrorHandler(Exception ex);
        public event RandomBurstLoopErrorHandler RandomBurstLoopErrorOccured;
        #endregion

        #region Block Reversal Loop
        private static bool _runBlockReversalThread;
        private static readonly object RunBlockReversalThreadLock = new object();
        public delegate void BlockReversalLoopErrorHandler(Exception ex);

        public event BlockReversalLoopErrorHandler BlockReversalLoopErrorOccured;
        #endregion

        #region Tech Reversal loop

        private static bool _runTechReversalThread;
        private static readonly object RunTechReversalThreadLock = new object();

        public delegate void TechReversalLoopErrorHandler(Exception ex);

        public event TechReversalLoopErrorHandler TechReversalLoopErrorOccured;
        #endregion


        #region Dll Imports
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        #endregion


        #region Replay Trigger
        private ReplayTrigger GetReplayTrigger()
        {
            ReplayTriggerTypes replayTriggerType = this.ReplayTriggerType;

            ReplayTriggerFactory factory;
            
            if (replayTriggerType == ReplayTriggerTypes.Keystroke)
            {
                Keyboard.DirectXKeyStrokes keyStroke = this.GetReplayKeyStroke();
                factory = new KeyboardTriggerFactory(keyStroke);
            }
            else
            {
                factory = new AsmInjectionTriggerFactory(this._process, this._memoryReader);
            }

            return factory.GetReplayTrigger();
        }

        public void ChangeReplayTrigger(ReplayTriggerTypes replayTriggerType)
        {
            if (replayTriggerType != this.ReplayTriggerType)
            {
                this.ReplayTriggerType = replayTriggerType;

                this._replayTrigger?.Dispose();

                this._replayTrigger = GetReplayTrigger();

                this._replayTrigger.InitTrigger();
            }
        }
        #endregion

        public void AttachToProcess()
        {
            var process = Process.GetProcessesByName(_ggProcName).FirstOrDefault();

            this._process = process ?? throw new Exception("GG process not found!");

            this._memoryReader = new MemoryReader(process);


            this._replayTrigger = GetReplayTrigger();

            this._replayTrigger.InitTrigger();

            StartDummyLoop();
        }



        public void BringWindowToFront()
        {
            IntPtr handle = GetForegroundWindow();
            if (this._process != null && this._process.MainWindowHandle != handle)
            {
                SetForegroundWindow(this._process.MainWindowHandle);
            }
        }

        public NameWakeupData GetDummy()
        {
            var index = _memoryReader.Read<int>(_dummyIdPtr);
            var result = _nameWakeupDataList[index];

            return result;

        }

        public bool SetInputInSlot(int slotNumber, SlotInput slotInput)
        {
            var baseAddress = this._memoryReader.GetAddressWithOffsets(_recordingSlotPtr.Pointer, _recordingSlotPtr.Offsets.ToArray());
            var slotAddress = IntPtr.Add(baseAddress, RecordingSlotSize * (slotNumber - 1));

            return this._memoryReader.Write(slotAddress, slotInput.Content);
        }
        public byte[] ReadInputInSlot(int slotNumber)
        {
            var baseAddress = this._memoryReader.GetAddressWithOffsets(_recordingSlotPtr.Pointer, _recordingSlotPtr.Offsets.ToArray());
            var slotAddress = IntPtr.Add(baseAddress, RecordingSlotSize * (slotNumber - 1));

            var readBytes = this._memoryReader.ReadBytes(slotAddress, RecordingSlotSize);

            var inputLength = Byte.MaxValue * readBytes[5] + readBytes[4];

            var headerLength = 4;

            var length = 2 * (inputLength + headerLength);


            byte[] result = new byte[length];
            Array.Copy(readBytes, result, 2 * (inputLength + headerLength));

            return result;
        }

        public bool WriteInputFile(string filePath, byte[] input)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.Write(input.Select(x => x.ToString("X")).Aggregate((a, b) => $"{a},{b}")
                    );
                }
            }
            catch (Exception e)
            {
                LogManager.Instance.WriteException(e);

                return false;
            }

            return true;
        }
        public byte[] ReadInputFile(string filePath)
        {
            try
            {
                string text;
                var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                {
                    text = streamReader.ReadToEnd().Trim();
                }

                var result = text.Split(',').Select(x => Convert.ToByte(x, 16)).ToArray();
                return result;
            }
            catch (Exception e)
            {
                LogManager.Instance.WriteException(e);
                return Array.Empty<byte>();
            }

        }

        public string TranslateFromFile(string filePath)
        {
            try
            {
                byte[] inputs = this.ReadInputFile(filePath);

                List<string> values = new List<string>();

                bool isP2 = inputs[0] == 1;

                for (int i = 9; i < inputs.Length; i += 2)
                {
                    ushort item = (ushort)((byte.MaxValue + 1) * inputs[i] + inputs[i - 1]);

                    string value = this.SingleInputParse(item, isP2);

                    values.Add(value);

                }

                return values.Aggregate((a, b) => $"{a},{b}");

            }
            catch (Exception e)
            {
                LogManager.Instance.WriteException(e);
                return string.Empty;
            }
        }

        public bool TranslateIntoFile(string filePath, string input)
        {
            try
            {
                var slotInput = new SlotInput(input);

                return this.WriteInputFile(filePath, slotInput.CleanInputs);
            }
            catch (Exception e)
            {
                LogManager.Instance.WriteException(e);
                return false;
            }
        }

        public void PlayReversal()
        {
            LogManager.Instance.WriteLine("Reversal!");

            BringWindowToFront();

            this._replayTrigger.TriggerReplay();

            LogManager.Instance.WriteLine("Reversal Done!");
        }

        public void StartWakeupReversalLoop(SlotInput slotInput, int wakeupReversalPercentage, bool playReversalOnWallSplat)
        {
            lock (RunReversalThreadLock)
            {
                _runReversalThread = true;
            }

            Thread reversalThread = new Thread(() =>
            {
                LogManager.Instance.WriteLine("Reversal Thread start");
                var currentDummy = GetDummy();
                bool localRunReversalThread = true;

                Random rnd = new Random();

                bool willReversal = rnd.Next(0, 101) <= wakeupReversalPercentage;

                while (localRunReversalThread && !this._process.HasExited)
                {
                    try
                    {
                        int wakeupTiming = GetWakeupTiming(currentDummy, playReversalOnWallSplat);


                        if (wakeupTiming != 0)
                        {
                            int fc = FrameCount();
                            var frames = wakeupTiming - slotInput.ReversalFrameIndex - 1;
                            while (FrameCount() < fc + frames)
                            {
                            }

                            if (willReversal)
                            {
                                PlayReversal();
                            }
                            willReversal = rnd.Next(0, 101) <= wakeupReversalPercentage;


                            Thread.Sleep(1); // ~1 frame
                            //Thread.Sleep(320); //20 frames, approximately, it's actually 333.333333333 ms.  Nobody should be able to be knocked down and get up in this time, causing the code to execute again.
                        }
                    }
                    catch (Exception ex)
                    {
                        LogManager.Instance.WriteException(ex);
                        StopReversalLoop();
                        ReversalLoopErrorOccured?.Invoke(ex);
                        return;
                    }

                    lock (RunReversalThreadLock)
                    {
                        localRunReversalThread = _runReversalThread;
                    }

                    Thread.Sleep(1);
                }


                LogManager.Instance.WriteLine("Reversal Thread ended");

            })
            { Name = "reversalThread" };

            reversalThread.Start();

            this.BringWindowToFront();
        }

        public void StopReversalLoop()
        {
            lock (RunReversalThreadLock)
            {
                _runReversalThread = false;
            }
        }

        public void StartRandomBurstLoop(int min, int max, int replaySlot, int burstPercentage)
        {
            lock (RunRandomBurstThreadLock)
            {
                _runRandomBurstThread = true;
            }

            Thread randomBurstThread = new Thread(() =>
            {
                LogManager.Instance.WriteLine("RandomBurst Thread start");
                bool localRunRandomBurstThread = true;

                var slotInput = new SlotInput("!5HD");
                SetInputInSlot(replaySlot, slotInput);

                Random rnd = new Random();

                int valueToBurst = rnd.Next(min, max + 1);
                bool willBurst = rnd.Next(0, 101) <= burstPercentage;


                while (localRunRandomBurstThread && !this._process.HasExited)
                {
                    try
                    {
                        int currentCombo = GetCurrentComboCount(1);



                        while (currentCombo > 0)
                        {
                            if (currentCombo == valueToBurst && willBurst)
                            {
                                PlayReversal();
                                Thread.Sleep(850); //50 frames, approximately, Burst recovery is around 50f. 
                            }

                            currentCombo = GetCurrentComboCount(1);

                            if (currentCombo == 0)
                            {
                                valueToBurst = rnd.Next(min, max + 1);
                                willBurst = rnd.Next(0, 101) <= burstPercentage;
                            }
                            Thread.Sleep(1);
                        }


                        lock (RunRandomBurstThreadLock)
                        {
                            localRunRandomBurstThread = _runRandomBurstThread;
                        }
                        Thread.Sleep(1);
                    }
                    catch (Exception ex)
                    {
                        LogManager.Instance.WriteException(ex);
                        StopRandomBurstLoop();
                        RandomBurstLoopErrorOccured?.Invoke(ex);
                        return;
                    }

                }

                LogManager.Instance.WriteLine("RandomBurst Thread ended");

            })
            {
                Name = "randomBurstThread"
            };

            randomBurstThread.Start();

            this.BringWindowToFront();
        }

        public void StopRandomBurstLoop()
        {
            lock (RunRandomBurstThreadLock)
            {
                _runRandomBurstThread = false;
            }
        }


        public void StartBlockReversalLoop(SlotInput slotInput, int blockstunReversalPercentage, int blockstunReversalDelay)
        {
            lock (RunBlockReversalThreadLock)
            {
                _runBlockReversalThread = true;
            }

            Thread blockReversalThread = new Thread(() =>
                {
                    LogManager.Instance.WriteLine("Block Reversal Thread start");

                    bool localRunBlockReversalThread = true;

                    Random rnd = new Random();

                    bool willReversal = rnd.Next(0, 101) <= blockstunReversalPercentage;

                    int oldBlockstun = 0;

                    while (localRunBlockReversalThread && !this._process.HasExited)
                    {
                        try
                        {
                            int blockStun = this.GetBlockstun(2);

                            if (slotInput.ReversalFrameIndex + 2 == blockStun && oldBlockstun != blockStun)
                            {
                                if (willReversal)
                                {
                                    this.Wait(blockstunReversalDelay);

                                    this.PlayReversal();
                                }

                                willReversal = rnd.Next(0, 101) <= blockstunReversalPercentage;

                                Thread.Sleep(32);
                            }

                            oldBlockstun = blockStun;

                            Thread.Sleep(10); //check about twice by frame
                        }
                        catch (Exception ex)
                        {
                            LogManager.Instance.WriteException(ex);
                            StopBlockReversalLoop();
                            BlockReversalLoopErrorOccured?.Invoke(ex);
                            return;
                        }

                        lock (RunBlockReversalThreadLock)
                        {
                            localRunBlockReversalThread = _runBlockReversalThread;
                        }

                        Thread.Sleep(1);
                    }


                    LogManager.Instance.WriteLine("Block Reversal Thread ended");

                })
            { Name = "blockReversalThread" };

            blockReversalThread.Start();

            this.BringWindowToFront();
        }

        public void StopBlockReversalLoop()
        {
            lock (RunBlockReversalThreadLock)
            {
                _runBlockReversalThread = false;
            }
        }
        
        public void StartTechReversalLoop(SlotInput slotInput, int percentage, int delay)
        {
            lock (RunTechReversalThreadLock)
            {
                _runTechReversalThread = true;
            }

            Thread techReversalThread = new Thread(() =>
                {
                    LogManager.Instance.WriteLine("Tech Reversal Thread start");

                    bool localRunTechReversalThread = true;

                    Random rnd = new Random();
                    
                    bool willReversal = rnd.Next(0, 101) <= percentage;

                    var oldAnimation = this.ReadAnimationString(2);

                    while (localRunTechReversalThread && !this._process.HasExited)
                    {


                        try
                        {
                            var animation = this.ReadAnimationString(2);

                            if (animation == TechAnimation && oldAnimation != animation)
                            {
                                if (willReversal)
                                {
                                    this.Wait(6); // tech recovery
                                    this.Wait(Math.Max(0, slotInput.ReversalFrameIndex));
                                    this.Wait(delay);

                                    this.PlayReversal();
                                }

                                willReversal = rnd.Next(0, 101) <= percentage;

                                Thread.Sleep(32);
                            }

                            oldAnimation = animation;
                            
                            Thread.Sleep(10); //check about twice by frame
                        }
                        catch (Exception ex)
                        {
                            LogManager.Instance.WriteException(ex);
                            StopTechReversalLoop();
                            TechReversalLoopErrorOccured?.Invoke(ex);
                            return;
                        }




                        lock (RunTechReversalThreadLock)
                        {
                            localRunTechReversalThread = _runTechReversalThread;
                        }
                        
                        Thread.Sleep(1);
                    }
                    
                    
                    
                    LogManager.Instance.WriteLine("Block Reversal Thread ended");
                })
                { Name = "techReversalThread" };
            
            techReversalThread.Start();
            
            this.BringWindowToFront();
        }
        
        public void StopTechReversalLoop()
        {
            lock (RunTechReversalThreadLock)
            {
                _runTechReversalThread = false;
            }
        }


        #region Private

        private string SingleInputParse(ushort input, bool isP2 = false)
        {

            string result = string.Empty;


            //direction
            if (IsDirectionPressed(input, Directions.Dir2) && IsDirectionPressed(input, Directions.Dir4))
            {
                result += !isP2 ? "1" : "3";
            }
            else if (IsDirectionPressed(input, Directions.Dir2) && IsDirectionPressed(input, Directions.Dir6))
            {
                result += !isP2 ? "3" : "1";
            }
            else if (IsDirectionPressed(input, Directions.Dir4) && IsDirectionPressed(input, Directions.Dir8))
            {
                result += !isP2 ? "7" : "9";
            }
            else if (IsDirectionPressed(input, Directions.Dir8) && IsDirectionPressed(input, Directions.Dir6))
            {
                result += !isP2 ? "9" : "7";
            }
            else if (IsDirectionPressed(input, Directions.Dir2))
            {
                result += "2";
            }
            else if (IsDirectionPressed(input, Directions.Dir6))
            {
                result += !isP2 ? "6" : "4";
            }
            else if (IsDirectionPressed(input, Directions.Dir4))
            {
                result += !isP2 ? "4" : "6";
            }
            else if (IsDirectionPressed(input, Directions.Dir8))
            {
                result += "8";
            }
            else
            {
                result += "5";
            }


            //button
            foreach (Buttons button in Enum.GetValues(typeof(Buttons)))
            {
                if (IsButtonPressed(input, button))
                {
                    result += button.ToString();
                }
            }

            return result;

        }

        private bool IsButtonPressed(ushort input, Buttons button)
        {
            return (input & (int)button) == (int)button;
        }
        private bool IsDirectionPressed(ushort input, Directions direction)
        {
            return (input & (int)direction) == (int)direction;
        }

        private string ReadAnimationString(int player)
        {
            const int length = 32;

            switch (player)
            {
                case 1:
                    return _memoryReader.ReadString(_p1AnimStringPtr, length);
                case 2:
                    return _memoryReader.ReadString(_p2AnimStringPtr, length);
                default:
                    return string.Empty;
            }
        }

        private int GetBlockstun(int player)
        {
            if (player == 2)
            {
                return this._memoryReader.Read<int>(_p2BlockStunPtr);
            }

            throw new NotImplementedException();
        }

        private int FrameCount()
        {
            return _memoryReader.Read<int>(_frameCountPtr);
        }
        private int GetWakeupTiming(NameWakeupData currentDummy, bool playReversalOnWallSplat)
        {
            var animationString = ReadAnimationString(2);

            if (animationString == FaceDownAnimation)
            {
                return currentDummy.FaceDownFrames;
            }
            if (animationString == FaceUpAnimation)
            {
                return currentDummy.FaceUpFrames;
            }
            if (animationString == WallSplatAnimation && playReversalOnWallSplat)
            {
                //wakeup timing is universal on wall splat recovery
                return WallSplatWakeupTiming;
            }

            return 0;

        }

        private int GetCurrentComboCount(int player)
        {
            //TODO find the pointer for player 2

            switch (player)
            {
                case 1:
                    return _memoryReader.Read<int>(_p1ComboCountPtr);
                case 2:
                    return _memoryReader.Read<int>(_p2ComboCountPtr);
                default:
                    throw new NotImplementedException();
            }
        }

        private int GetReplayKey()
        {
            var result = this._memoryReader.Read<int>(_replayKeyOffset);
            
            return result;
        }

        //TODO Remove?
        public Keyboard.DirectXKeyStrokes GetReplayKeyStroke()
        {
            int replayKeyCode = this.GetReplayKey();
            char replayKey = (char)replayKeyCode;

            if (!Enum.TryParse($"DIK_{replayKey}", out Keyboard.DirectXKeyStrokes stroke))
            {
                stroke = Keyboard.DirectXKeyStrokes.DIK_P;
            }

            return stroke;
        }


        private void Wait(int frames)
        {
            if (frames > 0)
            {
                int startFrame = this.FrameCount();
                int frameCount = 0;

                while (frameCount < frames)
                {
                    Thread.Sleep(10);

                    frameCount = this.FrameCount() - startFrame;
                }
            }
        }


        private void StartDummyLoop()
        {
            lock (RunDummyThreadLock)
            {
                _runDummyThread = true;
            }

            Thread dummyThread = new Thread(() =>
                {
                    LogManager.Instance.WriteLine("dummyThread start");
                    NameWakeupData currentDummy = null;
                    bool localRunDummyThread = true;

                    while (localRunDummyThread && !this._process.HasExited)
                    {
                        try
                        {
                            var dummy = GetDummy();

                            if (!Equals(dummy, currentDummy))
                            {
                                currentDummy = dummy;

                                DummyChanged?.Invoke(dummy);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogManager.Instance.WriteException(ex);
                            StopDummyLoop();
                            DummyLoopErrorOccured?.Invoke(ex);
                            return;
                        }

                        lock (RunDummyThreadLock)
                        {
                            localRunDummyThread = _runDummyThread;
                        }

                        Thread.Sleep(2000);
                    }

                    LogManager.Instance.WriteLine("dummyThread ended");
                })
            { Name = "dummyThread" };

            dummyThread.Start();
        }

        public void StopDummyLoop()
        {
            lock (RunDummyThreadLock)
            {
                _runDummyThread = false;
            }
        }

        #endregion

        #region Dispose Members
        public void Dispose()
        {
            StopDummyLoop();
            StopReversalLoop();
            StopBlockReversalLoop();
        }
        #endregion


        
    }
}
