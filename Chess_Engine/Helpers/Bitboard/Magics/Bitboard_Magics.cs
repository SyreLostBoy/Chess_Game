using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Engine.Helpers.Bitboard.Magics
{
    public static class AsBitboard_Magics
    {
        // Маска для ладьи и слона определяет битовые доски для каждой исходной клетки
        // Маска — это просто список допустимых ходов, доступных фигуре с исходной клетки
        // (на пустой доске), за исключением того, что ходы останавливаются на 1 клетку перед краем доски.
        public static readonly ulong[] Rook_Mask;
        public static readonly ulong[] Bishop_Mask;

        public static readonly ulong[][] Rook_Attacks;
        public static readonly ulong[][] Bishop_Attacks;

        public static ulong Get_Slider_Attacks(int square, ulong blockers, bool ortho)
        {
            return ortho ? Get_Rook_Attacks(square, blockers) : Get_Bishop_Attacks(square, blockers);
        }

        public static ulong Get_Rook_Attacks(int square, ulong blockers)
        {
            ulong key = ((blockers & Rook_Mask[square]) * AsPrecomputed_Magics.Rook_Magics[square]) >> AsPrecomputed_Magics.Rook_Shifts[square];
            return Rook_Attacks[square][key];
        }

        public static ulong Get_Bishop_Attacks(int square, ulong blockers)
        {
            ulong key = ((blockers & Bishop_Mask[square]) * AsPrecomputed_Magics.Bishop_Magics[square]) >> AsPrecomputed_Magics.Bishop_Shifts[square];
            return Bishop_Attacks[square][key];
        }

        static AsBitboard_Magics()
        {
            Rook_Mask = new ulong[64];
            Bishop_Mask = new ulong[64];

            for (int square_index = 0; square_index < 64; square_index++)
            {
                Rook_Mask[square_index] = AsMagic_Helper.Create_Movement_Mask(square_index, true);
                Bishop_Mask[square_index] = AsMagic_Helper.Create_Movement_Mask(square_index, false);
            }

            Rook_Attacks = new ulong[64][];
            Bishop_Attacks = new ulong[64][];

            for (int i = 0; i < 64; i++)
            {
                Rook_Attacks[i] = Create_Table(i, true, AsPrecomputed_Magics.Rook_Magics[i], AsPrecomputed_Magics.Rook_Shifts[i]);
                Bishop_Attacks[i] = Create_Table(i, false, AsPrecomputed_Magics.Bishop_Magics[i], AsPrecomputed_Magics.Bishop_Shifts[i]);
            }
        }

        private static ulong[] Create_Table(int square, bool rook, ulong magic, int left_shift)
        {
            int num_bits = 64 - left_shift;
            int lookup_size = 1 << num_bits;
            ulong[] table = new ulong[lookup_size];

            ulong movement_mask = AsMagic_Helper.Create_Movement_Mask(square, rook);
            ulong[] blocker_patterns = AsMagic_Helper.Create_All_Blocker_Bitboards(movement_mask);

            foreach (ulong pattern in blocker_patterns)
            {
                ulong index = (pattern * magic) >> left_shift;
                ulong moves = AsMagic_Helper.Legal_Move_Bitboard_From_Blockers(square, pattern, rook);
                table[index] = moves;
            }

            return table;
        }
    }
}
