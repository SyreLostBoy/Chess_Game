using Chess_Engine.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;


namespace Chess_Engine.Core
{
    public class AsPrecomputed_Move_Data
    {
        public static readonly ulong[,] Align_Mask;
        public static readonly ulong[,] Dir_Ray_Mask;

        // First 4 are orthogonal, last 4 are diagonals  ( N, S, W, E, NW, SE, NE, SW)
        public static readonly int[] Direction_Offsets = { 8, -8, -1, 1, 7, -7, 9, -9 };

        static readonly SCoordinate[] Dir_Offsets_2D =
        {
            new SCoordinate(0, 1),
            new SCoordinate(0, -1),
            new SCoordinate(-1, 0),
            new SCoordinate(1, 0),
            new SCoordinate(-1, 1),
            new SCoordinate(1, -1),
            new SCoordinate(1, 1),
            new SCoordinate(-1, -1)
        };

        public static readonly int[][] Num_Squares_To_Edge; // Stores number of moves available in each of the 8 directions for every square on the board

        public static readonly byte[][] Knight_Moves; // Stores array of indices for each square a knight can land on from any square on the board
        public static readonly byte[][] King_Moves;

        public static readonly byte[][] Pawn_Attack_Directions =
        {// Pawn attack directions for white and black (NW, NE; SW SE)

            new byte[] { 4, 6 },
            new byte[] { 7, 5 }
        };

        public static readonly int[][] Pawn_Attacks_White;
        public static readonly int[][] Pawn_Attacks_Black;
        public static readonly int[] Direction_Lookup;

        public static readonly ulong[] King_Attack_Bitboards;
        public static readonly ulong[] Knight_Attack_Bitboards;
        public static readonly ulong[][] Pawn_Attack_Bitboards;

        public static readonly ulong[] Rook_Moves;
        public static readonly ulong[] Bishop_Moves;
        public static readonly ulong[] Queen_Moves;

        public static int[,] Orthogonal_Distance; // Manhattan Distance (how many moves for a rook to get from square a to square b)
        public static int[,] King_Distance; // Chebyshev Distance (how many moves for a king to get from square a to square b)
        public static int[] Centre_Manhattan_Distance;

        public static int Num_Rook_Moves_To_Reach_Square(int start_square, int target_square)
        {
            return Orthogonal_Distance[start_square, target_square];
        }

        public static int Num_King_Moves_To_Reach_Square(int start_square, int target_square)
        {
            return King_Distance[start_square, target_square];
        }

        static AsPrecomputed_Move_Data()
        {
            Pawn_Attacks_White = new int[64][];
            Pawn_Attacks_Black = new int[64][];
            Knight_Moves = new byte[64][];
            King_Moves = new byte[64][];
            Num_Squares_To_Edge = new int[64][];

            Rook_Moves = new ulong[64];
            Bishop_Moves = new ulong[64];
            Queen_Moves = new ulong[64];

            int[] all_knight_jumps = { 15, 17, -17, -15, 10, -6, 6, -10 };
            Knight_Attack_Bitboards = new ulong[64];
            King_Attack_Bitboards = new ulong[64];
            Pawn_Attack_Bitboards = new ulong[64][];

            for (int square_index = 0; square_index < 64; square_index++)
            {
                int y = square_index / 8;
                int x = square_index - y * 8;

                int north = 7 - y;
                int south = y;
                int west = x;
                int east = 7 - x;

                Num_Squares_To_Edge[square_index] = new int[8];
                Num_Squares_To_Edge[square_index][0] = north;
                Num_Squares_To_Edge[square_index][1] = south;
                Num_Squares_To_Edge[square_index][2] = west;
                Num_Squares_To_Edge[square_index][3] = east;
                Num_Squares_To_Edge[square_index][4] = Min(north, west);
                Num_Squares_To_Edge[square_index][5] = Min(south, east);
                Num_Squares_To_Edge[square_index][6] = Min(north, east);
                Num_Squares_To_Edge[square_index][7] = Min(south, west);
            
                // Calculate all square knight can jump from current square
                List<byte> legal_knight_jumps = new List<byte>();
                ulong knight_bitboard = 0UL;

                foreach (int knight_jump_delta in all_knight_jumps)
                {
                    int knight_jump_square = square_index + knight_jump_delta;

                    if (knight_jump_square >= 0 && knight_jump_square < 64)
                    {
                        int knight_square_y = knight_jump_square / 8;
                        int knight_square_x = knight_jump_square - knight_square_y * 8;

                        int max_coord_move_dist = Max(Abs(x - knight_square_x), Abs(y - knight_square_y));

                        if (max_coord_move_dist == 2)
                        {
                            legal_knight_jumps.Add((byte)knight_jump_square);
                            knight_bitboard |= 1UL << knight_jump_square;
                        }
                    }
                }

                Knight_Moves[square_index] = legal_knight_jumps.ToArray();
                Knight_Attack_Bitboards[square_index] = knight_bitboard;

                // Calculate all squares king can move to from current square (without castling)
                List<byte> legal_king_moves = new List<byte>();

                foreach (int king_move_delta in Direction_Offsets)
                {
                    int king_move_square = square_index + king_move_delta;

                    if (king_move_square >= 0 && king_move_square < 64)
                    {
                        int king_square_y = king_move_square / 8;
                        int king_square_x = king_move_square - king_square_y * 8;

                        int max_coord_move_dist = Max(Abs(x - king_square_x), Abs(y - king_square_y));

                        if (max_coord_move_dist == 1)
                        {
                            legal_king_moves.Add((byte)king_move_square);
                            King_Attack_Bitboards[square_index] |= 1UL << king_move_square;
                        }

                    }
                }
                King_Moves[square_index] = legal_king_moves.ToArray();

                // Calculate legal pawn captures for white and black
                List<int> pawn_captures_white = new List<int>();
                List<int> pawn_captures_black = new List<int>();
                
                Pawn_Attack_Bitboards[square_index] = new ulong[2];

                if (x > 0)
                {
                    if (y < 7)
                    {
                        pawn_captures_white.Add(square_index + 7);
                        Pawn_Attack_Bitboards[square_index][ABoard.White_Index] |= 1UL << (square_index + 7);
                    }
                    if (y > 0)
                    {
                        pawn_captures_black.Add(square_index - 9);
                        Pawn_Attack_Bitboards[square_index][ABoard.Black_Index] |= 1UL << (square_index - 9);
                    }
                }

                if (x < 7)
                {
                    if (y < 7)
                    {
                        pawn_captures_white.Add(square_index + 9);
                        Pawn_Attack_Bitboards[square_index][ABoard.White_Index] |= 1UL << (square_index + 9);

                    }
                    if (y > 0)
                    {
                        pawn_captures_black.Add(square_index - 7);
                        Pawn_Attack_Bitboards[square_index][ABoard.Black_Index] |= 1UL << (square_index - 7);

                    }
                }

                Pawn_Attacks_White[square_index] = pawn_captures_white.ToArray();
                Pawn_Attacks_Black[square_index] = pawn_captures_black.ToArray();
                

                // Rook Moves
                for (int direction_index = 0; direction_index < 4; direction_index++)
                {
                    int current_dir_offset = Direction_Offsets[direction_index];

                    for (int n = 0; n < Num_Squares_To_Edge[square_index][direction_index]; n++)
                    {
                        int target_square = square_index + current_dir_offset * (n + 1);
                        Rook_Moves[square_index] |= 1UL << target_square;
                    }
                }

                // Bishop Moves
                for (int direction_index = 4; direction_index < 8; direction_index++)
                {
                    int current_dir_offset = Direction_Offsets[direction_index];

                    for (int n = 0; n < Num_Squares_To_Edge[square_index][direction_index]; n++)
                    {
                        int target_square = square_index + current_dir_offset * (n + 1);
                        Bishop_Moves[square_index] |= 1UL << target_square;
                    }
                }

                Queen_Moves[square_index] = Rook_Moves[square_index] | Bishop_Moves[square_index];
            }

            Direction_Lookup = new int[127];

            for (int i = 0; i < 127; i++)
            {
                int offset = i - 63;
                int abs_offset = Abs(offset);
                int abs_dir = 1;

                if (abs_offset % 9 == 0)
                {
                    abs_dir = 9;
                }
                else if (abs_offset % 8 == 0)
                {
                    abs_dir = 8;
                }
                else if (abs_offset % 7 == 0)
                {
                    abs_dir = 7;
                }

                Direction_Lookup[i] = abs_dir * Math.Sign(offset);
            }

            // Distance lookup
            Orthogonal_Distance = new int[64, 64];
            King_Distance = new int[64, 64];
            Centre_Manhattan_Distance = new int[64];

            for (int square_a = 0; square_a < 64; square_a++)
            {
                SCoordinate coord_a = AsBoard_Helper.Coord_From_Index(square_a);
                int file_dist_from_centre = Max(3 - coord_a.File_Index, coord_a.File_Index - 4);
                int rank_dist_from_centre = Max(3 - coord_a.Rank_Index, coord_a.Rank_Index - 4);

                Centre_Manhattan_Distance[square_a] = file_dist_from_centre + rank_dist_from_centre;

                for (int square_b = 0; square_b < 64; square_b++)
                {
                    SCoordinate coord_b = AsBoard_Helper.Coord_From_Index(square_b);
                    int rank_distance = Abs(coord_a.Rank_Index - coord_b.Rank_Index);
                    int file_distance = Abs(coord_a.File_Index - coord_b.File_Index);

                    Orthogonal_Distance[square_a, square_b] = file_distance + rank_distance;
                    King_Distance[square_a, square_b] = Max(file_distance, rank_distance);
                }
            }

            Align_Mask = new ulong[64, 64];
            for (int square_a = 0; square_a < 64; square_a++)
            {
                for (int square_b = 0; square_b < 64; square_b++)
                {
                    SCoordinate coord_a = AsBoard_Helper.Coord_From_Index(square_a);
                    SCoordinate coord_b = AsBoard_Helper.Coord_From_Index(square_b);
                    SCoordinate delta = coord_b - coord_a;
                    SCoordinate dir = new SCoordinate(Math.Sign(delta.File_Index), Math.Sign(delta.Rank_Index));
                    
                    for (int i = -8; i < 8; i++)
                    {
                        SCoordinate coord = AsBoard_Helper.Coord_From_Index(square_a) + dir * i;

                        if (coord.Is_Valid_Square())
                        {
                            Align_Mask[square_a, square_b] |= 1UL << (AsBoard_Helper.Index_From_Coord(coord));
                        }
                    }
                }
            }

            Dir_Ray_Mask = new ulong[8, 64];
            
            for (int dir_index = 0; dir_index < Dir_Offsets_2D.Length; dir_index++)
            {
                for (int square_index = 0; square_index < 64; square_index++)
                {
                    SCoordinate square = AsBoard_Helper.Coord_From_Index(square_index);

                    for (int i = 0; i < 8; i++)
                    {
                        SCoordinate coord = square + Dir_Offsets_2D[dir_index] * i;

                        if (coord.Is_Valid_Square())
                        {
                            Dir_Ray_Mask[dir_index, square_index] |= 1UL << (AsBoard_Helper.Index_From_Coord(coord));
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }
    }
}
