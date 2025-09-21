using System;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Colibri.ViewModel;
using Windows.Storage.Pickers;
using Windows.Storage;
using VkLib.Core.Video;
using Colibri.Services;

namespace Colibri.View
{
    public sealed partial class VideoView : Page
    {
        private VideoViewModel Vm => DataContext as VideoViewModel;

        public VideoView()
        {
            this.InitializeComponent();
            this.DataContext = new VideoViewModel();
            this.Loaded += VideoView_Loaded;
        }

        private async void VideoView_Loaded(object sender, RoutedEventArgs e)
        {
            // Load "My" by default when switching to tab
            if (RootPivot.SelectedIndex == 1)
                await Vm.EnsureMyVideosLoadedAsync();
        }

        private async void RootPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Vm == null) return;
            if (RootPivot.SelectedIndex == 1)
                await Vm.EnsureMyVideosLoadedAsync();
        }

        private async void Search_Click(object sender, RoutedEventArgs e)
        {
            if (Vm == null) return;
            await Vm.SearchAsync(refresh: true);
        }

        private async void SearchList_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
        {
            var def = args.GetDeferral();
            try { await Vm.SearchAsync(refresh: true); }
            finally { def.Complete(); }
        }

        private async void MyList_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
        {
            var def = args.GetDeferral();
            try { await Vm.LoadMyAsync(refresh: true); }
            finally { def.Complete(); }
        }

        private void VideosList_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Navigate to VideoPreviewView using ownerId/id (accessKey not present in search results typically)
            var video = e.ClickedItem as VkLib.Core.Video.VkVideo;
            if (video == null) return;
            Frame.Navigate(typeof(VideoPreviewView), new System.Collections.Generic.Dictionary<string, object>
            {
                {"ownerId", video.OwnerId},
                {"id", video.Id}
            });
        }

        private void SearchList_Loaded(object sender, RoutedEventArgs e)
        {
            var list = sender as ListView;
            var sv = FindDescendantScrollViewer(list);
            if (sv != null)
            {
                sv.ViewChanged += async (s, args) =>
                {
                    if (Vm == null || Vm.IsLoadingSearch) return;
                    var remaining = sv.ScrollableHeight - sv.VerticalOffset;
                    if (remaining < 200)
                    {
                        await Vm.SearchAsync(refresh: false);
                    }
                };
            }
        }

        private void MyList_Loaded(object sender, RoutedEventArgs e)
        {
            var list = sender as ListView;
            var sv = FindDescendantScrollViewer(list);
            if (sv != null)
            {
                sv.ViewChanged += async (s, args) =>
                {
                    if (Vm == null || Vm.IsLoadingMy) return;
                    var remaining = sv.ScrollableHeight - sv.VerticalOffset;
                    if (remaining < 200)
                    {
                        await Vm.LoadMyAsync(refresh: false);
                    }
                };
            }
        }

        private static ScrollViewer FindDescendantScrollViewer(DependencyObject root)
        {
            if (root == null) return null;
            if (root is ScrollViewer sv) return sv;
            int count = Windows.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < count; i++)
            {
                var child = Windows.UI.Xaml.Media.VisualTreeHelper.GetChild(root, i);
                var result = FindDescendantScrollViewer(child);
                if (result != null) return result;
            }
            return null;
        }

        private async void Upload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var picker = new FileOpenPicker();
                picker.ViewMode = PickerViewMode.Thumbnail;
                picker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
                picker.FileTypeFilter.Add(".mp4");
                picker.FileTypeFilter.Add(".mov");
                picker.FileTypeFilter.Add(".mkv");
                picker.FileTypeFilter.Add(".avi");
                StorageFile file = await picker.PickSingleFileAsync();
                if (file == null) return;

                // 1) Request upload URL
                VkVideoSaveResponse save = await ServiceLocator.Vkontakte.Video.Save(name: file.DisplayName);
                if (save == null || string.IsNullOrEmpty(save.UploadUrl))
                    return;

                // 2) Upload file stream
                using (var stream = await file.OpenStreamForReadAsync())
                {
                    await ServiceLocator.Vkontakte.Video.Upload(save.UploadUrl, file.Name, stream);
                }

                // 3) Refresh "My" list
                await Vm.LoadMyAsync(refresh: true);
            }
            catch { /* TODO: surface error to UI if needed */ }
        }
    }
}
