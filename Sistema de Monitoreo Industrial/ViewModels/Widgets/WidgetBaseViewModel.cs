using Sistema_de_Monitoreo_Industrial.Core;
using Sistema_de_Monitoreo_Industrial.Models;
using System;
using System.Reflection;
using System.Windows.Input;

namespace Sistema_de_Monitoreo_Industrial.ViewModels.Widgets
{
    public abstract class WidgetBaseViewModel : BindableBase
    {
        private string _title;
        public string Title { get => _title; set => SetProperty(ref _title, value); }

        public string RobotId { get; set; }      // Filtro ID
        public string PropertyName { get; set; } // Filtro Variable

        // Comando para cerrar este widget
        public ICommand RemoveCommand { get; private set; }
        public Action<WidgetBaseViewModel> OnRemoveRequested;

        public WidgetBaseViewModel(string title, string robotId, string propertyName)
        {
            Title = title;
            RobotId = robotId;
            PropertyName = propertyName;
            RemoveCommand = new RelayCommand(_ => OnRemoveRequested?.Invoke(this));
        }

        // Método abstracto que las gráficas específicas implementarán
        public abstract void Update(DatosProduccion dato);

        // Magia: Obtiene el valor de "Temperatura" o "OEE" usando el nombre string
        protected double ExtractValue(DatosProduccion dato)
        {
            var prop = typeof(DatosProduccion).GetProperty(PropertyName);
            if (prop == null) return 0.0;

            var val = prop.GetValue(dato);

            // Lógica para convertir el texto del Estado en número para la gráfica
            if (PropertyName == "Estado")
            {
                string status = val?.ToString().ToUpper() ?? "";
                if (status == "PRODUCCION") return 1.0;
                if (status == "MANTENIMIENTO") return 0.5;
                return 0.0;
            }

            return Convert.ToDouble(val ?? 0.0);
        }
    }
}