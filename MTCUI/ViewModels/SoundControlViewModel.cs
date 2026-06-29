using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using MTCUI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace MTCUI.ViewModels
{
    public partial class SoundControlViewModel : ViewModel
    {
        [ObservableProperty]
        private MediaPlayer _player;
    
        [ObservableProperty]
        private SoundItem _sound1;

        [ObservableProperty]
        private SoundItem _sound2;

        [ObservableProperty]
        private SoundItem _sound3;

        private SoundItem? _currentSound;
        private DispatcherQueue _dispatcher;

      

        public SoundControlViewModel()
        {
            Sound1 = new SoundItem("Внимание", "Vnimanie.mp3");
            Sound2 = new SoundItem("Огън", "Ogan.mp3");
            Sound3 = new SoundItem("Отбой", "Otboi.mp3");


            Player = new MediaPlayer();
            Player.MediaEnded += Player_MediaEnded; ;
        }

        public async Task InitializeAsync(DispatcherQueue dispatcherQueue)
        {
            _dispatcher = dispatcherQueue;
        }

        private void Player_MediaEnded(MediaPlayer sender, object args)
        {
            if (_currentSound != null)
            {
                _dispatcher.TryEnqueue(() =>
                {
                    _currentSound.Glyph = "\uE768";
                });
                //_currentSound = null;
            }
        }


        [RelayCommand]
        private async Task PlaySound(SoundItem sound)
        {
            // toggle на същия звук
            if (_currentSound == sound)
            {
                if (_player.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
                {
                    _player.Pause();
                    sound.Glyph = "\uE768";
                }
                else
                {
                    _player.Play();
                    sound.Glyph = "\uE769";
                }

                OnPropertyChanged(nameof(Sound1));
                OnPropertyChanged(nameof(Sound2));
                OnPropertyChanged(nameof(Sound3));
                return;
            }

            // спира стария
            if (_currentSound != null)
            {
                _currentSound.Glyph = "\uE768";
            }

            var path = Path.Combine(
                AppContext.BaseDirectory,
                "Assets",
                "Sounds",
                sound.FileName);

            var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(path);

            _player.Source = MediaSource.CreateFromStorageFile(file);
            _player.Play();

            sound.Glyph = "\uE769";
            _currentSound = sound;

            OnPropertyChanged(nameof(Sound1));
            OnPropertyChanged(nameof(Sound2));
            OnPropertyChanged(nameof(Sound3));
        }

        void PlaySound(string soundFileName)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Assets", "Sounds", soundFileName);
            Player.Source = MediaSource.CreateFromUri(new Uri(path));
            Player.Play();
        }

        void StopSound()
        {
            Player.Pause();
        }
    }
}
