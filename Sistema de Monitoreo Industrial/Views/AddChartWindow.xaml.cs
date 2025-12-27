using System.Windows;
using System.Windows.Controls;
using Sistema_de_Monitoreo_Industrial.ViewModels.Widgets;

namespace Sistema_de_Monitoreo_Industrial.Views
{
    public partial class AddChartWindow : Window
    {
        public WidgetBaseViewModel CreatedWidget { get; private set; }

        public AddChartWindow() => InitializeComponent();

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            string title = TxtTitle.Text;
            string type = (CmbType.SelectedItem as ComboBoxItem).Tag.ToString();
            string robot = (CmbRobot.SelectedItem as ComboBoxItem).Tag.ToString();

            // Obtenemos la variable seleccionada por defecto
            string prop = (CmbProp.SelectedItem as ComboBoxItem).Tag.ToString();

            // LÓGICA AUTOMÁTICA:
            // Si es un gráfico de estado, forzamos que lea la propiedad "Estado" 
            // independientemente de lo que diga el ComboBox de variables.
            if (type == "Status")
            {
                prop = "Estado";
            }

            switch (type)
            {
                case "Signal": CreatedWidget = new WidgetSignalViewModel(title, robot, prop); break;
                case "Gauge": CreatedWidget = new WidgetGaugeViewModel(title, robot, prop); break;
                case "Bar": CreatedWidget = new WidgetBarViewModel(title, robot, prop); break;
                case "Status": CreatedWidget = new WidgetStatusViewModel(title, robot, prop); break;
            }

            this.DialogResult = true;
            this.Close();
        }

        private void CmbType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Verificamos que los componentes ya existan (evita errores al abrir la ventana)
            if (CmbProp == null || TxtTitle == null) return;

            var selectedItem = CmbType.SelectedItem as ComboBoxItem;
            if (selectedItem == null) return;

            string tipoElegido = selectedItem.Tag.ToString();

            if (tipoElegido == "Status")
            {
                // 1. Bloqueamos el selector de variables
                CmbProp.IsEnabled = false;

                // 2. Opcional: Cambiamos el título automáticamente para ayudar al usuario
                TxtTitle.Text = "Estado Operacional";

                // 3. Opcional: Podemos cambiar la opacidad para que se vea más claro el bloqueo
                CmbProp.Opacity = 0.5;
            }
            else
            {
                // Si elige cualquier otro, rehabilitamos el selector
                CmbProp.IsEnabled = true;
                CmbProp.Opacity = 1.0;

                // Sugerencia de título genérico
                TxtTitle.Text = "Monitoreo de " + (CmbProp.SelectedItem as ComboBoxItem).Content;
            }
        }
    }
}