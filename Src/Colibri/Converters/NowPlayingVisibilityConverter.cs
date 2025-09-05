using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Colibri.Services;
using VkLib.Core.Audio;

namespace Colibri.Converters
{
    public sealed class NowPlayingVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var item = value as VkAudio;
            var current = ServiceLocator.AudioService?.CurrentAudio;
            if (item != null && current != null)
            {
                if (item.Id == current.Id && item.OwnerId == current.OwnerId)
                    return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
