using Chess_Engine.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Engine.Helpers
{
    public class AsBoard_Helper
    {
        public static readonly SCoordinate[] Rook_Directions = { new SCoordinate(-1, 0), new SCoordinate(1, 0), new SCoordinate(0, 1), new SCoordinate(0, -1) };
        public static readonly SCoordinate[] Bishop_Directions = { new SCoordinate(-1, 1), new SCoordinate(1, 1), new SCoordinate(1, -1), new SCoordinate(-1, -1) };

        public const string File_Names = "abcdefgh";
        public const string Rank_Names = "12345678";

        public const int a1 = 0;
        public const int b1 = 1;
        public const int c1 = 2;
        public const int d1 = 3;
        public const int e1 = 4;
        public const int f1 = 5;
        public const int g1 = 6;
        public const int h1 = 7;

        public const int a8 = 56;
        public const int b8 = 57;
        public const int c8 = 58;
        public const int d8 = 59;
        public const int e8 = 60;
        public const int f8 = 61;
        public const int g8 = 62;
        public const int h8 = 63;

        public static int Rank_Index(int square_index)
        {
            return square_index >> 3;
        }

        public static int File_Index(int square_index)
        {
            return square_index & 0b000111;
        }

        public static int Index_From_Coord(int file_index, int rank_index)
        {
            return rank_index * 8 + file_index;
        }

        public static int Index_From_Coord(SCoordinate coordinate)
        {
            return Index_From_Coord(coordinate.File_Index, coordinate.Rank_Index);
        }

        public static SCoordinate Coord_From_Index(int square_index)
        {
            return new SCoordinate(square_index);
        }

        public static bool Is_Light_Square(int file_index, int rank_index)
        {
            return (file_index + rank_index) % 2 != 0;
        }

        public static bool Is_Light_Square(int square_index)
        {
            return Is_Light_Square(File_Index(square_index), Rank_Index(square_index));
        }

        public static string Square_Name_From_Coordinate(int file_index, int rank_index)
        {
            return File_Names[file_index] + "" + (rank_index + 1);
        }

        public static string Square_Name_From_Index(int squareIndex)
        {
            return Square_Name_From_Coordinate(Coord_From_Index(squareIndex));
        }

        public static string Square_Name_From_Coordinate(SCoordinate coord)
        {
            return Square_Name_From_Coordinate(coord.File_Index, coord.Rank_Index);
        }

        public static int Square_Index_From_Name(string name)
        {
            char file_name = name[0];
            char rank_name = name[1];
            int file_index = File_Names.IndexOf(file_name);
            int rank_index = Rank_Names.IndexOf(rank_name);
            return Index_From_Coord(file_index, rank_index);
        }

        public static bool Is_Valid_Coordinate(int x, int y)
        {
            return x >= 0 && x < 8 && y >= 0 && y < 8;
        }

        public static string CreateDiagram(ABoard board, bool black_at_top = true, bool include_fen = true, bool include_zobrist_key = true)
        {/// Creates an ASCII-diagram of the current position.

            System.Text.StringBuilder result = new();
            int last_move_square = board.All_Game_Moves.Count > 0 ? board.All_Game_Moves[^1].Target_Square : -1;

            for (int y = 0; y < 8; y++)
            {
                int rank_index = black_at_top ? 7 - y : y;
                result.AppendLine("+---+---+---+---+---+---+---+---+");

                for (int x = 0; x < 8; x++)
                {
                    int file_index = black_at_top ? x : 7 - x;
                    int square_index = Index_From_Coord(file_index, rank_index);
                    bool highlight = square_index == last_move_square;
                    int piece = board.Square[square_index];
                    
                    if (highlight)
                    {
                        result.Append($"|({APiece.Get_Piece_Symbol(piece)})");
                    }
                    else
                    {
                        result.Append($"| {APiece.Get_Piece_Symbol(piece)} ");
                    }


                    if (x == 7)
                    {
                        // Show rank number
                        result.AppendLine($"| {rank_index + 1}");
                    }
                }

                if (y == 7)
                {
                    // Show file names
                    result.AppendLine("+---+---+---+---+---+---+---+---+");
                    const string file_names = "  a   b   c   d   e   f   g   h  ";
                    const string file_names_rev = "  h   g   f   e   d   c   b   a  ";
                    result.AppendLine(black_at_top ? file_names : file_names_rev);
                    result.AppendLine();

                    if (include_fen)
                    {
                        result.AppendLine($"Fen         : {AsFen_Utility.Get_Current_Fen(board)}");
                    }
                    if (include_zobrist_key)
                    {
                        result.AppendLine($"Zobrist Key : {board.Zobrist_Key}");
                    }
                }
            }

            return result.ToString();
        }

    }
}
