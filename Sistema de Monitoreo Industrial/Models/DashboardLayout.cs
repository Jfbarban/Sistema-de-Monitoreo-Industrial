using System;
using System.Collections.Generic;
using System.Text;

namespace Sistema_de_Monitoreo_Industrial.Models
{
    // Agrega esto en tu carpeta de Models o al final de MainWindow
    public class DashboardLayout
    {
        public string Nombre { get; set; }
        public List<ChartWidgetConfig> Widgets { get; set; } = new List<ChartWidgetConfig>();
    }
}
