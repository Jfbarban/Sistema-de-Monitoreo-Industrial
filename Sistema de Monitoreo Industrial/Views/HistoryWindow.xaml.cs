using Sistema_de_Monitoreo_Industrial.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Sistema_de_Monitoreo_Industrial.Views
{
    public partial class HistoryWindow : Window
    {
        private ICollectionView _historyView;

        public HistoryWindow()
        {
            InitializeComponent();
            CargarHistorial();
        }

        private void CargarHistorial()
        {
            string ruta = "alarm_history.json";
            if (System.IO.File.Exists(ruta))
            {
                try
                {
                    string json = System.IO.File.ReadAllText(ruta);
                    var lista = System.Text.Json.JsonSerializer.Deserialize<List<AlarmLog>>(json);

                    if (lista != null)
                    {
                        var listaOrdenada = lista.OrderByDescending(x => x.Timestamp).ToList();
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
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            }
        }

        // Este es el nuevo evento del botón
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