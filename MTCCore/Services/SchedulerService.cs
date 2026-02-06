using CommunityToolkit.Mvvm.Messaging;
using MTCCore.Messages.Timer;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MTCCore.Services
{
    public class SchedulerService : IDisposable
    {
        public delegate void TimerEventHandler(TimeSpan timeSpan);
        public event TimerEventHandler? OnTimeTick;
        private TimeSpan _time;

        private PeriodicTimer? _timer;
        private CancellationTokenSource? _cts;

        public bool IsRunning { get; private set; }


        // ▶ START
        public void Start()
        {
            if (IsRunning)
                return;

            IsRunning = true;
            _cts = new CancellationTokenSource();
            _timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

            _ = RunAsync();
        }

        // ⏹ STOP
        public void Stop()
        {
            if (!IsRunning)
                return;

            IsRunning = false;

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;

            _timer?.Dispose();
            _timer = null;
        }

        private async Task RunAsync()
        {
            try
            {
                while (IsRunning &&
                       _timer != null &&
                       await _timer.WaitForNextTickAsync(_cts!.Token))
                {
                    Tick();
                }
            }
            catch (OperationCanceledException)
            {

            }
        }

        private void Tick()
        {
            var now = DateTime.UtcNow;

            var step = TimeSpan.FromMilliseconds(1000);
            _time += step;

            OnTimeTick?.Invoke(_time);

            WeakReferenceMessenger.Default.Send(new TimerTickMessage(_time));
        }

        public void Dispose()
        {
            Stop();
        }

        public void Reset()
        {
            _time = TimeSpan.Zero;
        }
    }
}
