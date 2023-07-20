using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using TaskService.Commands;
using TaskService.Jobs;

namespace TaskService
{
    public class JobService : IDisposable
    {
        private readonly IBackgroundTaskQueue taskQueue;
        private readonly ILogger logger;
        private readonly ConcurrentDictionary<JobStartResult, Job> jobs = new ConcurrentDictionary<JobStartResult, Job>();
        private readonly SemaphoreSlim signal = new SemaphoreSlim(1);
        private readonly IServiceProvider serviceProvider;

        public Subject<ProgressReport> ProgressUpdates { get; private set; }

        public JobService(IBackgroundTaskQueue taskQueue, ILogger<JobService> logger, IHostApplicationLifetime hostApplicationLifetime, IServiceProvider serviceProvider)
        {
            this.ProgressUpdates = new Subject<ProgressReport>();
            this.taskQueue = taskQueue;
            this.logger = logger;
            this.serviceProvider = serviceProvider;

            hostApplicationLifetime.ApplicationStopping.Register(this.CancelAllTasks);
            this.StartMonitor();
        }

        private void StartMonitor()
        {
            new Timer(_ =>
            {
                // If anything stored
                if (this.GetLatestReports().Any()
                // If no jobs running
                && !this.GetLatestReports().Any(r => r.Status == JobStatus.Running)
                // If latest report is older than an hour
                && this.GetLatestReports().OrderBy(r => r.TimeStamp).FirstOrDefault()?.TimeStamp > DateTime.Now.AddHours(1)
                // If we have no observers subscribed
                && !this.ProgressUpdates.HasObservers)
                {
                    // Cleanup
                    this.jobs.Clear();
                    this.ProgressUpdates.Dispose();
                    this.ProgressUpdates = new Subject<ProgressReport>();
                }
            }, new object(), TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
        }

        private void CancelAllTasks()
        {
            // application is stopping, cancel all tasks
            logger.LogInformation("Application stop requested, stopping all jobs");
            foreach (var startResult in this.jobs.Keys)
            {
                logger.LogInformation($"Stoping job {startResult.JobId}");
                startResult.CancellationToken.Cancel();
            }
        }

        public async Task<JobStartResult> StartNewAsync(IJobCommand command)
        {
            logger.LogDebug("Received job command, waiting for signal");
            await this.signal.WaitAsync();

            try
            {
                // Prepare job
                var cts = new CancellationTokenSource();
                command.JobId = Guid.NewGuid().ToString();
                command.Token = cts.Token;
                command.Progress = new Progress<ProgressReport>(this.ReportProgress);
                var job = this.GetJobForCommand(command);
                var startResult = new JobStartResult
                {
                    JobId = command.JobId,
                    CancellationToken = cts
                };
                this.jobs.TryAdd(startResult, job);

                logger.LogDebug($"Adding {job.GetType().Name} with id {startResult.JobId} to background queue");
                taskQueue.QueueBackgroundWorkItem((cts.Token, () => job.RunAsync(command)));
                return startResult;
            }
            finally
            {
                this.signal.Release();
            }
        }

        public T TryGetResult<T>(string jobId)
        {
            var jobStartResult = this.jobs.Keys.FirstOrDefault(k => string.Equals(k.JobId, jobId));
            if (jobStartResult is null)
            {
                return default;
            }

            var job = this.jobs[jobStartResult];
            if (job.LastReport.Status == JobStatus.Completed && job is Job<T> resultJob)
            {
                return resultJob.Result;
            }

            return default;
        }

        public void CancelJob(string jobId)
        {
            var jobStart = this.jobs.Keys.FirstOrDefault(jsr => string.Equals(jsr.JobId, jobId));
            if (!(jobStart is null))
            {
                jobStart.CancellationToken.Cancel();
            }
        }

        public IEnumerable<ProgressReport> GetLatestReports() => this.jobs.Values.Select(j => j.LastReport);

        public void Dispose()
        {
            this.ProgressUpdates.Dispose();
            this.signal.Dispose();
        }

        private void ReportProgress(ProgressReport report)
        {
            this.ProgressUpdates.OnNext(report);
        }

        private Job GetJobForCommand(IJobCommand command)
        {
            return command switch
            {
                ImportSpotifyPlaylistCommand _ => this.serviceProvider.GetRequiredService<ImportSpotifyPlaylistJob>(),
                CleanupUnusedStorageCommand _ => this.serviceProvider.GetRequiredService<CleanupUnusedStorageJob>(),
                ReplaceGSPathCommand _ => this.serviceProvider.GetRequiredService<ReplaceGSPathJob>(),
                GeneratePlayedSongsReportCommand _ => this.serviceProvider.GetRequiredService<GeneratePlayedSongsReportJob>(),
                ExportPlaylistsCommand _ => this.serviceProvider.GetRequiredService<ExportPlaylistsJob>(),
                ImportPlaylistsCommand _ => this.serviceProvider.GetRequiredService<ImportPlaylistsJob>(),
                _ => throw new NotImplementedException("This job type is not implemented"),
            };
        }
    }
}