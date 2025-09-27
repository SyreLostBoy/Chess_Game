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
        public override IEnumerable<AMove> Get_Moves(APosition from_pos, ABoard board)
        {
            foreach (APosition to_pos in Get_Move_Positions(from_pos, board) )
            {
                yield return new AMove_Normal(from_pos, to_pos);
            }
        }
        public override bool Can_Capture_King(APosition from_pos, ABoard board)
        {
            APiece piece;

            foreach (AMove move in Get_Moves(from_pos, board) )
            {
                piece = board[move.To_Position];

                if (piece != null && piece.Type == EPiece_Type.King)
                {
                    return true;
                }
            }

            return false;
        }

        public override EPiece_Type Type => EPiece_Type.King;
        public override EColor Color { get; }

        private IEnumerable<APosition> Get_Move_Positions(APosition from_pos, ABoard board)
        {
            APosition to_pos;

            foreach (ADirection dir in Directions)
            {
                to_pos = from_pos + dir;

                if (!ABoard.Is_Inside_Board(to_pos))
                {
                    continue;
                }

                if (board.Is_Empty(to_pos) || board[to_pos].Color != this.Color)
                {
                    yield return to_pos;
                }
            }
        }

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
