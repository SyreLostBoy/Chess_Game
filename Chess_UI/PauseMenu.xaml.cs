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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Chess_UI
{
    /// <summary>
    /// Interaction logic for PauseMenu.xaml
    /// </summary>
    public partial class PauseMenu : UserControl
    {
        public event Action OnContinue;
        public event Action OnRestart;

        public PauseMenu()
        {
            InitializeComponent();
        }

        private void On_Continue_Click(object sender, RoutedEventArgs e)
        {
            OnContinue?.Invoke();
        }

        private void On_Restart_Click(object sender, RoutedEventArgs e)
        {
            OnRestart?.Invoke();
        }
    }
}
