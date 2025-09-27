using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Logic
{
    public class ABoard
    {
        public static ABoard Get_Initial_Board()
        {
            ABoard board = new ABoard();

            board.Add_Start_Pieces();

            return board;
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

                if (piece.Can_Capture_King(pos, this) )
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

        private void Add_Start_Pieces()
        {
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

        private readonly APiece[,] Pieces = new APiece[8, 8];
    }
}
