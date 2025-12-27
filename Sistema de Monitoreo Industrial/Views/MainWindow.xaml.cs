using System;
using System.Windows;
using System.Windows.Media;
using Sistema_de_Monitoreo_Industrial.Services;
using Sistema_de_Monitoreo_Industrial.ViewModels;
using Sistema_de_Monitoreo_Industrial.Views;

namespace Sistema_de_Monitoreo_Industrial.Views
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            btnConectar.IsChecked = false;

            // Inicializamos el ViewModel principal
            _viewModel = new MainViewModel();

            // Establecemos el DataContext para que los Bindings del XAML funcionen
            this.DataContext = _viewModel;

            // Carga inicial de configuración
            var config = ConfigService.Load();

            // 2. Rectificación del Log para InfluxDB
            txtConsola.AppendText($"\n[{DateTime.Now:HH:mm:ss}] [SISTEMA] Configuración Vertex-IoT cargada.");
            txtConsola.AppendText($"\n[{DateTime.Now:HH:mm:ss}] [DATABASE] Nodo InfluxDB detectado: {config.InfluxUrl}");
            txtConsola.AppendText($"\n[{DateTime.Now:HH:mm:ss}] [DATABASE] Bucket activo: {config.InfluxBucket}");

            // Opcional: Escribir el estado inicial en la consola
            txtConsola.AppendText($"\n[{DateTime.Now:HH:mm:ss}] Sistema Vertex-IoT iniciado. Estado: OFFLINE");

            txtConsola.ScrollToEnd();
        }

        private void BtnConectar_Click(object sender, RoutedEventArgs e)
        {
            var toggle = sender as System.Windows.Controls.Primitives.ToggleButton;
            var vm = this.DataContext as MainViewModel;

            if (toggle.IsChecked == true)
            {
                // --- MODO ONLINE ---
                toggle.Content = "DESCONECTAR";

                // 1. Cambiar LED a Verde
                LedOnline.Fill = new SolidColorBrush(Color.FromRgb(0, 255, 0));

                // 2. Cambiar Texto y Color (Naranja)
                txtEstadoConexion.Text = "CONEXIÓN ACTIVA | PROTOCOLO MQTT-PRO";
                txtEstadoConexion.Foreground = (SolidColorBrush)Application.Current.Resources["OrangeAccent"];

                if (vm != null) vm.IsConnected = true;

                txtConsola.AppendText($"\n[{DateTime.Now:HH:mm:ss}] SESIÓN INICIADA: Recibiendo datos de planta.");
            }
            else
            {
                // --- MODO OFFLINE ---
                toggle.Content = "CONECTAR SISTEMA";

                // 1. Cambiar LED a Rojo
                LedOnline.Fill = new SolidColorBrush(Colors.Red);

                // 2. Cambiar Texto y Color (Gris)
                txtEstadoConexion.Text = "SISTEMA OFFLINE | DESCONECTADO";
                txtEstadoConexion.Foreground = new SolidColorBrush(Colors.Gray);

                if (vm != null) vm.IsConnected = false;

                var sb = (System.Windows.Media.Animation.Storyboard)this.FindResource("AlertaCriticaStoryboard");
                sb.Stop();

                LedOnline.Fill = new SolidColorBrush(Colors.Red); // Rojo fijo de "Parado"

                txtConsola.AppendText($"\n[{DateTime.Now:HH:mm:ss}] ADVERTENCIA: Sistema fuera de línea.");
            }

            txtConsola.ScrollToEnd();
        }

        /// <summary>
        /// Manejador del evento clic para el botón "+ AÑADIR GRÁFICO"
        /// </summary>
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            // Creamos la instancia de la ventana de configuración
            AddChartWindow dialog = new AddChartWindow();

            // Establecemos esta ventana como dueña para que el diálogo aparezca centrado sobre ella
            dialog.Owner = this;

            // Mostramos la ventana como un diálogo modal (bloquea la principal hasta cerrar)
            if (dialog.ShowDialog() == true)
            {
                // Si el usuario presionó "AÑADIR", recuperamos el widget creado
                var nuevoWidget = dialog.CreatedWidget;

                if (nuevoWidget != null)
                {
                    // Lo agregamos a la colección observable del ViewModel
                    // WPF detectará este cambio automáticamente y dibujará el widget
                    _viewModel.AgregarWidgetExterno(nuevoWidget);
                }
            }
        }

        private void BtnLimpiarConsola_Click(object sender, RoutedEventArgs e)
        {
            txtConsola.Clear();
            txtConsola.Text = $"[{DateTime.Now:HH:mm:ss}] Consola limpiada.";
        }

        private void btnConfig_Click(object sender, RoutedEventArgs e)
        {
            ConfigWindow win = new ConfigWindow();
            win.Owner = this; // Para que aparezca centrada sobre la principal

            if (win.ShowDialog() == true)
            {
                // Si pulsó guardar, actualizaremos la consola
                txtConsola.AppendText($"\n[{DateTime.Now:HH:mm:ss}] Configuración actualizada y guardada.");
                txtConsola.ScrollToEnd();
            }
        }
    }
}