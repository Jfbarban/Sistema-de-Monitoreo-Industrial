namespace Sistema_de_Monitoreo_Industrial.Models
{
    public class ReglaAlerta
    {
        public string Variable { get; set; }      // Ejemplo: "Temperatura"
        public double UmbralMaximo { get; set; }  // Ejemplo: 75.0
        public string Unidad { get; set; }        // Ejemplo: "°C"
        public bool EmailNotificado { get; set; } = false; // Control de flujo para SMTP
    }
}