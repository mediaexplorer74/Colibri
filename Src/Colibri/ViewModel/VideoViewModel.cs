using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Colibri.Services;
using Jupiter.Mvvm;
using VkLib.Core.Video;
using VkLib.Core.Audio;

namespace Colibri.ViewModel
{
    public class VideoViewModel : ViewModelBase
    {
        public ObservableCollection<VkVideo> SearchResults { get; } = new ObservableCollection<VkVideo>();
        public ObservableCollection<VkVideo> MyVideos { get; } = new ObservableCollection<VkVideo>();

        private string _query;
        public string Query { get => _query; set => Set(ref _query, value); }

        private bool _isLoadingSearch;
        public bool IsLoadingSearch { get => _isLoadingSearch; set => Set(ref _isLoadingSearch, value); }

        private bool _isLoadingMy;
        public bool IsLoadingMy { get => _isLoadingMy; set => Set(ref _isLoadingMy, value); }

        private string _errorSearch;
        public string ErrorSearch { get => _errorSearch; set => Set(ref _errorSearch, value); }

        private string _errorMy;
        public string ErrorMy { get => _errorMy; set => Set(ref _errorMy, value); }

        private int _searchOffset;
        private int _myOffset;

        // Search filters
        private bool _isHdOnly;
        public bool IsHdOnly { get => _isHdOnly; set => Set(ref _isHdOnly, value); }

        private bool _isAdult;
        public bool IsAdult { get => _isAdult; set => Set(ref _isAdult, value); }

        // 0-DateAdded, 1-Duration, 2-Popularity
        private int _sortIndex;
        public int SortIndex { get => _sortIndex; set => Set(ref _sortIndex, value); }

        public async Task EnsureMyVideosLoadedAsync()
        {
            if (MyVideos.Count == 0 && !IsLoadingMy)
                await LoadMyAsync(refresh: false);
        }

        public async Task SearchAsync(bool refresh)
        {
            if (IsLoadingSearch) return;
            if (string.IsNullOrWhiteSpace(Query))
            {
                if (refresh) SearchResults.Clear();
                return;
            }

            IsLoadingSearch = true;
            ErrorSearch = null;
            try
            {
                if (refresh)
                {
                    _searchOffset = 0;
                    SearchResults.Clear();
                }

                var sort = VkAudioSortType.DateAdded;
                switch (SortIndex)
                {
                    case 1:
                        sort = VkAudioSortType.Duration;
                        break;
                    case 2:
                        sort = VkAudioSortType.Popularity;
                        break;
                    default:
                        sort = VkAudioSortType.DateAdded;
                        break;
                }

                var items = await ServiceLocator.Vkontakte.Video.Search(Query, count: 50, offset: _searchOffset, hdOnly: IsHdOnly, sort: sort, adult: IsAdult);
                if (items != null)
                {
                    foreach (var v in items)
                        SearchResults.Add(v);

                    // VK returns up to 200, we increment by 50 per page
                    _searchOffset += 50;
                }
            }
            catch (Exception ex)
            {
                ErrorSearch = ex.Message;
            }
            finally
            {
                IsLoadingSearch = false;
            }
        }

        public async Task LoadMyAsync(bool refresh)
        {
            if (IsLoadingMy) return;
            IsLoadingMy = true;
            ErrorMy = null;
            try
            {
                if (refresh)
                {
                    _myOffset = 0;
                    MyVideos.Clear();
                }

                var ownerId = ServiceLocator.Vkontakte.AccessToken?.UserId.ToString();
                var response = await ServiceLocator.Vkontakte.Video.Get(videos: null, ownerId: ownerId, count: 50, offset: _myOffset);
                if (response?.Items != null)
                {
                    foreach (var v in response.Items)
                        MyVideos.Add(v);
                    _myOffset += response.Items.Count;
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
    }
}
