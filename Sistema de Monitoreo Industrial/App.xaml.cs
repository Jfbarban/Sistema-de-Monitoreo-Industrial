using Sistema_de_Monitoreo_Industrial.Services; // Asegúrate de tener la ruta de tu servicio
using System;
using System.Windows;

namespace Sistema_de_Monitoreo_Industrial
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Mantenimiento inmediato al arrancar
            // Esto resuelve el caso de que la app se cerró ayer y se abre hoy
            LogMaintenanceService.EjecutarMantenimiento();

            // 2. Programar el mantenimiento automático para la medianoche
            ConfigurarCicloMantenimiento();
        }

        private void ConfigurarCicloMantenimiento()
        {
            var ahora = DateTime.Now;
            var proximaMedianoche = ahora.AddDays(1).Date;
            var tiempoEspera = proximaMedianoche - ahora;

            // Usamos System.Timers.Timer porque es más preciso para tiempos largos
            var timerMantenimiento = new System.Timers.Timer(tiempoEspera.TotalMilliseconds);
            timerMantenimiento.AutoReset = false;

            timerMantenimiento.Elapsed += (s, ev) =>
            {
                // Ejecuta la rotación de logs
                LogMaintenanceService.EjecutarMantenimiento();

                // Importante: Volver a programar para la siguiente medianoche
                ConfigurarCicloMantenimiento();

                // Liberar el timer actual
                timerMantenimiento.Dispose();
            };

            timerMantenimiento.Start();
        }
    }
}