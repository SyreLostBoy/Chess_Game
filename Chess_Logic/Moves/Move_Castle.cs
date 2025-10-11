using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Logic
{
    public class AMove_Castle : AMove
    {
        public override EMove_Type Move_Type { get; }
        public override APosition From_Position { get; }
        public override APosition To_Position { get; }

        private readonly ADirection King_Move_Dir;
        private readonly APosition Rook_From_Pos;
        private readonly APosition Rook_To_Pos;

        public AMove_Castle(EMove_Type move_type, APosition king_pos)
        {
            int king_row = king_pos.Row;

            Move_Type = move_type;
            From_Position = king_pos;

            if (Move_Type == EMove_Type.Castle_KS)
            {
                King_Move_Dir = ADirection.East;
                To_Position = new APosition(king_row, 6);
                Rook_From_Pos = new APosition(king_row, 7);
                Rook_To_Pos = new APosition(king_row, 5);
            }
            else if (Move_Type == EMove_Type.Castle_QS)
            {
                King_Move_Dir = ADirection.West;
                To_Position = new APosition(king_row, 2);
                Rook_From_Pos = new APosition(king_row, 0);
                Rook_To_Pos = new APosition(king_row, 3);
            }
        }

        public override void Act(ABoard board)
        {
            new AMove_Normal(From_Position, To_Position).Act(board);
            new AMove_Normal(Rook_From_Pos, Rook_To_Pos).Act(board);
        }

        public override bool Is_Legal(ABoard board)
        {
            APosition king_pos_in_copy;
            ABoard board_copy;
            EColor player_color = board[From_Position].Color;

            if (board.Is_In_Check(player_color) )
            {
                return false;
            }

            board_copy = board.Copy();
            king_pos_in_copy = From_Position;

            for (int i = 0; i < 2; i++)
            {
                new AMove_Normal(king_pos_in_copy, king_pos_in_copy + King_Move_Dir).Act(board_copy);
                king_pos_in_copy += King_Move_Dir;

                if (board_copy.Is_In_Check(player_color) )
                {
                    return false;
                }
            }

            return true;
        }

    }
}
