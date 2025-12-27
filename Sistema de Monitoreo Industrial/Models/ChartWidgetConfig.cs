using System;
using System.Collections.Generic;
using System.Text;

namespace Sistema_de_Monitoreo_Industrial.Models
{
    public class ChartWidgetConfig
    {
        public string Title { get; set; }
        public string ChartType { get; set; } // Signal, Gauge, Bar, Status
        public string VariableTag { get; set; } // Temperatura, OEE, etc.
        public string RobotId { get; set; }
    }
}
