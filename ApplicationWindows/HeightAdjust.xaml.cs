using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace Stacker
{
    /// <summary>
    /// Логика взаимодействия для HeightAdjust.xaml
    /// </summary>
    public partial class HeightAdjust : Window
    {
        public static Action OnClose;

        #region

        public HeightAdjust()
        {
            InitializeComponent();
            NewMainWindow.OnClose += CloseWindow;
            PlaceWindow();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            NewMainWindow.OnClose -= CloseWindow;
            OnClose?.Invoke();
            base.OnClosing(e);
        }

        #endregion

        #region XAML

        private void PlaceWindow()
        {
            const int MainWindowHeight = 250;
            const int Margin = 10;
            var desktopWorkingArea = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
            this.Left = desktopWorkingArea.Right - this.Width - Margin;
            this.Top = desktopWorkingArea.Bottom - this.Height - MainWindowHeight - 2 * Margin;
        }

        #endregion

        /*protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            this.DragMove();
        }*/

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
