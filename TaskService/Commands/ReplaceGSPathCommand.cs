using System;
using System.Collections.Generic;
using System.Threading;

namespace TaskService.Commands
{
    public class ReplaceGSPathCommand : IJobCommand
    {
        Dictionary<string, object> IJobCommand.AdditionalOptions { get; set; } = new Dictionary<string, object>();
        CancellationToken IJobCommand.Token { get; set; }
        IProgress<ProgressReport> IJobCommand.Progress { get; set; }
        string IJobCommand.JobId { get; set; }

        public string PlaylistImageSource { get; set; } = string.Empty;
        public string PlaylistImageDestination { get; set; } = string.Empty;
        public string SongPreviewSource { get; set; } = string.Empty;
        public string SongPreviewDestination { get; set; } = string.Empty;
        public bool RemoveQueryParametersPlaylistImage { get; set; }
        public bool RemoveQueryParametersSongPreview { get; set; }
    }
}