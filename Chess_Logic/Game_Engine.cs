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
        public ABoard Board { get; private set; }
        public AResult Result { get; private set; } = null;
        public EColor Current_Player_Color { get; private set; }

        private int No_Capture_Or_Pawn_Moves;

        public AsGame_Engine(EColor current_player_color, ABoard board)
        {
            Current_Player_Color = current_player_color;
            Board = board;
            No_Capture_Or_Pawn_Moves = 0;
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
            Board.Set_Pawn_Skip_Position(Current_Player_Color, null);
            
            if (move.Act(Board) )
            {// Capture or pawn move
                No_Capture_Or_Pawn_Moves = 0;
            }
            else
            {
                No_Capture_Or_Pawn_Moves++;
            }
            
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
            No_Capture_Or_Pawn_Moves = 0;
        }

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
            else if (Board.Is_Insufficient_Material() )
            {
                Result = AResult.Draw(EEnd_Reason.Insufficient_Material);
            }
            else if (Fifty_Move_Rule() )
            {
                Result = AResult.Draw(EEnd_Reason.Fifty_Move_Rule);
            }
        }

        private bool Fifty_Move_Rule()
        {
            int full_moves = No_Capture_Or_Pawn_Moves / 2;

            if (full_moves == 50)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
