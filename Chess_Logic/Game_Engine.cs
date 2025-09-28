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

            Check_For_Game_Over();
        }

        public IEnumerable<AMove> Get_All_Legal_Moves_For(EColor player_color)
        {
            IEnumerable<AMove> move_candidates = Board.Get_Piece_Positions_For(player_color).SelectMany(pos =>
            {
                APiece piece = Board[pos];

                return piece.Get_Moves(pos, Board);
            });

            return move_candidates.Where(move => move.Is_Legal(Board) );
        }

        public bool Is_Game_Over()
        {
            return Result != null;
        }

        public void Restart()
        {
            Board = ABoard.Get_Initial_Board();
            Current_Player_Color = EColor.White;
            Result = null;
        }

        public ABoard Board { get; private set; }
        public AResult Result { get; private set; } = null;
        public EColor Current_Player_Color { get; private set; }

        private void Check_For_Game_Over()
        {
            if (!Get_All_Legal_Moves_For(Current_Player_Color).Any() )
            {
                if (Board.Is_In_Check(Current_Player_Color) )
                {
                    Result = AResult.Win(Current_Player_Color.Opponent() );
                }
                else
                {
                    Result = AResult.Draw(EEnd_Reason.Stalemate);
                }
            }
        }
    }
}
