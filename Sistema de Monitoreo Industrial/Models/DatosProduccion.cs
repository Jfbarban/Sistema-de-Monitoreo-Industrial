using System;

namespace Sistema_de_Monitoreo_Industrial.Models
{
    public class DatosProduccion
    {
        public DateTime Timestamp { get; set; }
        public string NodoOrigen { get; set; }

        // Ahora aceptamos 'object', puede ser double, string, bool, etc.
        public Dictionary<string, object> Metricas { get; set; } = new Dictionary<string, object>();
    }
}