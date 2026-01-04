using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Engine.Core
{
    public class ATransposition_Table
    {
        public const int Lookup_Failed = -1;

        public STT_Entry[] Entries;
        private ulong Size_Mask;

        public ulong Count;
        public bool Enabled = true;
        ABoard Board;

        public ATransposition_Table(ABoard board, int size_mb)
        {
            Board = board;

            int tt_entry_size_bytes = STT_Entry.Get_Size();
            int desired_table_size_bytes = size_mb * 1024 * 1024;
            int num_entries = desired_table_size_bytes / tt_entry_size_bytes;

            Count = (ulong)num_entries;
            Entries = new STT_Entry[num_entries];
        }

        public STT_Entry Get_Entry(ulong zobrist_key)
        {
            return Entries[zobrist_key % (ulong)Entries.Length];
        }

        public void Clear()
        {
            for (int i = 0; i < Entries.Length; i++)
            {
                Entries[i] = new STT_Entry();
            }
        }

        public void Resize(int size_MB)
        {
            size_MB = Math.Clamp(size_MB, 1, 512); // От 1 МБ до 512 МБ

            int tt_entry_size_bytes = STT_Entry.Get_Size();
            int desired_table_size_bytes = size_MB * 1024 * 1024;

            // Вычисляем оптимальное количество записей (ближайшая степень двойки)
            int raw_num_entries = desired_table_size_bytes / tt_entry_size_bytes;

            // Находим ближайшую степень двойки (для эффективного вычисления индекса через & вместо %)
            int powerOfTwo = 1;
            
            while (powerOfTwo * 2 <= raw_num_entries && powerOfTwo * 2 > 0)
            {
                powerOfTwo *= 2;
            }

            int num_entries = powerOfTwo;

            // Сохраняем старые записи если они есть
            STT_Entry[] oldEntries = null;
            if (Entries != null)
            {
                oldEntries = Entries;
            }

            // Создаем новую таблицу
            Count = (ulong)num_entries;
            Size_Mask = Count - 1;
            Entries = new STT_Entry[num_entries];

            // Очищаем новую таблицу
            Clear();

            Console.WriteLine($"Transposition table resized to {num_entries} entries ({size_MB} MB)");
        }

        public ulong Index
        {
            get
            {
                return Board.Current_Game_State.Zobrist_Key % Count;
            }
        }

        public SMove Try_Get_Stored_Move()
        {
            return Entries[Index].Move;
        }

        public bool Try_Lookup_Evaluation(int depth, int ply_from_root, int alpha, int beta, out int eval)
        {
            eval = 0;
            return false;
        }

        public int Lookup_Evaluation(int depth, int ply_from_root, int alpha, int beta)
        {
            if (!Enabled)
            {
                return Lookup_Failed;
            }

            STT_Entry entry = Entries[Index];

            if (entry.Key != Board.Current_Game_State.Zobrist_Key)
            {
                return Lookup_Failed;
            }

            if (entry.Depth >= depth)
            {
                int correct_score = Correct_Retrieved_Mate_Score(entry.Value, ply_from_root);

                if (entry.Node_Type == ENode_Type.Exact)
                {
                    return correct_score;
                }

                if (entry.Node_Type == ENode_Type.Upper_Bound && correct_score <= alpha)
                {
                    return correct_score;
                }

                if (entry.Node_Type == ENode_Type.Lower_Bound && correct_score >= beta)
                {
                    return correct_score;
                }
            }

            return Lookup_Failed;
        }

        public void Store_Evaluation(int depth, int num_ply_searched, int eval, ENode_Type eval_type, SMove move)
        {
            if (!Enabled)
            {
                return;
            }

            ulong index = Index;

            STT_Entry entry = new STT_Entry(Board.Current_Game_State.Zobrist_Key, Correct_Mate_Score_For_Storage(eval, num_ply_searched), (byte)depth, eval_type, move);
        }

        private int Correct_Mate_Score_For_Storage(int score, int num_ply_searched)
        {
            if (ASearcher.Is_Mate_Score(score))
            {
                int sign = Math.Sign(score);
                return (score * sign + num_ply_searched) * sign;
            }

            return score;
        }

        int Correct_Retrieved_Mate_Score(int score, int num_ply_searched)
        {
            if (ASearcher.Is_Mate_Score(score))
            {
                int sign = Math.Sign(score);
                return (score * sign - num_ply_searched) * sign;
            }

            return score;
        }
    }
}
