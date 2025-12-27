using System.Windows.Media;
using Sistema_de_Monitoreo_Industrial.Models;

namespace Sistema_de_Monitoreo_Industrial.ViewModels.Widgets
{
    public class LabelWidgetViewModel : WidgetBaseViewModel
    {
        private string _displayText = "ESPERANDO...";
        public string DisplayText
        {
            get => _displayText;
            set => SetProperty(ref _displayText, value);
        }

        private Brush _backgroundBrush = Brushes.DimGray;
        public Brush BackgroundBrush
        {
            get => _backgroundBrush;
            set => SetProperty(ref _backgroundBrush, value);
        }

        public LabelWidgetViewModel(string title, string robotId, string variableTag)
        : base(title, robotId, variableTag)
        {
        }

        public LabelWidgetViewModel()
        : base("Sin Título", "Robot-000", "Variable")
        {
            // Este constructor se usará SOLO cuando abras el XAML en Visual Studio
        }

        public override void Update(DatosProduccion dato)
        {
            // Buscamos la variable en el diccionario dinámico
            if (dato.Metricas.TryGetValue(VariableTag, out object val))
            {
                string text = val?.ToString() ?? "N/A";
                DisplayText = text.ToUpper();

                // Asignamos color según el texto
                ActualizarColor(DisplayText);
            }
        }

        private void ActualizarColor(string text)
        {
            if (text.Contains("OK") || text.Contains("PRODUCCION") || text.Contains("ACTIVO"))
                BackgroundBrush = new SolidColorBrush(Color.FromRgb(39, 174, 96)); // Verde
            else if (text.Contains("FALLA") || text.Contains("ERROR") || text.Contains("PARO"))
                BackgroundBrush = new SolidColorBrush(Color.FromRgb(192, 57, 43)); // Rojo
            else
                BackgroundBrush = GenerarColorPorHash(text); // Color aleatorio consistente para otros textos
        }

        private Brush GenerarColorPorHash(string texto)
        {
            int hash = texto.GetHashCode();
            byte r = (byte)((hash & 0xFF0000) >> 16);
            byte g = (byte)((hash & 0x00FF00) >> 8);
            byte b = (byte)(hash & 0x0000FF);
            return new SolidColorBrush(Color.FromRgb(r, g, b));
        }
    }
}