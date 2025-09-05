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
        }

        private void Shell_Loaded(object sender, RoutedEventArgs e)
        {
            // Default navigation
            NavigateTo("Dialogs");
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
                case "Profile":
                    ContentFrame.Navigate(typeof(View.ProfileView));
                    break;
                default:
                    ContentFrame.Navigate(typeof(MainPage));
                    break;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            // Navigate to default content on first load
            NavigateTo("Dialogs");
        }

        // Bottom navigation handlers
        private void NavNews_Click(object sender, RoutedEventArgs e) => NavigateTo("News");
        private void NavDialogs_Click(object sender, RoutedEventArgs e) => NavigateTo("Dialogs");
        private void NavMusic_Click(object sender, RoutedEventArgs e) => NavigateTo("Music");
        private void NavVideo_Click(object sender, RoutedEventArgs e) => NavigateTo("Video");
        private void NavSettings_Click(object sender, RoutedEventArgs e) => NavigateTo("Settings");
        private void NavProfile_Click(object sender, RoutedEventArgs e) => NavigateTo("Profile");
    }
}
