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
        public override IEnumerable<AMove> Get_Moves(APosition from_pos, AsBoard board)
        {
            foreach (APosition to_pos in Get_Move_Positions_In_Directions(from_pos, board, Directions) )
            {
                yield return new AMove_Normal(from_pos, to_pos);
            }
        }

        public override EPiece_Type Type => EPiece_Type.Queen;
        public override EColor Color { get; }

        private static readonly ADirection[] Directions = new ADirection[]
        {
           ADirection.North,
           ADirection.East,
           ADirection.West,
           ADirection.South,
           ADirection.North_West,
           ADirection.North_East,
           ADirection.South_West,
           ADirection.South_East
        };
    }
}
