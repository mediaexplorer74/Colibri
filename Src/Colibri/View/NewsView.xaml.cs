using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Colibri.Model;
using Colibri.Services;

namespace Colibri.View
{
    public sealed partial class NewsView : Page
    {
        public class NewsViewModel
        {
            public ObservableCollection<AudioPost> Posts { get; } = new ObservableCollection<AudioPost>();
            public bool IsLoading { get; set; }
            public string Error { get; set; }
            public string NextFrom { get; set; }

            public async Task LoadAsync(bool refresh)
            {
                if (IsLoading) return;
                IsLoading = true;
                Error = null;
                try
                {
                    if (refresh)
                    {
                        NextFrom = null;
                        Posts.Clear();
                    }
                    var result = await ServiceLocator.FeedService.GetNewsAsync(count: 50, nextFrom: NextFrom, onlyWithAudio: false);
                    if (result?.Posts != null)
                    {
                        foreach (var p in result.Posts)
                            Posts.Add(p);
                    }
                    NextFrom = result?.NextFrom;
                }
                catch (Exception ex)
                {
                    Error = ex.Message;
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private NewsViewModel Vm => DataContext as NewsViewModel;

        public NewsView()
        {
            this.InitializeComponent();
            this.DataContext = new NewsViewModel();
            this.Loaded += NewsView_Loaded;
        }

        private async void NewsView_Loaded(object sender, RoutedEventArgs e)
        {
            await Vm.LoadAsync(refresh: true);
        }

        private async void RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
        {
            var deferral = args.GetDeferral();
            try
            {
                await Vm.LoadAsync(refresh: true);
            }
            finally
            {
                deferral.Complete();
            }
        }

        private void PostsList_Loaded(object sender, RoutedEventArgs e)
        {
            var list = sender as ListView;
            var sv = FindDescendantScrollViewer(list);
            if (sv != null)
            {
                sv.ViewChanged += async (s, args) =>
                {
                    if (!Vm.IsLoading && !string.IsNullOrEmpty(Vm.NextFrom))
                    {
                        var remaining = sv.ScrollableHeight - sv.VerticalOffset;
                        if (remaining < 200)
                        {
                            await Vm.LoadAsync(refresh: false);
                        }
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
    }
}
