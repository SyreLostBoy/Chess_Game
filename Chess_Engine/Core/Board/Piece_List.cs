using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Engine.Core
{
    public class APiece_List
    {
        public int[] Occupied_Squares; // Индексы клеток, занятых данной фигурой 

        private int[] Map; // Карта для перехода от индекса клетки к индексу в Occupied_Squares, где хранится эта клетка.
        private int Num_Of_Pieces;

        public APiece_List(int max_piece_count = 10)
        {
            Occupied_Squares = new int[max_piece_count];
            Map = new int[64];
            Num_Of_Pieces = 0;

            // Инициализируем карту значением -1 (указывает на отсутствие фигуры)
            for (int i = 0; i < Map.Length; i++)
            {
                Map[i] = -1;
            }
        }

        public int Count
        {
            get
            {
                return Num_Of_Pieces;
            }
        }

        public void Add_Piece(int square)
        {
            if (Num_Of_Pieces >= Occupied_Squares.Length)
            {
                Array.Resize(ref Occupied_Squares, Occupied_Squares.Length * 2);
            }

            Occupied_Squares[Num_Of_Pieces] = square;
            Map[square] = Num_Of_Pieces;
            Num_Of_Pieces++;
        }

        public void Remove_Piece(int square)
        {
            int piece_index = Map[square];

            if (piece_index == -1)
            {// Фигура не найдена
                return;
            }

            // Перемещаем последний элемент на позицию удаленного элемента
            Occupied_Squares[piece_index] = Occupied_Squares[Num_Of_Pieces - 1];

            // Обновляем карту для перемещенного элемента
            Map[Occupied_Squares[piece_index]] = piece_index;

            Map[square] = -1;

            Num_Of_Pieces--;
        }

        public void Move_Piece(int start_square, int target_square)
        {
            int piece_index = Map[start_square];
            
            if (piece_index == -1)
            {// Фигура не найдена
                return;
            }

            Occupied_Squares[piece_index] = target_square;

            Map[start_square] = -1;
            Map[target_square] = piece_index;
        }

        public int this[int index]
        {
            get
            {
                if (index < 0 || index >= Num_Of_Pieces)
                    throw new IndexOutOfRangeException($"Index {index} is out of range. Count: {Num_Of_Pieces}");
                return Occupied_Squares[index];
            }
        }

        public bool Contains(int square)
        {
            return Map[square] != -1;
        }

        public void Clear()
        {
            for (int i = 0; i < Num_Of_Pieces; i++)
            {
                Map[Occupied_Squares[i]] = -1;
            }

            Num_Of_Pieces = 0;
        }
    }
}
