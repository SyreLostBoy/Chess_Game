using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Engine.Core
{
    /*
    16-bit move representation.
    The format is as follows (ffffttttttssssss)
    Bits 0-5: start square index
    Bits 6-11: target square index
    Bits 12-15: flag (promotion type, etc)
    */

    public struct SMove
    {
        readonly ushort Move_Value; // 16-bit move value

        // Flags
        public const int No_Flag = 0b0000;
        public const int En_Passant_Capture_Flag = 0b0001;
        public const int Castle_Flag = 0b0010;
        public const int Pawn_Double_Move_Flag = 0b0011;

        public const int Promote_To_Queen_Flag = 0b0100;
        public const int Promote_To_Knight_Flag = 0b0101;
        public const int Promote_To_Rook_Flag = 0b0110;
        public const int Promote_To_Bishop_Flag = 0b0111;

        public static SMove Null_Move => new SMove(0);
        public ushort Value => Move_Value;
        public bool Is_Null => Move_Value == 0;
        public int Start_Square => Move_Value & Start_Square_Mask;
        public int Target_Square => (Move_Value & Target_Square_Mask) >> 6;
        public int Move_Flag => Move_Value >> 12;
        public bool Is_Promotion => Move_Flag >= Promote_To_Queen_Flag;

        // Masks
        private const ushort Start_Square_Mask = 0b0000000000111111;
        private const ushort Target_Square_Mask = 0b0000111111000000;
        private const ushort Flag_Mask = 0b1111000000000000;

        public SMove(ushort move_value)
        {
            Move_Value = move_value;
        }

        public SMove(int start_square, int target_square)
        {
            Move_Value = (ushort)(start_square | target_square << 6);
        }

        public SMove(int start_square, int target_square, int flag)
        {
            Move_Value = (ushort)(start_square | target_square << 6 | flag << 12);
        }

        public EPiece_Type Get_Promotion_Piece_Type()
        {
            switch (Move_Flag)
            {
                case Promote_To_Rook_Flag: return EPiece_Type.Rook;
                case Promote_To_Knight_Flag: return EPiece_Type.Knight;
                case Promote_To_Bishop_Flag: return EPiece_Type.Bishop;
                case Promote_To_Queen_Flag: return EPiece_Type.Queen;
                default: return EPiece_Type.None;
            }
        }

        public static bool Is_Same_Move(SMove a, SMove b)
        {
            return a.Move_Value == b.Move_Value;
        }
    }
}
