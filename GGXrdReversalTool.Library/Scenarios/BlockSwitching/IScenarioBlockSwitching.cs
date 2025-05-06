using GGXrdReversalTool.Library.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GGXrdReversalTool.Library.Scenarios.BlockSwitching
{
    public interface IScenarioBlockSwitching
    {
        IMemoryReader? MemoryReader { get; internal set; }
        void Init();
        void Tick(bool isUserControllingDummy);
        bool IsHitReversal();
        void OnStageReset();
    }
}
