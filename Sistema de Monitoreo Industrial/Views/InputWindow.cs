using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Sistema_de_Monitoreo_Industrial.Views
{
    public class InputWindow : Window
    {
        public string Respuesta { get; private set; }
        private TextBox txtInput;

        public InputWindow()
        {
            // 1. Configuración de Ventana (Estilo Moderno / Transparente)
            Title = "Guardar";
            Width = 350;
            Height = 200;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ShowInTaskbar = false;
            ResizeMode = ResizeMode.NoResize;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;

            // Permitir arrastrar la ventana haciendo clic en el fondo
            this.MouseDown += (s, e) => { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); };

            // 2. Contenedor Visual Principal (Borde con esquinas redondeadas)
            Border mainBorder = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1E1E")),
                CornerRadius = new CornerRadius(15),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333")),
                BorderThickness = new Thickness(1.5),
                Padding = new Thickness(25)
            };

            StackPanel mainStack = new StackPanel();

            // Etiqueta
            TextBlock lbl = new TextBlock
            {
                Text = "NOMBRE DEL DASHBOARD",
                Foreground = Brushes.Gray,
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 15),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // TextBox con tu estilo Moderno
            txtInput = new TextBox { Height = 38, Margin = new Thickness(0, 0, 0, 20), VerticalContentAlignment = VerticalAlignment.Center };
            if (Application.Current.TryFindResource("ModernTextBoxStyle") is Style st) txtInput.Style = st;

            // 3. Fila de Botones
            Grid buttonGrid = new Grid();
            buttonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            buttonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) }); // Espacio entre botones
            buttonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Botón CANCELAR
            Button btnCancel = new Button
            {
                Content = "CANCELAR",
                Height = 35,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2D2D2D")),
                Foreground = Brushes.White,
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Cursor = Cursors.Hand
            };
            btnCancel.Click += (s, e) => { DialogResult = false; };
            Grid.SetColumn(btnCancel, 0);

            // Botón GUARDAR
            Button btnOk = new Button
            {
                Content = "GUARDAR",
                Height = 35,
                Background = (SolidColorBrush)Application.Current.Resources["OrangeAccent"],
                Foreground = Brushes.White,
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Cursor = Cursors.Hand
            };
            btnOk.Click += (s, e) => {
                if (!string.IsNullOrWhiteSpace(txtInput.Text)) { Respuesta = txtInput.Text; DialogResult = true; }
            };
            Grid.SetColumn(btnOk, 2);

            // 4. Aplicar Plantilla con Redondeo y Hover de Borde Naranja
            ControlTemplate roundedTemplate = CreateRoundedButtonTemplate(8);
            btnCancel.Template = roundedTemplate;
            btnOk.Template = roundedTemplate;

            // Ensamblaje final
            buttonGrid.Children.Add(btnCancel);
            buttonGrid.Children.Add(btnOk);

            mainStack.Children.Add(lbl);
            mainStack.Children.Add(txtInput);
            mainStack.Children.Add(buttonGrid);

            mainBorder.Child = mainStack;
            this.Content = mainBorder;

            // Foco inicial
            this.Activated += (s, e) => txtInput.Focus();
        }

        /// <summary>
        /// Crea una plantilla de botón con bordes redondeados y efecto hover naranja
        /// </summary>
        private ControlTemplate CreateRoundedButtonTemplate(double radius)
        {
            ControlTemplate template = new ControlTemplate(typeof(Button));

            // Factoría para el Borde
            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
            border.Name = "BtnBorder";
            border.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(radius));
            border.SetValue(Border.BorderThicknessProperty, new Thickness(1.5));
            border.SetValue(Border.BorderBrushProperty, Brushes.Transparent); // Borde oculto por defecto
            border.SetValue(Border.SnapsToDevicePixelsProperty, true);

            // Factoría para el Contenido (Texto)
            FrameworkElementFactory presenter = new FrameworkElementFactory(typeof(ContentPresenter));
            presenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            presenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            border.AppendChild(presenter);

            template.VisualTree = border;

            // TRIGGER: Mouse Over
            Trigger mouseOverTrigger = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };

            // Obtener el color naranja de los recursos
            var orangeBrush = (SolidColorBrush)Application.Current.Resources["OrangeAccent"];

            // Acción: Cambiar el color del borde al pasar el mouse
            mouseOverTrigger.Setters.Add(new Setter
            {
                TargetName = "BtnBorder",
                Property = Border.BorderBrushProperty,
                Value = orangeBrush
            });

            // Acción: Ligero cambio de opacidad para feedback visual
            mouseOverTrigger.Setters.Add(new Setter
            {
                TargetName = "BtnBorder",
                Property = UIElement.OpacityProperty,
                Value = 0.9
            });

            template.Triggers.Add(mouseOverTrigger);

            return template;
        }
    }
}
