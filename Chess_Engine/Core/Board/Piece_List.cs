using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Engine.Core
{
    public class APiece_List
    {
        public int[] Occupied_Squares; // Indices of squares occupied by given piece 

        private int[] Map; // Map to go from index of a square, to the index in the Occupied_Squares where that square is stored
        private int Num_Of_Pieces;

        public APiece_List(int max_piece_count = 10)
        {
            Occupied_Squares = new int[max_piece_count];
            Map = new int[64];
            Num_Of_Pieces = 0;

            // Initialize map with -1 (indicating no piece)
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
                // Resize array if needed
                Array.Resize(ref Occupied_Squares, Occupied_Squares.Length * 2);
            }

            Occupied_Squares[Num_Of_Pieces] = square;
            Map[square] = Num_Of_Pieces;
            Num_Of_Pieces++;
        }

        public void Remove_Piece(int square)
        {
            int piece_index = Map[square];
            if (piece_index == -1) return; // Piece not found

            // Move last element to the position of the removed element
            Occupied_Squares[piece_index] = Occupied_Squares[Num_Of_Pieces - 1];

            // Update map for the moved element
            Map[Occupied_Squares[piece_index]] = piece_index;

            // Clear the removed square in map
            Map[square] = -1;

            Num_Of_Pieces--;
        }

        public void Move_Piece(int start_square, int target_square)
        {
            int piece_index = Map[start_square];
            if (piece_index == -1) return; // Piece not found

            Occupied_Squares[piece_index] = target_square;

            // Update map - clear old square and set new square
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
            // Clear all entries in map for currently occupied squares
            for (int i = 0; i < Num_Of_Pieces; i++)
            {
                Map[Occupied_Squares[i]] = -1;
            }
            Num_Of_Pieces = 0;
        }
    }
}
