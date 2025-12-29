namespace Sistema_de_Monitoreo_Industrial.ViewModels.Widgets
{
    public class WidgetBarViewModel : WidgetBaseViewModel
    {
        private double _valorActual;
        public double ValorActual { get => _valorActual; set => SetProperty(ref _valorActual, value); }

        public event System.Action RequestRefresh;

        public WidgetBarViewModel(string title, string robotId, string prop) : base(title, robotId, prop) { }

        public override void ProcesarDato(Models.DatosProduccion dato)
        {
            if (!string.IsNullOrEmpty(RobotId) && !dato.NodoOrigen.Contains(RobotId)) return;
            ValorActual = ExtractValue(dato);
            RequestRefresh?.Invoke();
        }
    }
}