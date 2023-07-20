using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;

namespace TaskService
{
    public class ImportSpotifyPlaylistCommand : IJobCommand
    {
        Dictionary<string, object> IJobCommand.AdditionalOptions { get; set; } = new Dictionary<string, object>();
        CancellationToken IJobCommand.Token { get; set; }
        IProgress<ProgressReport> IJobCommand.Progress { get; set; }
        string IJobCommand.JobId { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please fill out a valid spotify id")]
        public string SpotifyId { get; set; }

        public bool EnableBmi { get; set; } = true;
        public bool EnableAscap { get; set; } = true;
        public bool EnableSesac { get; set; } = true;

        [Required]
        [Range(10, int.MaxValue)]
        public int MinimumNumberOfSongs { get; set; } = 10;
    }
}