using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Logic
{
    public class AsBoard
    {
        public static AsBoard Get_Initial_Board()
        {
            AsBoard board = new AsBoard();

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
