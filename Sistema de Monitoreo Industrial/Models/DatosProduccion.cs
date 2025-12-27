using System;

namespace Sistema_de_Monitoreo_Industrial.Models
{
    public class DatosProduccion
    {
        public DateTime Timestamp { get; set; }
        public string NodoOrigen { get; set; }

        // Aquí guardaremos CUALQUIER campo que venga de InfluxDB
        public Dictionary<string, double> Metricas { get; set; } = new Dictionary<string, double>();
    }
}