using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Logic
{
    public class AQueen : APiece
    {
        public AQueen(EColor color, bool has_moved = false)
        {
            Color = color;
            Has_Moved = has_moved;
        }
        public override APiece Copy()
        {
            return new AQueen(Color, Has_Moved);
        }
        public override EPiece_Type Type => EPiece_Type.Queen;
        public override EColor Color { get; }

    }
}
