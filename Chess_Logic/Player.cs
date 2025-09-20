using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//------------------------------------------------------------------------------------------------------------
namespace Chess_Logic
{
    public enum EColor
    {
        None,

        White,
        Black
    }

    public static class APlayer_Extensions
    {
        public static EColor Determ_Turn(this EColor current_player_color)
        {
            switch (current_player_color)
            {
                case EColor.White:
                    return EColor.Black;
                case EColor.Black:
                    return EColor.White;
                default:
                    return EColor.None;
            }
        }
    }
}
//------------------------------------------------------------------------------------------------------------
