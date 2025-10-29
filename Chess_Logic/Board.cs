using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Logic
{
    public class ABoard
    {
        public APosition Check_King_Position;

        private readonly APiece[,] Pieces = new APiece[8, 8];
        private readonly Dictionary<EColor, APosition> Pawn_Skip_Positions = new Dictionary<EColor, APosition>
        {
            {EColor.White, null },
            {EColor.Black, null }
        };

        public static ABoard Get_Initial_Board()
        {// Legacy

            ABoard board = new ABoard();

            board.Add_Start_Pieces();

            return board;
        }

        public static ABoard Get_Board_From_Fen(string fen)
        {
            return AsFen_Parser.Parse_Board_From_FEN(fen);
        }

        public APiece this[int row, int col]
        {
            get { return Pieces[row, col]; }
            set { Pieces[row, col] = value; }
        }

        public APiece this[APosition pos]
        {
            get { return Pieces[pos.Row, pos.Column]; }
            set { Pieces[pos.Row, pos.Column] = value; }
        }

        public static bool Is_Inside_Board(APosition pos)
        {
            return (pos.Row >= 0 && pos.Row < 8) && (pos.Column >= 0 && pos.Column < 8);
        }

        public bool Is_Empty(APosition pos)
        {
            return Pieces[pos.Row, pos.Column] == null;
        }

        public bool Is_In_Check(EColor player_color)
        {
            APiece piece;

            foreach (APosition pos in Get_Piece_Positions_For(player_color.Opponent() ) )
            {
                piece = this[pos];

                if (piece.Can_Capture_King(pos, this, ref Check_King_Position) )
                {
                    return true;
                }
            }

            return false;
        }

        public IEnumerable<APosition> Get_Piece_Positions()
        {
            APosition pos;

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    pos = new APosition(row, col);

                    if (!Is_Empty(pos) )
                    {
                        yield return pos;  
                    }
                }
            }
        }

        public IEnumerable<APosition> Get_Piece_Positions_For(EColor color)
        {
            foreach(APosition pos in Get_Piece_Positions() )
            {
                if (this[pos].Color == color)
                {
                    yield return pos;
                }
            }
        }

        public ABoard Copy()
        {
            ABoard board_copy = new ABoard();

            foreach (APosition pos in Get_Piece_Positions() )
            {
                board_copy[pos] = this[pos].Copy();
            }

            return board_copy;
        }

        public ACounting Count_Pieces()
        {
            APiece piece; 
            ACounting counting = new ACounting();

            foreach (APosition pos in Get_Piece_Positions() )
            {
                piece = this[pos];
                counting.Increment(piece.Color, piece.Type);
            }

            return counting;
        }

        public bool Check_Insufficient_Material()
        {
            ACounting counting = Count_Pieces();

            if (Is_King_Bishop_VS_King(counting) || Is_King_Bishop_VS_King_Bishop(counting) || 
                Is_King_Knight_VS_King(counting) || Is_King_VS_King(counting) )
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public APosition Get_Pawn_Skip_Position(EColor player_color)
        {
            return Pawn_Skip_Positions[player_color];
        }

        public void Set_Pawn_Skip_Position(EColor player_color, APosition position)
        {
            Pawn_Skip_Positions[player_color] = position;
        }

        public void Reset_Castling_Flags()
        {
            APiece piece;

            foreach(APosition pos in Get_Piece_Positions() )
            {
                piece = this[pos];

                if (piece.Type == EPiece_Type.King || piece.Type == EPiece_Type.Rook)
                {
                    piece.Has_Moved = true;
                }
            }
        }

        public void Set_Castling_Right_For(EColor player_color, bool is_king_side)
        {
            int row = player_color == EColor.White ? 7 : 0;
            int king_col = 4;
            int rook_col = is_king_side ? 0 : 7;

            APiece king_piece = this[row, king_col];
            APiece rook_piece = this[row, rook_col];

            if (king_piece.Type == EPiece_Type.King && king_piece.Color == player_color)
            {
                king_piece.Has_Moved = false;
            }

            if (rook_piece.Type == EPiece_Type.Rook && rook_piece.Color == player_color)
            {
                rook_piece.Has_Moved = false;
            }
        }

        public bool Has_Castle_Right_KS(EColor player_color)
        {
            switch (player_color)
            {
                case EColor.Black:
                    return Is_Unmoved_King_And_Rook(new APosition(7, 4), new APosition(7, 7));

                case EColor.White:
                    return Is_Unmoved_King_And_Rook(new APosition(0, 4), new APosition(0, 7));

                default:
                    return false;
            }
        }

        public bool Has_Castle_Right_QS(EColor player_color)
        {
            switch (player_color)
            {
                case EColor.Black:
                    return Is_Unmoved_King_And_Rook(new APosition(7, 4), new APosition(0, 0));

                case EColor.White:
                    return Is_Unmoved_King_And_Rook(new APosition(0, 4), new APosition(7, 0));

                default:
                    return false;
            }
        }

        public bool Can_Capture_En_Passant(EColor player_color)
        {
            APosition[] pawn_positions;
            APosition skip_position = Get_Pawn_Skip_Position(player_color.Opponent());

            if (skip_position == null)
            {// opponent didn't move two squares
                return false;
            }

            switch (player_color)
            {
                case EColor.Black:
                    pawn_positions = new APosition[] { skip_position + ADirection.North_West, skip_position + ADirection.North_East };
                    break;

                case EColor.White:
                    pawn_positions = new APosition[] { skip_position + ADirection.South_West, skip_position + ADirection.South_East };
                    break;

                default:
                    pawn_positions = Array.Empty<APosition>();
                    break;
            }

            return Has_Pawn_In_Position(player_color, pawn_positions, skip_position);
        }

        private bool Has_Pawn_In_Position(EColor player_color, APosition[] pawn_positions, APosition skip_pos)
        {
            APiece piece;
            AMove_En_Passant en_passant_move;

            foreach (APosition pos in pawn_positions)
            {
                if (!Is_Inside_Board(pos) )
                {
                    continue;
                }

                piece = this[pos];
                
                if (piece == null || piece.Color != player_color || piece.Type != EPiece_Type.Pawn)
                {
                    continue;
                }

                en_passant_move = new AMove_En_Passant(pos, skip_pos);

                if (en_passant_move.Is_Legal(this) )
                {
                    return true;
                }
            }

            return false;
        }

        private void Add_Start_Pieces()
        {// Legacy. Теперь используется инициализация доски из FEN строки

            // 1. Задний ряд
            // 1.1 Черные фигуры
            Pieces[0, 0] = new ARook(EColor.Black);
            Pieces[0, 1] = new AKnight(EColor.Black);
            Pieces[0, 2] = new ABishop(EColor.Black);
            Pieces[0, 3] = new AQueen(EColor.Black);
            Pieces[0, 4] = new AKing(EColor.Black);
            Pieces[0, 5] = new ABishop(EColor.Black);
            Pieces[0, 6] = new AKnight(EColor.Black);
            Pieces[0, 7] = new ARook(EColor.Black);

            // 1.2 Белые фигуры
            Pieces[7, 0] = new ARook(EColor.White);
            Pieces[7, 1] = new AKnight(EColor.White);
            Pieces[7, 2] = new ABishop(EColor.White);
            Pieces[7, 3] = new AQueen(EColor.White);
            Pieces[7, 4] = new AKing(EColor.White);
            Pieces[7, 5] = new ABishop(EColor.White);
            Pieces[7, 6] = new AKnight(EColor.White);
            Pieces[7, 7] = new ARook(EColor.White);

            // 2. Передний ряд (пешки)
            for (int i = 0; i < 8; i++)
            {
                Pieces[1, i] = new APawn(EColor.Black);
                Pieces[6, i] = new APawn(EColor.White);
            }
        }

        private APosition Find_First_Piece(EColor color, EPiece_Type piece_type)
        {
            return Get_Piece_Positions_For(color).First(pos => this[pos].Type == piece_type);
        }

        private bool Is_Unmoved_King_And_Rook(APosition king_pos, APosition rook_pos)
        {
            APiece king, rook;

            if (Is_Empty(king_pos) || Is_Empty(rook_pos) )
            {
                return false;
            }

            king = this[king_pos];
            rook = this[king_pos];

            if (king.Type == EPiece_Type.King && rook.Type == EPiece_Type.Rook &&
                king.Has_Moved == false && rook.Has_Moved == false)
            {
                return true;
            }

            return false;
        }

        private bool Is_King_Bishop_VS_King_Bishop(ACounting counting)
        {
            APosition white_bishop_pos, black_bishop_pos;

            if (counting.Total_Count != 4)
            {
                return false;
            }

            if (counting.Get_White_Count(EPiece_Type.Bishop) != 1 || counting.Get_Black_Count(EPiece_Type.Bishop) != 1)
            {
                return false;
            }

            white_bishop_pos = Find_First_Piece(EColor.White, EPiece_Type.Bishop);
            black_bishop_pos = Find_First_Piece(EColor.Black, EPiece_Type.Bishop);

            if (white_bishop_pos.Get_Square_Color() == black_bishop_pos.Get_Square_Color())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool Is_King_VS_King(ACounting counting)
        {
            return counting.Total_Count == 2;
        }

        private static bool Is_King_Bishop_VS_King(ACounting counting)
        {
            if (counting.Total_Count == 3 && (counting.Get_White_Count(EPiece_Type.Bishop) == 1 || counting.Get_Black_Count(EPiece_Type.Bishop) == 1) )
            {
                return true;
            }

            return false;
        }

        private static bool Is_King_Knight_VS_King(ACounting counting)
        {
            if (counting.Total_Count == 3 && (counting.Get_White_Count(EPiece_Type.Knight) == 1 || counting.Get_Black_Count(EPiece_Type.Knight) == 1) )
            {
                return true;
            }

            return false;
        }
    }
}
