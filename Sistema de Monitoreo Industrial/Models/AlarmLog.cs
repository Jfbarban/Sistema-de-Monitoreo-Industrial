using System;
using System.Collections.Generic;
using System.Text;

namespace Sistema_de_Monitoreo_Industrial.Models
{
    public class AlarmLog
    {
        public DateTime Timestamp { get; set; }
        public string RobotId { get; set; }
        public string Variable { get; set; }
        public double Valor { get; set; }
        public double Umbral { get; set; }
        public string Mensaje { get; set; }
        public bool EsInicio { get; set; } // True si la alarma empezó, False si volvió a la normalidad
    }
}
