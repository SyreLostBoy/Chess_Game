using Chess_Engine;
using Chess_Engine.Bot;
using Chess_Engine.Core;
using System;

namespace Chess_UI.Bot_Controller
{
    public class ABot_Controller : IDisposable
    {
        public const int Bot_Think_Time_Ms = 7000; // 7 Секунд

        private ABot Bot;
        private ADifficulty_Controller Difficulty_Controller;
        private bool Is_Disposed = false;

        public event Action<string> On_Move_Chosen
        {
            add { if (Bot != null) Bot.On_Move_Chosen += value; }
            remove { if (Bot != null) Bot.On_Move_Chosen -= value; }
        }

        public ABot_Controller(ABoard board)
        {
            Difficulty_Controller = new ADifficulty_Controller();
            Bot = new ABot(board);
        }

        public ABot_Controller(ABoard board, ADifficulty_Controller difficulty_controller)
        {
            Difficulty_Controller = difficulty_controller;
            Bot = new ABot(board);
        }

        public void Set_Difficulty(EDifficulty difficulty)
        {
            Difficulty_Controller.Set_Difficulty(difficulty);
            Bot.Set_Difficulty(difficulty);
        }

        public ADifficulty_Settings Get_Difficulty_Settings()
        {
            return Difficulty_Controller.Current_Settings;
        }

        public void Update_Difficulty_Settings(ADifficulty_Settings settings)
        {
            Difficulty_Controller.Update_Settings(settings);
            Bot.Set_Difficulty(EDifficulty.Master); // Custom Difficulty
        }

        public void Make_Move(int think_time_ms = Bot_Think_Time_Ms)
        {
            if (Is_Disposed)
            {
                return;
            }

            int adjusted_time = Adjust_Think_Time_For_Difficulty(think_time_ms);

            Bot.Think_Timed(adjusted_time);
        }

        public void Make_Move_Adaptive(int time_remaining_white_ms, int time_remaining_black_ms, int increment_white_ms, int increment_black_ms)
        {
            if (Is_Disposed)
            {
                return;
            }

            if (!Difficulty_Controller.Current_Settings.Use_Time_Management)
            {
                Make_Move(Difficulty_Controller.Current_Settings.Max_Search_Time_Ms);
                return;
            }

            int think_time = Bot.Choose_Think_Time(time_remaining_white_ms, time_remaining_black_ms, increment_white_ms, increment_black_ms);

            // Дополнительная коррекция в зависимости от сложности
            think_time = Adjust_Think_Time_For_Difficulty(think_time);

            Bot.Think_Timed(think_time);
        }

        public void Stop_Thinking()
        {
            if (Is_Disposed)
            {
                return;
            }

            Bot.Stop_Thinking();
        }

        public void Dispose()
        {
            if (!Is_Disposed)
            {
                Bot?.Quit();
                Bot = null;
                Is_Disposed = true;
            }
        }

        public EDifficulty Current_Difficutly => Difficulty_Controller.Current_Difficulty;
        public ADifficulty_Settings Difficulty_Settings => Difficulty_Controller.Current_Settings;
    
        private int Adjust_Think_Time_For_Difficulty(int base_time)
        {
            ADifficulty_Settings settings = Difficulty_Controller.Current_Settings;

            // Для низких уровней думаем быстрее
            if (Difficulty_Controller.Current_Difficulty <= EDifficulty.Intermediate)
            {
                return Math.Min(base_time, settings.Max_Search_Time_Ms);
            }

            // Для высоких уровней можем увеличить время на размышление
            if (Difficulty_Controller.Current_Difficulty >= EDifficulty.Expert)
            {
                return Math.Min(base_time * 120 / 100, settings.Max_Search_Time_Ms); // +20% времени
            }

            return Math.Min(base_time, settings.Max_Search_Time_Ms);
        }
    }
}
