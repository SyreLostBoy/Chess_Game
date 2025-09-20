using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Logic
{
    public class AKing : APiece
    {
        public AKing(EColor color, bool has_moved = false)
        {
            Color = color;
            Has_Moved = has_moved;
        }
        public override APiece Copy()
        {
            return new AKing(Color, Has_Moved);
        }
        public override EPiece_Type Type => EPiece_Type.King;
        public override EColor Color { get; }

    }
}
