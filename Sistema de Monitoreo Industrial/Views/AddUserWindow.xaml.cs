using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Sistema_de_Monitoreo_Industrial.Models;
using Sistema_de_Monitoreo_Industrial.Services;

namespace Sistema_de_Monitoreo_Industrial.Views
{
    public partial class AddUserWindow : Window
    {
        private readonly SecurityService _securityService = new SecurityService();

        public AddUserWindow()
        {
            InitializeComponent();

            // Cargamos los valores del Enum directamente al ComboBox
            cbRoles.ItemsSource = System.Enum.GetValues(typeof(UserRole));

            // Seleccionamos el primero por defecto (opcional)
            cbRoles.SelectedIndex = 0;
        }

        private void BtnRegistrar_Click(object sender, RoutedEventArgs e)
        {
            string user = txtUsername.Text.Trim();
            string pass = txtPassword.Password;
            string? rolStr = cbRoles.SelectedItem.ToString();

            // 1. Validaciones básicas
            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                ConfirmDialog aviso = new ConfirmDialog("Por favor complete todos los campos.");
                aviso.Owner = this; // Centrar sobre la ventana de configuración
                aviso.ConfigurarComoAviso(); // Oculta el botón cancelar y cambia texto a 'ENTENDIDO'
                aviso.ShowDialog();

                return;
            }

            var usuarios = _securityService.CargarUsuarios();

            // 2. Verificar si ya existe
            if (usuarios.Any(u => u.Username.ToLower() == user.ToLower()))
            {
                ConfirmDialog aviso = new ConfirmDialog("Este nombre de usuario ya está registrado.");
                aviso.Owner = this; // Centrar sobre la ventana de configuración
                aviso.ConfigurarComoAviso(); // Oculta el botón cancelar y cambia texto a 'ENTENDIDO'
                aviso.ShowDialog();

                return;
            }

            // 3. Crear nuevo usuario (ajusta según tu Enum de roles)
            User nuevoUsuario = new User
            {
                Username = user,
                PasswordHash = _securityService.GenerarHash(pass),
                // Asumiendo que tu Enum tiene estos valores
                Role = (UserRole)System.Enum.Parse(typeof(UserRole), rolStr)
            };

            usuarios.Add(nuevoUsuario);
            _securityService.GuardarUsuarios(usuarios);

            ConfirmDialog aviso = new ConfirmDialog($"Usuario {user} registrado con éxito.");
            aviso.Owner = this; // Centrar sobre la ventana de configuración
            aviso.ConfigurarComoAviso(); // Oculta el botón cancelar y cambia texto a 'ENTENDIDO'
            aviso.ShowDialog();

            this.DialogResult = true;
            this.Close();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}