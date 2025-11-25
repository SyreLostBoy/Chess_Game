using Chess_Engine.Core.Evaluation;
using Chess_Engine.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Chess_Engine.Core
{
    public class AMove_Ordering
    {
        private int[] Move_Scores;
        private const int Max_Move_Count = 218;

        private const int Square_Controlled_By_Opponent_Pawn_Penalty = 350;
        private const int Captured_Piece_Value_Multiplier = 100;

        private ATransposition_Table Transposition_Table;
        private SMove Invalid_Move;

        public SKillers[] Killer_Moves;
        public int[,,] History;
        public const int Max_Killer_Move_Ply = 32;

        private const int Million = 1000000;
        private const int Hash_Move_Score = 100 * Million;
        private const int Winning_Capture_Bias = 8 * Million;
        private const int Promote_Bias = 6 * Million;
        private const int Killer_Bias = 4 * Million;
        private const int Losing_Capture_Bias = 2 * Million;
        private const int Regular_Bias = 0;

        public AMove_Ordering(AMove_Generator move_generator, ATransposition_Table transposition_table)
        {
            Move_Scores = new int[Max_Move_Count];
            Transposition_Table = transposition_table;
            Invalid_Move = SMove.Null_Move;
            Killer_Moves = new SKillers[Max_Killer_Move_Ply];
            History = new int[2, 64, 64];
        }

        public void Clear_History()
        {
            History = new int[2, 64, 64];
        }

        public void Clear_Killers()
        {
            Killer_Moves = new SKillers[Max_Killer_Move_Ply];
        }

        public void Clear()
        {
            Clear_Killers();
            Clear_History();
        }

        public void Order_Moves(SMove hash_move, ABoard board, System.Span<SMove> moves, ulong opponent_attacks, ulong opponent_pawn_attacks, bool in_quiescence_search, int ply)
        {
            ulong opponent_pieces = board.Enemy_Diagonal_Sliders | board.Enemy_Orthogonal_Sliders | board.Piece_Bitboards[APiece.Make_Piece(EPiece_Type.Knight, board.Opponent_Color)];
            ulong[] pawn_attacks = board.Is_White_To_Move ? AsBitboard_Utility.White_Pawn_Attacks : AsBitboard_Utility.Black_Pawn_Attacks;

            for (int i = 0; i < moves.Length; i++)
            {
                SMove move = moves[i];

                if (SMove.Is_Same_Move(move, hash_move))
                {
                    Move_Scores[i] = Hash_Move_Score;
                    continue;
                }

                int score = 0;
                int start_square = move.Start_Square;
                int target_square = move.Target_Square;

                int move_piece = board.Square[start_square];
                EPiece_Type move_piece_type = APiece.Get_Piece_Type(move_piece);
                EPiece_Type capture_piece_type = APiece.Get_Piece_Type(board.Square[target_square]);
                bool is_capture = capture_piece_type != EPiece_Type.None;
                int flag = moves[i].Move_Flag;
                int piece_value = Get_Piece_Value(move_piece_type);

                if (is_capture)
                {
                    int capture_material_delta = Get_Piece_Value(capture_piece_type) - piece_value;
                    bool opponent_can_recapture = AsBitboard_Utility.Contains_Square(opponent_pawn_attacks | opponent_attacks, target_square);

                    if (opponent_can_recapture)
                    {
                        score += (capture_material_delta >= 0 ? Winning_Capture_Bias : Losing_Capture_Bias) + capture_material_delta;
                    }
                    else
                    {
                        score += Winning_Capture_Bias + capture_material_delta;
                    }
                }

                if (move_piece_type == EPiece_Type.Pawn)
                {
                    if (flag == SMove.Promote_To_Queen_Flag && !is_capture)
                    {
                        score += Promote_Bias;
                    }
                }
                else if (move_piece_type == EPiece_Type.King)
                {
                    // King moves handled separately
                }
                else
                {
                    int to_score = AsPiece_Square_Table.Read(move_piece, target_square);
                    int from_score = AsPiece_Square_Table.Read(move_piece, start_square);
                    score += to_score - from_score;

                    if (AsBitboard_Utility.Contains_Square(opponent_pawn_attacks, target_square))
                    {
                        score -= 50;
                    }
                    else if (AsBitboard_Utility.Contains_Square(opponent_attacks, target_square))
                    {
                        score -= 25;
                    }
                }

                if (!is_capture)
                {
                    bool is_killer = !in_quiescence_search && ply < Max_Killer_Move_Ply && Killer_Moves[ply].Match(move);
                    score += is_killer ? Killer_Bias : Regular_Bias;
                    score += History[board.Move_Color_Index, move.Start_Square, move.Target_Square];
                }

                Move_Scores[i] = score;
            }

            Quick_Sort(moves, Move_Scores, 0, moves.Length - 1);
        }

        private static int Get_Piece_Value(EPiece_Type piece_type)
        {
            switch (piece_type)
            {
                case EPiece_Type.Queen:
                    return AEvaluator.Queen_Value;
                case EPiece_Type.Rook:
                    return AEvaluator.Rook_Value;
                case EPiece_Type.Knight:
                    return AEvaluator.Knight_Value;
                case EPiece_Type.Bishop:
                    return AEvaluator.Bishop_Value;
                case EPiece_Type.Pawn:
                    return AEvaluator.Pawn_Value;
                default:
                    return 0;
            }
        }

        public string Get_Score(int index)
        {
            int score = Move_Scores[index];

            int[] score_types = { Hash_Move_Score, Winning_Capture_Bias, Losing_Capture_Bias, Promote_Bias, Killer_Bias, Regular_Bias };
            string[] type_names = { "Hash Move", "Good Capture", "Bad Capture", "Promote", "Killer Move", "Regular" };
            string type_name = "";
            int closest = int.MaxValue;

            for (int i = 0; i < score_types.Length; i++)
            {
                int delta = System.Math.Abs(score - score_types[i]);
                if (delta < closest)
                {
                    closest = delta;
                    type_name = type_names[i];
                }
            }

            return $"{score} ({type_name})";
        }

        public static void Sort(System.Span<SMove> moves, int[] scores)
        {
            for (int i = 0; i < moves.Length - 1; i++)
            {
                for (int j = i + 1; j > 0; j--)
                {
                    int swap_index = j - 1;
                    if (scores[swap_index] < scores[j])
                    {
                        (moves[j], moves[swap_index]) = (moves[swap_index], moves[j]);
                        (scores[j], scores[swap_index]) = (scores[swap_index], scores[j]);
                    }
                }
            }
        }

        public static void Quick_Sort(System.Span<SMove> values, int[] scores, int low, int high)
        {
            if (low < high)
            {
                int pivot_index = Partition(values, scores, low, high);
                Quick_Sort(values, scores, low, pivot_index - 1);
                Quick_Sort(values, scores, pivot_index + 1, high);
            }
        }

        private static int Partition(System.Span<SMove> values, int[] scores, int low, int high)
        {
            int pivot_score = scores[high];
            int i = low - 1;

            for (int j = low; j <= high - 1; j++)
            {
                if (scores[j] > pivot_score)
                {
                    i++;
                    (values[i], values[j]) = (values[j], values[i]);
                    (scores[i], scores[j]) = (scores[j], scores[i]);
                }
            }

            (values[i + 1], values[high]) = (values[high], values[i + 1]);
            (scores[i + 1], scores[high]) = (scores[high], scores[i + 1]);

            return i + 1;
        }
    }
}
