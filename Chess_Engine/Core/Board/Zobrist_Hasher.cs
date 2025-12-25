using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Engine.Core
{
    public static class AsZobrist_Hasher
    {
        public static readonly ulong[,] Pieces_Array = new ulong[APiece.Max_Piece_Index + 1, 64]; // тип фигуры, цвет, индекс клетки
        // У каждого игрока есть 4 возможных варианта рокировки: ни одного, на ферзевом фланге, на королевском фланге, на обоих.
        // Таким образом, с учетом обеих сторон, существует 16 возможных вариантов.
        public static readonly ulong[] Castling_Rights = new ulong[16];
        
        // Файл En passant (0 = нет EP).
        // Ранг указывать не нужно, так как сторона, на которой нужно сделать ход, включена в ключ.
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

        /// <summary>
        ///Вычисляет ключ Зобриста на основе текущей позиции на доске.
        /// ПРИМЕЧАНИЕ: эта функция работает медленно и должна использоваться только при первоначальной настройке доски из FEN-нотации.
        /// </summary>
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
