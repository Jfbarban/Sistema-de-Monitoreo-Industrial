using System;

namespace Sistema_de_Monitoreo_Industrial.Models
{
    public class DatosProduccion
    {
        public DateTime Timestamp { get; set; }
        public string NodoOrigen { get; set; } // Ejemplo: "Envasado-01"

        // Variables de Telemetría
        public double Temperatura { get; set; }
        public double Latencia { get; set; }
        public double OEE { get; set; }
        public int ConteoPiezas { get; set; }
        public double Vibracion { get; set; }
        public double ConsumoKwh { get; set; }

        // Variables de Estado y Trazabilidad
        public string Estado { get; set; } // "PRODUCCION", "FALLA", "STOP"
        public string Lote { get; set; }   // <-- AGREGADO: Para identificar la orden de producción
    }
}