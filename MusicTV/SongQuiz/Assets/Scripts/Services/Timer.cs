using Assets.Scripts.Reusable;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Services
{
    public class Timer
    {
        private float time;
        private List<StepAction> stepActions = new List<StepAction>();
        private List<SecondAction> lastSecondsActions = new List<SecondAction>();
        public int Step { get; private set; } = 1;

        public bool Enabled { get; private set; }

        public float Time
        {
            get => this.time;
            private set
            {
                this.time = value;
                if (this.time < 0)
                {
                    this.Step++;
                    this.Enabled = false;
                    this.ExecuteStepActions();
                }
                this.ExecuteLastSecondsActions();
            }
        }

        private void ExecuteLastSecondsActions()
        {
            foreach (var action in this.lastSecondsActions)
            {
                if (!action.Executed && this.time <= action.Second)
                {
                    action.Action();
                    action.Executed = true;
                }
            }
        }

        public string Text => this.Enabled ? ((int)this.Time).ToString() : "<sprite index=0>";

        public void OnStep(int step, Action action)
        {
            this.stepActions.Add(new StepAction { Step = step, Action = action });
        }

        public void ExecuteStepActions()
        {
            var actions = this.stepActions.Where(kv => kv.Step == this.Step).Select(kv => kv.Action);
            foreach (var action in actions)
            {
                action();
            }
        }

        public void Tick(float elapsed)
        {
            if (this.Enabled && this.Time >= 0)
            {
                this.Time -= elapsed;
            }
        }

        public void SkipStep()
        {
            this.Step++;
            this.Enabled = false;
            this.ExecuteStepActions();
        }

        public void StartCountdown(float time)
        {
            this.Time = time;
            this.Enabled = true;
        }

        public void Pause()
        {
            this.Enabled = false;
        }

        public void Resume()
        {
            this.Enabled = true;
        }

        public void OnLastSeconds(int seconds, Action action)
        {
            for (int i = 0; i < seconds; i++)
            {
                this.lastSecondsActions.Add(new SecondAction { Second = i + 1, Action = action });
            }
        }
    }
}