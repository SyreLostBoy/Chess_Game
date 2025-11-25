using Chess_Engine.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Engine.Core.Evaluation
{
    public class AEvaluator
    {
        public const int Pawn_Value = 100;
        public const int Knight_Value = 300;
        public const int Bishop_Value = 320;
        public const int Rook_Value = 500;
        public const int Queen_Value = 900;

        private static readonly int[] Passed_Pawn_Bonuses = { 0, 120, 80, 50, 30, 15, 15 };
        private static readonly int[] Isolated_Pawn_Penalty_By_Count = { 0, -10, -25, -50, -75, -75, -75, -75, -75 };
        private static readonly int[] King_Pawn_Shield_Scores = { 4, 7, 4, 3, 6, 3 };

        private const float Endgame_Material_Start = Rook_Value * 2 + Bishop_Value + Knight_Value;
        private ABoard Board;

        public Evaluation_Data White_Eval;
        public Evaluation_Data Black_Eval;

        public int Evaluate(ABoard board)
        {
            Board = board;
            White_Eval = new Evaluation_Data();
            Black_Eval = new Evaluation_Data();

            SMaterial_Info white_material = Get_Material_Info(ABoard.White_Index);
            SMaterial_Info black_material = Get_Material_Info(ABoard.Black_Index);

            White_Eval.Material_Score = Count_Material(ABoard.White_Index);
            Black_Eval.Material_Score = Count_Material(ABoard.Black_Index);

            White_Eval.Piece_Square_Score = Evaluate_Piece_Square_Tables(ABoard.White_Index, black_material.Endgame_Transition);
            Black_Eval.Piece_Square_Score = Evaluate_Piece_Square_Tables(ABoard.Black_Index, white_material.Endgame_Transition);

            White_Eval.Mop_Up_Score = Mop_Up_Evaluation(true, white_material, black_material);
            Black_Eval.Mop_Up_Score = Mop_Up_Evaluation(false, black_material, white_material);

            White_Eval.Pawn_Score = Evaluate_Pawns(ABoard.White_Index);
            Black_Eval.Pawn_Score = Evaluate_Pawns(ABoard.Black_Index);

            White_Eval.Pawn_Shield_Score = Evaluate_King_Pawn_Shield(ABoard.White_Index, black_material, Black_Eval.Piece_Square_Score);
            Black_Eval.Pawn_Shield_Score = Evaluate_King_Pawn_Shield(ABoard.Black_Index, white_material, White_Eval.Piece_Square_Score);

            int white_total = White_Eval.Sum();
            int black_total = Black_Eval.Sum();
            int total_eval = white_total - black_total;

            return Board.Is_White_To_Move ? total_eval : -total_eval;
        }

        private int Evaluate_King_Pawn_Shield(int color_index, SMaterial_Info enemy_material, float enemy_piece_square_score)
        {
            if (enemy_material.Endgame_Transition >= 1)
            {
                return 0;
            }

            bool is_white = color_index == ABoard.White_Index;
            int friendly_pawn = APiece.Make_Piece(EPiece_Type.Pawn, is_white);
            int king_square = Board.King_Square[color_index];
            int king_file = AsBoard_Helper.File_Index(king_square);
            int penalty = 0;
            int uncastled_king_penalty = 0;

            if (king_file <= 2 || king_file >= 5)
            {
                int[] squares = is_white ?
                    AsPrecomputed_Evaluation_Data.Pawn_Shield_Squares_White[king_square] :
                    AsPrecomputed_Evaluation_Data.Pawn_Shield_Squares_Black[king_square];

                for (int i = 0; i < squares.Length / 2; i++)
                {
                    int shield_square_index = squares[i];
                    if (Board.Square[shield_square_index] != friendly_pawn)
                    {
                        if (squares.Length > 3 && Board.Square[squares[i + 3]] == friendly_pawn)
                        {
                            penalty += King_Pawn_Shield_Scores[i + 3];
                        }
                        else
                        {
                            penalty += King_Pawn_Shield_Scores[i];
                        }
                    }
                }
            }
            else
            {
                float enemy_development_score = System.Math.Clamp((enemy_piece_square_score + 10) / 130f, 0, 1);
                uncastled_king_penalty = (int)(50 * enemy_development_score);
            }

            int open_file_against_king_penalty = 0;
            if (enemy_material.Num_Rooks > 1 || (enemy_material.Num_Rooks > 0 && enemy_material.Num_Queens > 0))
            {
                int clamped_king_file = System.Math.Clamp(king_file, 1, 6);
                ulong my_pawns = enemy_material.Enemy_Pawns;

                for (int attack_file = clamped_king_file; attack_file <= clamped_king_file + 1; attack_file++)
                {
                    ulong file_mask = AsBit_Masks.File_Mask[attack_file];
                    bool is_king_file = attack_file == king_file;

                    if ((enemy_material.Pawns & file_mask) == 0)
                    {
                        open_file_against_king_penalty += is_king_file ? 25 : 15;

                        if ((my_pawns & file_mask) == 0)
                        {
                            open_file_against_king_penalty += is_king_file ? 15 : 10;
                        }
                    }
                }
            }

            float pawn_shield_weight = 1 - enemy_material.Endgame_Transition;
            if (Board.Queens[1 - color_index].Count == 0)
            {
                pawn_shield_weight *= 0.6f;
            }

            return (int)((-penalty - uncastled_king_penalty - open_file_against_king_penalty) * pawn_shield_weight);
        }

        private int Evaluate_Pawns(int color_index)
        {
            APiece_List pawns = Board.Pawns[color_index];
            bool is_white = color_index == ABoard.White_Index;
            ulong opponent_pawns = Board.Piece_Bitboards[APiece.Make_Piece(EPiece_Type.Pawn, !is_white)];
            ulong friendly_pawns = Board.Piece_Bitboards[APiece.Make_Piece(EPiece_Type.Pawn, is_white)];
            ulong[] masks = is_white ? AsBit_Masks.White_Passed_Pawn_Mask : AsBit_Masks.Black_Passed_Pawn_Mask;
            int bonus = 0;
            int num_isolated_pawns = 0;

            for (int i = 0; i < pawns.Count; i++)
            {
                int square = pawns[i];
                ulong passed_mask = masks[square];

                if ((opponent_pawns & passed_mask) == 0)
                {
                    int rank = AsBoard_Helper.Rank_Index(square);
                    int num_squares_from_promotion = is_white ? 7 - rank : rank;
                    bonus += Passed_Pawn_Bonuses[num_squares_from_promotion];
                }

                if ((friendly_pawns & AsBit_Masks.Adjacent_File_Masks[AsBoard_Helper.File_Index(square)]) == 0)
                {
                    num_isolated_pawns++;
                }
            }

            return bonus + Isolated_Pawn_Penalty_By_Count[num_isolated_pawns];
        }

        private int Mop_Up_Evaluation(bool is_white, SMaterial_Info my_material, SMaterial_Info enemy_material)
        {
            if (my_material.Material_Score > enemy_material.Material_Score + Pawn_Value * 2 && enemy_material.Endgame_Transition > 0)
            {
                int mop_up_score = 0;
                int friendly_index = is_white ? ABoard.White_Index : ABoard.Black_Index;
                int opponent_index = is_white ? ABoard.Black_Index : ABoard.White_Index;

                int friendly_king_square = Board.King_Square[friendly_index];
                int opponent_king_square = Board.King_Square[opponent_index];

                mop_up_score += (14 - AsPrecomputed_Move_Data.Orthogonal_Distance[friendly_king_square, opponent_king_square]) * 4;
                mop_up_score += AsPrecomputed_Move_Data.Centre_Manhattan_Distance[opponent_king_square] * 10;

                return (int)(mop_up_score * enemy_material.Endgame_Transition);
            }

            return 0;
        }

        private int Count_Material(int color_index)
        {
            int material = 0;
            material += Board.Pawns[color_index].Count * Pawn_Value;
            material += Board.Knights[color_index].Count * Knight_Value;
            material += Board.Bishops[color_index].Count * Bishop_Value;
            material += Board.Rooks[color_index].Count * Rook_Value;
            material += Board.Queens[color_index].Count * Queen_Value;

            return material;
        }

        private int Evaluate_Piece_Square_Tables(int color_index, float endgame_transition)
        {
            int value = 0;
            bool is_white = color_index == ABoard.White_Index;

            value += Evaluate_Piece_Square_Table(AsPiece_Square_Table.Rooks, Board.Rooks[color_index], is_white);
            value += Evaluate_Piece_Square_Table(AsPiece_Square_Table.Knights, Board.Knights[color_index], is_white);
            value += Evaluate_Piece_Square_Table(AsPiece_Square_Table.Bishops, Board.Bishops[color_index], is_white);
            value += Evaluate_Piece_Square_Table(AsPiece_Square_Table.Queens, Board.Queens[color_index], is_white);

            int pawn_early = Evaluate_Piece_Square_Table(AsPiece_Square_Table.Pawns, Board.Pawns[color_index], is_white);
            int pawn_late = Evaluate_Piece_Square_Table(AsPiece_Square_Table.Pawns_End, Board.Pawns[color_index], is_white);
            value += (int)(pawn_early * (1 - endgame_transition));
            value += (int)(pawn_late * endgame_transition);

            int king_early_phase = AsPiece_Square_Table.Read(AsPiece_Square_Table.King_Start, Board.King_Square[color_index], is_white);
            int king_late_phase = AsPiece_Square_Table.Read(AsPiece_Square_Table.King_End, Board.King_Square[color_index], is_white);
            value += (int)(king_early_phase * (1 - endgame_transition));
            value += (int)(king_late_phase * endgame_transition);

            return value;
        }

        private static int Evaluate_Piece_Square_Table(int[] table, APiece_List piece_list, bool is_white)
        {
            int value = 0;
            for (int i = 0; i < piece_list.Count; i++)
            {
                value += AsPiece_Square_Table.Read(table, piece_list[i], is_white);
            }
            return value;
        }

        private SMaterial_Info Get_Material_Info(int color_index)
        {
            int num_pawns = Board.Pawns[color_index].Count;
            int num_knights = Board.Knights[color_index].Count;
            int num_bishops = Board.Bishops[color_index].Count;
            int num_rooks = Board.Rooks[color_index].Count;
            int num_queens = Board.Queens[color_index].Count;

            bool is_white = color_index == ABoard.White_Index;
            ulong my_pawns = Board.Piece_Bitboards[APiece.Make_Piece(EPiece_Type.Pawn, is_white)];
            ulong enemy_pawns = Board.Piece_Bitboards[APiece.Make_Piece(EPiece_Type.Pawn, !is_white)];

            return new SMaterial_Info(num_pawns, num_knights, num_bishops, num_queens, num_rooks, my_pawns, enemy_pawns);
        }

        public struct Evaluation_Data
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
}
