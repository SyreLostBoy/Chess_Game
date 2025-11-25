using Chess_Engine.Core;
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
        public event Action OnRestart;
        public event Action OnExit;

        public Game_Over_Menu()
        {
            InitializeComponent();
        }

        public void SetResult(EGame_Result result)
        {
            switch (result)
            {
                case EGame_Result.White_Is_Mated:
                    Winner_Text.Text = "ЧЁРНЫЕ ПОБЕДИЛИ";
                    Reason_Text.Text = "Мат";
                    break;
                case EGame_Result.Black_Is_Mated:
                    Winner_Text.Text = "БЕЛЫЕ ПОБЕДИЛИ";
                    Reason_Text.Text = "Мат";
                    break;
                case EGame_Result.Stalemate:
                    Winner_Text.Text = "НИЧЬЯ";
                    Reason_Text.Text = "Пат";
                    break;
                case EGame_Result.Fifty_Move_Rule:
                    Winner_Text.Text = "НИЧЬЯ";
                    Reason_Text.Text = "Правило 50 ходов";
                    break;
                case EGame_Result.Repetition:
                    Winner_Text.Text = "НИЧЬЯ";
                    Reason_Text.Text = "Троекратное повторение";
                    break;
                case EGame_Result.Insufficient_Material:
                    Winner_Text.Text = "НИЧЬЯ";
                    Reason_Text.Text = "Недостаточно материала";
                    break;
                case EGame_Result.White_Timeout:
                    Winner_Text.Text = "ЧЁРНЫЕ ПОБЕДИЛИ";
                    Reason_Text.Text = "Время белых вышло";
                    break;
                case EGame_Result.Black_Timeout:
                    Winner_Text.Text = "БЕЛЫЕ ПОБЕДИЛИ";
                    Reason_Text.Text = "Время чёрных вышло";
                    break;
                case EGame_Result.White_Illegal_Move:
                    Winner_Text.Text = "ЧЁРНЫЕ ПОБЕДИЛИ";
                    Reason_Text.Text = "Нелегальный ход белых";
                    break;
                case EGame_Result.Black_Illegal_Move:
                    Winner_Text.Text = "БЕЛЫЕ ПОБЕДИЛИ";
                    Reason_Text.Text = "Нелегальный ход чёрных";
                    break;
                case EGame_Result.Draw_By_Arbiter:
                    Winner_Text.Text = "НИЧЬЯ";
                    Reason_Text.Text = "Решение арбитра";
                    break;
                default:
                    Winner_Text.Text = "ИГРА ОКОНЧЕНА";
                    Reason_Text.Text = "Неизвестный результат";
                    break;
            }
        }

        private void On_Restart_Click(object sender, RoutedEventArgs e)
        {
            OnRestart?.Invoke();
        }

        private void On_Exit_Click(object sender, RoutedEventArgs e)
        {
            OnExit?.Invoke();
        }
    }
}
