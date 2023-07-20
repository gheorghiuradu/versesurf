using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace TaskService.Commands
{
    public class ImportPlaylistsCommand : IJobCommand
    {
        Dictionary<string, object> IJobCommand.AdditionalOptions { get; set; } = new Dictionary<string, object>();
        CancellationToken IJobCommand.Token { get; set; }
        IProgress<ProgressReport> IJobCommand.Progress { get; set; }
        string IJobCommand.JobId { get; set; }
        public Stream ImportStream { get; set; }
    }
}