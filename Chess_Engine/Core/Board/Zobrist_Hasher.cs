using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Engine.Core
{
    public static class AsZobrist_Hasher
    {
        public static readonly ulong[,] Pieces_Array = new ulong[APiece.Max_Piece_Index + 1, 64]; // piece type, color, square index
        // Each player has 4 possible castling right states: none, queenside, kingside, both.
        // So, taking both sides into account, there are 16 possible states.
        public static readonly ulong[] Castling_Rights = new ulong[16];
        
        // En passant file (0 = no ep).
        //  Rank does not need to be specified since side to move is included in key
        public static readonly ulong[] En_Passant_File = new ulong[9];
        public static readonly ulong Side_To_Move;


        static AsZobrist_Hasher()
        {
            const int seed = 67347549;
            System.Random rng = new System.Random(seed);

            for (int square_index = 0; square_index < 64; square_index++)
            {
                foreach (int piece in APiece.Piece_Indices)
                {
                    Pieces_Array[piece, square_index] = Get_Random_Ulong(rng);
                }
            }


            for (int i = 0; i < Castling_Rights.Length; i++)
            {
                Castling_Rights[i] = Get_Random_Ulong(rng);
            }

            for (int i = 0; i < En_Passant_File.Length; i++)
            {
                En_Passant_File[i] = i == 0 ? 0 : Get_Random_Ulong(rng);
            }

            Side_To_Move = Get_Random_Ulong(rng);
        }

        // Calculate zobrist key from current board position.
        // NOTE: this function is slow and should only be used when the board is initially set up from fen.
        // During search, the key should be updated incrementally instead.
        public static ulong Calculate_Zobrist_Key(ABoard board)
        {
            ulong zobrist_key = 0;

            for (int square_index = 0; square_index < 64; square_index++)
            {
                int piece = board.Square[square_index];

                if (APiece.Get_Piece_Type(piece) != EPiece_Type.None)
                {
                    zobrist_key ^= Pieces_Array[piece, square_index];
                }
            }

            zobrist_key ^= En_Passant_File[board.Current_Game_State.En_Passant_File];

            if (board.Move_Color == APiece.Black)
            {
                zobrist_key ^= Side_To_Move;
            }

            zobrist_key ^= Castling_Rights[board.Current_Game_State.Castling_Rights];

            return zobrist_key;
        }

        static ulong Get_Random_Ulong(System.Random rng)
        {
            byte[] buffer = new byte[8];
            rng.NextBytes(buffer);
            return System.BitConverter.ToUInt64(buffer, 0);
        }
    }
}
