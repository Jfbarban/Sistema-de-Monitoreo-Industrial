using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Sistema_de_Monitoreo_Industrial.Utils
{
    // Convierte el booleano 'EsInicio' en un color (Rojo/Verde)
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool esInicio = (bool)value;
            return esInicio ? new SolidColorBrush(Color.FromRgb(200, 0, 0)) : new SolidColorBrush(Color.FromRgb(0, 150, 0));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
