using System.Windows.Controls;
using Sistema_de_Monitoreo_Industrial.ViewModels.Widgets;
using ScottPlot;

namespace Sistema_de_Monitoreo_Industrial.Views.Widgets
{
    public partial class GaugeWidgetView : UserControl
    {
        public GaugeWidgetView()
        {
            InitializeComponent();
            MyGaugePlot.Plot.HideGrid();
            MyGaugePlot.Plot.Axes.Frameless();
            MyGaugePlot.Plot.FigureBackground.Color = Color.FromHex("#1A1A1A");

            // FIJAR MÁRGENES DE LOS EJES:
            // Esto reserva un espacio fijo para los números del eje Y.
            // Así, aunque el número sea 1 o 1000, el gráfico no se mueve.
            MyGaugePlot.Plot.Axes.Left.MinimumSize = 50;
            MyGaugePlot.Plot.Axes.Left.MaximumSize = 50;

            this.DataContextChanged += (s, e) =>
            {
                if (DataContext is WidgetGaugeViewModel vm)
                {
                    // Binding "Manual" al cambio de propiedad
                    vm.PropertyChanged += (sender, args) =>
                    {
                        if (args.PropertyName == nameof(WidgetGaugeViewModel.CurrentValue))
                        {
                            Dispatcher.Invoke(() => ActualizarGauge(vm.CurrentValue));
                        }
                    };
                }
            };
        }

        private void ActualizarGauge(double val)
        {
            MyGaugePlot.Plot.Clear();
            var gauge = MyGaugePlot.Plot.Add.RadialGaugePlot(new double[] { val });
            gauge.Colors = new Color[] { Color.FromHex("#FF6D00") };
            gauge.StartingAngle = 270;
            MyGaugePlot.Plot.Add.Text($"{val:F1}", 0, 0).LabelFontColor = Colors.White;
            MyGaugePlot.Refresh();
        }
    }
}