using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Logic
{
    public class ARook : APiece
    {
        public ARook(EColor color, bool has_moved = false)
        {
            Color = color;
            Has_Moved = has_moved;
        }
        public override APiece Copy()
        {
            return new ARook(Color, Has_Moved);
        }
        public override EPiece_Type Type => EPiece_Type.Rook;
        public override EColor Color { get; }

    }
}
