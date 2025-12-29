using Sistema_de_Monitoreo_Industrial.Models;
using System;

namespace Sistema_de_Monitoreo_Industrial.ViewModels.Widgets
{
    public class WidgetStatusViewModel : WidgetBaseViewModel
    {
        public double[] StatusHistory { get; private set; } = new double[50];
        public event Action RequestRefresh;

        public WidgetStatusViewModel(string title, string robotId, string prop) : base(title, robotId, prop) { }

        public override void ProcesarDato(DatosProduccion dato)
        {
            if (!string.IsNullOrEmpty(RobotId) && !dato.NodoOrigen.Contains(RobotId)) return;

            double val = ExtractValue(dato); // Convierte "PRODUCCION" -> 1.0, etc.

            // DESPLAZAMIENTO: Mover historial a la izquierda
            Array.Copy(StatusHistory, 1, StatusHistory, 0, StatusHistory.Length - 1);

            // Insertar el nuevo estado al final
            StatusHistory[StatusHistory.Length - 1] = val;

            RequestRefresh?.Invoke();
        }
    }
}