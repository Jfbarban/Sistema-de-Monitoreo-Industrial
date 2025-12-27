using Sistema_de_Monitoreo_Industrial.Models;
using System;

namespace Sistema_de_Monitoreo_Industrial.ViewModels.Widgets
{
    public class WidgetSignalViewModel : WidgetBaseViewModel
    {
        public double[] DataBuffer { get; private set; } = new double[50];

        // Evento para avisar al UserControl que redibuje
        public event Action RequestRefresh;

        public WidgetSignalViewModel(string title, string robotId, string propertyName) 
            : base(title, robotId, propertyName) { }

        public override void Update(DatosProduccion dato)
        {
            // Filtro por Robot
            if (!string.IsNullOrEmpty(RobotId) && !dato.NodoOrigen.Contains(RobotId)) return;

            // Extraer valor (Temperatura, OEE, etc.)
            double val = ExtractValue(dato);

            // DESPLAZAMIENTO: Mover datos a la izquierda
            Array.Copy(DataBuffer, 1, DataBuffer, 0, DataBuffer.Length - 1);

            // Insertar el nuevo dato al final
            DataBuffer[DataBuffer.Length - 1] = val;

            // Notificar a la vista para refrescar
            RequestRefresh?.Invoke();
        }
    }
}