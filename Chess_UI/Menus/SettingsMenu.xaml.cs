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
    /// Interaction logic for SettingsMenu.xaml
    /// </summary>
    public partial class SettingsMenu : UserControl
    {
        public event Action On_Back;
        public event Action On_Settings_Saved;

        public SettingsMenu()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Save_Settings();
            On_Settings_Saved?.Invoke();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            On_Back?.Invoke();
        }

        private void Save_Settings()
        { //!!!
        }
    }
}
