using System.Windows.Controls;
using ScottPlot;
using Sistema_de_Monitoreo_Industrial.ViewModels.Widgets;

namespace Sistema_de_Monitoreo_Industrial.Views.Widgets
{
    public partial class BarWidgetView : UserControl
    {
        public BarWidgetView()
        {
            InitializeComponent();
            MyPlot.Plot.FigureBackground.Color = Color.FromHex("#1A1A1A");

            // FIJAR MÁRGENES DE LOS EJES:
            // Esto reserva un espacio fijo para los números del eje Y.
            // Así, aunque el número sea 1 o 1000, el gráfico no se mueve.
            MyPlot.Plot.Axes.Left.MinimumSize = 50;
            MyPlot.Plot.Axes.Left.MaximumSize = 50;

            this.DataContextChanged += (s, e) =>
            {
                if (DataContext is WidgetBarViewModel vm)
                {
                    vm.RequestRefresh += () => Dispatcher.Invoke(() =>
                    {
                        MyPlot.Plot.Clear();
                        var bar = MyPlot.Plot.Add.Bar(1, vm.ValorActual);
                        bar.Color = Color.FromHex("#FF6D00");

                        // Mantener la barra siempre centrada en pantalla
                        MyPlot.Plot.Axes.SetLimitsX(0, 2);
                        MyPlot.Plot.Axes.SetLimitsY(0, vm.ValorActual * 1.2 + 5); // Margen superior

                        MyPlot.Refresh();
                    });
                }
            };
        }
    }
}