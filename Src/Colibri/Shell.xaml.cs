using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Colibri
{
    public sealed partial class Shell : Page
    {
        public Shell()
        {
            this.InitializeComponent();
            this.Loaded += Shell_Loaded;
            this.SizeChanged += Shell_SizeChanged;
        }

        private void Shell_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateSplitViewMode();
            // Default to Dialogs
            NavigateTo("Dialogs");
            SetSelected("Dialogs");
        }

        private void Shell_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateSplitViewMode();
        }

        private void UpdateSplitViewMode()
        {
            // Simple adaptive behavior: overlay on narrow, compact inline on wide
            if (this.ActualWidth < 720)
            {
                SplitView.DisplayMode = SplitViewDisplayMode.Overlay;
                SplitView.IsPaneOpen = false;
            }
            else
            {
                SplitView.DisplayMode = SplitViewDisplayMode.CompactInline;
                SplitView.IsPaneOpen = true;
            }
        }

        private void MenuList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MenuList.SelectedItem == null)
                return;

            var lvi = MenuList.SelectedItem as ListViewItem ?? MenuList.ContainerFromItem(MenuList.SelectedItem) as ListViewItem;
            var tag = lvi?.Tag as string;
            if (!string.IsNullOrEmpty(tag))
            {
                NavigateTo(tag);
            }
        }

        private void NavigateTo(string tag)
        {
            switch (tag)
            {
                case "News":
                    ContentFrame.Navigate(typeof(View.NewsView));
                    break;
                case "Dialogs":
                    ContentFrame.Navigate(typeof(MainPage));
                    break;
                case "Music":
                    ContentFrame.Navigate(typeof(View.MusicView));
                    break;
                case "Video":
                    ContentFrame.Navigate(typeof(View.VideoView));
                    break;
                case "Settings":
                    ContentFrame.Navigate(typeof(View.SettingsView));
                    break;
                default:
                    ContentFrame.Navigate(typeof(MainPage));
                    break;
            }

            // Auto-close pane for overlay mode to free screen space on mobile
            if (SplitView.DisplayMode == SplitViewDisplayMode.Overlay)
            {
                SplitView.IsPaneOpen = false;
            }
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            SplitView.IsPaneOpen = !SplitView.IsPaneOpen;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            // Default selection: Dialogs
            if (MenuList.Items?.Count > 0)
            {
                // Find the first item with Tag="Dialogs"
                foreach (var item in MenuList.Items)
                {
                    var lvi = item as ListViewItem ?? MenuList.ContainerFromItem(item) as ListViewItem;
                    var tag = (lvi?.Tag as string) ?? (item as FrameworkElement)?.Tag as string;
                    if (string.Equals(tag, "Dialogs", StringComparison.OrdinalIgnoreCase))
                    {
                        MenuList.SelectedItem = item;
                        NavigateTo("Dialogs");
                        break;
                    }
                }
            }
        }

        private void SetSelected(string tag)
        {
            foreach (var item in MenuList.Items)
            {
                if (item is ListViewItem lvi)
                {
                    if (string.Equals(lvi.Tag as string, tag, StringComparison.OrdinalIgnoreCase))
                    {
                        MenuList.SelectedItem = lvi;
                        break;
                    }
                }
            }
        }
    }
}
