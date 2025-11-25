using Chess_Engine.Core.Evaluation;
using Chess_Engine.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Engine.Core
{
    public class ASearcher
    {
        private const int Transposition_Table_Size_MB = 64;
        private const int Max_Extensions = 16;
        private const int Immediate_Mate_Score = 100000;
        private const int Positive_Infinity = 9999999;
        private const int Negative_Infinity = -Positive_Infinity;

        public event Action<SMove>? On_Search_Complete;

        public int Current_Depth;
        public SMove Best_Move_So_Far => Best_Move;
        public int Best_Eval_So_Far => Best_Eval;

        private bool Is_Playing_White;
        private SMove Best_Move_This_Iteration;
        private int Best_Eval_This_Iteration;
        private SMove Best_Move;
        private int Best_Eval;
        private bool Has_Searched_At_Least_One_Move;
        private bool Search_Cancelled;

        public SSearch_Diagnostics Search_Diagnostics;
        private int Current_Iteration_Depth;
        private System.Diagnostics.Stopwatch Search_Iteration_Timer;
        private System.Diagnostics.Stopwatch Search_Total_Timer;
        public string Debug_Info;

        private readonly ATransposition_Table Transposition_Table;
        private readonly ARepetition_Table Repetition_Table;
        private readonly AMove_Generator Move_Generator;
        private readonly AMove_Ordering Move_Ordering;
        private readonly AEvaluator Evaluation;
        private readonly ABoard Board;

        public ASearcher(ABoard board)
        {
            Board = board;

            Evaluation = new AEvaluator();
            Move_Generator = new AMove_Generator();
            Transposition_Table = new ATransposition_Table(Board, Transposition_Table_Size_MB);
            Move_Ordering = new AMove_Ordering(Move_Generator, Transposition_Table);
            Repetition_Table = new ARepetition_Table();

            Move_Generator.Promotion_Mode = EPromotion_Mode.Queen_And_Knight;

            Search(1, 0, Negative_Infinity, Positive_Infinity);
        }

        public void Start_Search()
        {
            Best_Eval_This_Iteration = Best_Eval = 0;
            Best_Move_This_Iteration = Best_Move = SMove.Null_Move;

            Is_Playing_White = Board.Is_White_To_Move;

            Move_Ordering.Clear_History();
            Repetition_Table.Init(Board);

            Current_Depth = 0;
            Debug_Info = "Starting search with FEN " + AsFen_Utility.Get_Current_Fen(Board);
            Search_Cancelled = false;
            Search_Diagnostics = new SSearch_Diagnostics();
            Search_Iteration_Timer = new System.Diagnostics.Stopwatch();
            Search_Total_Timer = System.Diagnostics.Stopwatch.StartNew();

            Run_Iterative_Deepening_Search();

            if (Best_Move.Is_Null)
            {
                Best_Move = Move_Generator.Generate_Moves(Board)[0];
            }

            On_Search_Complete?.Invoke(Best_Move);
            Search_Cancelled = false;
        }

        private void Run_Iterative_Deepening_Search()
        {
            for (int search_depth = 1; search_depth <= 256; search_depth++)
            {
                Has_Searched_At_Least_One_Move = false;
                Debug_Info += "\nStarting Iteration: " + search_depth;
                Search_Iteration_Timer.Restart();
                Current_Iteration_Depth = search_depth;
                Search(search_depth, 0, Negative_Infinity, Positive_Infinity);

                if (Search_Cancelled)
                {
                    if (Has_Searched_At_Least_One_Move)
                    {
                        Best_Move = Best_Move_This_Iteration;
                        Best_Eval = Best_Eval_This_Iteration;
                        Search_Diagnostics.Move = AsMove_Utility.Get_Move_Name_UCI(Best_Move);
                        Search_Diagnostics.Eval = Best_Eval;
                        Search_Diagnostics.Move_Is_From_Partial_Search = true;
                        Debug_Info += "\nUsing partial search result: " + AsMove_Utility.Get_Move_Name_UCI(Best_Move) + " Eval: " + Best_Eval;
                    }

                    Debug_Info += "\nSearch aborted";
                    break;
                }
                else
                {
                    Current_Depth = search_depth;
                    Best_Move = Best_Move_This_Iteration;
                    Best_Eval = Best_Eval_This_Iteration;

                    Debug_Info += "\nIteration result: " + AsMove_Utility.Get_Move_Name_UCI(Best_Move) + " Eval: " + Best_Eval;
                    
                    if (Is_Mate_Score(Best_Eval))
                    {
                        Debug_Info += " Mate in ply: " + Num_Ply_To_Mate_From_Score(Best_Eval);
                    }

                    Best_Eval_This_Iteration = int.MinValue;
                    Best_Move_This_Iteration = SMove.Null_Move;

                    Search_Diagnostics.Num_Completed_Iterations = search_depth;
                    Search_Diagnostics.Move = AsMove_Utility.Get_Move_Name_UCI(Best_Move);
                    Search_Diagnostics.Eval = Best_Eval;

                    if (Is_Mate_Score(Best_Eval) && Num_Ply_To_Mate_From_Score(Best_Eval) <= search_depth)
                    {
                        Debug_Info += "\nExitting search due to mate found within search depth";
                        break;
                    }
                }
            }
        }

        public (SMove move, int eval) Get_Search_Result()
        {
            return (Best_Move, Best_Eval);
        }

        public void End_Search()
        {
            Search_Cancelled = true;
        }

        private int Search(int ply_remaining, int ply_from_root, int alpha, int beta, int num_extensions = 0, SMove prev_move = default, bool prev_was_capture = false)
        {
            if (Search_Cancelled)
            {
                return 0;
            }

            if (ply_from_root > 0)
            {
                if (Board.Current_Game_State.Fifty_Move_Counter >= 100 || Repetition_Table.Contains(Board.Current_Game_State.Zobrist_Key))
                {
                    return 0;
                }

                alpha = Math.Max(alpha, -Immediate_Mate_Score + ply_from_root);
                beta = Math.Min(beta, Immediate_Mate_Score - ply_from_root);
                
                if (alpha >= beta)
                {
                    return alpha;
                }
            }

            int tt_val = Transposition_Table.Lookup_Evaluation(ply_remaining, ply_from_root, alpha, beta);
            if (tt_val != ATransposition_Table.Lookup_Failed)
            {
                if (ply_from_root == 0)
                {
                    Best_Move_This_Iteration = Transposition_Table.Try_Get_Stored_Move();
                    Best_Eval_This_Iteration = Transposition_Table.Entries[Transposition_Table.Index].Value;
                }
                return tt_val;
            }

            if (ply_remaining == 0)
            {
                int eval = Quiescence_Search(alpha, beta);
                return eval;
            }

            Span<SMove> moves = stackalloc SMove[256];
            Move_Generator.Generate_Moves(Board, ref moves, captures_only: false);
            SMove prev_best_move = ply_from_root == 0 ? Best_Move : Transposition_Table.Try_Get_Stored_Move();
            Move_Ordering.Order_Moves(prev_best_move, Board, moves, Move_Generator.Opponent_Attack_Map, Move_Generator.Opponent_Pawn_Attack_Map, false, ply_from_root);

            if (moves.Length == 0)
            {
                if (Move_Generator.Is_In_Check())
                {
                    int mate_score = Immediate_Mate_Score - ply_from_root;
                    return -mate_score;
                }
                else
                {
                    return 0;
                }
            }

            if (ply_from_root > 0)
            {
                bool was_pawn_move = APiece.Get_Piece_Type(Board.Square[prev_move.Target_Square]) == EPiece_Type.Pawn;
                Repetition_Table.Push(Board.Current_Game_State.Zobrist_Key, prev_was_capture || was_pawn_move);
            }

            int evaluation_bound = (int)ENode_Type.Upper_Bound;
            SMove best_move_in_this_position = SMove.Null_Move;

            for (int i = 0; i < moves.Length; i++)
            {
                SMove move = moves[i];
                EPiece_Type captured_piece_type = APiece.Get_Piece_Type(Board.Square[move.Target_Square]);
                bool is_capture = captured_piece_type != EPiece_Type.None;
                Board.Make_Move(moves[i], is_search: true);

                int extension = 0;
                if (num_extensions < Max_Extensions)
                {
                    EPiece_Type moved_piece_type = APiece.Get_Piece_Type(Board.Square[move.Target_Square]);
                    int target_rank = AsBoard_Helper.Rank_Index(move.Target_Square);
                    
                    if (Board.Is_In_Check())
                    {
                        extension = 1;
                    }
                    else if (moved_piece_type == EPiece_Type.Pawn && (target_rank == 1 || target_rank == 6))
                    {
                        extension = 1;
                    }
                }

                bool needs_full_search = true;
                int eval = 0;
                if (extension == 0 && ply_remaining >= 3 && i >= 3 && !is_capture)
                {
                    const int reduce_depth = 1;
                    eval = -Search(ply_remaining - 1 - reduce_depth, ply_from_root + 1, -alpha - 1, -alpha, num_extensions, move, is_capture);
                    needs_full_search = eval > alpha;
                }

                if (needs_full_search)
                {
                    eval = -Search(ply_remaining - 1 + extension, ply_from_root + 1, -beta, -alpha, num_extensions + extension, move, is_capture);
                }
                Board.Unmake_Move(moves[i], is_search: true);

                if (Search_Cancelled)
                {
                    return 0;
                }

                if (eval >= beta)
                {
                    Transposition_Table.Store_Evaluation(ply_remaining, ply_from_root, beta, ENode_Type.Lower_Bound, moves[i]);

                    if (!is_capture)
                    {
                        if (ply_from_root < AMove_Ordering.Max_Killer_Move_Ply)
                        {
                            Move_Ordering.Killer_Moves[ply_from_root].Add(move);
                        }
                        int history_score = ply_remaining * ply_remaining;
                        Move_Ordering.History[Board.Move_Color_Index, moves[i].Start_Square, moves[i].Target_Square] += history_score;
                    }
                    if (ply_from_root > 0)
                    {
                        Repetition_Table.Try_Pop();
                    }

                    Search_Diagnostics.Num_Cut_Offs++;
                    return beta;
                }

                if (eval > alpha)
                {
                    evaluation_bound = (int)ENode_Type.Exact;
                    best_move_in_this_position = moves[i];

                    alpha = eval;
                    if (ply_from_root == 0)
                    {
                        Best_Move_This_Iteration = moves[i];
                        Best_Eval_This_Iteration = eval;
                        Has_Searched_At_Least_One_Move = true;
                    }
                }
            }

            if (ply_from_root > 0)
            {
                Repetition_Table.Try_Pop();
            }

            Transposition_Table.Store_Evaluation(ply_remaining, ply_from_root, alpha, (ENode_Type)evaluation_bound, best_move_in_this_position);

            return alpha;
        }

        private int Quiescence_Search(int alpha, int beta)
        {
            if (Search_Cancelled)
            {
                return 0;
            }

            int eval = Evaluation.Evaluate(Board);
            Search_Diagnostics.Num_Positions_Evaluated++;
            if (eval >= beta)
            {
                Search_Diagnostics.Num_Cut_Offs++;
                return beta;
            }
            if (eval > alpha)
            {
                alpha = eval;
            }

            Span<SMove> moves = stackalloc SMove[128];
            Move_Generator.Generate_Moves(Board, ref moves, captures_only: true);
            Move_Ordering.Order_Moves(SMove.Null_Move, Board, moves, Move_Generator.Opponent_Attack_Map, Move_Generator.Opponent_Pawn_Attack_Map, true, 0);
            
            for (int i = 0; i < moves.Length; i++)
            {
                Board.Make_Move(moves[i], true);
                eval = -Quiescence_Search(-beta, -alpha);
                Board.Unmake_Move(moves[i], true);

                if (eval >= beta)
                {
                    Search_Diagnostics.Num_Cut_Offs++;
                    return beta;
                }
                if (eval > alpha)
                {
                    alpha = eval;
                }
            }

            return alpha;
        }

        public static bool Is_Mate_Score(int score)
        {
            if (score == int.MinValue)
            {
                return false;
            }
            const int max_mate_depth = 1000;
            return Math.Abs(score) > Immediate_Mate_Score - max_mate_depth;
        }

        public static int Num_Ply_To_Mate_From_Score(int score)
        {
            return Immediate_Mate_Score - Math.Abs(score);
        }

        public string Announce_Mate()
        {
            if (Is_Mate_Score(Best_Eval_This_Iteration))
            {
                int num_ply_to_mate = Num_Ply_To_Mate_From_Score(Best_Eval_This_Iteration);
                int num_moves_to_mate = (int)Math.Ceiling(num_ply_to_mate / 2f);

                string side_with_mate = (Best_Eval_This_Iteration * ((Board.Is_White_To_Move) ? 1 : -1) < 0) ? "Black" : "White";

                return $"{side_with_mate} can mate in {num_moves_to_mate} move{((num_moves_to_mate > 1) ? "s" : "")}";
            }
            return "No mate found";
        }

        public void Clear_For_New_Position()
        {
            Transposition_Table.Clear();
            Move_Ordering.Clear_Killers();
        }

        public ATransposition_Table Get_Transposition_Table() => Transposition_Table;
    }
}
