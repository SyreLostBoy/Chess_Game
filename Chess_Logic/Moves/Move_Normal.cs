using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Logic
{
    public class AMove_Normal : AMove
    {
        public override EMove_Type Move_Type => EMove_Type.Normal;
        public override APosition From_Position { get; }
        public override APosition To_Position { get; }

        public AMove_Normal(APosition from_pos, APosition to_pos)
        {
            From_Position = from_pos;
            To_Position = to_pos;
        }

        public override bool Act(ABoard board)
        {
            bool captured;
            APiece piece = board[From_Position];

            captured = !board.Is_Empty(To_Position);

            board[To_Position] = piece;
            board[From_Position] = null;

            piece.Has_Moved = true;

            return captured || piece.Type == EPiece_Type.Pawn;
        }
    }
}
