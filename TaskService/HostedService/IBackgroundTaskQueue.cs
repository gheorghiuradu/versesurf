using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaskService
{
    public interface IBackgroundTaskQueue
    {
        void QueueBackgroundWorkItem((CancellationToken token, Func<Task> task) workItem);

        Task<(CancellationToken token, Func<Task> task)> DequeueAsync(
            CancellationToken cancellationToken);
    }
}