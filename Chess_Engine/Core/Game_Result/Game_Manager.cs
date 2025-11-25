using Chess_Engine.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Engine.Core
{
    public static class AsGame_Manager
    {
        public static bool Is_Draw_Result(EGame_Result result)
        {
            return result is EGame_Result.Draw_By_Arbiter or EGame_Result.Fifty_Move_Rule or EGame_Result.Repetition or EGame_Result.Stalemate or EGame_Result.Insufficient_Material;
        }

        public static bool Is_Win_Result(EGame_Result result)
        {
            return Is_White_Wins_Result(result) || Is_Black_Wins_Result(result);
        }

        public static bool Is_White_Wins_Result(EGame_Result result)
        {
            return result is EGame_Result.Black_Is_Mated or EGame_Result.Black_Timeout or EGame_Result.Black_Illegal_Move;
        }

        public static bool Is_Black_Wins_Result(EGame_Result result)
        {
            return result is EGame_Result.White_Is_Mated or EGame_Result.White_Timeout or EGame_Result.White_Illegal_Move;
        }

        public static EGame_Result Get_Game_State(ABoard board)
        {
            AMove_Generator move_generator = new AMove_Generator();
            Span<SMove> moves = move_generator.Generate_Moves(board);

            // 1. Look for mate/stalemate
            if (moves.Length == 0)
            {
                if (move_generator.Is_In_Check())
                {
                    return (board.Is_White_To_Move) ? EGame_Result.White_Is_Mated : EGame_Result.Black_Is_Mated;
                }
                return EGame_Result.Stalemate;
            }

            // 2. Fifty Move Rule
            if (board.Fify_Move_Counter >= 100)
            {
                return EGame_Result.Fifty_Move_Rule;
            }

            // 3. Threefold repetition
            int rep_count = board.Repetition_Position_History.Count((x => x == board.Zobrist_Key));

            if (rep_count == 3)
            {
                return EGame_Result.Repetition;
            }

            // 4. Look for insufficient material
            if (Is_Insufficient_Material(board) )
            {
                return EGame_Result.Insufficient_Material;
            }

            return EGame_Result.In_Progress;
        }

        public static bool Is_Insufficient_Material(ABoard board)
        {    
            if (board.Pawns[ABoard.White_Index].Count > 0 || board.Pawns[ABoard.Black_Index].Count > 0)
            {// Can't have insufficient material with pawns on the board
                return false;
            }

            if (board.Friendly_Orthogonal_Sliders != 0 || board.Enemy_Orthogonal_Sliders != 0)
            {// Can't have insufficient material with queens/rooks on the board
                return false;
            }

            // If no pawns, or rooks on the board, then consider knight an bishop cases
            int num_white_bishops = board.Bishops[ABoard.White_Index].Count;
            int num_black_bishops = board.Bishops[ABoard.Black_Index].Count;
            int num_white_knights = board.Knights[ABoard.White_Index].Count;
            int num_black_knights = board.Knights[ABoard.Black_Index].Count;
            int num_white_minors = num_white_bishops + num_white_knights;
            int num_black_minors = num_black_bishops + num_black_knights;
            int num_minors = num_white_minors + num_black_minors;

            if (num_minors <= 1)
            {// Lone kings or king vs king + single minor: is insufficient
                return true;
            }

            if (num_minors == 2 && num_white_bishops == 1 && num_black_knights == 1)
            {// Bishop vs bishop is insufficient when bishop are same color
                bool white_bishop_is_light_square = AsBoard_Helper.Is_Light_Square(board.Bishops[ABoard.White_Index][0]);
                bool black_bishop_is_light_square = AsBoard_Helper.Is_Light_Square(board.Bishops[ABoard.Black_Index][0]);
                
                return white_bishop_is_light_square == black_bishop_is_light_square;
            }

            return false;
        }
    }
}
