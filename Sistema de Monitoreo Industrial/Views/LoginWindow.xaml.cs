using System.Windows;
using System.Windows.Input;
using Sistema_de_Monitoreo_Industrial.Services;

namespace Sistema_de_Monitoreo_Industrial.Views
{
    public partial class LoginWindow : Window
    {
        private readonly SecurityService _securityService;

        private bool _Clicked = false;

        public LoginWindow(SecurityService securityService)
        {
            InitializeComponent();
            _securityService = securityService;

            // Foco inicial en el usuario
            this.Activated += (s, e) => txtUser.Focus();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (_securityService.Autenticar(txtUser.Text, txtPass.Password))
            {
                // Si es correcto, devolvemos TRUE para que App.xaml.cs abra la MainWindow
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Credenciales incorrectas. Verifique su usuario y contraseña.",
                                "Error de Acceso", MessageBoxButton.OK, MessageBoxImage.Error);
                txtPass.Clear();
                txtPass.Focus();
            }
        }

        private void btnChecked(object sender, RoutedEventArgs e)
        {
            if (_Clicked)
            {
                _Clicked = false;
                txtPass.Password = txtPassVisible.Text;
                txtPassVisible.Visibility = Visibility.Collapsed;
                txtPass.Visibility = Visibility.Visible;
                btnPassVisibility.Content = "👁";

                txtPass.Focus();
            }
            else
            {
                _Clicked = true;

                txtPassVisible.Text = txtPass.Password;
                txtPass.Visibility = Visibility.Collapsed;
                txtPassVisible.Visibility = Visibility.Visible;
                btnPassVisibility.Content = "👁‍🗨";

                txtPassVisible.Focus();
                txtPassVisible.SelectionStart = txtPassVisible.Text.Length;
            }
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
    }
}