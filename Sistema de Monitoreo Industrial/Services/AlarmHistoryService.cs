using Sistema_de_Monitoreo_Industrial.Models;
using Sistema_de_Monitoreo_Industrial.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;

namespace Sistema_de_Monitoreo_Industrial.Services
{
    public static class AlarmHistoryService
    {
        private static string rutaArchivo = "alarm_history.json";

        static MainWindow mainWin = Application.Current.MainWindow as Sistema_de_Monitoreo_Industrial.Views.MainWindow;

        public static async Task RegistrarAlarmaAsync(AlarmLog log)
        {
            try
            {
                List<AlarmLog> historial = new List<AlarmLog>();
                if (File.Exists(rutaArchivo))
                {
                    string json = File.ReadAllText(rutaArchivo);
                    historial = JsonSerializer.Deserialize<List<AlarmLog>>(json) ?? new List<AlarmLog>();
                }

                historial.Add(log);

                // Mantener solo los últimos 1000 registros para no saturar
                if (historial.Count > 1000) historial.RemoveAt(0);

                string finalJson = JsonSerializer.Serialize(historial, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(rutaArchivo, finalJson);
            }
            catch (Exception ex)
            {
                mainWin.EscribirEnConsola($"Error al registrar alarma: {ex.Message}");
            }
        }
    }
}
