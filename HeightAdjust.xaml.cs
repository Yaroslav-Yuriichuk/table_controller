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

namespace Table
{
    /// <summary>
    /// Логика взаимодействия для HeightAdjust.xaml
    /// </summary>
    public partial class HeightAdjust : Window
    {
        public HeightAdjust()
        {
            InitializeComponent();
            NewMainWindow.OnClose += CloseWindow;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            this.DragMove();
        }

        #region
        private void ApplyChanges(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        private void CloseWindow()
        {
            this.Close();
        }

        #endregion
    }
}
