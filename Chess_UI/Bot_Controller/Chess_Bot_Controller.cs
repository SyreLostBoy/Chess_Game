using Chess_AI;
using Chess_Logic;
using Microsoft.VisualBasic;
using System.Security.Cryptography.X509Certificates;
using System.Windows;

namespace Chess_UI
{
    public class ABot_Controller
    {
        private readonly AChess_Bot Chess_Bot = new AChess_Bot();
        private readonly MainWindow Main_Window;
        private bool is_bot_turn = false;

        public ABot_Controller(MainWindow main_window)
        {
            Main_Window = main_window;
        }

        public async void Make_Bot_Move(AsGame_State game_state, EColor bot_color)
        {
            if (is_bot_turn || game_state.Is_Game_Over() )
            {
                return;
            }

            is_bot_turn = true;

            await Task.Run(() =>
            {

                AMove best_move = Chess_Bot.Find_Best_Move(game_state, bot_color);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (best_move != null && !game_state.Is_Game_Over())
                    {
                        Execute_Bot_Move(best_move, game_state);
                    }

                    is_bot_turn = false;
                });
            });
        }

        private void Execute_Bot_Move(AMove move, AsGame_State game_state)
        {
            if (move.Move_Type == EMove_Type.Pawn_Promotion)
            {
                AMove_Pawn_Promotion promotion_move = new AMove_Pawn_Promotion(move.From_Position, move.To_Position, EPiece_Type.Queen);
                Main_Window.Handle_Move(promotion_move);
            }
            else
            {
                Main_Window.Handle_Move(move);
            }
        }

        public bool Is_Bot_Turn()
        {
            return is_bot_turn;
        }
    }
}
