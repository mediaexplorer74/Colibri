using System;
using System.Collections.Generic;
using System.Linq;
using Windows.System.Profile;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Colibri.Controls;
using Colibri.Helpers;
using Colibri.Services;
using Jupiter.Utils.Extensions;
using Jupiter.Utils.Helpers;
using VkLib.Core.Attachments;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Colibri.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VideoPreviewView : Page
    {
        private VkVideoAttachment _videoAttachment;
        private long? _ownerId;
        private long? _videoId;
        private string _accessKey;

        public VideoPreviewView()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (DeviceHelper.IsMobile())
                WebView.Margin = new Thickness();

            var p = e.Parameter as Dictionary<string, object>;
            if (p != null)
            {
                if (p.ContainsKey("video") && p["video"] is VkVideoAttachment att)
                {
                    _videoAttachment = att;
                }
                else
                {
                    if (p.ContainsKey("ownerId")) _ownerId = System.Convert.ToInt64(p["ownerId"]);
                    if (p.ContainsKey("id")) _videoId = System.Convert.ToInt64(p["id"]);
                    if (p.ContainsKey("accessKey")) _accessKey = p["accessKey"] as string;
                }

                LoadVideo();
            }

            //var currentView = SystemNavigationManager.GetForCurrentView();
            //currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            //currentView.BackRequested += CurrentView_BackRequested;

            base.OnNavigatedTo(e);
        }

        private async void LoadVideo()
        {
            LoadingIndicator.IsBusy = true;

            try
            {
                string idParam = null;
                if (_videoAttachment != null)
                {
                    // Include access key only if present
                    if (!string.IsNullOrEmpty(_videoAttachment.AccessKey))
                        idParam = $"{_videoAttachment.OwnerId}_{_videoAttachment.Id}_{_videoAttachment.AccessKey}";
                    else
                        idParam = $"{_videoAttachment.OwnerId}_{_videoAttachment.Id}";
                }
                else if (_ownerId.HasValue && _videoId.HasValue)
                {
                    if (!string.IsNullOrEmpty(_accessKey))
                        idParam = $"{_ownerId.Value}_{_videoId.Value}_{_accessKey}";
                    else
                        idParam = $"{_ownerId.Value}_{_videoId.Value}";
                }

                if (string.IsNullOrEmpty(idParam))
                {
                    LoadingIndicator.Error = Localizator.String("Error/ChatVideoAttachmentLoadCommonError");
                    LoadingIndicator.IsBusy = false;
                    return;
                }

                var response = await ServiceLocator.Vkontakte.Video.Get(new[] { idParam });

                if (response != null && !response.Items.IsNullOrEmpty())
                {
                    var video = response.Items.First();
                    WebView.Navigate(new Uri(video.Player));
                }
                else
                {
                    LoadingIndicator.Error = Localizator.String("Error/ChatVideoAttachmentLoadCommonError");
                    LoadingIndicator.IsBusy = false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Unable to load video {_videoAttachment.OwnerId}_{_videoAttachment.Id}_{_videoAttachment.AccessKey} info");

                LoadingIndicator.Error = Localizator.String("Error/ChatVideoAttachmentLoadCommonError");
                LoadingIndicator.IsBusy = false;
            }
        }

        private void VideoPreviewView_OnUnloaded(object sender, RoutedEventArgs e)
        {
            WebView.NavigateToString("");

            //var currentView = SystemNavigationManager.GetForCurrentView();
            //currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            //currentView.BackRequested -= CurrentView_BackRequested;
        }


        private void CurrentView_BackRequested(object sender, BackRequestedEventArgs e)
        {
            PopupControl.CloseCurrent();
        }

        private void WebView_OnNavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            LoadingIndicator.IsBusy = true;
        }

        private void WebView_OnNavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            LoadingIndicator.IsBusy = false;
        }

        private void WebView_OnNavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
        {
            LoadingIndicator.Error = Localizator.String("Error/ChatVideoAttachmentLoadCommonError");
        }
    }
}