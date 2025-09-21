using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Logic
{
    public class AsGame_State
    {
        public AsGame_State(EColor current_player_colro, AsBoard board)
        {
            Current_Player_Color = current_player_colro;
            Board = board;
        }

        public AsBoard Board { get; }
        public EColor Current_Player_Color { get; private set; }
    }
}
