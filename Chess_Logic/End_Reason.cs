using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Logic
{
    public enum EEnd_Reason
    {
        Checkmate,
        Stalemate,
        Fifty_Move_Rule,
        Insufficient_Material,
        Threefold_Repetition
    }
}
