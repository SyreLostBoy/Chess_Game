using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Engine.Helpers
{
    public static class AsBitboard_Utility
    {
        public const ulong File_A = 0x101010101010101;

        public const ulong Rank_1 = 0b11111111;
        public const ulong Rank_2 = Rank_1 << 8;
        public const ulong Rank_3 = Rank_2 << 8;
        public const ulong Rank_4 = Rank_3 << 8;
        public const ulong Rank_5 = Rank_4 << 8;
        public const ulong Rank_6 = Rank_5 << 8;
        public const ulong Rank_7 = Rank_6 << 8;
        public const ulong Rank_8 = Rank_7 << 8;

        public const ulong Not_A_File = ~File_A;
        public const ulong Not_H_File = ~(File_A << 7);

        public static readonly ulong[] Knight_Attacks;
        public static readonly ulong[] King_Moves;
        public static readonly ulong[] White_Pawn_Attacks;
        public static readonly ulong[] Black_Pawn_Attacks;

        static AsBitboard_Utility()
        {
            Knight_Attacks = new ulong[64];
            King_Moves = new ulong[64];
            White_Pawn_Attacks = new ulong[64];
            Black_Pawn_Attacks = new ulong[64];

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    Process_Square(x, y);
                }
            }

        }

        public static int Pop_LSB(ref ulong b)
        {// Get index of least significant set bit in given 64bit value. Clears the bit to zero.
            int i = BitOperations.TrailingZeroCount(b);
            b &= (b - 1);
            return i;
        }

        public static void Set_Square(ref ulong bitboard, int square_index)
        {
            bitboard |= 1UL << square_index;
        }

        public static void Clear_Square(ref ulong bitboard, int square_index)
        {
            bitboard &= ~(1UL << square_index);
        }

        public static void Toggle_Square(ref ulong bitboard, int square_index)
        {
            bitboard ^= 1UL << square_index;
        }

        public static void Toggle_Squares(ref ulong bitboard, int square_a, int square_b)
        {
            bitboard ^= (1UL << square_a | 1UL << square_b);
        }

        public static bool Contains_Square(ulong bitboard, int square)
        {
            return ((bitboard >> square) & 1) != 0;
        }

        public static ulong Get_Pawn_Attacks(ulong pawn_bitboard, bool is_white)
        {
            // Первая половина атак рассчитывается путем смещения всех пешек на северо-восток: northEastAttacks = pawnBitboard << 9
            // ВАЖНО: Пешки в файле h будут перенесены в файл a, поэтому к файлу a применяется маска: northEastAttacks &= notAFile
            // (Все пешки, которые изначально были в файле a, будут смещены в файл b, поэтому файл a должен быть пустым).
            // Вторая половина атак рассчитывается путем смещения всех пешек на северо-запад. На этот раз необходимо примениить маску к файлу h.
            // Объединяем две половины, чтобы получить битовую доску со всеми атаками пешек: northEastAttacks | northWestAttacks

            if (is_white)
            {
                return ((pawn_bitboard << 9) & Not_A_File) | ((pawn_bitboard << 7) & Not_H_File);
            }

            return ((pawn_bitboard >> 7) & Not_A_File) | ((pawn_bitboard >> 9) & Not_H_File);
        }

        public static ulong Shift(ulong bitboard, int num_squares_to_shift)
        {
            if (num_squares_to_shift > 0)
            {
                return bitboard << num_squares_to_shift;
            }
            else
            {
                return bitboard >> -num_squares_to_shift;
            }

        }

        private static void Process_Square(int x, int y)
        {
            int square_index = y * 8 + x;

            (int x, int y)[] ortho_dir = { (-1, 0), (0, 1), (1, 0), (0, -1) };
            (int x, int y)[] diag_dir = { (-1, -1), (-1, 1), (1, 1), (1, -1) };
            (int x, int y)[] knight_jumps = { (-2, -1), (-2, 1), (-1, 2), (1, 2), (2, 1), (2, -1), (1, -2), (-1, -2) };


            for (int dir_index = 0; dir_index < 4; dir_index++)
            {
                // Вертикальные и диагональные направления
                for (int dst = 1; dst < 8; dst++)
                {
                    int ortho_x = x + ortho_dir[dir_index].x * dst;
                    int ortho_y = y + ortho_dir[dir_index].y * dst;
                    int diag_x = x + diag_dir[dir_index].x * dst;
                    int diag_y = y + diag_dir[dir_index].y * dst;

                    if (Valid_Square_Index(ortho_x, ortho_y, out int ortho_target_index))
                    {
                        if (dst == 1)
                        {
                            King_Moves[square_index] |= 1ul << ortho_target_index;
                        }
                    }

                    if (Valid_Square_Index(diag_x, diag_y, out int diag_target_index))
                    {
                        if (dst == 1)
                        {
                            King_Moves[square_index] |= 1ul << diag_target_index;
                        }
                    }
                }

                // Перемещения коня
                for (int i = 0; i < knight_jumps.Length; i++)
                {
                    int knight_x = x + knight_jumps[i].x;
                    int knight_y = y + knight_jumps[i].y;
                    if (Valid_Square_Index(knight_x, knight_y, out int knight_target_square))
                    {
                        Knight_Attacks[square_index] |= 1ul << knight_target_square;
                    }
                }

                // Атаки пешек
                if (Valid_Square_Index(x + 1, y + 1, out int white_pawn_right))
                {
                    White_Pawn_Attacks[square_index] |= 1ul << white_pawn_right;
                }
                if (Valid_Square_Index(x - 1, y + 1, out int white_pawn_left))
                {
                    White_Pawn_Attacks[square_index] |= 1ul << white_pawn_left;
                }


                if (Valid_Square_Index(x + 1, y - 1, out int black_pawn_attack_right))
                {
                    Black_Pawn_Attacks[square_index] |= 1ul << black_pawn_attack_right;
                }
                if (Valid_Square_Index(x - 1, y - 1, out int black_pawn_attack_left))
                {
                    Black_Pawn_Attacks[square_index] |= 1ul << black_pawn_attack_left;
                }

            }
        }

        private static bool Valid_Square_Index(int x, int y, out int index)
        {
            index = y * 8 + x;
            return x >= 0 && x < 8 && y >= 0 && y < 8;
        }

    }
}
