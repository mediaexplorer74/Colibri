using System;
using System.Threading.Tasks;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
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
            // Subscribe to playback changes to highlight the now-playing item
            ServiceLocator.AudioService.PlayStateChanged += AudioService_PlayStateChanged;
            this.Unloaded += MusicView_Unloaded;

            _scrollIdleTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
            _scrollIdleTimer.Tick += (s, e) =>
            {
                _suppressSelectionUpdates = false;
                _scrollIdleTimer.Stop();
                // After scroll ends, one-time sync
                HighlightNowPlayingInActiveList();
            };
        }

        // Infinite scroll for Feed
        private void FeedListView_Loaded(object sender, RoutedEventArgs e)
        {
            var list = sender as ListView;
            var sv = FindDescendantScrollViewer(list);
            if (sv != null)
            {
                sv.ViewChanged += async (s, args) =>
                {
                    if (Vm == null) return;
                    // Near bottom? load more if available and not already loading
                    if (!Vm.IsLoadingFeed && !string.IsNullOrEmpty(Vm.FeedNextFrom))
                    {
                        var remaining = sv.ScrollableHeight - sv.VerticalOffset;
                        if (remaining < 200)
                        {
                            // Load next page without clearing
                            await Vm.LoadFeedAsync(false);
                        }
                    }
                };
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Load default tab data
            if (RootPivot.SelectedIndex == 0)
                await Vm.EnsureMyMusicLoadedAsync();
            else
                await Vm.EnsurePopularLoadedAsync();

            // Initial selection sync in case something is already playing
            HighlightNowPlayingInActiveList();
        }

        private async void RootPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Vm == null)
                return;
            if (RootPivot.SelectedIndex == 0)
                await Vm.EnsureMyMusicLoadedAsync();
            else if (RootPivot.SelectedIndex == 1)
                await Vm.EnsurePopularLoadedAsync();
            else if (RootPivot.SelectedIndex == 2)
                await Vm.EnsureFeedLoadedAsync();
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

            // Immediately highlight the clicked track
            HighlightNowPlayingInActiveList();
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

        private void AudioService_PlayStateChanged(object sender, EventArgs e)
        {
            // Sync selection when track changes (e.g., next/previous/auto-advance)
            if (!_suppressSelectionUpdates)
                HighlightNowPlayingInActiveList();
        }

        private async void FeedList_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
        {
            var deferral = args.GetDeferral();
            try
            {
                if (Vm != null)
                {
                    Vm.RefreshFeedCommand?.Execute(null);
                    while (Vm.IsLoadingFeed)
                        await Task.Delay(50);
                }
            }
            finally
            {
                deferral.Complete();
            }
        }

        private void TracksInPost_ItemClick(object sender, ItemClickEventArgs e)
        {
            var track = e.ClickedItem as VkAudio;
            if (track == null || string.IsNullOrEmpty(track.Url))
                return;

            // Build queue from the tracks within the clicked post list
            var list = sender as ListView;
            var items = list?.Items?.Cast<VkAudio>().ToList();
            if (items != null && items.Count > 0)
            {
                var index = items.IndexOf(track);
                ServiceLocator.AudioService.SetQueue(items, index);
            }

            ServiceLocator.AudioService.PlayVkAudio(track);
        }

        private void HighlightNowPlayingInActiveList()
        {
            var current = ServiceLocator.AudioService.CurrentAudio;
            if (current == null)
                return;

            var list = GetActiveListView();
            if (list == null || list.Items == null)
                return;

            var items = list.Items.OfType<VkAudio>().ToList();
            var match = items.FirstOrDefault(a => a.Id == current.Id && a.OwnerId == current.OwnerId);
            if (match != null)
            {
                list.SelectedItem = match;
                list.ScrollIntoView(match);
            }
        }

        private ListView GetActiveListView()
        {
            if (RootPivot.SelectedIndex == 0)
                return MyListView;
            if (RootPivot.SelectedIndex == 1)
                return PopularListView;
            return null;
        }

        private void MusicView_Unloaded(object sender, RoutedEventArgs e)
        {
            ServiceLocator.AudioService.PlayStateChanged -= AudioService_PlayStateChanged;
            this.Unloaded -= MusicView_Unloaded;
        }

        // Row glyph visibility helper used from XAML with x:Bind
        public Visibility IsItemSelected(object item, ListView list)
        {
            if (item == null || list == null)
                return Visibility.Collapsed;
            return ReferenceEquals(item, list.SelectedItem) ? Visibility.Visible : Visibility.Collapsed;
        }

        // Suppress selection updates while the user scrolls
        private bool _suppressSelectionUpdates;
        private DispatcherTimer _scrollIdleTimer;

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            var list = sender as ListView;
            var sv = FindDescendantScrollViewer(list);
            if (sv != null)
            {
                sv.ViewChanged += (s, args) =>
                {
                    _suppressSelectionUpdates = true;
                    _scrollIdleTimer.Stop();
                    _scrollIdleTimer.Start();
                };
            }
        }

        private static ScrollViewer FindDescendantScrollViewer(DependencyObject root)
        {
            if (root == null) return null;
            if (root is ScrollViewer sv) return sv;
            int count = VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                var result = FindDescendantScrollViewer(child);
                if (result != null) return result;
            }
            return null;
        }
    }
}
