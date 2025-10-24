using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Logic
{
    public class AsFEN_Generator
    {
        public static string Generate_State_String(ABoard board, EColor current_player_color)
        {
            StringBuilder string_builder = new StringBuilder();

            Add_Piece_Placement(board, ref string_builder);
            string_builder.Append(' ');

            Add_Current_Player(current_player_color, ref string_builder);
            string_builder.Append(' ');

            Add_Castling_Rights(board, ref string_builder);
            string_builder.Append(' ');

            Add_En_Passant(board, current_player_color, ref string_builder);

            return string_builder.ToString();
        }

        private static void Add_Row_Data(ABoard board, int row, ref StringBuilder string_builder)
        {
            int empty_squares = 0;

            for (int col = 0; col < 8; col++)
            {
                if (board[row, col] == null)
                {
                    empty_squares++;
                    continue;
                }

                if (empty_squares > 0)
                {
                    string_builder.Append(empty_squares);
                    empty_squares = 0;
                }

                string_builder.Append(AsFen_Parser.Get_Char_From_Piece(board[row, col]));
            }

            if (empty_squares > 0)
            {
                string_builder.Append(empty_squares);
            }
        }

        private static void Add_Piece_Placement(ABoard board, ref StringBuilder string_builder)
        {
            for (int row = 0; row < 8; row++)
            {
                if (row != 0)
                {
                    string_builder.Append('/');
                }

                Add_Row_Data(board, row, ref string_builder);
            }
        }

        private static void Add_Current_Player(EColor current_player_color, ref StringBuilder string_builder)
        {
            if (current_player_color == EColor.White)
            {
                string_builder.Append('w');
            }
            else
            {
                string_builder.Append('b');

            }
        }

        private static void Add_Castling_Rights(ABoard board, ref StringBuilder string_builder)
        {
            bool castle_white_ks = board.Has_Castle_Right_KS(EColor.White);
            bool castle_white_qs = board.Has_Castle_Right_QS(EColor.White);
            
            bool castle_black_ks = board.Has_Castle_Right_KS(EColor.Black);
            bool castle_black_qs = board.Has_Castle_Right_QS(EColor.Black);

            if (!(castle_white_ks || castle_white_qs || castle_black_ks || castle_black_qs))
            {
                string_builder.Append('-');
                return;
            }

            if (castle_white_ks)
            {
                string_builder.Append('K');
            }
           
            if (castle_white_qs)
            {
                string_builder.Append('Q');
            }

            if (castle_black_ks)
            {
                string_builder.Append('k');
            }

            if (castle_black_qs)
            {
                string_builder.Append('q');
            }
        }

        private static void Add_En_Passant(ABoard board, EColor current_player_color, ref StringBuilder string_builder)
        {
            char file;
            int rank;
            APosition skip_pos;

            if (!board.Can_Capture_En_Passant(current_player_color))
            {
                string_builder.Append('-');
                return;
            }

            skip_pos = board.Get_Pawn_Skip_Position(current_player_color.Opponent() );

            file = (char)('a' + skip_pos.Column);
            rank = 8 - skip_pos.Row;

            string_builder.Append(file);
            string_builder.Append(rank);
        }
    }
}
