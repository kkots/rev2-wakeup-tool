﻿using System.Runtime.InteropServices;
using GGXrdReversalTool.Library.Configuration;
using GGXrdReversalTool.Library.Input;
using GGXrdReversalTool.Library.Logging;
using GGXrdReversalTool.Library.Memory;
using GGXrdReversalTool.Library.Models;
using GGXrdReversalTool.Library.Models.Inputs;

namespace GGXrdReversalTool.Library.Scenarios.Action.Implementations;

public class PlayReversalAction : IScenarioAction
{
    public IMemoryReader? MemoryReader { get; set; }
    public SlotInput[] Inputs { get; set; } = {};
    public bool IsRunning { get; private set; }
    public bool[] GuaranteeChargeInputArray { get; set; } = { false, false, false };

    public void Init(int slotNumber)
    {
        if (MemoryReader == null)
        {
            return;
        }

        MemoryReader.WriteInputInSlot(SlotNumber, Inputs[slotNumber - 1]);
    }
    public int SlotNumber { get; set; } = 1;

    public void Execute(int slotNumberGame, int slotNumberTool)
    {
        if (MemoryReader == null)
        {
            return;
        }
        LogManager.Instance.WriteLine("PlayReversalAction Execute!");
        int playerSide = MemoryReader.GetPlayerSide();
        int dummySide = 1 - playerSide;
        if (GuaranteeChargeInputArray[slotNumberTool - 1])
        {
            MemoryReader.GuaranteeChargeInput(dummySide);
        }
        MemoryReader.SetDummyPlayback(slotNumberGame, 0, MemoryReader.GetFacing(dummySide));
        IsRunning = true;
    }

    public void Tick()
    {
        if (MemoryReader == null)
        {
            return;
        }
        var dummySide = 1 - MemoryReader.GetPlayerSide();
        var dummyMode = MemoryReader.GetDummyMode();
        if (dummyMode is < 2 or > 3) // neither playing nor recording
        {
            int slotSetting = MemoryReader.GetTrainingRecordingSlot();
            if (slotSetting is >= 0 and <= 2)
            {
                MemoryReader.SetDummyRecordingSlot(slotSetting + 1);
            }
            IsRunning = false;
        }
    }
}
