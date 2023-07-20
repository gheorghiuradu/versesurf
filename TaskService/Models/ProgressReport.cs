using System;
using TaskService.Jobs;

namespace TaskService
{
    public class ProgressReport
    {
        public string JobId { get; internal set; } = string.Empty;
        public string DisplayName { get; internal set; } = string.Empty;
        public int Value { get; internal set; }
        public string Message { get; internal set; } = string.Empty;
        public JobStatus Status { get; internal set; }
        public DateTime TimeStamp { get; } = DateTime.Now;
    }
}