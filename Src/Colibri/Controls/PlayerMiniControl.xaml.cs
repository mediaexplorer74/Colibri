using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Imaging;
using Colibri.Services;

namespace Colibri.Controls
{
    public sealed partial class PlayerMiniControl : UserControl
    {
        private bool _updatingFromCode;

        public PlayerMiniControl()
        {
            this.InitializeComponent();
            this.Loaded += PlayerMiniControl_Loaded;
            this.Unloaded += PlayerMiniControl_Unloaded;
        }

        private void PlayerMiniControl_Loaded(object sender, RoutedEventArgs e)
        {
            ServiceLocator.AudioService.PlayStateChanged += AudioService_PlayStateChanged;
            ServiceLocator.AudioService.PositionChanged += AudioService_PositionChanged;
            RefreshUi();
        }

        private void PlayerMiniControl_Unloaded(object sender, RoutedEventArgs e)
        {
            ServiceLocator.AudioService.PlayStateChanged -= AudioService_PlayStateChanged;
            ServiceLocator.AudioService.PositionChanged -= AudioService_PositionChanged;
        }

        private void AudioService_PlayStateChanged(object sender, EventArgs e)
        {
            RefreshUi();
        }

        private void AudioService_PositionChanged(object sender, TimeSpan e)
        {
            _updatingFromCode = true;
            try
            {
                var duration = ServiceLocator.AudioService.CurrentTrack?.Duration ?? TimeSpan.Zero;
                if (duration.TotalSeconds > 0)
                {
                    PositionSlider.Maximum = duration.TotalSeconds;
                    PositionSlider.Value = e.TotalSeconds;
                    TimeLabel.Text = $"{FormatTs(e)} / {FormatTs(duration)}";
                }
                else
                {
                    PositionSlider.Maximum = 100;
                    PositionSlider.Value = 0;
                    TimeLabel.Text = FormatTs(e);
                }
            }
            finally
            {
                _updatingFromCode = false;
            }
        }

        private void RefreshUi()
        {
            var svc = ServiceLocator.AudioService;
            var track = svc.CurrentTrack;

            var isPlaying = svc.IsPlaying;
            PlayPauseGlyph.Text = isPlaying ? "⏸" : "▶";

            if (track != null)
            {
                TrackTitle.Text = track.Title;
                TrackArtist.Text = track.Artist;
                // Update artwork
                try
                {
                    if (!string.IsNullOrEmpty(svc.CurrentArtworkUrl))
                        ArtworkImage.Source = new BitmapImage(new Uri(svc.CurrentArtworkUrl));
                    else
                        ArtworkImage.Source = null;
                }
                catch { ArtworkImage.Source = null; }
                this.Visibility = Visibility.Visible;

                // Update position display once
                AudioService_PositionChanged(this, svc.Position);
            }
            else
            {
                TrackTitle.Text = string.Empty;
                TrackArtist.Text = string.Empty;
                TimeLabel.Text = string.Empty;
                PositionSlider.Value = 0;
                this.Visibility = Visibility.Collapsed;
            }
        }

        private static string FormatTs(TimeSpan ts)
        {
            if (ts.TotalHours >= 1)
                return ts.ToString("hh\\:mm\\:ss");
            return ts.ToString("mm\\:ss");
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            var svc = ServiceLocator.AudioService;
            if (svc.IsPlaying)
                svc.Pause();
            else
                svc.Play();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            ServiceLocator.AudioService.Stop();
        }

        private void PositionSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_updatingFromCode)
                return;

            var duration = ServiceLocator.AudioService.CurrentTrack?.Duration ?? TimeSpan.Zero;
            if (duration.TotalSeconds > 0)
            {
                ServiceLocator.AudioService.Seek(TimeSpan.FromSeconds(e.NewValue));
            }
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            ServiceLocator.AudioService.Previous();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            ServiceLocator.AudioService.Next();
        }
    }
}
