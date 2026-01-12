using Chess_Engine.Bot;
using Chess_Engine.Core;
using Chess_Engine.Helpers;
using Chess_UI.Bot_Controller;
using Chess_UI.Menus;
using Sound_System;
using System.Windows;


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

        public SGame_Settings Current_Settings { get; private set; }
        private Timer Bot_Moves_Gap_Timer;
        private const int Move_Delay_Ms = 1000;

        public event Action<SMove> On_Move_Made;
        public event Action<EGame_Result> On_Game_Ended;
        public event Action<bool> On_Board_Enabled_Changed;
        public event Action On_Bot_Vs_Bot_Started;
        public event Action On_Bot_Vs_Bot_Stopped;

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

            if (Current_Settings.Game_Mode != EGame_Mode.Human_Vs_Human)
            {
                Bot_Controller.Set_Difficulty(Current_Settings.Bot_Difficulty);
            }
        }

        public void Start_New_Game(SGame_Settings settings)
        {
            Current_Settings = settings;

            Playing_Against_Bot = settings.Game_Mode != EGame_Mode.Human_Vs_Human;

            if (settings.Game_Mode == EGame_Mode.Human_Vs_Bot)
            {
                Human_Is_White = settings.Player_Is_White ?? new Random().Next(0, 2) == 0;
            }
            else
            {
                Human_Is_White = true; // Для других режимов неважно
            }

            Board.Load_Start_Position();
            Apply_Settings(settings);

            if (Playing_Against_Bot)
            {
                if (Bot_Controller == null)
                    Initialize_Bot();

                if (Current_Settings.Game_Mode == EGame_Mode.Bot_Vs_Bot)
                {
                    Set_Board_Enabled(false);
                    Start_Bot_Vs_Bot_Game();
                    On_Bot_Vs_Bot_Started?.Invoke();
                }
                else
                {
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
                
            }
            else
            {
                Set_Board_Enabled(true);
            }
        }

        private void Start_Bot_Vs_Bot_Game()
        {
            Bot_Moves_Gap_Timer?.Dispose();

            if (Is_Game_Over())
            {
                return;
            }

            Bot_Moves_Gap_Timer = new Timer(Bot_Vs_Bot_Timer_Callback, null, Move_Delay_Ms, Timeout.Infinite);
        }

        private void Bot_Vs_Bot_Timer_Callback(object state)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (Current_Settings.Game_Mode != EGame_Mode.Bot_Vs_Bot || Is_Game_Over())
                {
                    Bot_Moves_Gap_Timer?.Dispose();
                    Bot_Moves_Gap_Timer = null;
                    return;
                }

                if (Bot_Controller != null)
                {
                    Bot_Controller.Make_Move();
                }

                if (Bot_Moves_Gap_Timer != null && !Is_Game_Over())
                {
                    Bot_Moves_Gap_Timer.Change(Move_Delay_Ms, Timeout.Infinite);
                }
            });
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

            if (Current_Settings.Game_Mode == EGame_Mode.Bot_Vs_Bot || Is_Game_Over())
            {
                if (Bot_Moves_Gap_Timer == null)
                {
                    Start_Bot_Vs_Bot_Game();
                }
            }

            if (Playing_Against_Bot && Current_Settings.Game_Mode != EGame_Mode.Bot_Vs_Bot && !Is_Game_Over() )
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
            Span<SMove> moves = Move_Generator.Generate_Moves(Board);

            return moves.ToArray().Any(m => m.Start_Square == move.Start_Square && m.Target_Square == move.Target_Square && m.Move_Flag == move.Move_Flag);
        }

        public IEnumerable<SMove> Get_Legal_Moves_For_Square(int square)
        {
            var all_moves = Move_Generator.Generate_Moves(Board);

            return all_moves.ToArray().Where(m => m.Start_Square == square);
        }

        public bool Should_Bot_Move()
        {
            if (!Playing_Against_Bot || Is_Game_Over())
            {
                return false;
            }

            if (Current_Settings.Game_Mode == EGame_Mode.Bot_Vs_Bot)
            {// Управление через таймер
                return false; 
            }

            bool white_to_move = Board.Is_White_To_Move;
            return (Human_Is_White && !white_to_move) || (!Human_Is_White && white_to_move);
        }

        public void Apply_Settings(SGame_Settings settings)
        {
            Current_Settings = settings;
            Sound_System.Enabled = settings.Additional_Settings.Enable_Sound;

            if (settings.Game_Mode != EGame_Mode.Human_Vs_Human && Bot_Controller != null)
            {
                Bot_Controller.Set_Difficulty(settings.Bot_Difficulty);
            }

            Difficulty_Controller.Set_Difficulty(settings.Bot_Difficulty);
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
            {
                return -1;
            }

            return Board.Is_White_To_Move ? Board.King_Square[ABoard.White_Index] : Board.King_Square[ABoard.Black_Index];
        }

        public string Get_Current_FEN()
        {
            return Board.Current_FEN;
        }

        public void Set_Sound_System_Enabled(bool enabled)
        {
            Sound_System.Enabled = enabled;
        }

        public void Dispose()
        {
            Bot_Controller?.Dispose();
        }

        private void Make_Bot_Move()
        {
            if (Bot_Controller != null && Should_Bot_Move())
            {
                Task.Run(() => Bot_Controller?.Make_Move());
            }
        }

        private void Handle_Bot_Move(string move_string)
        {
            if (string.IsNullOrEmpty(move_string) || move_string == "null")
            {
                return;
            }

            SMove move = AsMove_Utility.Get_Move_From_UCI_Name(move_string, Board);
            
            Try_Make_Move(move);
        }

        private void Check_Game_State()
        {
            EGame_Result game_state = AsGame_Manager.Get_Game_State(Board);
            
            if (game_state != EGame_Result.In_Progress)
            {
                if (Current_Settings.Game_Mode == EGame_Mode.Bot_Vs_Bot)
                { 
                    Bot_Moves_Gap_Timer?.Dispose();
                    Bot_Moves_Gap_Timer = null;
                }

                On_Game_Ended?.Invoke(game_state);
                On_Board_Enabled_Changed?.Invoke(false);
            }
        }

    }
}
