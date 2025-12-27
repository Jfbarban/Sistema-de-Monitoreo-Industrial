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
                        var dato = new DatosProduccion
                        {
                            Timestamp = record.GetTime()?.ToDateTimeUtc().ToLocalTime() ?? DateTime.Now,
                            NodoOrigen = record.GetValueByKey("id_robot")?.ToString() ?? "N/A"
                        };

                        foreach (var entry in record.Values)
                        {
                            // Saltamos metadatos del motor de Influx
                            if (entry.Key.StartsWith("_") || entry.Key == "id_robot" || entry.Key == "result" || entry.Key == "table")
                                continue;

                            // Guardamos el objeto original (puede ser double o string)
                            dato.Metricas[entry.Key] = entry.Value;
                        }
                        lista.Add(dato);
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

        //Metodo para obtener los robots que existen en la base de datos
        public async Task<List<string>> ObtenerRobotsDisponibles()
        {
            var robots = new List<string>();
            try
            {
                using var client = new InfluxDBClient(_settings.InfluxUrl, _settings.InfluxToken);
                // Consulta Flux para obtener valores únicos de la etiqueta 'id_robot'
                string query = $@"import ""influxdata/influxdb/schema""
                          schema.tagValues(bucket: ""{_settings.InfluxBucket}"", tag: ""id_robot"")";

                var tables = await client.GetQueryApi().QueryAsync(query, _settings.InfluxOrg);

                foreach (var record in tables.SelectMany(t => t.Records))
                {
                    robots.Add(record.GetValue().ToString());
                }
            }
            catch (Exception ex) { /* Manejar error en consola */ }
            return robots.Any() ? robots : new List<string> { "Sin Robots Detectados" };
        }

        //Metodo para obtener con valores únicos de campos (variables) para un robot específico
        public async Task<List<string>> ObtenerCamposPorRobot(string robotId)
        {
            var campos = new List<string>();
            try
            {
                using var client = new InfluxDBClient(_settings.InfluxUrl, _settings.InfluxToken);

                // Esta consulta busca todos los nombres de columnas (_field) para un robot específico
                string query = $@"
            from(bucket: ""{_settings.InfluxBucket}"")
            |> range(start: -30d) 
            |> filter(fn: (r) => r[""id_robot""] == ""{robotId}"")
            |> keep(columns: [""_field""])
            |> distinct(column: ""_field"")";

                var tables = await client.GetQueryApi().QueryAsync(query, _settings.InfluxOrg);

                foreach (var record in tables.SelectMany(t => t.Records))
                {
                    campos.Add(record.GetValue().ToString());
                }
            }
            catch (Exception ex) { /* Log en consola */ }
            return campos;
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