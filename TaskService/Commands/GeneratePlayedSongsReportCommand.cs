using System;
using System.Collections.Generic;
using System.Threading;

namespace TaskService.Commands
{
    public class GeneratePlayedSongsReportCommand : IJobCommand
    {
        Dictionary<string, object> IJobCommand.AdditionalOptions { get; set; } = new Dictionary<string, object>();
        CancellationToken IJobCommand.Token { get; set; }
        IProgress<ProgressReport> IJobCommand.Progress { get; set; }
        string IJobCommand.JobId { get; set; }
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime EndDate { get; set; } = DateTime.UtcNow;
    }
}