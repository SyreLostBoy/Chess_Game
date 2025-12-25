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

    public struct STT_Entry
    { // Запись в таблице транспозиции
        public readonly ulong Key;
        public readonly int Value;
        public readonly SMove Move;
        public readonly byte Depth; //Сколько полуходов было просмотрено вперед от этой позиции
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
