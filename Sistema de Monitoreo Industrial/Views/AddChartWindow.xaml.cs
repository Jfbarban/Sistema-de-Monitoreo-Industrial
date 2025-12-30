using Sistema_de_Monitoreo_Industrial.Models;
using Sistema_de_Monitoreo_Industrial.Services;
using System.Collections.Generic;
using System.Linq; // No olvides el Linq para el .Select
using System.Windows;
using System.Windows.Controls;

namespace Sistema_de_Monitoreo_Industrial.Views
{
    public partial class AddChartWindow : Window
    {
        private DatabaseService _dbService = new DatabaseService();
        public ChartWidgetConfig CreatedWidget { get; private set; }

        public AddChartWindow()
        {
            InitializeComponent();
            CargarListasDinamicas();
        }

        private async void CargarListasDinamicas()
        {
            // 1. Cargamos Tipos de Gráficos (Actualizado con "Label")
            CmbType.ItemsSource = new List<object> {
                new { Nombre = "Señal Temporal (Líneas)", Tag = "Signal" },
                new { Nombre = "Medidor Radial (Gauge)", Tag = "Gauge" },
                new { Nombre = "Valor de Texto (Label)", Tag = "Label" }, // Nombre unificado
                new { Nombre = "Producción Acumulada (Barras)", Tag = "Bar" },
                new { Nombre = "Estado Operacional (Status)", Tag = "Status" }
            };
            CmbType.SelectedIndex = 0;

            // 2. Cargamos Robots
            var robotsDetectados = await _dbService.ObtenerRobotsDisponibles();
            CmbRobot.ItemsSource = robotsDetectados;

            if (CmbRobot.Items.Count > 0)
                CmbRobot.SelectedIndex = 0;
        }

        private async void CmbRobot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbRobot.SelectedItem == null) return;

            string robotSeleccionado = CmbRobot.SelectedItem.ToString();
            CmbProp.IsEnabled = false;

            var campos = await _dbService.ObtenerCamposPorRobot(robotSeleccionado);

            CmbProp.ItemsSource = campos.Select(c => new {
                Nombre = FormatearNombreCampo(c),
                Tag = c
            }).ToList();

            CmbProp.SelectedIndex = 0;
            CmbProp.IsEnabled = true;
        }

        // --- NUEVA LÓGICA DE BLOQUEO ---
        private void CmbProp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbProp.SelectedItem == null) return;

            // Obtenemos el Tag técnico (ej: "estado_motor")
            string tag = (CmbProp.SelectedItem as dynamic).Tag.ToString().ToLower();

            // CRITERIO INDUSTRIAL: Si el nombre contiene palabras clave de texto
            bool esTexto = tag.Contains("estado") ||
                           tag.Contains("modo") ||
                           tag.Contains("operacion") ||
                           tag.Contains("lote") ||
                           tag.Contains("msg");

            if (esTexto)
            {
                // Forzamos el tipo "Label" y bloqueamos el ComboBox de tipo
                CmbType.SelectedValue = "Label";
                // Nota: Si usas SelectedValue con objetos anónimos, asegúrate que 
                // SelectedValuePath="Tag" esté puesto en el XAML del ComboBox

                // Forma manual por si acaso:
                foreach (var item in CmbType.Items)
                {
                    if ((item as dynamic).Tag == "Label")
                    {
                        CmbType.SelectedItem = item;
                        break;
                    }
                }

                CmbType.IsEnabled = false;
                // NUEVO: Bloquear el campo de alerta si es texto
                txtThreshold.Text = "";
                txtThreshold.IsEnabled = false;
                txtThreshold.Opacity = 0.5;
            }
            else
            {
                // Si es un número (temperatura, etc.), permitimos elegir Gauge o Signal
                CmbType.IsEnabled = true;
                // NUEVO: Habilitar alerta si es un número (temperatura, presión, etc.)
                txtThreshold.IsEnabled = true;
                txtThreshold.Opacity = 1.0;
            }
        }

        private string FormatearNombreCampo(string tecnico)
        {
            if (string.IsNullOrEmpty(tecnico)) return tecnico;
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo
                   .ToTitleCase(tecnico.Replace("_", " ").ToLower());
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e) => this.Close();

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtTitle.Text) || CmbRobot.SelectedItem == null || CmbProp.SelectedItem == null)
            {
                return;
            }

            // 1. CAPTURAR EL UMBRAL (THRESHOLD)
            // Buscamos el TextBox del umbral (asegúrate que en el XAML se llame TxtThreshold)
            double? alertValue = null;

            // Solo intentamos leer el umbral si el campo no está vacío y es un número
            // Usamos TxtThreshold que es el nombre estándar que te sugerí para el XAML
            if (!string.IsNullOrWhiteSpace(txtThreshold.Text))
            {
                if (double.TryParse(txtThreshold.Text, out double result))
                {
                    alertValue = result;
                }
                else
                {
                    ConfirmDialog aviso = new ConfirmDialog("El límite de alerta debe ser un número válido.");
                    aviso.Owner = this; // Centrar sobre la ventana de configuración
                    aviso.ConfigurarComoAviso(); // Oculta el botón cancelar y cambia texto a 'ENTENDIDO'
                    aviso.ShowDialog();
                    return;
                }
            }

            CreatedWidget = new ChartWidgetConfig
            {
                Title = TxtTitle.Text,
                ChartType = (CmbType.SelectedItem as dynamic).Tag,
                VariableTag = (CmbProp.SelectedItem as dynamic).Tag,
                RobotId = CmbRobot.SelectedItem.ToString(),

                // 2. ASIGNAR EL VALOR AL OBJETO CONFIG
                AlertThreshold = alertValue
            };

            this.DialogResult = true;
            this.Close();
        }
    }
}