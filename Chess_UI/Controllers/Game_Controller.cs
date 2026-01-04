using Chess_Engine.Bot;
using Chess_Engine.Core;
using Chess_Engine.Helpers;
using Chess_UI.Bot_Controller;
using Sound_System;


namespace Chess_UI.Game_Controller
{
    public class AsGame_Controller : IDisposable
    {
        public ABoard Board { get; private set; }
        public AMove_Generator Move_Generator { get; private set; }
        public AsSound_System Sound_System { get; private set; }
        public ABot_Controller Bot_Controller { get; private set; }
        public ADifficulty_Controller Difficulty_Controller { get; private set; }
        public bool Playing_Against_Bot { get; private set; }
        public bool Human_Is_White { get; private set; }
        public bool Is_Board_Enabled { get; private set; } = true;

        public event Action<SMove> On_Move_Made;
        public event Action<EGame_Result> On_Game_Ended;
        public event Action<bool> On_Board_Enabled_Changed;

        public AsGame_Controller()
        {
            Board = ABoard.Create_Board();
            Move_Generator = new AMove_Generator();
            Sound_System = new AsSound_System();
            Difficulty_Controller = new ADifficulty_Controller();
        }

        public void Initialize_Bot()
        {
            Bot_Controller = new ABot_Controller(Board, Difficulty_Controller);
            Bot_Controller.On_Move_Chosen += Handle_Bot_Move;
        }

        public void Start_New_Game(bool playing_against_bot, bool human_is_white)
        {
            Playing_Against_Bot = playing_against_bot;
            Human_Is_White = human_is_white;

            Board.Load_Start_Position();

            if (Playing_Against_Bot)
            {
                if (Bot_Controller == null)
                    Initialize_Bot();

                Bot_Controller.Set_Difficulty(Difficulty_Controller.Current_Difficulty);

                bool bot_should_move_first = (Human_Is_White && !Board.Is_White_To_Move) || (!Human_Is_White && Board.Is_White_To_Move);

                if (bot_should_move_first)
                {
                    Set_Board_Enabled(false);
                    Make_Bot_Move();
                }
                else
                {
                    Set_Board_Enabled(true);
                }
            }
            else
            {
                Set_Board_Enabled(true);
            }
        }

        public bool Try_Make_Move(SMove move)
        {
            if (!Is_Legal_Move(move))
            {
                return false;
            }

            bool is_capture = Board.Square[move.Target_Square] != (int)EPiece_Type.None;

            Board.Make_Move(move);
            Sound_System.Play_Move_Sound(move, is_capture);

            On_Move_Made?.Invoke(move);
            Check_Game_State();

            if (Playing_Against_Bot && !Is_Game_Over() )
            {
                bool bot_should_move = Should_Bot_Move();

                if (bot_should_move)
                {
                    Set_Board_Enabled(false);
                    Make_Bot_Move();
                }
                else
                {
                    Set_Board_Enabled(true);
                }
            }

            return true;
        }

        public bool Is_Legal_Move(SMove move)
        {
            var moves = Move_Generator.Generate_Moves(Board);

            return moves.ToArray().Any(m => m.Start_Square == move.Start_Square &&
                m.Target_Square == move.Target_Square &&
                m.Move_Flag == move.Move_Flag);
        }

        public IEnumerable<SMove> Get_Legal_Moves_For_Square(int square)
        {
            var all_moves = Move_Generator.Generate_Moves(Board);

            return all_moves.ToArray().Where(m => m.Start_Square == square);
        }

        public bool Should_Bot_Move()
        {
            if (!Playing_Against_Bot || Is_Game_Over())
                return false;

            bool white_to_move = Board.Is_White_To_Move;
            return (Human_Is_White && !white_to_move) || (!Human_Is_White && white_to_move);
        }

        public bool Is_Game_Over()
        {
            EGame_Result game_state = AsGame_Manager.Get_Game_State(Board);
            return game_state != EGame_Result.In_Progress;
        }

        public void Set_Bot_Difficulty(EDifficulty difficulty)
        {
            Difficulty_Controller.Set_Difficulty(difficulty);
            Bot_Controller?.Set_Difficulty(difficulty);
        }

        public void UpdateDifficultySettings(ADifficulty_Settings settings)
        {
            Difficulty_Controller.Update_Settings(settings);
            Bot_Controller?.Update_Difficulty_Settings(settings);
        }

        public void Set_Board_Enabled(bool enabled)
        {
            if (Is_Board_Enabled != enabled)
            {
                Is_Board_Enabled = enabled;
                On_Board_Enabled_Changed?.Invoke(enabled);
            }
        }

        public SMove? Get_Last_Move()
        {
            return Board.All_Game_Moves.Count > 0 ? Board.All_Game_Moves[^1] : null;
        }

        public int Get_Check_King_Square()
        {
            if (!Board.Is_In_Check())
                return -1;

            return Board.Is_White_To_Move ? Board.King_Square[ABoard.White_Index] : Board.King_Square[ABoard.Black_Index];
        }

        public string Get_Current_FEN()
        {
            return Board.Current_FEN;
        }

        public void Dispose()
        {
            Bot_Controller?.Dispose();
        }

        private void Make_Bot_Move()
        {
            if (Bot_Controller != null && Should_Bot_Move())

            Task.Run(() => Bot_Controller?.Make_Move());
        }

        private void Handle_Bot_Move(string move_string)
        {
            if (string.IsNullOrEmpty(move_string) || move_string == "null")
                return;

            SMove move = AsMove_Utility.Get_Move_From_UCI_Name(move_string, Board);
            
            Try_Make_Move(move);
        }

        private void Check_Game_State()
        {
            EGame_Result game_state = AsGame_Manager.Get_Game_State(Board);
            
            if (game_state != EGame_Result.In_Progress)
            {
                On_Game_Ended?.Invoke(game_state);
                On_Board_Enabled_Changed?.Invoke(false);
            }
        }

    }
}
