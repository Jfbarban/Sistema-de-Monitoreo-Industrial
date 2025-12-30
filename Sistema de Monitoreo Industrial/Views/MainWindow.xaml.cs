using Sistema_de_Monitoreo_Industrial.Models;
using Sistema_de_Monitoreo_Industrial.Services;
using Sistema_de_Monitoreo_Industrial.ViewModels;
using Sistema_de_Monitoreo_Industrial.ViewModels.Widgets;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Sistema_de_Monitoreo_Industrial.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        // 1. Definimos la propiedad que leerá el XAML
        public string UsuarioNombreLogueado { get; set; }

        public string UsuarioRolLogueado { get; set; }

        private string rutaMasterDashboards = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dashboards_master.json");

        public MainWindow()
        {
            InitializeComponent();

            if (SessionManager.UsuarioActual != null)
            {
                UsuarioNombreLogueado = SessionManager.UsuarioActual.Username.ToUpper();
                UsuarioRolLogueado = SessionManager.UsuarioActual.Role.ToString().ToUpper();
            }
            else
            {
                UsuarioNombreLogueado = "INVITADO";
                UsuarioRolLogueado = "N/A";
            }
            this.DataContext = this;

            // 1. Estado inicial
            btnConectar.IsChecked = false;
            _viewModel = new MainViewModel();
            this.DataContext = _viewModel;

            // 2. Carga configuración InfluxDB
            var config = ConfigService.Load();

            // 3. Log de inicio
            EscribirEnConsola($"[SISTEMA] Nodo central iniciado. Esperando despliegue de telemetría...");
            EscribirEnConsola($"[SISTEMA] Configuración Vertex-IoT cargada.");
            EscribirEnConsola($"[DATABASE] Conectado a Nodo: {config.InfluxUrl}");
            EscribirEnConsola($"[DATABASE] Bucket activo: {config.InfluxBucket}");
            EscribirEnConsola($"Sistema listo. Estado actual: OFFLINE");

            btnGuardarLayout.IsEnabled = false;
            panelDashboardsSaved.Children.Clear();

            txtConsola.ScrollToEnd();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (SessionManager.UsuarioActual != null)
            {
                UsuarioNombreLogueado = SessionManager.UsuarioActual.Username.ToUpper();
            }
            this.DataContext = this;
        }

        // Muestra el menú cuando haces clic en el botón de usuario
        private void btnUserMenu_Click(object sender, RoutedEventArgs e)
        {
            userContextMenu.PlacementTarget = btnUserMenu;
            userContextMenu.IsOpen = true;
        }

        // Evento para cambiar la contraseña (Placeholder por ahora)
        private void MenuItemPassword_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Aquí abriremos la ventana para cambiar tu contraseña.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Evento para cerrar sesión
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("¿Desea cerrar la sesión actual?", "Cerrar Sesión", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            }
        }

        // --- REFRESCAR PANEL (Desde un único archivo) ---
        public void RefrescarListaDashboards()
        {
            try
            {
                panelDashboardsSaved.Children.Clear();

                if (!System.IO.File.Exists(rutaMasterDashboards)) return;

                string json = System.IO.File.ReadAllText(rutaMasterDashboards);
                var todosLosDashboards = System.Text.Json.JsonSerializer.Deserialize<List<DashboardLayout>>(json);

                if (todosLosDashboards == null) return;

                foreach (var dash in todosLosDashboards)
                {
                    DockPanel itemPanel = new DockPanel { Margin = new Thickness(0, 0, 0, 8) };

                    // BOTÓN ELIMINAR
                    Button btnDelete = CreateCircularDeleteButton();
                    btnDelete.Click += (s, e) => { ConfirmarEliminarDashboardMaster(dash.Nombre); };
                    DockPanel.SetDock(btnDelete, Dock.Right);

                    // BOTÓN DE CARGA
                    Button btnDash = new Button
                    {
                        Content = $"📊  {dash.Nombre.ToUpper()}",
                        Style = (Style)this.FindResource("BtnDark"),
                        Height = 40,
                        HorizontalContentAlignment = HorizontalAlignment.Left,
                        Padding = new Thickness(12, 0, 0, 0)
                    };
                    btnDash.Click += (s, e) => { CargarDesdeObjeto(dash); };

                    itemPanel.Children.Add(btnDelete);
                    itemPanel.Children.Add(btnDash);
                    panelDashboardsSaved.Children.Add(itemPanel);
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("Error: " + ex.Message); }
        }

        // --- GUARDAR EN EL ARCHIVO MAESTRO ---
        private void BtnGuardarLayout_Click(object sender, RoutedEventArgs e)
        {
            InputWindow input = new InputWindow { Owner = this };
            if (input.ShowDialog() != true) return;

            string nombreNuevo = input.Respuesta;

            // 1. Crear la "receta" del dashboard actual
            var nuevoDash = new DashboardLayout { Nombre = nombreNuevo };
            foreach (var widgetVM in _viewModel.Widgets)
            {
                nuevoDash.Widgets.Add(new ChartWidgetConfig
                {
                    Title = widgetVM.Title,
                    ChartType = GetWidgetTypeString(widgetVM), // Método auxiliar para identificar el tipo
                    RobotId = widgetVM.RobotId,
                    VariableTag = widgetVM.VariableTag,
                    AlertThreshold = widgetVM.Threshold
                });
            }

            // 2. Cargar lista existente o crear nueva
            List<DashboardLayout> listaMaster = new List<DashboardLayout>();
            if (System.IO.File.Exists(rutaMasterDashboards))
            {
                string jsonExistente = System.IO.File.ReadAllText(rutaMasterDashboards);
                listaMaster = System.Text.Json.JsonSerializer.Deserialize<List<DashboardLayout>>(jsonExistente) ?? new List<DashboardLayout>();
            }

            // 3. Reemplazar si ya existe o añadir
            listaMaster.RemoveAll(d => d.Nombre == nombreNuevo);
            listaMaster.Add(nuevoDash);

            // 4. Guardar archivo único
            string jsonFinal = System.Text.Json.JsonSerializer.Serialize(listaMaster, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(rutaMasterDashboards, jsonFinal);

            EscribirEnConsola($"[STORAGE] Dashboard {nombreNuevo} guardado.");
            txtConsola.ScrollToEnd();

            RefrescarListaDashboards();
        }

        // --- ELIMINAR DEL ARCHIVO MAESTRO ---
        private void ConfirmarEliminarDashboardMaster(string nombre)
        {
            ConfirmDialog dialogo = new ConfirmDialog($"¿Deseas eliminar '{nombre}' de la lista maestra?");
            dialogo.Owner = this;

            if (dialogo.ShowDialog() == true)
            {
                if (System.IO.File.Exists(rutaMasterDashboards))
                {
                    string json = System.IO.File.ReadAllText(rutaMasterDashboards);
                    var lista = System.Text.Json.JsonSerializer.Deserialize<List<DashboardLayout>>(json);

                    lista.RemoveAll(d => d.Nombre == nombre);

                    string nuevoJson = System.Text.Json.JsonSerializer.Serialize(lista, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                    System.IO.File.WriteAllText(rutaMasterDashboards, nuevoJson);

                    RefrescarListaDashboards();
                    EscribirEnConsola($"[STORAGE] Dashboard {nombre} removido.");
                    txtConsola.ScrollToEnd();
                }
            }
        }

        // --- MÉTODOS AUXILIARES ---
        private void CargarDesdeObjeto(DashboardLayout dash)
        {
            _viewModel.Widgets.Clear();
            foreach (var c in dash.Widgets)
            {
                _viewModel.AgregarWidgetExterno(c);
            }
            EscribirEnConsola($"[STORAGE] Dashboard '{dash.Nombre}' cargado.");
        }

        private string GetWidgetTypeString(WidgetBaseViewModel vm)
        {
            if (vm is WidgetSignalViewModel) return "Signal";
            if (vm is WidgetGaugeViewModel) return "Gauge";
            if (vm is WidgetBarViewModel) return "Bar";
            if (vm is WidgetStatusViewModel) return "Status";
            return "Label";
        }

        private Button CreateCircularDeleteButton()
        {
            Button btnDelete = new Button
            {
                Content = "✕",
                Width = 26,
                Height = 26,
                Foreground = Brushes.White,
                Background = new SolidColorBrush(Color.FromRgb(180, 50, 50)),
                BorderThickness = new Thickness(0),
                Margin = new Thickness(8, 0, 0, 0),
                Cursor = Cursors.Hand,
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                RenderTransformOrigin = new Point(0.5, 0.5) // Importante para que escale desde el centro
            };

            // 1. Transformación para el efecto de clic
            ScaleTransform scale = new ScaleTransform(1, 1);
            btnDelete.RenderTransform = scale;

            // 2. Plantilla (ControlTemplate)
            ControlTemplate template = new ControlTemplate(typeof(Button));

            // Elemento Border (Raíz)
            FrameworkElementFactory borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.Name = "CircleBorder";
            borderFactory.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
            borderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(13));
            borderFactory.SetValue(Border.BorderThicknessProperty, new Thickness(1.5));
            borderFactory.SetValue(Border.BorderBrushProperty, Brushes.Transparent);
            borderFactory.SetValue(Border.SnapsToDevicePixelsProperty, true);

            // ContentPresenter (La X)
            FrameworkElementFactory contentFactory = new FrameworkElementFactory(typeof(ContentPresenter));
            contentFactory.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentFactory.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            borderFactory.AppendChild(contentFactory);

            template.VisualTree = borderFactory;

            // --- ANIMACIONES Y TRIGGERS ---

            // A. TRIGGER: MOUSE OVER (Hover)
            Trigger mouseOverTrigger = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };
            mouseOverTrigger.Setters.Add(new Setter
            {
                TargetName = "CircleBorder",
                Property = Border.BorderBrushProperty,
                Value = Brushes.White // El borde se ilumina en blanco
            });
            template.Triggers.Add(mouseOverTrigger);

            // B. TRIGGER: IS PRESSED (Efecto Clic con Storyboard)
            // Cuando se presiona el botón, se hace más pequeño
            Trigger pressedTrigger = new Trigger { Property = Button.IsPressedProperty, Value = true };

            // Usamos Storyboard para asegurar compatibilidad y suavidad
            Storyboard sbClick = new Storyboard();
            DoubleAnimation animX = new DoubleAnimation(0.9, TimeSpan.FromMilliseconds(50));
            DoubleAnimation animY = new DoubleAnimation(0.9, TimeSpan.FromMilliseconds(50));

            Storyboard.SetTargetProperty(animX, new PropertyPath("RenderTransform.ScaleX"));
            Storyboard.SetTargetProperty(animY, new PropertyPath("RenderTransform.ScaleY"));

            sbClick.Children.Add(animX);
            sbClick.Children.Add(animY);

            // Eventos para disparar la animación
            // Nota: En C# dinámico, los EventTriggers en plantillas son más complejos, 
            // pero podemos usar Setters para el estado visual inmediato:
            pressedTrigger.Setters.Add(new Setter { Property = UIElement.RenderTransformProperty, Value = new ScaleTransform(0.85, 0.85) });
            template.Triggers.Add(pressedTrigger);

            btnDelete.Template = template;

            return btnDelete;
        }

        // --- LÓGICA ORIGINAL (SIN CAMBIOS) ---

        private void BtnConectar_Click(object sender, RoutedEventArgs e)
        {
            var toggle = sender as System.Windows.Controls.Primitives.ToggleButton;
            var vm = this.DataContext as MainViewModel;
            var sbAlerta = (Storyboard)this.FindResource("AlertaCriticaStoryboard");

            if (toggle.IsChecked == true)
            {
                toggle.Content = "DESCONECTAR";
                LedOnline.Fill = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                txtEstadoConexion.Text = "TELEMETRÍA ACTIVA | INFLUXDB SERIES";
                txtEstadoConexion.Foreground = (SolidColorBrush)Application.Current.Resources["OrangeAccent"];
                if (vm != null) { vm.IniciarConexion();  }
                ;
                EscribirEnConsola($"[SISTEMA] Sesión iniciada. Consultando series temporales.");

                btnGuardarLayout.IsEnabled = true;
                RefrescarListaDashboards();
                EscribirEnConsola($"[SISTEMA] Dashboard cargado.");
            }
            else
            {
                toggle.Content = "CONECTAR SISTEMA";
                if (sbAlerta != null) sbAlerta.Stop();
                LedOnline.Fill = new SolidColorBrush(Colors.Red);
                txtEstadoConexion.Text = "SISTEMA EN PAUSA | DESCONECTADO";
                txtEstadoConexion.Foreground = new SolidColorBrush(Colors.Gray);
                if (vm != null) { vm.DetenerConexion(); };
                btnGuardarLayout.IsEnabled = false;
                panelDashboardsSaved.Children.Clear();
                EscribirEnConsola($"[ADVERTENCIA] El usuario ha detenido el monitoreo.");
            }
            txtConsola.ScrollToEnd();
        }

        private void BtnVerHistorial_Click(object sender, RoutedEventArgs e)
        {
            HistoryWindow win = new HistoryWindow { Owner = this };
            win.ShowDialog();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            AddChartWindow dialog = new AddChartWindow();
            dialog.Owner = this;
            if (dialog.ShowDialog() == true && dialog.CreatedWidget != null)
            {
                _viewModel.AgregarWidgetExterno(dialog.CreatedWidget);
            }
        }

        private void btnConfig_Click(object sender, RoutedEventArgs e)
        {
            ConfigWindow win = new ConfigWindow();
            win.Owner = this;
            if (win.ShowDialog() == true)
            {
                var config = ConfigService.Load();
                EscribirEnConsola($"[SISTEMA] Configuración actualizada.");
                EscribirEnConsola($"[DATABASE] Nueva IP Nodo: {config.InfluxUrl}");
                txtConsola.ScrollToEnd();
            }
        }

        public void EscribirEnConsola(string mensaje)
        {
            // Usamos Dispatcher por si la llamada viene desde el hilo del correo (async)
            Dispatcher.Invoke(() => {
                // 1. Color por defecto (Verde Matrix)
                Brush colorLinea = (Brush)new BrushConverter().ConvertFrom("#00FF00");

                // 2. Lógica de colores según el contenido
                if (mensaje.Contains("[ERROR]"))
                    colorLinea = Brushes.Tomato; // Rojo suave para que se lea bien en negro
                else if (mensaje.Contains("[ALERTA]"))
                    colorLinea = Brushes.Yellow;
                else if (mensaje.Contains("[INFO]"))
                    colorLinea = Brushes.Cyan;
                else if (mensaje.Contains("[SUCCESS]"))
                    colorLinea = Brushes.LimeGreen;

                // 3. Crear la nueva línea
                Run run = new Run($"[{DateTime.Now:HH:mm:ss}] {mensaje}\n")
                {
                    Foreground = colorLinea
                };

                // 4. Añadir al párrafo y auto-scroll
                logParagraph.Inlines.Add(run);
                txtConsola.ScrollToEnd();
            });
        }

        private void BtnLimpiarConsola_Click(object sender, RoutedEventArgs e)
        {
            // 1. Limpiamos todas las líneas del párrafo
            logParagraph.Inlines.Clear();

            // 2. Usamos tu nuevo método para escribir el mensaje de reseteo
            // Así mantienes el formato de hora y el color verde por defecto automáticamente
            EscribirEnConsola("[SISTEMA] Consola reseteada por el operador.");
        }
    }

}