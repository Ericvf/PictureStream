using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace PictureStream.App.Framework
{
    public sealed class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool isVisible = false;
            bool inversed = parameter != null;

            if (value is bool)
                isVisible = (bool)value;

            //if (value is string)
            //    visibility = !string.IsNullOrEmpty((string)value);

            //if (value is BitmapImage && value != null)
            //    visibility = true;

            if (value is IList)
                isVisible = (value as IList).Count > 0;

            if (value is int)
                isVisible = (int)value > 0;

            if (inversed)
                isVisible = !isVisible;

            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
