using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Colibri.Services;
using Colibri.ViewModel.Messaging;
using GalaSoft.MvvmLight.Messaging;
using VkLib.Core.Auth;

namespace Colibri.View
{
    public sealed partial class ProfileView : Page
    {
        public ProfileView()
        {
            this.InitializeComponent();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            // Clear tokens
            ServiceLocator.Vkontakte.AccessToken = new VkAccessToken();
            AppSettings.AccessToken = null;

            // Notify and navigate to login
            Messenger.Default.Send(new LoginStateChangedMessage { IsLoggedIn = false });
            Frame.Navigate(typeof(LoginView));
        }
    }
}
