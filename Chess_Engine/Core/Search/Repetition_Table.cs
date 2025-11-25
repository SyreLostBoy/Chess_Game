using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Engine.Core
{
    public class ARepetition_Table
    {
        readonly ulong[] Hashes;
        readonly int[] Start_Indices;
        int Count;

        public ARepetition_Table()
        {
            Hashes = new ulong[256];
            Start_Indices = new int[Hashes.Length + 1];
        }

        public void Init(ABoard board)
        {
            ulong[] initial_hashes = board.Repetition_Position_History.Reverse().ToArray();
            Count = initial_hashes.Length;

            for (int i = 0; i < Count; i++)
            {
                Hashes[i] = initial_hashes[i];
                Start_Indices[i] = 0;
            }

            Start_Indices[Count] = 0;
        }

        public void Push(ulong hash, bool reset)
        {
            if (Count < Hashes.Length)
            {
                Hashes[Count] = hash;
                Start_Indices[Count + 1] = reset ? Count : Start_Indices[Count];
            }

            Count++;
        }

        public void Try_Pop()
        {
            Count = Math.Max(0, Count - 1);
        }

        public bool Contains(ulong hash)
        {
            int start = Start_Indices[Count];

            for (int i = start; i < Count - 1; i++)
            {
                if (Hashes[i] == hash)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
