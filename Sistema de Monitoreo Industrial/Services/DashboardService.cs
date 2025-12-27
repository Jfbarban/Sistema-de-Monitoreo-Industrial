using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Sistema_de_Monitoreo_Industrial.Models;

namespace Sistema_de_Monitoreo_Industrial.Services
{
    public class DashboardService
    {
        private readonly string _folderPath;

        public DashboardService()
        {
            // Carpeta en Documentos o en la ruta de ejecución
            _folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Dashboards");
            if (!Directory.Exists(_folderPath)) Directory.CreateDirectory(_folderPath);
        }

        public void Guardar(string nombre, List<ChartWidgetConfig> configs)
        {
            var filePath = Path.Combine(_folderPath, $"{nombre}.json");
            string json = JsonSerializer.Serialize(configs, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }

        public List<string> ListarDashboards()
        {
            return Directory.GetFiles(_folderPath, "*.json")
                            .Select(Path.GetFileNameWithoutExtension)
                            .ToList();
        }

        public List<ChartWidgetConfig> Cargar(string nombre)
        {
            var filePath = Path.Combine(_folderPath, $"{nombre}.json");
            if (!File.Exists(filePath)) return null;
            return JsonSerializer.Deserialize<List<ChartWidgetConfig>>(File.ReadAllText(filePath));
        }
    }
}