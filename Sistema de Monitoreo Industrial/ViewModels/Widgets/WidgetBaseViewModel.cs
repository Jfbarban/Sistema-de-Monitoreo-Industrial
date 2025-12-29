using Sistema_de_Monitoreo_Industrial.Core;
using Sistema_de_Monitoreo_Industrial.Models;
using Sistema_de_Monitoreo_Industrial.Services;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace Sistema_de_Monitoreo_Industrial.ViewModels.Widgets
{
    public abstract class WidgetBaseViewModel : BindableBase
    {
        private string _title;
        public string Title { get => _title; set => SetProperty(ref _title, value); }

        public string RobotId { get; set; }     // Filtro ID (Ej: "Envasado-01")
        public string VariableTag { get; set; } // Nombre de la métrica (Ej: "presion", "temperatura")

        public double? Threshold { get; set; } // El límite configurado
        private bool _isAlerting;
        public bool IsAlerting
        {
            get => _isAlerting;
            set { _isAlerting = value; OnPropertyChanged(); }
        }
        private bool _emailSent = false; // Candado para no hacer spam
        public ICommand SilenceCommand { get; private set; }

        public ICommand RemoveCommand { get; private set; }
        public Action<WidgetBaseViewModel> OnRemoveRequested;

        // EL CONSTRUCTOR DEBE RECIBIR Y ASIGNAR LA VARIABLE
        public WidgetBaseViewModel(string title, string robotId, string variableTag)
        {
            Title = title;
            RobotId = robotId;
            VariableTag = variableTag; // <-- AQUÍ SE RECIBE EL VALOR DEL COMBOBOX

            RemoveCommand = new RelayCommand(_ => OnRemoveRequested?.Invoke(this));
            SilenceCommand = new RelayCommand(_ => SilenciarAlerta());
        }

        private void SilenciarAlerta()
        {
            IsAlerting = false;
            // Opcional: No reseteamos _emailSent para no volver a enviar correo 
            // hasta que el valor baje y vuelva a subir.
        }

        public void Update(DatosProduccion dato)
        {
            // 1. Verificación de Alerta
            if (Threshold.HasValue && dato.Metricas.TryGetValue(VariableTag, out object valorObj))
            {
                double valorActual = Convert.ToDouble(valorObj);

                // Si superamos el límite
                if (valorActual > Threshold.Value)
                {
                    // Activar parpadeo visual si no estaba activo
                    if (!IsAlerting)
                    {
                        IsAlerting = true;

                        // --- REGISTRO DE INICIO DE ALARMA ---
                        AlarmHistoryService.RegistrarAlarmaAsync(new AlarmLog
                        {
                            Timestamp = DateTime.Now,
                            RobotId = this.RobotId,
                            Variable = this.VariableTag,
                            Valor = valorActual,
                            Umbral = Threshold.Value,
                            Mensaje = $"Límite excedido en {Title}",
                            EsInicio = true
                        });
                    }

                    // Enviar correo si no se ha enviado ya en este ciclo de error
                    if (!_emailSent)
                    {
                        NotificationService.EnviarEmailGmailAsync(
                            $"ALERTA: {Title}",
                            $"La variable {VariableTag} en {RobotId} alcanzó {valorActual}. Límite: {Threshold.Value}");

                        _emailSent = true; // Bloqueamos futuros correos
                    }
                }
                else
                {
                    // Si el valor vuelve a la normalidad, reseteamos el sistema
                    if (valorActual < (Threshold.Value * 0.95)) // Histéresis del 5% opcional
                    {
                        IsAlerting = false;

                        IsAlerting = false;

                        // --- REGISTRO DE VUELTA A LA NORMALIDAD ---
                        AlarmHistoryService.RegistrarAlarmaAsync(new AlarmLog
                        {
                            Timestamp = DateTime.Now,
                            RobotId = this.RobotId,
                            Variable = this.VariableTag,
                            Valor = valorActual,
                            Umbral = Threshold.Value,
                            Mensaje = $"Valor normalizado en {Title}",
                            EsInicio = false
                        });

                        _emailSent = false; // Permitimos enviar correo de nuevo si vuelve a fallar
                    }
                }
            }

            // 2. Procesamiento específico del gráfico (Signal, Gauge, etc.)
            ProcesarDato(dato);
        }


        public abstract void ProcesarDato(DatosProduccion dato);

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