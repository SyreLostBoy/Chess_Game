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

        // Первые 4 вертикали, последние 4 диагонали     ( С, Ю, З, В, СЗ, ЮВ, СВ, ЮЗ)
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

        public static readonly int[][] Num_Squares_To_Edge; // Сохраняет количество ходов, доступных в каждом из 8 направлений для каждой клетки на доске.

        public static readonly byte[][] Knight_Moves; // Хранит массив индексов для каждой клетки, на которую может сходить конь с любой клетки доски.
        public static readonly byte[][] King_Moves;

        public static readonly byte[][] Pawn_Attack_Directions =
        {// Направления атаки пешкой для белых и чёрных

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

        public static int[,] Orthogonal_Distance; // Манхэттенское расстояние (сколько ходов ладье нужно сделать, чтобы добраться с поля a на поле b)
        public static int[,] King_Distance; // Расстояние Чебышева (сколько ходов потребуется королю, чтобы добраться с клетки А на клетку Б)
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

            Knight_Attack_Bitboards = new ulong[64];
            King_Attack_Bitboards = new ulong[64];
            Pawn_Attack_Bitboards = new ulong[64][];

            Direction_Lookup = new int[127];
            Align_Mask = new ulong[64, 64];
            Dir_Ray_Mask = new ulong[8, 64];

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

                Calculate_Knight_Jumps(square_index);
                Calculate_King_Moves_Without_Castling(square_index);
                Calculate_Pawn_Captures(square_index);

                Calculate_Slider_Moves(square_index, true);
                Calculate_Slider_Moves(square_index, false);

                Queen_Moves[square_index] = Rook_Moves[square_index] | Bishop_Moves[square_index];
            }

            Calculate_Direction_Lookup();
            Calculate_Distance_Lookup();

            Calculate_Align_Mask();
            Calculate_Dir_Ray_Mask();
        }

        private static void Calculate_Knight_Jumps(int square_index)
        {// Рассчитывает, на какой квадрат может сходить конь с текущего квадрата
            int y = square_index / 8;
            int x = square_index - y * 8;
            List<byte> legal_knight_jumps = new List<byte>();
            ulong knight_bitboard = 0UL;
            int[] all_knight_jumps = { 15, 17, -17, -15, 10, -6, 6, -10 };

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
        }

        private static void Calculate_King_Moves_Without_Castling(int square_index)
        { // Вычисляет все клетки, на которые король может переместиться с текущей клетки (без рокировки)
            int y = square_index / 8;
            int x = square_index - y * 8;
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
        }

        private static void Calculate_Pawn_Captures(int square_index)
        {// Рассчитывает допустимые взятия пешек для белых и черных
            int y = square_index / 8;
            int x = square_index - y * 8;
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
        }

        private static void Calculate_Slider_Moves(int square_index, bool is_orthogonal)
        {
            int start_index = is_orthogonal ? 0 : 4;
            int end_index = is_orthogonal ? 4 : 8;

            for (int direction_index = start_index; direction_index < end_index; direction_index++)
            {
                int current_dir_offset = Direction_Offsets[direction_index];

                for (int n = 0; n < Num_Squares_To_Edge[square_index][direction_index]; n++)
                {
                    int target_square = square_index + current_dir_offset * (n + 1);

                    if (is_orthogonal)
                    {
                        Rook_Moves[square_index] |= 1UL << target_square;
                    }
                    else
                    {
                        Bishop_Moves[square_index] |= 1UL << target_square;
                    }

                }

            }
        }

        private static void Calculate_Direction_Lookup()
        {
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
        }

        private static void Calculate_Distance_Lookup()
        {
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
        }

        private static void Calculate_Align_Mask()
        {
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
        }

        private static void Calculate_Dir_Ray_Mask()
        {
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
