using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MultiRPC.Functions
{
    class SimpleUpdater : IUpdate
    {
        public bool IsChecking { get; private set; }
        public bool IsUpdating { get; private set; }
        public bool BeenUpdated { get; private set; }
        public string NewVersion { get; private set; }

        public Task Check(bool showNoUpdateMessage = false)
        {
            return Task.CompletedTask;
        }

        public Task Update()
        {
            return Task.CompletedTask;
        }
    }
}
