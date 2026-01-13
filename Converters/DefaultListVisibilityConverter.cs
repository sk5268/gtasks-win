using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Google_Tasks_Client
{
    public class DefaultListVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // Usually "@default" is the ID for the default task list in Google Tasks
            if (value is string id && id == "@default")
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
