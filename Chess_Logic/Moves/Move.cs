using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Logic
{
    public abstract class AMove
    {
        public abstract EMove_Type Move_Type { get; }
        public abstract APosition From_Position { get; }
        public abstract APosition To_Position { get; }

        public abstract bool Act(ABoard board); // True if piece captured or pawn moved

        public virtual bool Is_Legal(ABoard board)
        {
            EColor player_color = board[From_Position].Color;
            ABoard board_copy = board.Copy();
            
            Act(board_copy);

            return !board_copy.Is_In_Check(player_color);
        }
    }
}
