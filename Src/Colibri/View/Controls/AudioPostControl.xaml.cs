using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Colibri.Services;
using VkLib.Core.Audio;
using Windows.System;
using Colibri.Model;

namespace Colibri.View.Controls
{
    public sealed partial class AudioPostControl : UserControl
    {
        public AudioPostControl()
        {
            this.InitializeComponent();
        }

        private void TracksList_ItemClick(object sender, ItemClickEventArgs e)
        {
            var track = e.ClickedItem as VkAudio;
            if (track == null || string.IsNullOrEmpty(track.Url))
                return;

            var list = sender as ListView;
            var items = list?.Items?.Cast<VkAudio>().ToList();
            if (items != null && items.Count > 0)
            {
                var index = items.IndexOf(track);
                ServiceLocator.AudioService.SetQueue(items, index);
            }

            ServiceLocator.AudioService.PlayVkAudio(track);
        }

        private async void Author_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is AudioPost post && post.AuthorUri != null)
            {
                await Launcher.LaunchUriAsync(post.AuthorUri);
            }
        }

        private async void Date_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is AudioPost post && post.PostUri != null)
            {
                await Launcher.LaunchUriAsync(post.PostUri);
            }
        }
    }
}
