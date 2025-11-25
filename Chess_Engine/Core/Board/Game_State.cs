using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Engine.Core
{
    public struct SGame_State
    {
        public readonly EPiece_Type Captured_Piece_Type;
        public readonly int En_Passant_File;
        public readonly int Castling_Rights;
        public readonly int Fifty_Move_Counter;
        public readonly ulong Zobrist_Key;

        // Masks
        public const int Clear_White_Kingside_Mask = 0b1110;
        public const int Clear_White_Queenside_Mask = 0b1101;
        public const int Clear_Black_Kingside_Mask = 0b1011;
        public const int Clear_Black_Queenside_Mask = 0b0111;

        public SGame_State(EPiece_Type captured_piece_type, int en_passant_file, int castling_rights, int fifty_move_counter, ulong zobrist_key)
        {
            Captured_Piece_Type = captured_piece_type;
            En_Passant_File = en_passant_file;
            Castling_Rights = castling_rights;
            Fifty_Move_Counter = fifty_move_counter;
            Zobrist_Key = zobrist_key;
        }

        public bool Has_Kingside_Castle_Right(bool is_white)
        {
            int mask = is_white ? 1 : 4;
            return (Castling_Rights & mask) != 0;
        }

        public bool Has_Queenside_Castle_Right(bool is_white)
        {
            int mask = is_white ? 2 : 8;
            return (Castling_Rights & mask) != 0;
        }
    }
}
