using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace MTCUI.Services
{
    public delegate void TimerIntervalElapsedEventHandler(object sender, DateTime dateTime);

    public interface ITimerWorker
    {
        event TimerIntervalElapsedEventHandler TimerIntervalElapsed;
        void Start();
        void Stop();
    }

    public class TimerWorker : ITimerWorker
    {
        private readonly System.Timers.Timer _timer;
        private readonly Dictionary<TimerIntervalElapsedEventHandler, List<ElapsedEventHandler>> _handlers = new();

        public TimerWorker()
        {
            _timer = new System.Timers.Timer();
            _timer.Interval = 1000;
        }

        public event TimerIntervalElapsedEventHandler TimerIntervalElapsed
        {
            add
            {
                void internalHandler(object sender, ElapsedEventArgs args) { value.Invoke(sender, args.SignalTime); }

                if (!_handlers.ContainsKey(value))
                {
                    _handlers.Add(value, []);
                }

                _handlers[value].Add(internalHandler);

                _timer.Elapsed += internalHandler;
            }

            remove
            {
                _timer.Elapsed -= _handlers[value].Last();

                _handlers[value].RemoveAt(_handlers[value].Count - 1);

                if (!_handlers[value].Any())
                {
                    _handlers.Remove(value);
                }
            }
        }


        public void Start()
        {
            _timer.Enabled = true;

        }

        public void Stop()
        {
            _timer.Enabled = false;
        }
    }
}
