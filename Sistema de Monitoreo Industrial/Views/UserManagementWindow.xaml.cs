using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Sistema_de_Monitoreo_Industrial.Models;
using Sistema_de_Monitoreo_Industrial.Services;

namespace Sistema_de_Monitoreo_Industrial.Views
{
    public partial class UserManagementWindow : Window
    {
        private readonly SecurityService _securityService = new SecurityService();

        // El comando que escuchará el botón de eliminar
        public ICommand EliminarCommand { get; }

        public UserManagementWindow()
        {
            InitializeComponent();

            // 1. Inicializar el comando ANTES de cargar los datos
            EliminarCommand = new RelayCommand<User>(EliminarUsuario);

            // 2. Establecer DataContext para que el Binding del botón funcione
            this.DataContext = this;

            CargarUsuarios();
        }

        private void CargarUsuarios()
        {
            var usuarios = _securityService.CargarUsuarios();
            dgUsuarios.ItemsSource = null;
            dgUsuarios.ItemsSource = usuarios;
        }

        private void EliminarUsuario(User user)
        {
            if (user == null) return;

            ConfirmDialog aviso;

            if (user.Username.ToLower() == "admin")
            {
                aviso = new ConfirmDialog("No se puede eliminar al administrador raíz.");
                aviso.Owner = this; // Centrar sobre la ventana de configuración
                aviso.ConfigurarComoAviso(); // Oculta el botón cancelar y cambia texto a 'ENTENDIDO'
                aviso.ShowDialog();

                return;
            }

            aviso = new ConfirmDialog($"¿Eliminar usuario {user.Username}?");
            aviso.Owner = this; // Centrar sobre la ventana de configuración

            if (aviso.ShowDialog() == true)
            {
                var usuarios = _securityService.CargarUsuarios();
                var userParaBorrar = usuarios.Find(u => u.Username == user.Username);
                if (userParaBorrar != null)
                {
                    usuarios.Remove(userParaBorrar);
                    _securityService.GuardarUsuarios(usuarios);
                    CargarUsuarios();
                }
            }
        }

        private void BtnNuevoUsuario_Click(object sender, RoutedEventArgs e)
        {
            AddUserWindow win = new AddUserWindow();
            win.Owner = this; // Centrado sobre la ventana de gestión

            if (win.ShowDialog() == true)
            {
                // Si se registró con éxito, recargamos la tabla de la ventana principal de gestión
                CargarUsuarios();
            }
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e) => this.Close();
    }

    // Clase necesaria para manejar los comandos sin "código directo"
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        public RelayCommand(Action<T> execute) => _execute = execute;
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) => _execute((T)parameter);
        public event EventHandler CanExecuteChanged;
    }
}