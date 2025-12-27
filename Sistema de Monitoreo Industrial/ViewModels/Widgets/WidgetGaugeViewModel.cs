using Sistema_de_Monitoreo_Industrial.Models;

namespace Sistema_de_Monitoreo_Industrial.ViewModels.Widgets
{
    public class WidgetGaugeViewModel : WidgetBaseViewModel
    {
        private double _currentValue;
        public double CurrentValue
        {
            get => _currentValue;
            set => SetProperty(ref _currentValue, value);
        }

        public WidgetGaugeViewModel(string title, string robotId, string propertyName)
            : base(title, robotId, propertyName) { }

        public override void Update(DatosProduccion dato)
        {
            if (!string.IsNullOrEmpty(RobotId) && !dato.NodoOrigen.Contains(RobotId)) return;

            CurrentValue = ExtractValue(dato);
        }
    }
}