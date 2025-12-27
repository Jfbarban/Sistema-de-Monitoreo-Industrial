using System;
using System.Linq; // INDISPENSABLE para generar los arreglos
using System.Windows.Controls;
using ScottPlot;
using Sistema_de_Monitoreo_Industrial.ViewModels.Widgets;

namespace Sistema_de_Monitoreo_Industrial.Views.Widgets
{
    public partial class StatusWidgetView : UserControl
    {
        public StatusWidgetView()
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
                if (DataContext is WidgetStatusViewModel vm)
                {
                    vm.RequestRefresh += () => Dispatcher.Invoke(() =>
                    {
                        MyPlot.Plot.Clear();
                        double[] xs = Enumerable.Range(0, vm.StatusHistory.Length).Select(i => (double)i).ToArray();
                        double[] ysBajo = new double[vm.StatusHistory.Length];

                        var fill = MyPlot.Plot.Add.FillY(xs, vm.StatusHistory, ysBajo);

                        // Color según el último estado
                        double ultimo = vm.StatusHistory.Last();
                        if (ultimo >= 1.0) fill.FillColor = Color.FromHex("#2ECC71"); // Verde
                        else if (ultimo >= 0.5) fill.FillColor = Color.FromHex("#F1C40F"); // Amarillo
                        else fill.FillColor = Color.FromHex("#E74C3C"); // Rojo

                        // Auto-Centrado: X de 0 a 50, Y de -0.1 a 1.1 (fijo para estados)
                        MyPlot.Plot.Axes.SetLimitsX(0, vm.StatusHistory.Length);
                        MyPlot.Plot.Axes.SetLimitsY(-0.1, 1.1);

                        MyPlot.Refresh();
                    });
                }
            };
        }
    }
}