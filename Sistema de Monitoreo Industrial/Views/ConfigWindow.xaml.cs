using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using Sistema_de_Monitoreo_Industrial.Models;
using Sistema_de_Monitoreo_Industrial.Services;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Sistema_de_Monitoreo_Industrial.Views
{
    /// <summary>
    /// Lógica de interacción para ConfigWindow.xaml
    /// </summary>
    public partial class ConfigWindow : Window
    {
        public AppSettings CurrentSettings { get; private set; }

        public ConfigWindow()
        {
            InitializeComponent();

            // Cargamos la configuración actual para mostrarla en los campos
            CurrentSettings = ConfigService.Load();
            CargarInterfaz();
        }

        private async void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            btnTest.Content = "CONECTANDO...";
            btnTest.IsEnabled = false; // Bloqueamos para evitar clics dobles

            try
            {
                // Usamos los valores actuales de los TextBox
                using var client = new InfluxDBClient(txtUrl.Text, txtToken.Text);

                // Verificamos salud del servidor de forma asíncrona
                var health = await client.HealthAsync();

                if (health.Status == InfluxDB.Client.Api.Domain.HealthCheck.StatusEnum.Pass)
                {
                    MostrarAviso("CONEXIÓN EXITOSA\n\nEl servidor InfluxDB responde correctamente.");
                }
                else
                {
                    MostrarAviso("AVISO DE SISTEMA\n\nServidor detectado, pero el estado de salud reportado no es óptimo.");
                }
            }
            catch (Exception ex)
            {
                // Mostramos el error detallado en tu ventana personalizada
                MostrarAviso($"FALLO DE CONEXIÓN\n\nDetalle: {ex.Message}");
            }
            finally
            {
                btnTest.Content = "PROBAR CONEXIÓN";
                btnTest.IsEnabled = true;
            }
        }

        // Método auxiliar para no repetir código de la ventana
        private void MostrarAviso(string mensaje)
        {
            // Nota: Asegúrate de que el namespace coincida (ControlBrazoRobotico o el de tu app)
            ConfirmDialog aviso = new ConfirmDialog(mensaje);
            aviso.Owner = this; // Centrar sobre la ventana de configuración
            aviso.ConfigurarComoAviso(); // Oculta el botón cancelar y cambia texto a 'ENTENDIDO'
            aviso.ShowDialog();
        }

        private void CargarInterfaz()
        {
            // Mapeamos los campos del JSON a los TextBox de la ventana
            txtUrl.Text = CurrentSettings.InfluxUrl;
            txtOrg.Text = CurrentSettings.InfluxOrg;
            txtBucket.Text = CurrentSettings.InfluxBucket;
            txtToken.Text = CurrentSettings.InfluxToken;

            // Si tienes el checkbox de animaciones:
            // chkAnimations.IsChecked = CurrentSettings.EnableAnimations;
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // 1. Recolectamos los datos de los TextBox y los volcamos al objeto CurrentSettings
            CurrentSettings.InfluxUrl = txtUrl.Text;
            CurrentSettings.InfluxOrg = txtOrg.Text;
            CurrentSettings.InfluxBucket = txtBucket.Text;
            CurrentSettings.InfluxToken = txtToken.Text;

            // 2. Guardamos mediante el servicio (esto sobrescribe el config.json)
            try
            {
                ConfigService.Save(CurrentSettings);

                // Indicamos que el cierre fue por éxito (Guardar)
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar la configuración: {ex.Message}",
                                "Error de Archivo", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
