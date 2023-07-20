using System.Threading;

namespace TaskService
{
    public class JobStartResult
    {
        public string JobId { get; set; }
        public CancellationTokenSource CancellationToken { get; set; }
    }
}