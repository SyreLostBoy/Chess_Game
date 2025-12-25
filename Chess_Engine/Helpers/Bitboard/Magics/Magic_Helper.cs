using Chess_Engine.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Engine.Helpers.Bitboard.Magics
{
    public static class AsMagic_Helper
    {
        public static ulong[] Create_All_Blocker_Bitboards(ulong movement_mask)
        {
            // Создаем список индексов битов, установленных в маске движения
            List<int> move_square_indices = new();
            
            for (int i = 0; i < 64; i++)
            {
                if (((movement_mask >> i) & 1) == 1)
                {
                    move_square_indices.Add(i);
                }
            }

            // Рассчитываем общее количество различных игровых полей (по одному для каждого возможного расположения фигур)
            int num_patterns = 1 << move_square_indices.Count; // 2^n
            ulong[] blocker_bitboards = new ulong[num_patterns];

            // Создаем все битборды
            for (int pattern_index = 0; pattern_index < num_patterns; pattern_index++)
            {
                for (int bitIndex = 0; bitIndex < move_square_indices.Count; bitIndex++)
                {
                    int bit = (pattern_index >> bitIndex) & 1;
                    blocker_bitboards[pattern_index] |= (ulong)bit << move_square_indices[bitIndex];
                }
            }

            return blocker_bitboards;
        }


        public static ulong Create_Movement_Mask(int square_index, bool ortho)
        {
            ulong mask = 0;
            SCoordinate[] directions = ortho ? AsBoard_Helper.Rook_Directions : AsBoard_Helper.Bishop_Directions;
            SCoordinate start_coord = new SCoordinate(square_index);

            foreach (SCoordinate dir in directions)
            {
                for (int dst = 1; dst < 8; dst++)
                {
                    SCoordinate coord = start_coord + dir * dst;
                    SCoordinate next_coord = start_coord + dir * (dst + 1);

                    if (next_coord.Is_Valid_Square())
                    {
                        AsBitboard_Utility.Set_Square(ref mask, coord.Square_Index);
                    }
                    else { break; }
                }
            }
            return mask;
        }

        public static ulong Legal_Move_Bitboard_From_Blockers(int start_square, ulong blocker_bitboard, bool ortho)
        {
            ulong bitboard = 0;

            SCoordinate[] directions = ortho ? AsBoard_Helper.Rook_Directions : AsBoard_Helper.Bishop_Directions;
            SCoordinate startCoord = new SCoordinate(start_square);

            foreach (SCoordinate dir in directions)
            {
                for (int dst = 1; dst < 8; dst++)
                {
                    SCoordinate coord = startCoord + dir * dst;

                    if (coord.Is_Valid_Square())
                    {
                        AsBitboard_Utility.Set_Square(ref bitboard, coord.Square_Index);
                        if (AsBitboard_Utility.Contains_Square(blocker_bitboard, coord.Square_Index))
                        {
                            break;
                        }
                    }
                    else 
                    { 
                        break; 
                    }
                }
            }

            return bitboard;
        }
    }
}
