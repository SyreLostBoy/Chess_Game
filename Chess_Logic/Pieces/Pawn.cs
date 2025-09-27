using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Logic
{
    public class APawn : APiece 
    {
        public APawn(EColor color, bool has_moved = false)
        {
            Color = color;
            Has_Moved = has_moved;

            if (Color == EColor.White)
            {
                Forward_Direction = ADirection.North;
            }
            else if (Color == EColor.Black)
            {
                Forward_Direction = ADirection.South;
            }
        }
        public override APiece Copy()
        {
            return new APawn(Color, Has_Moved);
        }
        public override IEnumerable<AMove> Get_Moves(APosition from_pos, AsBoard board)
        {
            return Get_Forward_Moves(from_pos, board).Concat(Get_Diagonal_Moves(from_pos, board));
        }

        public override EPiece_Type Type => EPiece_Type.Pawn;
        public override EColor Color { get; }
        

        private static bool Can_Move_To(APosition pos, AsBoard board)
        {
            return AsBoard.Is_Inside_Board(pos) && board.Is_Empty(pos);
        }
        private bool Can_Capture_At(APosition pos, AsBoard board)
        {
            if (!AsBoard.Is_Inside_Board(pos) || board.Is_Empty(pos) )
            {
                return false;
            }

            return board[pos].Color != this.Color;
        }
        private IEnumerable<AMove> Get_Forward_Moves(APosition from, AsBoard board)
        {
            APosition one_move_pos = from + Forward_Direction;
            APosition two_move_pos;

            if (Can_Move_To(one_move_pos, board) )
            {
                yield return new AMove_Normal(from, one_move_pos);

                two_move_pos = one_move_pos + Forward_Direction;

                if (!Has_Moved && Can_Move_To(two_move_pos, board) )
                {
                    yield return new AMove_Normal(from, two_move_pos);
                }
            }
        }
        private IEnumerable<AMove> Get_Diagonal_Moves(APosition from_pos, AsBoard board)
        {
            APosition to_pos;

            ADirection[] horizontal_directions = new ADirection[]
            {
                ADirection.West,
                ADirection.East
            };

            foreach (ADirection dir in horizontal_directions)
            {
                to_pos = from_pos + Forward_Direction + dir; // Forward_Direction + dir = диагональное направление относительно прямого движения пешки

                if (Can_Capture_At(to_pos, board) )
                {
                    yield return new AMove_Normal(from_pos, to_pos);
                }
            }
        }

        private ADirection Forward_Direction;

    }
}
