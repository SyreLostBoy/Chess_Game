using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Logic
{
    public static class AsFen_Parser
    {
        public static ABoard Parse_Board_From_FEN(string fen)
        {
            ABoard board = new ABoard();
            string[] fen_parts;

            if (string.IsNullOrWhiteSpace(fen))
            {
                return ABoard.Get_Initial_Board();
            }

            fen_parts = fen.Trim().Split(' ');

            if (fen_parts.Length < 1)
            {
                return ABoard.Get_Initial_Board();
            }

            Parse_Piece_Placement(fen_parts[0], board);

            if (fen_parts.Length > 2)
            {
                Parse_Castling_Rights(fen_parts[2], board);
            }

            if (fen_parts.Length > 3)
            {
                Parse_En_Passant_Target(fen_parts[3], board);
            }

            return board;
        }

        public static EColor Parse_Active_Color(string fen)
        {
            string[] fen_parts = fen.Trim().Split(' ');

            if (fen_parts.Length < 2)
            {
                return EColor.White;
            }

            switch (fen_parts[1].ToLower())
            {
                case "w":
                    return EColor.White;
                case "b":
                    return EColor.Black;
                default:
                    return EColor.None;
            }
        }

        public static AsGame_State Parse_Game_State_From_FEN(string fen)
        {
            ABoard board = Parse_Board_From_FEN(fen);
            EColor current_player_color = Parse_Active_Color(fen);

            return new AsGame_State(current_player_color, board);
        }

        private static void Parse_Piece_Placement(string piece_placement, ABoard board)
        {
            int col;
            int empty_squares;
            APiece piece;
            string[] ranks = piece_placement.Split('/');

            if (ranks.Length != 8)
            {
                return;
            }

            for (int row = 0; row < 8; row++)
            {
                col  = 0;

                foreach (char c in ranks[row])
                {
                    if (col >= 8)
                    {
                        break;
                    }

                    if (char.IsDigit(c) )
                    {
                        empty_squares = c - '0';
                        col += empty_squares;
                    }
                    else
                    {
                        piece = Get_Piece_From_Char(c);
                        board[row, col] = piece;

                        if (piece.Type == EPiece_Type.Pawn)
                        {
                            Set_Pawn_Moved_State(piece, row, piece.Color);
                        }

                        col++;
                    }

                }
            }
        }

        public static APiece Get_Piece_From_Char(char piece_char)
        {
            EPiece_Type piece_type;
            EColor color = char.IsUpper(piece_char) ? EColor.White : EColor.Black;

            switch (char.ToLower(piece_char))
            {
                case 'p':
                    piece_type = EPiece_Type.Pawn;
                    break;
                case 'n':
                    piece_type = EPiece_Type.Knight;
                    break;
                case 'b':
                    piece_type = EPiece_Type.Bishop;
                    break;
                case 'r':
                    piece_type = EPiece_Type.Rook;
                    break;
                case 'q':
                    piece_type = EPiece_Type.Queen;
                    break;
                case 'k':
                    piece_type = EPiece_Type.King;
                    break;
                default: 
                    piece_type = EPiece_Type.Pawn;
                    break;
            }

            switch (piece_type)
            {       
                case EPiece_Type.Pawn:
                    return new APawn(color);
                
                case EPiece_Type.Knight:
                    return new AKnight(color);
                
                case EPiece_Type.Bishop:
                    return new ABishop(color);
                
                case EPiece_Type.Rook:
                    return new ARook(color);
                
                case EPiece_Type.Queen:
                    return new AQueen(color);

                case EPiece_Type.King:
                    return new AKing(color);

                default:
                    return new APawn(color);
            }
        }
        public static char Get_Char_From_Piece(APiece piece)
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
        private static void Set_Pawn_Moved_State(APiece pawn, int row, EColor color)
        {
            bool is_on_starting_position;

            if (pawn == null || pawn.Type != EPiece_Type.Pawn)
            {
                return;
            }

            is_on_starting_position = (color == EColor.White && row == 6) || (color == EColor.Black && row == 1);

            pawn.Has_Moved = !is_on_starting_position;
        }

        private static void Parse_Castling_Rights(string castling_rights, ABoard board)
        {
            if (castling_rights == "-")
            {
                return;
            }

            board.Reset_Castling_Flags();

            foreach (char c in castling_rights)
            {
                switch (c)
                {
                    case 'K':
                        board.Set_Castling_Right_For(EColor.White, true);
                        break;
                    case 'Q':
                        board.Set_Castling_Right_For(EColor.White, false);
                        break;
                    case 'k':
                        board.Set_Castling_Right_For(EColor.Black, true);
                        break;
                    case 'q':
                        board.Set_Castling_Right_For(EColor.Black, false);
                        break;
                }
            }
        }

        private static void Parse_En_Passant_Target(string en_passant_target, ABoard board)
        {
            EColor player_color;
            APosition pos;
            int row, col;

            if (en_passant_target == "-" || en_passant_target.Length != 2)
            {
                return;
            }

            col = en_passant_target[0] - 'a';
            row = 8 - (en_passant_target[1] - '0');

            pos = new APosition(row, col);

            if (!ABoard.Is_Inside_Board(pos) )
            {
                return;
            }

            player_color = (row == 2) ? EColor.White : EColor.Black;

            board.Set_Pawn_Skip_Position(player_color.Opponent(), pos);
        }
    }
}
