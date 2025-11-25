using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Engine.Core
{
    public struct SKillers
    {
        public SMove Move_A;
        public SMove Move_B;

        public void Add(SMove move)
        { 
            if (move.Value != Move_A.Value)
            {
                Move_B = Move_A;
                Move_A = move;
            }
        }

        public bool Match(SMove move)
        {
            return move.Value == Move_A.Value || move.Value == Move_B.Value;
        }
    }
}
