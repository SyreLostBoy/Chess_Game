using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Logic
{
    public class AMove_En_Passant : AMove
    {
        public override EMove_Type Move_Type => EMove_Type.En_Passant;
        public override APosition From_Position { get; }
        public override APosition To_Position { get; }

        private readonly APosition Capture_Pos;

        public AMove_En_Passant(APosition from_pos, APosition to_pos)
        {
            From_Position = from_pos;
            To_Position = to_pos;
            Capture_Pos = new APosition(from_pos.Row, to_pos.Column);
        }

        public override void Act(ABoard board)
        {
            new AMove_Normal(From_Position, To_Position).Act(board);
            board[Capture_Pos] = null;
        }
    }
}
