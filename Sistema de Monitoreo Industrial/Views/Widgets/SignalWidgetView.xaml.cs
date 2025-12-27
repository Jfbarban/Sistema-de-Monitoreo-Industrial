using System.Windows.Controls;
using Sistema_de_Monitoreo_Industrial.ViewModels.Widgets;
using ScottPlot;
using System;

namespace Sistema_de_Monitoreo_Industrial.Views.Widgets
{
    public partial class SignalWidgetView : UserControl
    {
        public SignalWidgetView()
        {
            InitializeComponent();

            // 1. Configuración Estética Fija
            MyPlot.Plot.FigureBackground.Color = Color.FromHex("#1A1A1A");
            MyPlot.Plot.DataBackground.Color = Color.FromHex("#1A1A1A");
            MyPlot.Plot.Axes.Color(Color.FromHex("#888888"));

            // 2. Fijar márgenes del eje Y para que la gráfica no "baile"
            MyPlot.Plot.Axes.Left.MinimumSize = 50;
            MyPlot.Plot.Axes.Left.MaximumSize = 50;

            this.DataContextChanged += (s, e) =>
            {
                if (DataContext is WidgetSignalViewModel vm)
                {
                    // Limpiamos y preparamos la señal UNA SOLA VEZ
                    MyPlot.Plot.Clear();
                    var sig = MyPlot.Plot.Add.Signal(vm.DataBuffer); // ScottPlot mira este array directamente
                    sig.Color = Color.FromHex("#00E5FF");
                    sig.LineWidth = 2;

                    // Suscripción al evento que dispara el Timer del DatabaseService
                    vm.RequestRefresh += () =>
                    {
                        // IMPORTANTE: El Invoke debe asegurar que el gráfico se entere del cambio
                        Dispatcher.Invoke(() =>
                        {
                            // Actualizar límites X para asegurar que vemos los 50 puntos
                            MyPlot.Plot.Axes.SetLimitsX(0, vm.DataBuffer.Length);

                            // Forzar el auto-escalado vertical para que se vea el movimiento
                            MyPlot.Plot.Axes.AutoScaleY();

                            // REDIBUJAR
                            MyPlot.Refresh();
                        });
                    };
                }
            };
        }
    }
}