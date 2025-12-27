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

        public string RobotId { get; set; }     // Filtro ID (Ej: "Envasado-01")
        public string VariableTag { get; set; } // Nombre de la métrica (Ej: "presion", "temperatura")

        public ICommand RemoveCommand { get; private set; }
        public Action<WidgetBaseViewModel> OnRemoveRequested;

        // EL CONSTRUCTOR DEBE RECIBIR Y ASIGNAR LA VARIABLE
        public WidgetBaseViewModel(string title, string robotId, string variableTag)
        {
            Title = title;
            RobotId = robotId;
            VariableTag = variableTag; // <-- AQUÍ SE RECIBE EL VALOR DEL COMBOBOX

            RemoveCommand = new RelayCommand(_ => OnRemoveRequested?.Invoke(this));
        }

        public abstract void Update(DatosProduccion dato);

        protected double ExtractValue(DatosProduccion dato)
        {
            // Ahora VariableTag ya no será nulo porque viene desde el constructor
            if (dato == null || string.IsNullOrEmpty(VariableTag)) return 0;

            if (dato.Metricas != null && dato.Metricas.TryGetValue(VariableTag, out double valor))
            {
                return valor;
            }

            return 0;
        }
    }
}