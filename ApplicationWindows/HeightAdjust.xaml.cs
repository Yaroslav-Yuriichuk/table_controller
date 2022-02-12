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

namespace Stacker.ApplicationWindows
{
    public partial class HeightAdjust : Window
    {
        public static Action OnClose;

        #region Window methods

        public HeightAdjust()
        {
            InitializeComponent();
            Subscribe();
            PlaceWindow();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Unsubscribe();
            OnClose?.Invoke();
            base.OnClosing(e);
        }

        #endregion

        #region Events

        private void Subscribe()
        {
            MainWindow.OnClose += CloseWindow;
        }

        private void Unsubscribe()
        {
            MainWindow.OnClose -= CloseWindow;
        }

        #endregion

        #region UI

        private void PlaceWindow()
        {
            const int MainWindowHeight = 250;
            const int Margin = 10;

            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Right - this.Width - Margin;
            this.Top = desktopWorkingArea.Bottom - this.Height - MainWindowHeight - 3 * Margin / 2;
        }

        #endregion

        /*protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            this.DragMove();
        }*/

        #region Actions
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
