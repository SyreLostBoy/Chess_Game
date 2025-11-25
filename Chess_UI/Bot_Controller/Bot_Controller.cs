using Chess_Engine;
using Chess_Engine.Core;

namespace Chess_UI.Bot_Controller
{
    public class ABot_Controller : IDisposable
    {
        public const int Bot_Think_Time_Ms = 7000; // 7 Секунд

        private ABot Bot;
        private bool Is_Disposed = false;

        public event Action<string> On_Move_Chosen
        {
            add { if (Bot != null) Bot.On_Move_Chosen += value; }
            remove { if (Bot != null) Bot.On_Move_Chosen -= value; }
        }

        public ABot_Controller(ABoard board)
        {
            Bot = new ABot(board);
        }

        public void Make_Move(int think_time_ms = Bot_Think_Time_Ms)
        {
            if (Is_Disposed)
            {
                return;
            }

            Bot.Think_Timed(think_time_ms);
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
    }
}
