using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Logic
{
    public class AsGame_Engine
    {
        public AsGame_Engine(EColor current_player_color, ABoard board)
        {
            Current_Player_Color = current_player_color;
            Board = board;
        }

        public IEnumerable<AMove> Get_Legal_Moves_For_Piece(APosition pos)
        {
            APiece piece;
            IEnumerable<AMove> move_candidates;

            if (Board.Is_Empty(pos) || Board[pos].Color != Current_Player_Color)
            {
                return Enumerable.Empty<AMove>();
            }

            piece = Board[pos];

            move_candidates = piece.Get_Moves(pos, Board);

            return move_candidates.Where(move => move.Is_Legal(Board) );
        }

        public void Act_Move(AMove move)
        {
            move.Act(Board);
            Current_Player_Color = Current_Player_Color.Opponent();
        }

        public ABoard Board { get; }
        public EColor Current_Player_Color { get; private set; }
    }
}
