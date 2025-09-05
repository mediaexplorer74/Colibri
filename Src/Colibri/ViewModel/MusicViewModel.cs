using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Colibri.Services;
using GalaSoft.MvvmLight.Command;
using Jupiter.Mvvm;
using VkLib.Core.Audio;

namespace Colibri.ViewModel
{
    public class MusicViewModel : ViewModelBase
    {
        public ObservableCollection<VkAudio> MyMusic { get; } = new ObservableCollection<VkAudio>();
        public ObservableCollection<VkAudio> Popular { get; } = new ObservableCollection<VkAudio>();

        private bool _isLoadingMy;
        public bool IsLoadingMy { get => _isLoadingMy; set => Set(ref _isLoadingMy, value); }

        private bool _isLoadingPopular;
        public bool IsLoadingPopular { get => _isLoadingPopular; set => Set(ref _isLoadingPopular, value); }

        private string _errorMy;
        public string ErrorMy { get => _errorMy; set => Set(ref _errorMy, value); }

        private string _errorPopular;
        public string ErrorPopular { get => _errorPopular; set => Set(ref _errorPopular, value); }

        public RelayCommand RefreshMyCommand { get; private set; }
        public RelayCommand RefreshPopularCommand { get; private set; }

        public MusicViewModel()
        {
            RefreshMyCommand = new RelayCommand(async () => await LoadMyMusicAsync(true));
            RefreshPopularCommand = new RelayCommand(async () => await LoadPopularAsync(true));
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
    }
}
