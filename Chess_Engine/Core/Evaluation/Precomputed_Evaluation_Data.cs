using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Engine.Core
{
    public static class AsPrecomputed_Evaluation_Data
    {
        public static readonly int[][] Pawn_Shield_Squares_White;
        public static readonly int[][] Pawn_Shield_Squares_Black;

        static AsPrecomputed_Evaluation_Data()
        {
            Pawn_Shield_Squares_White = new int[64][];
            Pawn_Shield_Squares_Black = new int[64][];

            for (int square_index = 0; square_index < 64; square_index++)
            {
                Create_Pawn_Shield_Square(square_index);
            }
        }

        private static void Create_Pawn_Shield_Square(int square_index)
        {
            List<int> shield_indices_white = new List<int>();
            List<int> shield_indices_black = new List<int>();
            SCoordinate coord = new SCoordinate(square_index);
            int rank = coord.Rank_Index;
            int file = Math.Clamp(coord.File_Index, 1, 6);

            for (int file_offset = -1; file_offset <= 1; file_offset++)
            {
                Add_If_Valid(new SCoordinate(file + file_offset, rank + 1), shield_indices_white);
                Add_If_Valid(new SCoordinate(file + file_offset, rank - 1), shield_indices_black);

            }

            for (int file_offset = -1; file_offset <= 1; file_offset++)
            {
                Add_If_Valid(new SCoordinate(file + file_offset, rank + 2), shield_indices_white);
                Add_If_Valid(new SCoordinate(file + file_offset, rank - 2), shield_indices_black);

            }

            Pawn_Shield_Squares_White[square_index] = shield_indices_white.ToArray();
            Pawn_Shield_Squares_Black[square_index] = shield_indices_black.ToArray();

        }

        private static void Add_If_Valid(SCoordinate coord, List<int> list)
        {
            if (coord.Is_Valid_Square())
            {
                list.Add(coord.Square_Index);
            }
        }
    }
}
