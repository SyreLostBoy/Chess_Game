using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Engine.Core.Evaluation
{
    public struct SEvaluation_Data
    {
        public int Material_Score;
        public int Mop_Up_Score;
        public int Piece_Square_Score;
        public int Pawn_Score;
        public int Pawn_Shield_Score;

        public int Sum()
        {
            return Material_Score + Mop_Up_Score + Piece_Square_Score + Pawn_Score + Pawn_Shield_Score;
        }
    }
}
