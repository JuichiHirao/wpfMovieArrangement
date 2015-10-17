using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace wpfMovieArrangement
{
    [ValueConversion( typeof( ValidationError ), typeof( bool ) )]
    public class TextBoxHasErrorToButtonIsEnabledConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            return value as ValidationError == null;
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            throw new System.NotImplementedException();
        }
    }
}