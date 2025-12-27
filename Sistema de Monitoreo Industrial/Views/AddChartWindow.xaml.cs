using Sistema_de_Monitoreo_Industrial.Models;
using Sistema_de_Monitoreo_Industrial.Services;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Sistema_de_Monitoreo_Industrial.Views
{
    public partial class AddChartWindow : Window
    {
        private DatabaseService _dbService = new DatabaseService();

        // Propiedad que leerá la MainWindow tras el ShowDialog
        public ChartWidgetConfig CreatedWidget { get; private set; }

        public AddChartWindow()
        {
            InitializeComponent();
            CargarListasDinamicas();
        }

        private async void CargarListasDinamicas()
        {
            // 1. Cargamos Tipos de Gráficos
            CmbType.ItemsSource = new List<object> {
                new { Nombre = "Señal Temporal (Líneas)", Tag = "Signal" },
                new { Nombre = "Medidor Radial (Gauge)", Tag = "Gauge" },
                new { Nombre = "Producción Acumulada (Barras)", Tag = "Bar" },
                new { Nombre = "Estado Operacional (Status)", Tag = "Status" }
            };
            CmbType.SelectedIndex = 0;

            // 2. Cargamos Variables (Nombres amigables vs Nombres en InfluxDB)
            CmbProp.ItemsSource = new List<object> {
                new { Nombre = "Temperatura Motor", Tag = "Temperatura" },
                new { Nombre = "Eficiencia OEE", Tag = "OEE" },
                new { Nombre = "Latencia de Red", Tag = "Latencia" },
                new { Nombre = "Vibración Eje X", Tag = "Vibracion" },
                new { Nombre = "Conteo de Piezas", Tag = "ConteoPiezas" }
            };
            CmbProp.SelectedIndex = 0;

            // 3. CARGA REALMENTE DINÁMICA: Consultamos los robots existentes en la DB
            var robotsDetectados = await _dbService.ObtenerRobotsDisponibles();
            CmbRobot.ItemsSource = robotsDetectados;

            if (CmbRobot.Items.Count > 0)
                CmbRobot.SelectedIndex = 0;
        }

        private async void CmbRobot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbRobot.SelectedItem == null) return;

            string robotSeleccionado = CmbRobot.SelectedItem.ToString();

            // Mostramos un estado de carga opcional
            CmbProp.IsEnabled = false;

            // Obtenemos los campos reales de la DB para ese robot
            var campos = await _dbService.ObtenerCamposPorRobot(robotSeleccionado);

            // Mapeamos a una lista de objetos para mantener la estructura Nombre/Tag
            // Aquí puedes usar un diccionario para poner nombres bonitos si lo deseas, 
            // pero por defecto usamos el nombre técnico de la DB.
            CmbProp.ItemsSource = campos.Select(c => new {
                Nombre = FormatearNombreCampo(c),
                Tag = c
            }).ToList();

            CmbProp.SelectedIndex = 0;
            CmbProp.IsEnabled = true;
        }

        // Método simple para que "temperatura_motor" se vea como "Temperatura Motor"
        private string FormatearNombreCampo(string tecnico)
        {
            if (string.IsNullOrEmpty(tecnico)) return tecnico;
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo
                   .ToTitleCase(tecnico.Replace("_", " ").ToLower());
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e) => this.Close();

        private void CmbType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) { /* Lógica de UI */ }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validaciones básicas
            if (string.IsNullOrWhiteSpace(TxtTitle.Text) || CmbRobot.SelectedItem == null)
            {
                // Aquí podrías usar tu ConfirmDialog como aviso
                return;
            }

            // 2. Creamos el objeto con la selección del usuario
            CreatedWidget = new ChartWidgetConfig
            {
                Title = TxtTitle.Text,
                ChartType = (CmbType.SelectedItem as dynamic).Tag,
                VariableTag = (CmbProp.SelectedItem as dynamic).Tag,
                RobotId = CmbRobot.SelectedItem.ToString()
            };

            // 3. Cerramos con éxito
            this.DialogResult = true;
            this.Close();
        }
    }
}