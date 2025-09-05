using System;
using System.Threading.Tasks;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Colibri.ViewModel;
using Colibri.Services;
using VkLib.Core.Audio;
using VkLib.Core.Attachments;

namespace Colibri.View
{
    public sealed partial class MusicView : Page
    {
        private MusicViewModel Vm => DataContext as MusicViewModel;

        public MusicView()
        {
            this.InitializeComponent();
            this.DataContext = new MusicViewModel();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Load default tab data
            if (RootPivot.SelectedIndex == 0)
                await Vm.EnsureMyMusicLoadedAsync();
            else
                await Vm.EnsurePopularLoadedAsync();
        }

        private async void RootPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Vm == null)
                return;
            if (RootPivot.SelectedIndex == 0)
                await Vm.EnsureMyMusicLoadedAsync();
            else if (RootPivot.SelectedIndex == 1)
                await Vm.EnsurePopularLoadedAsync();
        }

        private void TracksList_ItemClick(object sender, ItemClickEventArgs e)
        {
            var track = e.ClickedItem as VkAudio;
            if (track == null || string.IsNullOrEmpty(track.Url))
                return;

            // Build queue from the current list and start playback using VkAudio
            var list = sender as ListView;
            var items = list?.Items?.Cast<VkAudio>().ToList();
            if (items != null && items.Count > 0)
            {
                var index = items.IndexOf(track);
                ServiceLocator.AudioService.SetQueue(items, index);
            }

            ServiceLocator.AudioService.PlayVkAudio(track);
        }

        private async void MyList_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
        {
            var deferral = args.GetDeferral();
            try
            {
                if (Vm != null)
                {
                    Vm.RefreshMyCommand?.Execute(null);
                    // Wait until loading finishes to complete the refresh visual
                    while (Vm.IsLoadingMy)
                        await Task.Delay(50);
                }
            }
            finally
            {
                deferral.Complete();
            }
        }

        private async void PopularList_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
        {
            var deferral = args.GetDeferral();
            try
            {
                if (Vm != null)
                {
                    Vm.RefreshPopularCommand?.Execute(null);
                    while (Vm.IsLoadingPopular)
                        await Task.Delay(50);
                }
            }
            finally
            {
                deferral.Complete();
            }
        }
    }
}
