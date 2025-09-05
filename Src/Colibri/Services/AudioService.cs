using System;
using System.Collections.Generic;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using GalaSoft.MvvmLight.Threading;
using VkLib.Core.Attachments;
using VkLib.Core.Audio;

namespace Colibri.Services
{
    public class AudioService
    {
        private DispatcherTimer _positionTimer;
        private IList<VkAudio> _queue;
        private int _queueIndex = -1;

        //events
        public event EventHandler<TimeSpan> PositionChanged;
        public event EventHandler PlayStateChanged;

        //properties
        public bool IsPlaying => BackgroundMediaPlayer.Current.CurrentState == MediaPlayerState.Playing
            || BackgroundMediaPlayer.Current.CurrentState == MediaPlayerState.Opening
            || BackgroundMediaPlayer.Current.CurrentState == MediaPlayerState.Buffering;

        public TimeSpan Position => BackgroundMediaPlayer.Current.Position;

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
            BackgroundMediaPlayer.Current.SetUriSource(new Uri(audio.Url));
            BackgroundMediaPlayer.Current.Play();

            _positionTimer.Start();
            // Notify listeners that something changed
            PlayStateChanged?.Invoke(this, EventArgs.Empty);
        }

        public void PlayVkAudio(VkAudio audio)
        {
            if (audio == null || string.IsNullOrEmpty(audio.Url))
                return;

            CurrentAudio = audio;
            CurrentTrack = new VkAudioAttachment(audio);
            CurrentArtworkUrl = audio?.Album?.Thumb?.Photo135 ?? audio?.Album?.Thumb?.Photo68 ?? null;

            BackgroundMediaPlayer.Current.SetUriSource(new Uri(audio.Url));
            BackgroundMediaPlayer.Current.Play();
            _positionTimer.Start();
            PlayStateChanged?.Invoke(this, EventArgs.Empty);
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
            BackgroundMediaPlayer.Current.Play();

            _positionTimer.Start();
        }

        public void Pause()
        {
            BackgroundMediaPlayer.Current.Pause();

            _positionTimer.Stop();
        }

        public void Stop()
        {
            BackgroundMediaPlayer.Current.Pause();
            BackgroundMediaPlayer.Current.Position = TimeSpan.Zero;
            _positionTimer.Stop();
            PlayStateChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Seek(TimeSpan position)
        {
            BackgroundMediaPlayer.Current.Position = position;
        }

        private void Initialize()
        {
            BackgroundMediaPlayer.Current.CurrentStateChanged += MediaPlayerOnCurrentStateChanged;
            //BackgroundMediaPlayer.Current.MediaEnded += MediaPlayerOnMediaEnded;

            _positionTimer = new DispatcherTimer();
            _positionTimer.Interval = TimeSpan.FromMilliseconds(500);
            _positionTimer.Tick += PositionTimerOnTick;
        }

        private void PositionTimerOnTick(object sender, object o)
        {
            PositionChanged?.Invoke(this, Position);
        }

        private void MediaPlayerOnCurrentStateChanged(MediaPlayer sender, object args)
        {
            Logger.Info(sender.CurrentState.ToString());

            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                if (sender.CurrentState == MediaPlayerState.Playing)
                    _positionTimer.Start();
                else
                    _positionTimer.Stop();

                PlayStateChanged?.Invoke(this, EventArgs.Empty);
            });
        }
    }
}