using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Logic
{
    public static class AsConfig
    {
        //Звуки
        public static readonly string Audio_Base_Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Audio");
        public static readonly Dictionary<EMove_Type, string> Move_Sounds = new Dictionary<EMove_Type, string>
        {
            {EMove_Type.Normal, "Move.wav" },
            {EMove_Type.Double_Pawn_Move, "Move.wav" },
            {EMove_Type.En_Passant, "Capture.wav" },
            {EMove_Type.Castle_KS, "Move.wav" },
            {EMove_Type.Castle_QS, "Move.wav" },
            {EMove_Type.Pawn_Promotion, "Move.wav" }
        };

        // Стандартные FEN позиции
        public static readonly string Start_Position = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        public static readonly string Empty_Board = "8/8/8/8/8/8/8/8 w - - 0 1";
        public static readonly string King_And_Pawn_Endgame = "8/8/8/8/8/k7/P7/K7 w - - 0 1";
        public static readonly string Checkmate_Position = "r1bqkbnr/pppp1ppp/2n5/4p3/2B1P3/5Q2/PPPP1PPP/RNB1K1NR b KQkq - 0 1";
        public static readonly string Castling_Position = "r3k2r/pppppppp/8/8/8/8/PPPPPPPP/R3K2R w KQkq - 0 1";
    }
}
