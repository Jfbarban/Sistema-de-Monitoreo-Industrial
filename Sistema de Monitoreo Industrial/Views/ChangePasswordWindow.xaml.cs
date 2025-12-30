using System.Linq;
using System.Windows;
using Sistema_de_Monitoreo_Industrial.Services;

namespace Sistema_de_Monitoreo_Industrial.Views
{
    public partial class ChangePasswordWindow : Window
    {
        private readonly SecurityService _securityService = new SecurityService();

        private bool _Clicked1 = false;

        private bool _Clicked2 = false;

        public ChangePasswordWindow()
        {
            InitializeComponent();
        }

        private void btn1Checked(object sender, RoutedEventArgs e)
        {
            if (_Clicked1)
            {
                _Clicked1 = false;
                txtPass1.Password = txtPass1Visible.Text;
                txtPass1Visible.Visibility = Visibility.Collapsed;
                txtPass1.Visibility = Visibility.Visible;
                btnPass1Visibility.Content = "👁";

                txtPass1.Focus();
            }
            else
            {
                _Clicked1 = true;

                txtPass1Visible.Text = txtPass1.Password;
                txtPass1.Visibility = Visibility.Collapsed;
                txtPass1Visible.Visibility = Visibility.Visible;
                btnPass1Visibility.Content = "👁‍🗨";

                txtPass1Visible.Focus();
                txtPass1Visible.SelectionStart = txtPass1Visible.Text.Length;
            }
        }

        private void btn2Checked(object sender, RoutedEventArgs e)
        {
            if (_Clicked2)
            {
                _Clicked2 = false;
                txtPass2.Password = txtPass2Visible.Text;
                txtPass2Visible.Visibility = Visibility.Collapsed;
                txtPass2.Visibility = Visibility.Visible;
                btnPass2Visibility.Content = "👁";

                txtPass2.Focus();
            }
            else
            {
                _Clicked2 = true;

                txtPass2Visible.Text = txtPass2.Password;
                txtPass2.Visibility = Visibility.Collapsed;
                txtPass2Visible.Visibility = Visibility.Visible;
                btnPass2Visibility.Content = "👁‍🗨";

                txtPass2Visible.Focus();
                txtPass2Visible.SelectionStart = txtPass2Visible.Text.Length;
            }
        }

        // Se ejecuta cada vez que el usuario escribe en CUALQUIERA de los dos campos
        private void Password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            string p1 = txtPass1.Password;
            string p2 = txtPass2.Password;

            // --- VALIDACIÓN 1: LONGITUD ---
            bool longitudValida = p1.Length >= 4;

            if (string.IsNullOrEmpty(p1))
            {
                txtValidaLongitud.Visibility = Visibility.Collapsed;
            }
            else if (!longitudValida)
            {
                txtValidaLongitud.Text = "⚠ Mínimo 4 caracteres (Llevas " + p1.Length + ")";
                txtValidaLongitud.Foreground = System.Windows.Media.Brushes.Orange; // Color de advertencia
                txtValidaLongitud.Visibility = Visibility.Visible;
            }
            else
            {
                txtValidaLongitud.Text = "✔ Longitud correcta";
                txtValidaLongitud.Foreground = System.Windows.Media.Brushes.LimeGreen;
                txtValidaLongitud.Visibility = Visibility.Visible;
            }

            // --- VALIDACIÓN 2: COINCIDENCIA ---
            bool coinciden = (p1 == p2) && !string.IsNullOrEmpty(p2);

            if (string.IsNullOrEmpty(p2))
            {
                txtValidaCoincidencia.Visibility = Visibility.Collapsed;
            }
            else if (!coinciden)
            {
                txtValidaCoincidencia.Text = "❌ Las contraseñas no coinciden";
                txtValidaCoincidencia.Foreground = System.Windows.Media.Brushes.Red;
                txtValidaCoincidencia.Visibility = Visibility.Visible;
            }
            else
            {
                txtValidaCoincidencia.Text = "✔ Las contraseñas coinciden";
                txtValidaCoincidencia.Foreground = System.Windows.Media.Brushes.LimeGreen;
                txtValidaCoincidencia.Visibility = Visibility.Visible;
            }

            // --- ACTIVAR BOTÓN ---
            // Solo se habilita si ambas condiciones son verdaderas
            btnGuardar.IsEnabled = longitudValida && coinciden;
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            var user = SessionManager.UsuarioActual;
            if (user == null) return;

            var usuarios = _securityService.CargarUsuarios();
            var userDb = usuarios.FirstOrDefault(u => u.Username == user.Username);

            if (userDb != null)
            {
                // Aplicamos el Hash a la nueva clave
                userDb.PasswordHash = _securityService.GenerarHash(txtPass1.Password);
                _securityService.GuardarUsuarios(usuarios);

                MessageBox.Show("Contraseña actualizada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}