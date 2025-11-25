using Chess_Logic;

namespace Chess_AI
{
    public class APooled_Game_State
    {
        public AsGame_State State { get; }

        private static readonly Queue<AsGame_State> pool = new Queue<AsGame_State>();
    
        public APooled_Game_State(AsGame_State original, AMove move)
        {
            if (pool.Count > 0)
            {
                State = pool.Dequeue();
                State.Board = original.Board.Copy();
                State.Act_Move(move);
            }
            else
            {
                State = Create_Test_State(original, move);
            }
        }

        public void Dispose()
        {
            if (pool.Count < 10)
            {
                pool.Enqueue(State);
            }
        }

        private static AsGame_State Create_Test_State(AsGame_State original_state, AMove move)
        {
            ABoard board_copy = original_state.Board.Copy();
            AsGame_State test_state = new AsGame_State(original_state.Current_Player_Color, board_copy);

            test_state.Act_Move(move);

            return test_state;
        }
    }
}
