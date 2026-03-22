using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Dispatching;
using MTCCore.Messages.Timer;
using MTCCore.Services.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MTCUI.ViewModels
{
    public partial class TimerViewModel : ObservableObject
    {
        private readonly Clock _clock;

        [ObservableProperty]
        private string _timerText = "00:00:00";

        [ObservableProperty]
        private string _startStopText = "СТАРТ";

        private bool _isStarted = false;
        
        private DispatcherQueue _dispatcher;

        public TimerViewModel(Clock clock)
        {
            _clock = clock;

            WeakReferenceMessenger.Default.Register<TimerTickMessage>(this, (r, m) =>
            {
                _dispatcher.TryEnqueue(() =>
                {
                    TimerText = m.Time.ToString(@"hh\:mm\:ss");
                });
            });
        }

        public async Task InitializeAsync(DispatcherQueue dispatcherQueue)
        {
            _dispatcher = dispatcherQueue;
        }

        [RelayCommand]
        void StartStop()
        {
            if (_isStarted)
            {
                _clock.Stop();
                ChangeContent("СТАРТ");
                _isStarted = false;
                return;
            }

            _clock.Start();
            ChangeContent("СТОП");
            _isStarted = true;
        }

        [RelayCommand]
        void Reset()
        {
            _clock.Reset();
        }

        private void ChangeContent(string text)
        {
            _dispatcher.TryEnqueue(() =>
            {
                StartStopText = text;
            });
        }
    }
}
