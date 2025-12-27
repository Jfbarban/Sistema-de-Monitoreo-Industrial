using InfluxDB.Client;
using Sistema_de_Monitoreo_Industrial.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media; // Para acceder a Application.Current

namespace Sistema_de_Monitoreo_Industrial.Services
{
    public class DatabaseService
    {
        private readonly AppSettings _settings;
        private int _reintentosFallidos = 0;
        private const int MaxReintentos = 3;

        public DatabaseService()
        {
            _settings = ConfigService.Load();
        }

        public async Task<List<DatosProduccion>> ObtenerUltimaTelemetria()
        {
            var lista = new List<DatosProduccion>();

            try
            {
                using var client = new InfluxDBClient(_settings.InfluxUrl, _settings.InfluxToken);

                string query = $@"from(bucket: ""{_settings.InfluxBucket}"")
                    |> range(start: -1m)
                    |> filter(fn: (r) => r[""_measurement""] == ""telemetria_farmaceutica"")
                    |> pivot(rowKey:[""_time""], columnKey: [""_field""], valueColumn: ""_value"")
                    |> sort(columns: [""_time""], desc: true)
                    |> limit(n: 20)";

                var tables = await client.GetQueryApi().QueryAsync(query, _settings.InfluxOrg);

                if (tables != null)
                {
                    // Si llegamos aquí, la conexión es exitosa
                    if (_reintentosFallidos > 0)
                    {
                        LogConsola("SISTEMA", "Conexión restablecida con InfluxDB.", "#00FF00");
                        _reintentosFallidos = 0;
                    }

                    foreach (var record in tables.SelectMany(t => t.Records))
                    {
                        lista.Add(new DatosProduccion
                        {
                            Timestamp = record.GetTime()?.ToDateTimeUtc().ToLocalTime() ?? DateTime.Now,
                            NodoOrigen = record.GetValueByKey("id_robot")?.ToString() ?? "N/A",
                            Latencia = Convert.ToDouble(record.GetValueByKey("latencia_ms") ?? 0.0),
                            Temperatura = Convert.ToDouble(record.GetValueByKey("temperatura_motor_C") ?? 0.0),
                            OEE = Convert.ToDouble(record.GetValueByKey("oee") ?? 0.0),
                            ConteoPiezas = Convert.ToInt32(record.GetValueByKey("conteo_piezas") ?? 0),
                            Vibracion = Convert.ToDouble(record.GetValueByKey("vibracion_mm_s") ?? 0.0),
                            ConsumoKwh = Convert.ToDouble(record.GetValueByKey("consumo_kwh") ?? 0.0),
                            Lote = record.GetValueByKey("lote_produccion")?.ToString() ?? "S/L",
                            Estado = record.GetValueByKey("estado_operacional")?.ToString() ?? "FALLA"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _reintentosFallidos++;
                LogConsola("ERROR", $"Fallo en lectura InfluxDB (Intento {_reintentosFallidos}/{MaxReintentos}): {ex.Message}", "#FF4444");

                if (_reintentosFallidos >= MaxReintentos)
                {
                    LogConsola("CRÍTICO", "Se alcanzó el máximo de reintentos. Verifique la configuración de red.", "#FF0000");
                }
            }

            return lista;
        }

        // Método auxiliar para escribir en la consola de la MainWindow
        private void LogConsola(string categoria, string mensaje, string colorHex)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var mainWin = Application.Current.MainWindow as Sistema_de_Monitoreo_Industrial.Views.MainWindow;
                if (mainWin != null)
                {
                    // 1. Escribir en consola
                    string logMsg = $"\n[{DateTime.Now:HH:mm:ss}] [{categoria}] {mensaje}";
                    mainWin.txtConsola.AppendText(logMsg);
                    mainWin.txtConsola.ScrollToEnd();

                    // 2. Control visual del LED según categoría
                    var sb = (System.Windows.Media.Animation.Storyboard)mainWin.FindResource("AlertaCriticaStoryboard");

                    if (categoria == "CRÍTICO")
                    {
                        mainWin.LedOnline.Fill = new SolidColorBrush(Colors.Red);
                        sb.Begin(); // Inicia parpadeo rápido
                    }
                    else if (categoria == "SISTEMA" && _reintentosFallidos == 0)
                    {
                        sb.Stop(); // Detiene parpadeo
                        mainWin.LedOnline.Fill = new SolidColorBrush(Color.FromRgb(0, 255, 0)); // Vuelve a Verde
                    }
                }
            });
        }
    }
}