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

namespace Stacker.ApplicationWindows
{
    /// <summary>
    /// Логика взаимодействия для NewNotification.xaml
    /// </summary>
    public partial class NewNotification : Window
    {

        #region WINDOW_METHODS

        public NewNotification()
        {
            InitializeComponent();
            PlaceNotification();
        }

        #endregion

        #region XAML

        private void PlaceNotification()
        {
            const int margin = 10;
            var desktopWorkingArea = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
            this.Left = desktopWorkingArea.Right - this.Width - margin;
            this.Top = desktopWorkingArea.Bottom - this.Height - margin;
        }

        #endregion
    }
}
