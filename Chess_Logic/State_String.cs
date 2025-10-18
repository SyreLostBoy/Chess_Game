using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Logic
{
    public class AState_String
    {
        private readonly StringBuilder String_Builder = new StringBuilder();

        public AState_String(ABoard board, EColor current_player_color)
        {
            Add_Piece_Placement(board);
            String_Builder.Append(' ');

            Add_Current_Player(current_player_color);
            String_Builder.Append(' ');

            Add_Castling_Rights(board);
            String_Builder.Append(' ');

            Add_En_Passant(board, current_player_color);
        }

        public override string ToString()
        {
            return String_Builder.ToString();
        }

        private static char Get_Piece_Char(APiece piece)
        {
            char piece_char;

            switch (piece.Type)
            {
                case EPiece_Type.Pawn:
                    piece_char = 'p';
                    break;
                case EPiece_Type.Bishop:
                    piece_char = 'b';
                    break;
                case EPiece_Type.Knight:
                    piece_char = 'n';
                    break;
                case EPiece_Type.Rook:
                    piece_char = 'r';
                    break;
                case EPiece_Type.Queen:
                    piece_char = 'q';
                    break;
                case EPiece_Type.King:
                    piece_char = 'k';
                    break;
                default:
                    piece_char = ' ';
                    break;
            }

            if (piece.Color == EColor.White)
            {
                piece_char = char.ToUpper(piece_char);
            }

            return piece_char;
        }

        private void Add_Row_Data(ABoard board, int row)
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
                    String_Builder.Append(empty_squares);
                    empty_squares = 0;
                }

                String_Builder.Append(Get_Piece_Char(board[row, col]));
            }

            if (empty_squares > 0)
            {
                String_Builder.Append(empty_squares);
            }
        }

        private void Add_Piece_Placement(ABoard board)
        {
            for (int row = 0; row < 8; row++)
            {
                if (row != 0)
                {
                    String_Builder.Append('/');
                }

                Add_Row_Data(board, row);
            }
        }

        private void Add_Current_Player(EColor current_player_color)
        {
            if (current_player_color == EColor.White)
            {
                String_Builder.Append('w');
            }
            else
            {
                String_Builder.Append('b');

            }
        }

        private void Add_Castling_Rights(ABoard board)
        {
            bool castle_white_ks = board.Has_Castle_Right_KS(EColor.White);
            bool castle_white_qs = board.Has_Castle_Right_QS(EColor.White);
            
            bool castle_black_ks = board.Has_Castle_Right_KS(EColor.Black);
            bool castle_black_qs = board.Has_Castle_Right_QS(EColor.Black);

            if (!(castle_white_ks || castle_white_qs || castle_black_ks || castle_black_qs))
            {
                String_Builder.Append('-');
                return;
            }

            if (castle_white_ks)
            {
                String_Builder.Append('K');
            }
           
            if (castle_white_qs)
            {
                String_Builder.Append('Q');
            }

            if (castle_black_ks)
            {
                String_Builder.Append('k');
            }

            if (castle_black_qs)
            {
                String_Builder.Append('q');
            }
        }

        private void Add_En_Passant(ABoard board, EColor current_player_color)
        {
            char file;
            int rank;
            APosition skip_pos;

            if (!board.Can_Capture_En_Passant(current_player_color))
            {
                String_Builder.Append('-');
                return;
            }

            skip_pos = board.Get_Pawn_Skip_Position(current_player_color.Opponent() );

            file = (char)('a' + skip_pos.Column);
            rank = 8 - skip_pos.Row;

            String_Builder.Append(file);
            String_Builder.Append(rank);
        }
    }
}
