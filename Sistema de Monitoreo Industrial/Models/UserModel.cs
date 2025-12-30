namespace Sistema_de_Monitoreo_Industrial.Models
{
    public enum UserRole
    {
        Operador,      // Solo ve y reconoce alarmas
        Mantenimiento, // Puede editar umbrales
        Administrador  // Control total y gestión de usuarios
    }

    public class User
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; } // Nunca texto plano
        public UserRole Role { get; set; }
        public DateTime LastLogin { get; set; }
    }
}