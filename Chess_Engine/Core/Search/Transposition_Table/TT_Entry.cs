using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Engine.Core
{
    public enum ENode_Type
    {
        Exact,
        Lower_Bound,
        Upper_Bound
    }

    // Transposition table entry
    public struct STT_Entry
    {
        public readonly ulong Key;
        public readonly int Value;
        public readonly SMove Move;
        public readonly byte Depth; //How many ply were searched ahead from this position
        public readonly ENode_Type Node_Type;

        public STT_Entry(ulong key, int value, byte depth, ENode_Type node_type, SMove move)
        {
            Key = key;
            Value = value;
            Move = move;
            Depth = depth;
            Node_Type = node_type;
        }

        public static int Get_Size()
        {
            return System.Runtime.InteropServices.Marshal.SizeOf<STT_Entry>();
        }
    }
}
