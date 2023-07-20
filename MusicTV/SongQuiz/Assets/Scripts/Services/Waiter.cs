using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Services
{
    public class Waiter
    {
        private readonly float waitSeconds;

        public Waiter(float waitSeconds)
        {
            this.waitSeconds = waitSeconds;
        }

        public void WithRetry(Action action)
        {
            var successful = false;
            while (!successful)
            {
                try
                {
                    action();
                    successful = true;
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    System.Threading.Thread.Sleep(TimeSpan.FromSeconds(waitSeconds));
                }
            }
        }

        public bool WithRetry(Action action, int retryCount)
        {
            var successful = false;
            var runCount = 0;

            while (!successful && runCount < retryCount)
            {
                try
                {
                    action();
                    successful = true;
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    System.Threading.Thread.Sleep(TimeSpan.FromSeconds(waitSeconds));
                }
                finally
                {
                    runCount++;
                }
            }

            return successful;
        }

        public async Task WithRetryAsync(Func<Task> task)
        {
            var successful = false;
            while (!successful)
            {
                try
                {
                    await task();
                    successful = true;
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    await new WaitForSecondsRealtime(waitSeconds);
                }
            }
        }

        public async Task<bool> WithRetryAsync(Func<Task> task, int retryCount)
        {
            var successful = false;
            var runCount = 0;
            while (!successful && runCount < retryCount)
            {
                try
                {
                    await task();
                    successful = true;
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    await new WaitForSecondsRealtime(waitSeconds);
                }
                finally
                {
                    runCount++;
                }
            }

            return successful;
        }
    }
}