using Sistema_de_Monitoreo_Industrial.Models;

namespace Sistema_de_Monitoreo_Industrial.Services
{
    public static class SessionManager
    {
        // El usuario que inició sesión
        public static User UsuarioActual { get; private set; }

        public static void IniciarSesion(User usuario)
        {
            UsuarioActual = usuario;
        }

        public static void CerrarSesion()
        {
            UsuarioActual = null;
        }

        public static bool EstaAutenticado => UsuarioActual != null;

        // Método de conveniencia para verificar permisos rápidamente
        public static bool TienePermiso(UserRole rolRequerido)
        {
            if (!EstaAutenticado) return false;
            return UsuarioActual.Role >= rolRequerido;
        }
    }
}