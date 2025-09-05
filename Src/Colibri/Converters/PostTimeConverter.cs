using System;
using Windows.UI.Xaml.Data;

namespace Colibri.Converters
{
    // Formats dates similar to social feeds: "just now", "5 min ago", "yesterday", or date.
    public class PostTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is DateTime dt))
                return string.Empty;

            var now = DateTime.Now;
            var ts = now - dt;

            if (ts.TotalSeconds < 60)
                return "just now";
            if (ts.TotalMinutes < 60)
                return string.Format("{0} min ago", (int)ts.TotalMinutes);
            if (dt.Date == now.Date)
                return dt.ToString("HH:mm");
            if (dt.Date == now.Date.AddDays(-1))
                return "yesterday";
            if (dt.Year == now.Year)
                return dt.ToString("MMM d");
            return dt.ToString("MMM d, yyyy");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
