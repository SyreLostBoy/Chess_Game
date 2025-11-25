using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Engine.Core
{
    public enum EGame_Result
    {
        Not_Started,
        In_Progress,
        White_Is_Mated,
        Black_Is_Mated,
        Stalemate,
        Repetition,
        Fifty_Move_Rule,
        Insufficient_Material,
        Draw_By_Arbiter,
        White_Timeout,
        Black_Timeout,
        White_Illegal_Move,
        Black_Illegal_Move
    };
}
