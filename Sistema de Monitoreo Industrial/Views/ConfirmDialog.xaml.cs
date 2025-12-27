using System;
using System.Windows;

namespace Sistema_de_Monitoreo_Industrial.Views
{
    public partial class ConfirmDialog : Window
    {
        // Propiedad para saber qué eligió el usuario si no se usa ShowDialog
        public bool Resultado { get; private set; } = false;

        public ConfirmDialog(string mensaje)
        {
            InitializeComponent();
            txtMensaje.Text = mensaje;
        }

        /// <summary>
        /// Configura la ventana para mostrar solo un botón de cerrar.
        /// Útil para errores de conexión o avisos de sistema.
        /// </summary>
        public void ConfigurarComoAviso()
        {
            btnCancelar.Visibility = Visibility.Collapsed;
            btnAceptar.Content = "ENTENDIDO";
            btnAceptar.Width = 150; // Un poco más ancho al estar solo
            btnAceptar.Margin = new Thickness(0); // Centrar el botón único
        }

        private void BtnSi_Click(object sender, RoutedEventArgs e)
        {
            this.Resultado = true;
            this.DialogResult = true; // Cierra la ventana y retorna true
        }

        private void BtnNo_Click(object sender, RoutedEventArgs e)
        {
            this.Resultado = false;
            this.DialogResult = false; // Cierra la ventana y retorna false
        }

        // Permite arrastrar la ventana aunque no tenga barra de título
        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }
    }
}