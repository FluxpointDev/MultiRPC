using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MultiRPC.Functions
{
    interface IUpdate
    {
        bool IsChecking { get; }
        bool IsUpdating { get; }
        bool BeenUpdated { get; }
        string NewVersion { get; }

        Task Check(bool showNoUpdateMessage = false);

        Task Update();
    }
}
