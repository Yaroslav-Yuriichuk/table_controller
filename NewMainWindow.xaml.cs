using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Markup;

namespace Table
{
    /// <summary>
    /// Логика взаимодействия для NewMainWindow.xaml
    /// </summary>
    public partial class NewMainWindow : Window
    {
        public NewMainWindow()
        {
            InitializeComponent();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            this.DragMove();
        }

        #region XAML
        private Button CreateDeskTemplate(string name)
        {
            Button desk = new Button();

            desk.Name = $"{name}";
            desk.MouseDoubleClick += Connect;
            desk.VerticalAlignment = VerticalAlignment.Top;
            desk.Margin = new Thickness(0, 3, 0, 0);

            Style style = Application.Current.FindResource("DeskButtonStyle") as Style;

            string template = "<ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' TargetType=\"Button\"> " +
                                    "<Border Background = \"{TemplateBinding Background}\" Width = \"160\" Height = \"30\" CornerRadius = \"15\">" +
                                            "<Grid>" +
                                                "<Grid.ColumnDefinitions>" +
                                                    "<ColumnDefinition></ColumnDefinition>" +
                                                    "<ColumnDefinition Width = \"25\"></ColumnDefinition>" +
                                                "</Grid.ColumnDefinitions>" +
                                               $"<Label Content = \"{name}\" FontSize = \"14\" FontWeight = \"DemiBold\" VerticalAlignment = \"Top\" Margin = \"10 0 0 0\" />" +
                                                "<Border Grid.Column = \"1\" Height = \"15\" Width = \"15\" Background = \"#23DA36\" CornerRadius = \"7.5\" />" + 
                                            "</Grid>" +
                                    "</Border>" +
                              "</ControlTemplate> ";


            desk.Style = style;
            desk.Template = (ControlTemplate) XamlReader.Parse(template);

            Grid.SetRow(desk, DesksList.Children.Count);

            return desk;
        }

        private void AddDeskToList()
        {
            DesksList.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(40) });
            DesksList.Children.Add(CreateDeskTemplate($"DESK_{DesksList.Children.Count}"));
        }
        #endregion

        #region BUTTONS

        private void RefreshDesks(object sender, RoutedEventArgs e)
        {
            AddDeskToList();
        }

        private void Connect(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(((Button)sender).Name);
        }

        #endregion
    }
}
