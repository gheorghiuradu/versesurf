using System;
using System.Collections.Generic;
using System.Threading;

namespace TaskService
{
    public interface IJobCommand
    {
        internal Dictionary<string, object> AdditionalOptions { get; set; }
        internal CancellationToken Token { get; set; }
        internal IProgress<ProgressReport> Progress { get; set; }
        internal string JobId { get; set; }
    }
}