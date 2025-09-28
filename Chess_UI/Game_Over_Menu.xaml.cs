using Chess_Logic;
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
    /// Interaction logic for Game_Over_Menu.xaml
    /// </summary>
    public partial class Game_Over_Menu : UserControl
    {
        public Game_Over_Menu(AResult result, EColor current_player)
        {
            InitializeComponent();

            Winner_Text.Text = Get_Winner_Text(result.Winner);
            Reason_Text.Text = Get_Reason_Text(result.End_Reason, current_player);
        }

        public event Action<EOption> Option_Selected;

        private void On_Restart_Click(object sender, RoutedEventArgs event_args)
        {
            Option_Selected?.Invoke(EOption.Restart);
        }

        private void On_Exit_Click(object sender, RoutedEventArgs event_args)
        {
            Option_Selected?.Invoke(EOption.Exit);
        }
        private static string Get_Winner_Text(EColor winner_color)
        {
            switch (winner_color)
            {
                case EColor.White:
                    return "ПОБЕДА БЕЛОГО";
                case EColor.Black:
                    return "ПОБЕДА ЧЕРНОГО";
                case EColor.None:
                default:
                    return "НИЧЬЯ";
            }
        }

        private static string Get_Player_String(EColor winner_color)
        {
            switch (winner_color)
            {
                case EColor.White:
                    return "БЕЛЫЙ";
                case EColor.Black:
                    return "ЧЁРНЫЙ";
                case EColor.None:
                default:
                    return "";
            }
        }

        private static string Get_Reason_Text(EEnd_Reason reason, EColor current_player)
        {
            switch (reason)
            {
                case EEnd_Reason.Checkmate:
                    return $"ШАХ И МАТ - {Get_Player_String(current_player)} НЕ ИМЕЕТ ХОДОВ";
                case EEnd_Reason.Stalemate:
                    return $"ПАТ - { Get_Player_String(current_player)} НЕ ИМЕЕТ ХОДОВ";
                case EEnd_Reason.Fifry_Move_Rule:
                    return "ПРАВИЛО 50 ХОДОВ";
                case EEnd_Reason.Insufficient_Material:
                    return "МЁРТВАЯ ПОЗИЦИЯ (НЕДОСТАТОЧНО ФИГУР)";
                case EEnd_Reason.Threefold_Repetition:
                    return "ТРОЕКРАТНОЕ ПОВТОРЕНИЕ";
                default:
                    return "НЕИЗВЕСТНАЯ ПРИЧИНА";
            }
        }
    }
}
