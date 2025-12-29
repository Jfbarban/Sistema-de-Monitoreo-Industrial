using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace Sistema_de_Monitoreo_Industrial.Utils
{
    // Convierte el booleano 'EsInicio' en texto (CRÍTICO / NORMAL)
    public class BoolToStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool esInicio = (bool)value;
            return esInicio ? "CRÍTICO" : "NORMAL";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
