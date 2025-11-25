
namespace Chess_Logic
{
    public class AsGame_State
    {
        public ABoard Board { get; private set; }
        public AResult Result { get; private set; } = null;
        public EColor Current_Player_Color { get; private set; }

        private int No_Capture_Or_Pawn_Moves;
        private string State_String;

        private readonly Dictionary<string, int> State_History = new Dictionary<string, int>();

        public AsGame_State(EColor current_player_color, ABoard board)
        {
            Current_Player_Color = current_player_color;
            Board = board;
            No_Capture_Or_Pawn_Moves = 0;

            State_String = AsFEN_Generator.Generate_State_String(Board, Current_Player_Color);
            
            State_History[State_String] = 1;
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
                State_History.Clear();
            }
            else
            {
                No_Capture_Or_Pawn_Moves++;
            }

            Current_Player_Color = Current_Player_Color.Opponent();
            
            Update_State_String();
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

        public bool Has_Piece_At(APosition pos)
        {
            return !Board.Is_Empty(pos);
        }

        public bool Is_Game_Over()
        {
            return Result != null;
        }

        public APosition Get_Check_King_Position()
        {
            if (Board.Is_In_Check(Current_Player_Color) )
            {
                return Board.Check_King_Position;
            }

            return null;
        }

        public void Restart()
        {
            Board = ABoard.Get_Board_From_Fen(AsConfig.Start_Position);
            Current_Player_Color = EColor.White;
            Result = null;
            No_Capture_Or_Pawn_Moves = 0;

            State_History.Clear();
            State_String = AsFEN_Generator.Generate_State_String(Board, Current_Player_Color);
            State_History[State_String] = 1;
        }

        private void Update_State_String()
        {
            State_String = AsFEN_Generator.Generate_State_String(Board, Current_Player_Color);

            if (!State_History.ContainsKey(State_String) )
            {
                State_History[State_String] = 1;
            }
            else
            {
                State_History[State_String]++;
            }
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
            else if (Board.Check_Insufficient_Material() )
            {
                Result = AResult.Draw(EEnd_Reason.Insufficient_Material);
            }
            else if (Check_Fifty_Move_Rule() )
            {
                Result = AResult.Draw(EEnd_Reason.Fifty_Move_Rule);
            }
            else if (Check_Threefold_Repetition() )
            {
                Result = AResult.Draw(EEnd_Reason.Threefold_Repetition);
            }
        }

        private bool Check_Fifty_Move_Rule()
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

        private bool Check_Threefold_Repetition()
        {
            return State_History[State_String] == 3;
        }
    }
}
