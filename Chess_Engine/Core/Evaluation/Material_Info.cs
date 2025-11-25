using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Engine.Core.Evaluation
{
    public struct SMaterial_Info
    {
        public readonly int Material_Score;
        public readonly int Num_Pawns;
        public readonly int Num_Knights;
        public readonly int Num_Bishops;
        public readonly int Num_Queens;
        public readonly int Num_Rooks;
        public readonly int Num_Majors;
        public readonly int Num_Minors;

        public readonly ulong Pawns;
        public readonly ulong Enemy_Pawns;

        public readonly float Endgame_Transition;

        public SMaterial_Info(int num_pawns, int num_knights, int num_bishops, int num_rooks, int num_queens, ulong my_pawns, ulong enemy_pawns)
        {
            Num_Pawns = num_pawns;
            Num_Knights = num_knights;
            Num_Bishops = num_bishops;
            Num_Rooks = num_rooks;
            Num_Queens = num_queens;
            Pawns = my_pawns;
            Enemy_Pawns = enemy_pawns;

            Num_Majors = Num_Rooks + Num_Queens;
            Num_Minors = Num_Bishops + Num_Knights;

            Material_Score = 0;
            Material_Score += Num_Pawns * AEvaluator.Pawn_Value;
            Material_Score += Num_Knights * AEvaluator.Knight_Value;
            Material_Score += Num_Bishops * AEvaluator.Bishop_Value;
            Material_Score += Num_Rooks * AEvaluator.Rook_Value;
            Material_Score += Num_Queens * AEvaluator.Queen_Value;

            // Endgame Transition (0.0 -> 1.0)
            const int queen_endgame_weight = 45;
            const int rook_endgame_weight = 20;
            const int bishop_endgame_weight = 10;
            const int knight_endgame_weight = 10;

            const int endgame_start_weight = 2 * rook_endgame_weight + 2 * bishop_endgame_weight + 2 * knight_endgame_weight + queen_endgame_weight;
            int endgame_weight_sum = Num_Queens * queen_endgame_weight + Num_Rooks * rook_endgame_weight + Num_Bishops * bishop_endgame_weight + Num_Knights * knight_endgame_weight;
            
            Endgame_Transition = 1.0f - Math.Min(1.0f, endgame_weight_sum / (float)endgame_start_weight);
        }
    }
}
