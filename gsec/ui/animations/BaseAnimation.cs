using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace gsec.ui.animations
{
    public abstract class BaseAnimation
    {
        protected abstract double DurationSeconds { get; }
        protected static readonly int ANIM_INTERVAL = 50;

        protected CancellationTokenSource cancelSource;
        protected Stopwatch stopwatch = new Stopwatch();
        protected Thread animLoop;

        public Action<BaseAnimation> OnFinish = null;

        public void Start()
        {
            if (CanStart())
            {
                cancelSource = new CancellationTokenSource();
                animLoop = new Thread(() => animate(cancelSource.Token));
                animLoop.Start();
            }
        }

        public void Stop()
        {
            if (CanStop() && cancelSource != null)
            {
                cancelSource.Cancel();
            }
        }

        protected void animate(CancellationToken cancelToken)
        {
            stopwatch.Restart();

            Init();

            double elapsedSeconds = (double)stopwatch.ElapsedMilliseconds / 1000;

            while (elapsedSeconds < DurationSeconds)
            {
                if (cancelToken.IsCancellationRequested)
                {
                    break;
                }

                elapsedSeconds = (double)stopwatch.ElapsedMilliseconds / 1000;

                Update(elapsedSeconds);

                Thread.Sleep(ANIM_INTERVAL);
            }

            Finish();
            stopwatch.Stop();

            OnFinish?.Invoke(this);
        }

        public virtual bool CanStart() => true;
        public virtual bool CanStop() => true;

        protected abstract void Update(double elapsedSeconds);

        protected virtual void Init()
        {
        }

        protected virtual void Finish()
        {
        }
    }
}
