using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Input;
using Sistema_de_Monitoreo_Industrial.Core;
using Sistema_de_Monitoreo_Industrial.Models;
using Sistema_de_Monitoreo_Industrial.Services;
using Sistema_de_Monitoreo_Industrial.ViewModels.Widgets;

namespace Sistema_de_Monitoreo_Industrial.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private readonly DatabaseService _dbService;
        private readonly DispatcherTimer _timer;

        private bool _isConnected = false;
        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                _isConnected = value;
                OnPropertyChanged(nameof(IsConnected));
            }
        }

        public ObservableCollection<WidgetBaseViewModel> Widgets { get; set; } = new ObservableCollection<WidgetBaseViewModel>();

        public ICommand OpenAddWindowCommand { get; private set; }

        public MainViewModel()
        {
            _dbService = new DatabaseService();

            // Comando para abrir ventana
            OpenAddWindowCommand = new RelayCommand(_ => AbrirVentanaConfig());

            // Timer
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += async (s, e) => await LoopDatos();
            _timer.Start();
        }

        private void AbrirVentanaConfig()
        {
            // Lógica ejecutada desde la View (Code Behind) usualmente, 
            // pero aquí lanzaremos evento o se manejará en MainWindow.xaml.cs
            // (Ver abajo en MainWindow.xaml.cs para la integración fácil)
        }

        public void AgregarWidgetExterno(ChartWidgetConfig config)
        {
            WidgetBaseViewModel nuevoWidgetVm = null;

            // "Fábrica" de Widgets: Según el tipo elegido en la ventana, creamos el VM adecuado
            switch (config.ChartType)
            {
                case "Signal":
                    nuevoWidgetVm = new WidgetSignalViewModel(config.Title, config.RobotId, config.VariableTag);
                    break;
                case "Gauge":
                    nuevoWidgetVm = new WidgetGaugeViewModel(config.Title, config.RobotId, config.VariableTag);
                    break;
                case "Bar":
                    nuevoWidgetVm = new WidgetBarViewModel(config.Title, config.RobotId, config.VariableTag);
                    break;
                case "Status":
                    nuevoWidgetVm = new WidgetStatusViewModel(config.Title, config.RobotId, config.VariableTag);
                    break;
                case "Label":
                    nuevoWidgetVm = new LabelWidgetViewModel(config.Title, config.RobotId, config.VariableTag);
                    break;
            }

            if (nuevoWidgetVm != null)
            {
                // Configuramos la acción de borrado que ya tenías
                nuevoWidgetVm.OnRemoveRequested = (w) => Widgets.Remove(w);

                // Lo añadimos a la colección observable para que aparezca en la UI
                Widgets.Add(nuevoWidgetVm);
            }
        }

        private async Task LoopDatos()
        {
            if (!IsConnected) return;

            var datos = await _dbService.ObtenerUltimaTelemetria();

            if (datos == null && _dbService._Conectado)
            {
                // --- AQUÍ SE DESCONECTA EL SISTEMA ---
                _timer.Stop(); // Detenemos el reloj permanentemente

                // Usamos el Storyboard que arreglamos con el Clone() 
                // para dejar el LED en rojo fijo
                _dbService.LogConsola("SISTEMA", "Motor de telemetría detenido. Intervención requerida.", "#FF0000");

                return;
            }

            // Ordenamos por tiempo descendente
            var dataOrdenada = datos.OrderByDescending(d => d.Timestamp).ToList();

            // Repartir datos a los widgets
            foreach (var widget in Widgets)
            {
                // Buscamos el dato más nuevo que coincida con el ID del widget
                var datoRobot = dataOrdenada.FirstOrDefault(d => d.NodoOrigen.Contains(widget.RobotId));

                if (datoRobot != null)
                {
                    widget.Update(datoRobot);
                }
            }
        }
    }
}