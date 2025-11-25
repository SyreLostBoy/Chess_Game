using Chess_AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Logic.Player
{
    public class AAI_Player : APlayer_Base
    {
        protected readonly AChess_Bot Chess_Bot;
        protected AsGame_State Current_Game_State;
        protected Task Thinking_Task;

        public int Search_Depth { get; set; } = 3;
        public int Thinking_Delay { get; set; } = 1000;

        protected AAI_Player(EColor color)
            : base(color)
        {
            Chess_Bot = new AChess_Bot();
        }

        public override bool Is_Human => false;

        public override void Start_Turn(AsGame_State game_state)
        {
            if (Is_Thinking)
            {
                return;
            }

            Current_Game_State = game_state;
            Is_Thinking = true;

            Thinking_Task = Task.Run(async () =>
            {
                AMove best_move;

                if (Thinking_Delay > 0)
                {
                    await Task.Delay(Thinking_Delay);
                }

                best_move = Chess_Bot.Find_Best_Move(game_state, Color);

                if (best_move != null && Is_Thinking)
                {
                    Move_Selected(best_move);
                }

                Is_Thinking = false;
            });
        }

        public override void Cancel_Turn()
        {
            Is_Thinking = false;
            Thinking_Task = null;
        }

    }
}
