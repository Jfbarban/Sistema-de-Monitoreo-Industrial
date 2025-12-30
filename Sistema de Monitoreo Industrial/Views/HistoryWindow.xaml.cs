using Microsoft.Win32;
using Sistema_de_Monitoreo_Industrial.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading; // Librería necesaria para el Timer

namespace Sistema_de_Monitoreo_Industrial.Views
{
    public partial class HistoryWindow : Window
    {
        private ICollectionView _historyView;
        private DispatcherTimer _timerRefresco; // El temporizador
        private string _ultimoHashArchivo = ""; // Para detectar cambios reales

        public HistoryWindow()
        {
            InitializeComponent();
            CargarHistorial();
            ConfigurarAutoRefresco(); // Iniciamos el timer al abrir
        }

        // --- LÓGICA DEL TIMER ---
        private void ConfigurarAutoRefresco()
        {
            _timerRefresco = new DispatcherTimer();
            _timerRefresco.Interval = TimeSpan.FromSeconds(2); // Refresca cada 2 segundos
            _timerRefresco.Tick += (s, e) => CargarHistorial(true);
            _timerRefresco.Start();
        }

        private void CargarHistorial(bool esAutomatico = false)
        {
            // Dentro de CargarHistorial()
            lock (LogMaintenanceService.ArchivoLock)
            {

                string ruta = "alarm_history.json";

                if (System.IO.File.Exists(ruta))
                {
                    try
                    {
                        // Optimización: Solo procesar si el archivo realmente cambió de tamaño o fecha
                        var info = new FileInfo(ruta);
                        string hashActual = $"{info.Length}_{info.LastWriteTime}";
                        if (esAutomatico && hashActual == _ultimoHashArchivo) return;
                        _ultimoHashArchivo = hashActual;

                        string json = System.IO.File.ReadAllText(ruta);
                        var lista = System.Text.Json.JsonSerializer.Deserialize<List<AlarmLog>>(json);

                        if (lista != null)
                        {
                            var listaOrdenada = lista.OrderByDescending(x => x.Timestamp).ToList();

                            // Si es la primera vez, creamos la View
                            if (_historyView == null)
                            {
                                _historyView = CollectionViewSource.GetDefaultView(listaOrdenada);
                                _historyView.Filter = (obj) =>
                                {
                                    if (string.IsNullOrWhiteSpace(txtSearch.Text)) return true;
                                    var log = obj as AlarmLog;
                                    string query = txtSearch.Text.ToLower();
                                    return log.RobotId.ToLower().Contains(query) ||
                                           log.Variable.ToLower().Contains(query);
                                };
                                dgHistory.ItemsSource = _historyView;
                            }
                            else
                            {
                                // Si ya existe, actualizamos la fuente de datos
                                // Esto mantiene el filtro actual del usuario
                                dgHistory.ItemsSource = listaOrdenada;
                                _historyView = CollectionViewSource.GetDefaultView(listaOrdenada);
                                _historyView.Refresh();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // En refresco automático no mostramos error para no interrumpir al usuario
                        if (!esAutomatico) MessageBox.Show("Error: " + ex.Message);
                    }
                }
            }
        }

        // Detener el timer al cerrar la ventana para liberar memoria
        protected override void OnClosed(EventArgs e)
        {
            _timerRefresco?.Stop();
            base.OnClosed(e);
        }

        // --- TUS EVENTOS EXISTENTES (SIN CAMBIOS) ---

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            if (dgHistory.ItemsSource == null) return;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Archivo CSV (*.csv)|*.csv";
            sfd.FileName = $"Reporte_Alarmas_{DateTime.Now:yyyyMMdd_HHmm}.csv";

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    StringBuilder csvContent = new StringBuilder();
                    csvContent.AppendLine("Fecha/Hora;Nodo;Variable;Valor;Estado;Tipo;Reconocida;ComentarioOperador;FechaReconocimiento");

                    foreach (var item in _historyView)
                    {
                        var log = item as AlarmLog;
                        if (log != null)
                        {
                            string tipo = log.EsInicio ? "CRITICO" : "NORMAL";
                            csvContent.AppendLine($"{log.Timestamp:dd/MM/yyyy HH:mm:ss};" +
                                                  $"{log.RobotId};" +
                                                  $"{log.Variable};" +
                                                  $"{log.Valor};" +
                                                  $"{log.Mensaje};" +
                                                  $"{tipo};" +
                                                  $"{log.Reconocida};" +
                                                  $"{log.ComentarioOperador};" +
                                                  $"{log.FechaReconocimiento}");
                        }
                    }

                    File.WriteAllText(sfd.FileName, csvContent.ToString(), Encoding.UTF8);
                    MessageBox.Show("Archivo exportado con éxito.", "Exportar Datos",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al exportar: {ex.Message}", "Error",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            _historyView?.Refresh();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            _historyView?.Refresh();
        }
    }
}