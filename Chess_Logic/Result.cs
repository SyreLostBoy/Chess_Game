using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Logic
{
    public class AResult
    {
        AResult(EColor winner_color, EEnd_Reason end_reason)
        {
            Winner = winner_color;
            End_Reason = end_reason;
        }

        public EColor Winner { get; }
        public EEnd_Reason End_Reason;

        public static AResult Win(EColor winner_color)
        {
            return new AResult(winner_color, EEnd_Reason.Checkmate);
        }

        public static AResult Draw(EEnd_Reason end_reason)
        {
            return new AResult(EColor.None, end_reason);
        }
    }
}
