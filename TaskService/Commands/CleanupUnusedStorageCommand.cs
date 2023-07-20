using System;
using System.Collections.Generic;
using System.Threading;

namespace TaskService.Commands
{
    public class CleanupUnusedStorageCommand : IJobCommand
    {
        Dictionary<string, object> IJobCommand.AdditionalOptions { get; set; } = new Dictionary<string, object>();
        CancellationToken IJobCommand.Token { get; set; }
        IProgress<ProgressReport> IJobCommand.Progress { get; set; }
        string IJobCommand.JobId { get; set; }
    }
}