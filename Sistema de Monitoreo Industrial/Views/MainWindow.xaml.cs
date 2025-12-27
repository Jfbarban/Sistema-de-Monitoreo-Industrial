using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Sistema_de_Monitoreo_Industrial.Services;
using Sistema_de_Monitoreo_Industrial.ViewModels;
using Sistema_de_Monitoreo_Industrial.Views;

namespace Sistema_de_Monitoreo_Industrial.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            // 1. Estado inicial del botón y ViewModel
            btnConectar.IsChecked = false;
            _viewModel = new MainViewModel();
            this.DataContext = _viewModel;

            // 2. Carga inicial de configuración desde el JSON
            var config = ConfigService.Load();

            // 3. Log de inicio rectificado para InfluxDB
            txtConsola.AppendText($"\n[{DateTime.Now:HH:mm:ss}] [SISTEMA] Configuración Vertex-IoT cargada.");
            txtConsola.AppendText($"\n[{DateTime.Now:HH:mm:ss}] [DATABASE] Conectado a Nodo: {config.InfluxUrl}");
            txtConsola.AppendText($"\n[{DateTime.Now:HH:mm:ss}] [DATABASE] Bucket activo: {config.InfluxBucket}");
            txtConsola.AppendText($"\n[{DateTime.Now:HH:mm:ss}] Sistema listo. Estado actual: OFFLINE");

            txtConsola.ScrollToEnd();
        }

        private void BtnConectar_Click(object sender, RoutedEventArgs e)
        {
            var toggle = sender as System.Windows.Controls.Primitives.ToggleButton;
            var vm = this.DataContext as MainViewModel;

            // Obtenemos el Storyboard de alerta por si necesitamos detenerlo
            var sbAlerta = (Storyboard)this.FindResource("AlertaCriticaStoryboard");

            if (toggle.IsChecked == true)
            {
                // --- MODO ONLINE ---
                toggle.Content = "DESCONECTAR";

                // LED Verde (Normal)
                LedOnline.Fill = new SolidColorBrush(Color.FromRgb(0, 255, 0));

                // Texto informativo ajustado a InfluxDB
                txtEstadoConexion.Text = "TELEMETRÍA ACTIVA | INFLUXDB SERIES";
                txtEstadoConexion.Foreground = (SolidColorBrush)Application.Current.Resources["OrangeAccent"];

                if (vm != null) vm.IsConnected = true;

                txtConsola.AppendText($"\n[{DateTime.Now:HH:mm:ss}] [SISTEMA] Sesión iniciada. Consultando series temporales.");
            }
            else
            {
                // --- MODO OFFLINE ---
                toggle.Content = "CONECTAR SISTEMA";

                // Detenemos cualquier alerta roja parpadeante si existía
                sbAlerta.Stop();

                // LED Rojo fijo
                LedOnline.Fill = new SolidColorBrush(Colors.Red);

                txtEstadoConexion.Text = "SISTEMA EN PAUSA | DESCONECTADO";
                txtEstadoConexion.Foreground = new SolidColorBrush(Colors.Gray);

                if (vm != null) vm.IsConnected = false;

                txtConsola.AppendText($"\n[{DateTime.Now:HH:mm:ss}] [ADVERTENCIA] El usuario ha detenido el monitoreo.");
            }

            txtConsola.ScrollToEnd();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            AddChartWindow dialog = new AddChartWindow();
            dialog.Owner = this;

            if (dialog.ShowDialog() == true)
            {
                // Esto es un ChartWidgetConfig
                var config = dialog.CreatedWidget;

                if (config != null)
                {
                    // Ahora el VM procesa la configuración y crea el objeto visual
                    _viewModel.AgregarWidgetExterno(config);
                }
            }
        }

        private void btnConfig_Click(object sender, RoutedEventArgs e)
        {
            ConfigWindow win = new ConfigWindow();
            win.Owner = this;

            if (win.ShowDialog() == true)
            {
                // Recargamos configuración para aplicar cambios de URL/Token inmediatamente
                var config = ConfigService.Load();

                txtConsola.AppendText($"\n[{DateTime.Now:HH:mm:ss}] [SISTEMA] Configuración actualizada.");
                txtConsola.AppendText($"\n[{DateTime.Now:HH:mm:ss}] [DATABASE] Nueva IP Nodo: {config.InfluxUrl}");
                txtConsola.ScrollToEnd();
            }
        }

        private void BtnLimpiarConsola_Click(object sender, RoutedEventArgs e)
        {
            txtConsola.Clear();
            txtConsola.Text = $"[{DateTime.Now:HH:mm:ss}] Consola reseteada por el operador.";
        }
    }
}