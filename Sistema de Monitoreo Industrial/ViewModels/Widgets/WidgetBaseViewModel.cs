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
            if (dato == null || string.IsNullOrEmpty(VariableTag)) return 0;

            if (dato.Metricas.TryGetValue(VariableTag, out object valorRaw))
            {
                // 1. Si ya es un número, lo convertimos directamente
                if (valorRaw is double || valorRaw is float || valorRaw is int || valorRaw is long)
                {
                    return Convert.ToDouble(valorRaw);
                }

                // 2. Si es un string, intentamos convertirlo
                string stringVal = valorRaw?.ToString() ?? "";

                // Intento de parseo numérico por si el string es "25.5"
                if (double.TryParse(stringVal, out double parseado))
                {
                    return parseado;
                }

                // 3. MAPEO DINÁMICO DE ESTADOS (Lógica de respaldo para texto)
                // Esto permite que cualquier texto se convierta en un nivel para la gráfica
                return MapearTextoANumero(stringVal);
            }

            return 0;
        }

        private double MapearTextoANumero(string texto)
        {
            switch (texto.ToUpper())
            {
                case "PRODUCCION": case "OK": case "ACTIVO": return 1.0;
                case "MANTENIMIENTO": case "ADVERTENCIA": return 0.5;
                case "FALLA": case "ERROR": case "PARO": return 0.0;
                default:
                    // Si es un texto desconocido pero tiene contenido, devolvemos un valor genérico
                    return string.IsNullOrEmpty(texto) ? 0.0 : 0.8;
            }
        }
    }
}