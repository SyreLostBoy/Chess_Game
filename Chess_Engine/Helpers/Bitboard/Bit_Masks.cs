using static System.Math;

namespace Chess_Engine.Helpers
{
    public static class AsBit_Masks
    {// A collection of precomputed bitboards.

        public const ulong File_A = 0x101010101010101;
        public const ulong White_Kingside_Mask = 1UL << AsBoard_Helper.f1 | 1UL << AsBoard_Helper.g1;
        public const ulong Black_Kingside_Mask = 1UL << AsBoard_Helper.f8 | 1UL << AsBoard_Helper.g8;

        public const ulong White_Queenside_Mask_2 = 1UL << AsBoard_Helper.d1 | 1UL << AsBoard_Helper.c1;
        public const ulong Black_Queenside_Mask_2 = 1UL << AsBoard_Helper.d8 | 1UL << AsBoard_Helper.c8;

        public const ulong White_Queenside_Mask = White_Queenside_Mask_2 | 1UL << AsBoard_Helper.b1;
        public const ulong Black_Queenside_Mask = Black_Queenside_Mask_2 | 1UL << AsBoard_Helper.b8;

        public static readonly ulong[] White_Passed_Pawn_Mask;
        public static readonly ulong[] Black_Passed_Pawn_Mask;

        public static readonly ulong[] White_Pawn_Support_Mask;
        public static readonly ulong[] Black_Pawn_Support_Mask;

        public static readonly ulong[] File_Mask;
        public static readonly ulong[] Adjacent_File_Masks;

        public static readonly ulong[] King_Safety_Mask;

        public static readonly ulong[] White_Forward_File_Mask;
        public static readonly ulong[] Black_Forward_File_Mask;

        public static readonly ulong[] Triple_File_Mask; // Mask of three consecutive files centred at given file index. // For example, given file '3', the mask would contains files [2,3,4].
    
        static AsBit_Masks()
        {
            File_Mask = new ulong[8];
            Adjacent_File_Masks = new ulong[8];

            for (int i = 0; i < 8; i++)
            {
                File_Mask[i] = File_A << i;
                ulong left = i > 0 ? File_A << (i - 1) : 0;
                ulong right = i < 7 ? File_A << (i + 1) : 0;
                Adjacent_File_Masks[i] = left | right;
            }

            Triple_File_Mask = new ulong[8];

            for (int i = 0; i < 8; i++)
            {
                int clamped_file = System.Math.Clamp(i, 1, 6);
                Triple_File_Mask[i] = File_Mask[clamped_file] | Adjacent_File_Masks[clamped_file];
            }

            White_Passed_Pawn_Mask = new ulong[64];
            Black_Passed_Pawn_Mask = new ulong[64];
            White_Pawn_Support_Mask = new ulong[64];
            Black_Pawn_Support_Mask = new ulong[64];
            White_Forward_File_Mask = new ulong[64];
            Black_Forward_File_Mask = new ulong[64];

            for (int square = 0; square < 64; square++)
            {
                int file = AsBoard_Helper.File_Index(square);
                int rank = AsBoard_Helper.Rank_Index(square);
                ulong adjacent_files = File_A << Max(0, file - 1) | File_A << Min(7, file + 1);

                ulong white_forward_mask = ~(ulong.MaxValue >> (64 - 8 * (rank + 1)));
                ulong black_forward_mask = ((1UL << 8 * rank) - 1);

                White_Passed_Pawn_Mask[square] = (File_A << file | adjacent_files) & white_forward_mask;
                Black_Passed_Pawn_Mask[square] = (File_A << file | adjacent_files) & black_forward_mask;

                ulong adjacent = (1UL << (square - 1) | 1UL << (square - 1)) & adjacent_files;

                White_Pawn_Support_Mask[square] = adjacent | AsBitboard_Utility.Shift(adjacent, -8);
                White_Pawn_Support_Mask[square] = adjacent | AsBitboard_Utility.Shift(adjacent, +8);

                White_Forward_File_Mask[square] = white_forward_mask & File_Mask[file];
                Black_Forward_File_Mask[square] = black_forward_mask & File_Mask[file];
            }

            King_Safety_Mask = new ulong[64];

            for (int i = 0; i < 64; i++)
            {
                King_Safety_Mask[i] = AsBitboard_Utility.King_Moves[i] | (1UL << i);
            }
        }
    }
}
