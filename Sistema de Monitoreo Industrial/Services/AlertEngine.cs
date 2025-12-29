using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Sistema_de_Monitoreo_Industrial.Models;

namespace Sistema_de_Monitoreo_Industrial.Services
{
    public class AlertEngine
    {
        private readonly string _pathLogs = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs_alertas.json");
        private List<ReglaAlerta> _reglas;

        // Evento para que la MainWindow se entere de la alerta
        public Action<string, string> OnAlertTriggered;

        public AlertEngine()
        {
            // Configuración inicial de umbrales industriales
            _reglas = new List<ReglaAlerta>
            {
                new ReglaAlerta { Variable = "Temperatura", UmbralMaximo = 40.0, Unidad = "°C" },
                new ReglaAlerta { Variable = "Vibracion", UmbralMaximo = 8.5, Unidad = "mm/s" }
            };
        }

        public void ProcesarTelemetria(List<DatosProduccion> datos)
        {
            if (datos == null || !datos.Any()) return;
            var ultimoDato = datos.First(); // Analizamos la muestra más reciente

            foreach (var regla in _reglas)
            {
                if (ultimoDato.Metricas.TryGetValue(regla.Variable, out object valorObj))
                {
                    double valorActual = Convert.ToDouble(valorObj);

                    if (valorActual > regla.UmbralMaximo)
                    {
                        // 1. Persistencia en JSON
                        RegistrarEnHistorial(ultimoDato, regla, valorActual);

                        // 2. Notificar a la UI
                        OnAlertTriggered?.Invoke(regla.Variable, $"{valorActual}{regla.Unidad} en {ultimoDato.NodoOrigen}");

                        // 3. Notificar por Email (Solo una vez por incidencia)
                        if (!regla.EmailNotificado)
                        {
                            string cuerpo = $"<h3>Alerta Crítica</h3><p>Variable: {regla.Variable}<br>Valor: {valorActual}{regla.Unidad}</p>";
                            NotificationService.EnviarEmailGmailAsync($"[ALERTA] {regla.Variable} fuera de rango", cuerpo);
                            regla.EmailNotificado = true;
                        }
                    }
                    else
                    {
                        // Si el valor vuelve a la normalidad, permitimos futuros correos
                        regla.EmailNotificado = false;
                    }
                }
            }
        }

        private void RegistrarEnHistorial(DatosProduccion dato, ReglaAlerta regla, double valor)
        {
            try
            {
                var entry = new { T = DateTime.Now, Id = dato.NodoOrigen, Var = regla.Variable, Val = valor };
                List<object> logs = File.Exists(_pathLogs)
                    ? JsonSerializer.Deserialize<List<object>>(File.ReadAllText(_pathLogs))
                    : new List<object>();

                logs.Add(entry);
                File.WriteAllText(_pathLogs, JsonSerializer.Serialize(logs));
            }
            catch { }
        }
    }
}