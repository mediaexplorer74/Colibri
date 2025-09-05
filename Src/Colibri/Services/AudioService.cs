using System;
using System.Collections.Generic;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using GalaSoft.MvvmLight.Threading;
using VkLib.Core.Attachments;
using VkLib.Core.Audio;
using Windows.Storage.Streams;

namespace Colibri.Services
{
    public class AudioService
    {
        private DispatcherTimer _positionTimer;
        private IList<VkAudio> _queue;
        private int _queueIndex = -1;
        private MediaPlayer _player;
        private SystemMediaTransportControls _smtc;

        //events
        public event EventHandler<TimeSpan> PositionChanged;
        public event EventHandler PlayStateChanged;

        //properties
        public bool IsPlaying => _player?.PlaybackSession.PlaybackState == MediaPlaybackState.Playing
            || _player?.PlaybackSession.PlaybackState == MediaPlaybackState.Opening
            || _player?.PlaybackSession.PlaybackState == MediaPlaybackState.Buffering;

        public TimeSpan Position => _player?.PlaybackSession.Position ?? TimeSpan.Zero;

        public VkAudioAttachment CurrentTrack { get; private set; }
        public VkAudio CurrentAudio { get; private set; }
        public string CurrentArtworkUrl { get; private set; }

        public AudioService()
        {
            Initialize();
        }

        public void PlayAudio(VkAudioAttachment audio)
        {
            CurrentTrack = audio;
            if (_player == null)
                Initialize();
            _player.Source = MediaSource.CreateFromUri(new Uri(audio.Url));
            _player.Play();

            _positionTimer.Start();
            // Notify listeners that something changed
            PlayStateChanged?.Invoke(this, EventArgs.Empty);

            UpdateSmtcMetadata();
            UpdateSmtcPlaybackStatus();
        }

        public void PlayVkAudio(VkAudio audio)
        {
            if (audio == null || string.IsNullOrEmpty(audio.Url))
                return;

            CurrentAudio = audio;
            CurrentTrack = new VkAudioAttachment(audio);
            CurrentArtworkUrl = audio?.Album?.Thumb?.Photo135 ?? audio?.Album?.Thumb?.Photo68 ?? null;

            if (_player == null)
                Initialize();
            _player.Source = MediaSource.CreateFromUri(new Uri(audio.Url));
            _player.Play();
            _positionTimer.Start();
            PlayStateChanged?.Invoke(this, EventArgs.Empty);

            UpdateSmtcMetadata();
            UpdateSmtcPlaybackStatus();
        }

        public void SetQueue(IList<VkAudio> tracks, int startIndex)
        {
            _queue = tracks;
            _queueIndex = Math.Max(0, Math.Min(startIndex, (_queue?.Count ?? 1) - 1));
        }

        public void Next()
        {
            if (_queue == null || _queue.Count == 0)
                return;
            _queueIndex = (_queueIndex + 1) % _queue.Count;
            PlayVkAudio(_queue[_queueIndex]);
        }

        public void Previous()
        {
            if (_queue == null || _queue.Count == 0)
                return;
            _queueIndex = (_queueIndex - 1 + _queue.Count) % _queue.Count;
            PlayVkAudio(_queue[_queueIndex]);
        }

        public void Play()
        {
            _player?.Play();

            _positionTimer.Start();
            UpdateSmtcPlaybackStatus();
        }

        public void Pause()
        {
            _player?.Pause();

            _positionTimer.Stop();
            UpdateSmtcPlaybackStatus();
        }

        public void Stop()
        {
            if (_player == null) return;
            _player.Pause();
            _player.PlaybackSession.Position = TimeSpan.Zero;
            _positionTimer.Stop();
            PlayStateChanged?.Invoke(this, EventArgs.Empty);
            UpdateSmtcPlaybackStatus();
        }

        public void Seek(TimeSpan position)
        {
            if (_player == null) return;
            _player.PlaybackSession.Position = position;
        }

        private void Initialize()
        {
            if (_player != null)
                return;
            _player = new MediaPlayer();
            _player.AutoPlay = false;
            _player.PlaybackSession.PlaybackStateChanged += PlaybackSessionOnPlaybackStateChanged;
            _player.MediaEnded += PlayerOnMediaEnded;

            // System Media Transport Controls (hardware/media keys)
            _smtc = SystemMediaTransportControls.GetForCurrentView();
            _smtc.IsEnabled = true;
            _smtc.IsPlayEnabled = true;
            _smtc.IsPauseEnabled = true;
            _smtc.IsStopEnabled = true;
            _smtc.IsNextEnabled = true;
            _smtc.IsPreviousEnabled = true;
            _smtc.ButtonPressed += Smtc_ButtonPressed;

            _positionTimer = new DispatcherTimer();
            _positionTimer.Interval = TimeSpan.FromMilliseconds(500);
            _positionTimer.Tick += PositionTimerOnTick;
        }

        private void PositionTimerOnTick(object sender, object o)
        {
            PositionChanged?.Invoke(this, Position);
        }

        private void PlaybackSessionOnPlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                if (sender.PlaybackState == MediaPlaybackState.Playing)
                    _positionTimer.Start();
                else
                    _positionTimer.Stop();

                PlayStateChanged?.Invoke(this, EventArgs.Empty);

                UpdateSmtcPlaybackStatus();
            });
        }

        private void PlayerOnMediaEnded(MediaPlayer sender, object args)
        {
            // Auto-advance to next track
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                Next();
            });
        }

        private void Smtc_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    Play();
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    Pause();
                    break;
                case SystemMediaTransportControlsButton.Stop:
                    Stop();
                    break;
                case SystemMediaTransportControlsButton.Next:
                    Next();
                    break;
                case SystemMediaTransportControlsButton.Previous:
                    Previous();
                    break;
                // Some older contracts may not include PlayPause; fall back to separate Play/Pause buttons
            }
        }

        private void UpdateSmtcPlaybackStatus()
        {
            if (_smtc == null) return;
            var state = _player?.PlaybackSession?.PlaybackState ?? MediaPlaybackState.None;
            switch (state)
            {
                case MediaPlaybackState.Playing:
                    _smtc.PlaybackStatus = MediaPlaybackStatus.Playing;
                    break;
                case MediaPlaybackState.Paused:
                    _smtc.PlaybackStatus = MediaPlaybackStatus.Paused;
                    break;
                case MediaPlaybackState.None:
                    _smtc.PlaybackStatus = MediaPlaybackStatus.Closed;
                    break;
                default:
                    _smtc.PlaybackStatus = MediaPlaybackStatus.Stopped;
                    break;
            }
        }

        private void UpdateSmtcMetadata()
        {
            if (_smtc == null) return;
            var updater = _smtc.DisplayUpdater;
            updater.Type = MediaPlaybackType.Music;
            var audio = CurrentAudio;
            if (audio != null)
            {
                updater.MusicProperties.Title = audio.Title ?? string.Empty;
                updater.MusicProperties.Artist = audio.Artist ?? string.Empty;
                if (!string.IsNullOrEmpty(CurrentArtworkUrl))
                {
                    try
                    {
                        updater.Thumbnail = RandomAccessStreamReference.CreateFromUri(new Uri(CurrentArtworkUrl));
                    }
                    catch { /* ignore bad urls */ }
                }
            }
            updater.Update();
        }
    }
}