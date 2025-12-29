using System;
using System.IO;

public static class LogMaintenanceService
{
    // ESTO ES LO QUE FALTABA: El objeto de referencia para el lock
    public static readonly object ArchivoLock = new object();

    private static string _rutaLog = "alarm_history.json";
    private static string _carpetaBackup = "Backups";

    public static void EjecutarMantenimiento()
    {
        // Envolvemos todo en el lock para que si la HistoryWindow está leyendo,
        // el mantenimiento espere a que termine (y viceversa).
        lock (ArchivoLock)
        {
            try
            {
                if (!File.Exists(_rutaLog)) return;

                FileInfo info = new FileInfo(_rutaLog);

                // Regla 1: Si el archivo es de días anteriores, rotar.
                if (info.LastWriteTime.Date < DateTime.Now.Date)
                {
                    RotarArchivo(info.LastWriteTime);
                }
                // Regla 2: Si el archivo pesa más de 10MB
                else if (info.Length > 10 * 1024 * 1024)
                {
                    RotarArchivo(DateTime.Now);
                }
            }
            catch (Exception ex)
            {
                // Loguear error técnico si es necesario
                System.Diagnostics.Debug.WriteLine("Error en mantenimiento: " + ex.Message);
            }
        }
    }

    private static void RotarArchivo(DateTime fechaLog)
    {
        try
        {
            if (!Directory.Exists(_carpetaBackup)) Directory.CreateDirectory(_carpetaBackup);

            string nombreBackup = $"alarm_history_{fechaLog:yyyy-MM-dd}.json";
            string destino = Path.Combine(_carpetaBackup, nombreBackup);

            if (File.Exists(destino))
                destino = destino.Replace(".json", $"_{DateTime.Now:HHmm}.json");

            // Mover el archivo actual al backup
            File.Move(_rutaLog, destino);

            // Crear archivo nuevo vacío con estructura JSON válida
            File.WriteAllText(_rutaLog, "[]");
        }
        catch (IOException ex)
        {
            // Este error ocurre si el archivo está siendo usado por otro proceso externo (ej. antivirus)
            System.Diagnostics.Debug.WriteLine("No se pudo rotar el archivo: " + ex.Message);
        }
    }
}