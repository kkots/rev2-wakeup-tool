using System.Diagnostics;
using GGXrdReversalTool.Library.Characters;
using GGXrdReversalTool.Library.Memory.Pointer;
using GGXrdReversalTool.Library.Models;
using GGXrdReversalTool.Library.Models.Inputs;

namespace GGXrdReversalTool.Library.Memory;

public interface IMemoryReader
{
    string ReadAnimationString(int player);
    int FrameCount();
    Character GetCurrentDummy();
    bool SetDummyPlayback(int slotNumber, int inputIndex, int startingSide);
    bool SetDummyRecordingSlot(int slotNumber);
    int GetDummyMode();
    int GetTrainingRecordingSlot();
    bool WriteInputInSlot(int slotNumber, SlotInput slotInput);
    int GetComboCount(int player);
    int GetBlockstun(int player);
    int GetHitstop(int player);
    int GetFacing(int player);
    int GetAnimFrame(int player);
    int GetSlowdownFrames(int player);
    int GetSuperflashFreezeFrames(int player);
    int GetPlayerSide();
    bool IsTrainingMode();
    public bool IsWorldInTick();
    public uint GetEngineTickCount();

    Process Process { get; }
    SlotInput ReadInputFromSlot(int slotNumber);
    void LockDummy(int player, out uint oldFlags);
    void UnlockDummy(int player, uint oldFlags);
    int GetTimeUntilTech(int player);
    int GetYPos(int player);
    /// <summary>
    /// Guarantees charge input for the dummy in direction opposite from the player.
    /// So for ex. if player is on the left of the dummy the dummy would have charged
    /// (held) right.
    /// </summary>
    /// <param name="dummy">The dummy create charge for</param>
    /// <param name="player">The player away from whom to charge</param>
    /// <returns>true on success, false on failure</returns>
    bool GuaranteeChargeInput(int dummy, int player);
}
