using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace wpfMovieArrangement
{
    class RowColorConverterKoreanPorno : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int status = System.Convert.ToInt32(value);

            if (status == 1)
                return new LinearGradientBrush(Colors.DimGray, Colors.DimGray, 45);

            if (status == 2)
                return new LinearGradientBrush(Colors.PaleGreen, Colors.PaleGreen, 45);

            return new LinearGradientBrush(Colors.White, Colors.White, 45);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
