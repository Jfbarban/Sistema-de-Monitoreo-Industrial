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
                MessageBox.Show("Por favor complete todos los campos.", "Aviso");
                return;
            }

            var usuarios = _securityService.CargarUsuarios();

            // 2. Verificar si ya existe
            if (usuarios.Any(u => u.Username.ToLower() == user.ToLower()))
            {
                MessageBox.Show("Este nombre de usuario ya está registrado.", "Error");
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

            MessageBox.Show($"Usuario {user} registrado con éxito.", "Éxito");
            this.DialogResult = true;
            this.Close();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}