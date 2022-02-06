using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Input;
using Stacker.Controllers;

namespace Stacker
{
    public static class XAMLUtil
    {
        private static Border CreateStatusBorder()
        {
            Border status = new Border();

            status.Height = 15;
            status.Width = 15;
            status.CornerRadius = new CornerRadius(7.5);
            Grid.SetColumn(status, 1);

            return status;
        }

        private static Label CreateNameLabel(string deskName)
        {
            System.Windows.Controls.Label label = new System.Windows.Controls.Label();

            label.Content = deskName;
            label.FontSize = 14;
            label.FontWeight = FontWeights.DemiBold;
            label.Margin = new Thickness(10, 0, 0, 0);

            return label;
        }

        private static Tuple<Grid, Border> CreateDeskGrid(string deskName)
        {
            Grid grid = new Grid();

            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(25) });

            Border status = CreateStatusBorder();

            grid.Children.Add(CreateNameLabel(deskName));
            grid.Children.Add(status);

            return new Tuple<Grid, Border>(grid, status);
        }

        public static Tuple<Border, Border> CreateDeskBorder(string deskName)
        {
            Border border = new Border();

            border.Width = 160;
            border.Height = 30;
            border.CornerRadius = new CornerRadius(15);
            border.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#556B80");

            Tuple<Grid, Border> values = CreateDeskGrid(deskName);
            border.Child = values.Item1;

            return new Tuple<Border, Border>(border, values.Item2);
        }

        public static Tuple<Button, Border> CreateDeskTemplate(string deskXName, string deskName,
            Grid desksList, MouseButtonEventHandler connect)
        {
            Button button = new Button();
            Style style = System.Windows.Application.Current.FindResource("DeskButtonStyle") as Style;

            button.Name = deskXName;
            button.Width = 160;
            button.Height = 30;
            button.Background = Brushes.Transparent;
            button.MouseDoubleClick += connect;
            
            Tuple<Border, Border> values = CreateDeskBorder(deskName);
            button.Content = values.Item1;
            button.Style = style;

            string template = "<ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' TargetType=\"Button\">" +
                                    "<ContentPresenter VerticalAlignment=\"Center\" HorizontalAlignment=\"Center\" /> " +
                               "</ControlTemplate>";
            button.Template = (ControlTemplate)XamlReader.Parse(template);
            
            //Grid.SetRow(button, StackerController.Instance.FoundDesks.Count);
            Grid.SetRow(button, desksList.Children.Count);
            
            return new Tuple<System.Windows.Controls.Button, Border>(button, values.Item2);
        }
    }
}
