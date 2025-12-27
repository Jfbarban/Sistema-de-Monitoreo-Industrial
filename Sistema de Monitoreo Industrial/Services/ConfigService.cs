using Sistema_de_Monitoreo_Industrial.Models;
using System;
using System.IO;
using System.Text.Json;

namespace Sistema_de_Monitoreo_Industrial.Services
{
    public static class ConfigService
    {
        private static readonly string FileName = "config.json";
        // Obtenemos la ruta en la carpeta de ejecución
        private static readonly string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileName);

        public static AppSettings Load()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    // Si no existe, creamos uno por defecto
                    var defaultConfig = new AppSettings();
                    Save(defaultConfig);
                    return defaultConfig;
                }

                string jsonString = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<AppSettings>(jsonString) ?? new AppSettings();
            }
            catch (Exception)
            {
                return new AppSettings(); // En caso de error, devolver valores seguros
            }
        }

        public static void Save(AppSettings settings)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true }; // Para que el JSON sea legible
                string jsonString = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(FilePath, jsonString);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al guardar la configuración: {ex.Message}");
            }
        }
    }
}
