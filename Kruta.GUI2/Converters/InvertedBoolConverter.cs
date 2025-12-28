using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Kruta.GUI2.Converters
{
    public class InvertedBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b) return !b; // Если НЕ занят (IsBusy=false), то кнопка активна (true)
            return true;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
