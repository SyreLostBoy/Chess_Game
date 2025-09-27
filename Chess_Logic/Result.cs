using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Logic
{
    public class AsResult
    {
        AsResult(EColor winner_color, EEnd_Reason end_reason)
        {
            Winner = winner_color;
            End_Reason = end_reason;
        }

        public EColor Winner { get; }
        public EEnd_Reason End_Reason;

        public static AsResult Win(EColor winner_color)
        {
            return new AsResult(winner_color, EEnd_Reason.Checkmate);
        }

        public static AsResult Draw(EEnd_Reason end_reason)
        {
            return new AsResult(EColor.None, end_reason);
        }
    }
}
