using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Logic
{
    public class AKnight: APiece
    {
        public AKnight(EColor color, bool has_moved = false)
        {
            Color = color;
            Has_Moved = has_moved;
        }
        public override APiece Copy()
        {
            return new AKnight(Color, Has_Moved);
        }
        public override IEnumerable<AMove> Get_Moves(APosition from_pos, ABoard board)
        {
            foreach (APosition to_pos in Get_Allowed_Move_Positions(from_pos, board))
            {
                yield return new AMove_Normal(from_pos, to_pos);
            }
        }
        public override EPiece_Type Type => EPiece_Type.Knight;
        public override EColor Color { get; }

        private static IEnumerable<APosition> Get_Potential_To_Positions(APosition from_pos)
        {
            ADirection[] vertical_directions = new ADirection[]
            {
                ADirection.North,
                ADirection.South
            };
            ADirection[] horizontal_directions = new ADirection[]
            {
                ADirection.West,
                ADirection.East
            };

            foreach (ADirection ver_dir in vertical_directions)
            {
                foreach (ADirection hor_dir in horizontal_directions)
                {
                    yield return from_pos + 2 * ver_dir + hor_dir;
                    yield return from_pos + 2 * hor_dir + ver_dir;
                }
            }
        }

        private IEnumerable<APosition> Get_Allowed_Move_Positions(APosition from_pos, ABoard board)
        {
            foreach(APosition to_pos in Get_Potential_To_Positions(from_pos) )
            {
                if ( ABoard.Is_Inside_Board(to_pos) && (board.Is_Empty(to_pos) || board[to_pos].Color != this.Color) )
                {
                    yield return to_pos;
                }
            }

            //return Get_Potential_To_Positions(from_pos).Where(pos => ABoard.Is_Inside_Board(to_pos) && (board.Is_Empty(to_pos) || board[to_pos].Color != this.Color) );
        }

    }
}
