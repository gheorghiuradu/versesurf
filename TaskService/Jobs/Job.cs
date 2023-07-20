using System;
using System.Threading.Tasks;

namespace TaskService.Jobs
{
    public abstract class Job<TResult> : Job
    {
        public TResult Result { get; protected set; }

        protected void ReportCompleted(TResult result, string message = null)
        {
            this.Result = result;
            base.ReportCompleted(message);
        }
    }

    public abstract class Job
    {
        protected IProgress<ProgressReport> progress;
        public ProgressReport LastReport { get; } = new ProgressReport { Status = JobStatus.NotStarted };

        public abstract Task RunAsync(IJobCommand command);

        protected void ReportStarted(string jobId, string message = null, string displayName = null)
        {
            this.LastReport.JobId = jobId;
            this.LastReport.DisplayName = displayName ?? this.GetType().Name;
            this.LastReport.Value = 0;
            if (this.LastReport.Status == JobStatus.NotStarted)
            {
                this.LastReport.Status = JobStatus.Running;
            }
            if (!string.IsNullOrWhiteSpace(message))
            {
                this.LastReport.Message = message;
            }

            this.progress.Report(this.LastReport);
        }

        protected void ReportProgress(int value, string message = null)
        {
            if (this.LastReport.Status == JobStatus.NotStarted)
            {
                this.LastReport.Status = JobStatus.Running;
            }
            this.LastReport.Value = value;
            this.LastReport.Message = message;

            this.progress.Report(this.LastReport);
        }

        protected void ReportProgress(string message)
        {
            if (this.LastReport.Status == JobStatus.NotStarted)
            {
                this.LastReport.Status = JobStatus.Running;
            }
            this.LastReport.Message = message;

            this.progress.Report(this.LastReport);
        }

        protected void ReportFail(string message)
        {
            this.LastReport.Status = JobStatus.Failed;
            this.LastReport.Message = message;

            this.progress.Report(this.LastReport);
        }

        protected void ReportCancel()
        {
            this.LastReport.Status = JobStatus.Canceled;
            this.LastReport.Message = "Job was canceled.";

            this.progress.Report(this.LastReport);
        }

        protected void ReportCompleted(string message = null)
        {
            this.LastReport.Status = JobStatus.Completed;
            this.LastReport.Message = message;
            this.LastReport.Value = 100;

            this.progress.Report(this.LastReport);
            GC.Collect();
        }
    }

    public enum JobStatus
    {
        NotStarted,
        Running,
        Failed,
        Canceled,
        Completed
    }
}