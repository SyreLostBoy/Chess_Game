using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Types
{
    public enum EMove_Type
    {
        Normal,
        Castle_KS, // Castle king side
        Castle_QS, // Castle queen side
        Double_Pawn_Move,
        En_Passant,
        Pawn_Promotion
    }
}
