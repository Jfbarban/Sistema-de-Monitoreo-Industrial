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

        public void AgregarWidgetExterno(WidgetBaseViewModel widget)
        {
            // Configurar borrado
            widget.OnRemoveRequested = (w) => Widgets.Remove(w);
            Widgets.Add(widget);
        }

        private async Task LoopDatos()
        {
            var datos = await _dbService.ObtenerUltimaTelemetria();
            if (datos == null || !datos.Any()) return;

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