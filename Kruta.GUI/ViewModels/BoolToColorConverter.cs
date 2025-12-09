using System;
using System.Collections.Generic;
using System.Text;
using System;
using System.Globalization;
using Microsoft.Maui.Graphics;

namespace Kruta.GUI.ViewModels
{
    // Вспомогательный конвертер для XAML
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isMyTurn)
            {
                // Зеленый, если ваш ход (активно)
                return isMyTurn ? Colors.Green : Colors.DarkGray;
            }
            return Colors.DarkGray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
