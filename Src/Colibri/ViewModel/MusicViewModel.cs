using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Colibri.Services;
using GalaSoft.MvvmLight.Command;
using Jupiter.Mvvm;
using VkLib.Core.Audio;
using System.Collections.Generic;
using Colibri.Model;

namespace Colibri.ViewModel
{
    public class MusicViewModel : ViewModelBase
    {
        public ObservableCollection<VkAudio> MyMusic { get; } = new ObservableCollection<VkAudio>();
        public ObservableCollection<VkAudio> Popular { get; } = new ObservableCollection<VkAudio>();
        public ObservableCollection<AudioPost> FeedPosts { get; } = new ObservableCollection<AudioPost>();

        private bool _isLoadingMy;
        public bool IsLoadingMy { get => _isLoadingMy; set => Set(ref _isLoadingMy, value); }

        private bool _isLoadingPopular;
        public bool IsLoadingPopular { get => _isLoadingPopular; set => Set(ref _isLoadingPopular, value); }

        private bool _isLoadingFeed;
        public bool IsLoadingFeed { get => _isLoadingFeed; set => Set(ref _isLoadingFeed, value); }

        private string _errorMy;
        public string ErrorMy { get => _errorMy; set => Set(ref _errorMy, value); }

        private string _errorPopular;
        public string ErrorPopular { get => _errorPopular; set => Set(ref _errorPopular, value); }

        private string _errorFeed;
        public string ErrorFeed { get => _errorFeed; set => Set(ref _errorFeed, value); }

        private string _feedNextFrom;
        public string FeedNextFrom { get => _feedNextFrom; set => Set(ref _feedNextFrom, value); }

        public RelayCommand RefreshMyCommand { get; private set; }
        public RelayCommand RefreshPopularCommand { get; private set; }
        public RelayCommand RefreshFeedCommand { get; private set; }

        public MusicViewModel()
        {
            RefreshMyCommand = new RelayCommand(async () => await LoadMyMusicAsync(true));
            RefreshPopularCommand = new RelayCommand(async () => await LoadPopularAsync(true));
            RefreshFeedCommand = new RelayCommand(async () => await LoadFeedAsync(true));
        }

        public async Task EnsureMyMusicLoadedAsync()
        {
            if (MyMusic.Count == 0 && !IsLoadingMy)
                await LoadMyMusicAsync(false);
        }

        public async Task EnsurePopularLoadedAsync()
        {
            if (Popular.Count == 0 && !IsLoadingPopular)
                await LoadPopularAsync(false);
        }

        public async Task EnsureFeedLoadedAsync()
        {
            if (FeedPosts.Count == 0 && !IsLoadingFeed)
                await LoadFeedAsync(false);
        }

        private async Task LoadMyMusicAsync(bool force)
        {
            if (IsLoadingMy)
                return;
            IsLoadingMy = true;
            ErrorMy = null;
            try
            {
                var response = await ServiceLocator.Vkontakte.Audio.Get();
                MyMusic.Clear();
                if (response != null && response.Items != null)
                {
                    foreach (var a in response.Items)
                        MyMusic.Add(a);
                }
            }
            catch (Exception ex)
            {
                ErrorMy = ex.Message;
            }
            finally
            {
                IsLoadingMy = false;
            }
        }

        private async Task LoadPopularAsync(bool force)
        {
            if (IsLoadingPopular)
                return;
            IsLoadingPopular = true;
            ErrorPopular = null;
            try
            {
                var response = await ServiceLocator.Vkontakte.Audio.GetPopular();
                Popular.Clear();
                if (response != null && response.Items != null)
                {
                    foreach (var a in response.Items)
                        Popular.Add(a);
                }
            }
            catch (Exception ex)
            {
                ErrorPopular = ex.Message;
            }
            finally
            {
                IsLoadingPopular = false;
            }
        }

        public async Task LoadFeedAsync(bool force)
        {
            if (IsLoadingFeed)
                return;
            IsLoadingFeed = true;
            ErrorFeed = null;
            try
            {
                // For refresh, reset paging and clear list
                if (force)
                    FeedNextFrom = null;

                var result = await ServiceLocator.FeedService.GetNewsAsync(count: 50, nextFrom: FeedNextFrom);
                if (force)
                    FeedPosts.Clear();

                if (result?.Posts != null)
                {
                    foreach (var p in result.Posts)
                        FeedPosts.Add(p);
                }
                FeedNextFrom = result?.NextFrom;
            }
            catch (Exception ex)
            {
                ErrorFeed = ex.Message;
            }
            finally
            {
                IsLoadingFeed = false;
            }
        }
    }
}
