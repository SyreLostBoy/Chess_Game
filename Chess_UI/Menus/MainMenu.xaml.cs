using Chess_Engine.Bot;
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

namespace Chess_UI.Menus
{
    /// <summary>
    /// Interaction logic for MainMenu.xaml
    /// </summary>
    public partial class MainMenu : UserControl
    {
        // События для взаимодействия с главным окном
        public event Action<GameSettings> OnGameStarted;
        public event Action OnExitRequested;

        public MainMenu()
        {
            InitializeComponent();
        }
    }

    // Класс для хранения настроек игры
    public class GameSettings
    {
        public GameMode GameMode { get; set; }
        public PlayerSide PlayerSide { get; set; }
        public EDifficulty BotDifficulty { get; set; }
        public TimeControl TimeControl { get; set; }
        public int TimePerMoveMinutes { get; set; }
    }

    public enum GameMode
    {
        HumanVsHuman,
        HumanVsBot,
        BotVsBot
    }

    public enum PlayerSide
    {
        White,
        Black,
        Random
    }

    public enum TimeControl
    {
        Unlimited,
        Limited,
        Tournament
    }
}
