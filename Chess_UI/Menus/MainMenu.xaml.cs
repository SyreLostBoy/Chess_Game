using Chess_Engine.Bot;
using Chess_Engine.Core;
using Sound_System;
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
    
    public enum EGame_Mode
    {
        Human_Vs_Human,
        Human_Vs_Bot,
        Bot_Vs_Bot
    }

    public struct SGame_Settings
    {
        public EGame_Mode Game_Mode { get; set; }
        public bool? Player_Is_White { get; set; }
        public EDifficulty Bot_Difficulty { get; set; }
        public STime_Settings Time_Settings{ get; set; }
        public SAdditional_Settings Additional_Settings { get; set; }
    }

    public class STime_Settings
    {
        public int Base_Time_Seconds { get; set; }
        public int Increment_Seconds { get; set; }
        public bool Unlimited;
    }

    public struct SAdditional_Settings
    {
        public bool Enable_Sound { get; set; }
        public bool Highlight_Moves { get; set; }
        public bool Show_Legal_Moves { get; set; }
    }

    public partial class AsMain_Menu : UserControl
    {
        public event Action<SGame_Settings> On_Game_Started;
        public event Action On_Show_Settings;
        public event Action On_Exit;

        public AsMain_Menu()
        {
            InitializeComponent();
            Setup_Event_Handlers();
        }

        private void Setup_Event_Handlers()
        {
            HumanVsBotRadio.Checked += (s, e) => Update_Controls_Visibility();
            HumanVsHumanRadio.Checked += (s, e) => Update_Controls_Visibility();
            BotVsBotRadio.Checked += (s, e) => Update_Controls_Visibility();

            Update_Controls_Visibility();
        }

        private void Update_Controls_Visibility()
        {
            bool is_human_vs_bot = HumanVsBotRadio.IsChecked == true;
            bool is_bot_vs_bot = BotVsBotRadio.IsChecked == true;
            bool is_human_vs_human = HumanVsHumanRadio.IsChecked == true;

            SideSelectionPanel.Visibility = is_bot_vs_bot ? Visibility.Collapsed : Visibility.Visible;

            DifficultyPanel.Visibility = (is_human_vs_bot || is_bot_vs_bot) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            SGame_Settings settings = Get_Game_Settings();
            On_Game_Started?.Invoke(settings);
        }

        private SGame_Settings Get_Game_Settings()
        {
            EGame_Mode game_mode = EGame_Mode.Human_Vs_Bot;

            if (HumanVsHumanRadio.IsChecked == true)
            {
                game_mode = EGame_Mode.Human_Vs_Human;
            }
            else if (BotVsBotRadio.IsChecked == true)
            {
                game_mode = EGame_Mode.Bot_Vs_Bot;
            }

            bool? player_is_white = null;

            if (WhiteSideRadio.IsChecked == true)
            {
                player_is_white = true;
            }
            else if (BlackSideRadio.IsChecked == true)
            {
                player_is_white = false;
            }

            EDifficulty difficulty = EDifficulty.Intermediate;

            if (DifficultyComboBox.SelectedItem is ComboBoxItem selected_item)
            {
                string tag = (string)selected_item.Tag;

                switch (tag)
                {
                    case "Beginner":
                        difficulty = EDifficulty.Beginner;
                        break;
                    case "Intermediate":
                        difficulty = EDifficulty.Intermediate;
                        break;
                    case "Advanced":
                        difficulty = EDifficulty.Advanced;
                        break;
                    case "Expert":
                        difficulty = EDifficulty.Expert;
                        break;
                    case "Master":
                        difficulty = EDifficulty.Master;
                        break;
                    default:
                        difficulty = EDifficulty.Intermediate;
                        break;
                }
            }

            STime_Settings time_settings = Parse_Time_Settings();

            SAdditional_Settings additional_settings = new SAdditional_Settings()
            {
                Enable_Sound = SoundCheckBox.IsChecked == true,
                Highlight_Moves = HighlightMovesCheckBox.IsChecked == true,
                Show_Legal_Moves = ShowLegalMovesCheckBox.IsChecked == true
            };

            return new SGame_Settings()
            {
                Game_Mode = game_mode,
                Player_Is_White = player_is_white,
                Bot_Difficulty = difficulty,
                Time_Settings = time_settings,
                Additional_Settings = additional_settings
            };
        }

        private STime_Settings Parse_Time_Settings()
        {
            if (TimeControlComboBox.SelectedItem is ComboBoxItem selected_item)
            {
                string tag = selected_item.Tag as string;
                
                if (string.IsNullOrEmpty(tag))
                    return new STime_Settings { Unlimited = true };

                if (tag.Contains("+"))
                {
                    string[] parts = tag.Split('+');
                    
                    if (parts.Length == 2 && int.TryParse(parts[0], out int baseTime) && int.TryParse(parts[1], out int increment))
                    {
                        return new STime_Settings
                        {
                            Base_Time_Seconds = baseTime,
                            Increment_Seconds = increment,
                            Unlimited = false
                        };
                    }
                }
                else if (int.TryParse(tag, out int seconds))
                {
                    return new STime_Settings
                    {
                        Base_Time_Seconds = seconds,
                        Increment_Seconds = 0,
                        Unlimited = false
                    };
                }
            }

            return new STime_Settings{Unlimited = true }; 
        }

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            On_Show_Settings?.Invoke();
        }
        
        private void ExitButton_Click(Object sender, EventArgs e)
        {
            On_Exit?.Invoke();
        }

    }
}
