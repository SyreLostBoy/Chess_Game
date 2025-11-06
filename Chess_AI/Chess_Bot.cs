using Chess_Logic;
using System.ComponentModel;
using System.Security;

namespace Chess_AI
{
    public class AChess_Bot
    {
        private const int Max_Depth = 3;
        private int Nodes_Evaluated;

        public AMove Find_Best_Move(AsGame_State game_state, EColor bot_color)
        {
            AMove best_move = null;
            int best_score = int.MinValue;
            int score = 0;
            Nodes_Evaluated = 0;
            
            var possible_moves = game_state.Get_All_Legal_Moves_For(bot_color).ToList();

            if (possible_moves.Count == 0)
                return best_move;

            // Сортировка ходов для улучшения отсечения
            var sorted_moves = Order_Moves(possible_moves, game_state.Board, bot_color);

            foreach (var move in sorted_moves)
            {
                AsGame_State test_state = Create_Test_State(game_state, move);

                score = -Alpha_Beta(test_state, Max_Depth - 1, int.MinValue + 1, int.MaxValue - 1, bot_color.Opponent());

                if (score > best_score)
                {
                    best_score = score;
                    best_move = move;
                }

            }

            return best_move ?? sorted_moves[0];
        }

        private AsGame_State Create_Test_State(AsGame_State original_state, AMove move)
        {
            ABoard board_copy = original_state.Board.Copy();

            AsGame_State test_state = new AsGame_State(original_state.Current_Player_Color, board_copy);

            test_state.Act_Move(move);
            
            return test_state;
        }

        private int Alpha_Beta(AsGame_State game_state, int depth, int alpha, int beta, EColor current_color)
        {
            AsGame_State test_state;
            int score;

            Nodes_Evaluated++;

            if (depth == 0 || game_state.Is_Game_Over() )
            {
                return Evaluate_Position(game_state, current_color);
            }

            var moves = game_state.Get_All_Legal_Moves_For(current_color).ToList();

            if (moves.Count == 0)
            {
                return Evaluate_Terminal_Position(game_state, current_color);
            }

            List<AMove> ordered_moves = Order_Moves(moves, game_state.Board, current_color);

            foreach (AMove move in ordered_moves)
            {
                test_state = Create_Test_State(game_state, move);

                score = -Alpha_Beta(test_state, depth - 1, -beta, -alpha, current_color.Opponent() );

                if (score >= beta)
                {
                    return beta;
                }

                if (score > alpha)
                {
                    alpha = score;
                }

            }

            return alpha;
        }

        private int Evaluate_Position(AsGame_State game_state, EColor color)
        {
            int score = 0;

            // Материальный баланс
            score += Evaluate_Material(game_state.Board, color);

            // Позиционная оценка
            score += Evaluate_Piece_Positions(game_state.Board, color);

            // Мобильность
            score += Evaluate_Mobility(game_state, color);

            // Безопасность короля
            score += Evaluate_King_Safety(game_state.Board, color);

            // Шах
            if (game_state.Board.Is_In_Check(color.Opponent() ) )
            {
                score += 50;
            }

            return score;
        }

        private int Evaluate_Material(ABoard board, EColor color)
        {
            int score = 0;
            int value;
            int opponent_score = 0;
            APiece piece;

            foreach (APosition pos in board.Get_Piece_Positions() )
            {
                piece = board[pos];
                value = Get_Piece_Value(piece.Type);

                if (piece.Color == color)
                {
                    score += value;
                }
                else
                {
                    opponent_score += value;
                }
            }

            return score - opponent_score;
        }

        private int Evaluate_Piece_Positions(ABoard board, EColor color)
        {
            int score = 0;
            bool is_endgame = Is_Endgame(board);
            APiece piece;

            foreach (APosition pos in board.Get_Piece_Positions_For(color) )
            {
                piece = board[pos];
                score += Get_Position_Score(piece.Type, pos, color, is_endgame);
            }

            return score;
        }

        private int Evaluate_Mobility(AsGame_State game_state, EColor color)
        {
            int mobility = game_state.Get_All_Legal_Moves_For(color).Count();
            int opponent_mobility = game_state.Get_All_Legal_Moves_For(color.Opponent()).Count();

            return (mobility - opponent_mobility) * 2;
        }

        private int Evaluate_King_Safety(ABoard board, EColor color)
        {
            int score = 0;

            if (Is_King_Exposed(board, color))
            {
                score -= 40;
            }

            return score;
        }

        private int Evaluate_Terminal_Position(AsGame_State game_state, EColor color)
        {
            if (game_state.Result != null)
            {
                if (game_state.Result.Winner == color)
                {
                    return 10000;
                }
                else if (game_state.Result.Winner == color.Opponent() )
                {
                    return -10000;
                }
                else
                {
                    return 0; // Ничья
                }
            }

            return Evaluate_Position(game_state, color);
        }

        private List<AMove> Order_Moves(List<AMove> moves, ABoard board, EColor color)
        {
            return moves.OrderByDescending(move =>
            {
                int score = 0;
                APiece target_piece = board[move.To_Position];
                ABoard test_board;

                // Взятие фигур
                if (target_piece != null)
                {
                    score += 1000 + Get_Piece_Value(target_piece.Type) - Get_Piece_Value(board[move.From_Position].Type);
                }

                // Шах
                test_board = board.Copy();
                
                move.Act(test_board);

                if (test_board.Is_In_Check(color.Opponent() ) )
                {
                    score += 500;
                }

                // Продвижение пешки
                if (board[move.From_Position].Type == EPiece_Type.Pawn &&
                    (move.To_Position.Row == 0 || move.To_Position.Row == 7))
                {
                    score += 300;
                }

                return score;

            }).ToList();
        }

        private int Get_Piece_Value(EPiece_Type type)
        {
            switch (type)
            {
                case EPiece_Type.Pawn: return 100;
                case EPiece_Type.Knight: return 320;
                case EPiece_Type.Bishop: return 330;
                case EPiece_Type.Rook: return 500;
                case EPiece_Type.Queen: return 900;
                case EPiece_Type.King: return 10000;
                default: return 0;
            }
        }

        private int Get_Position_Score(EPiece_Type type, APosition pos, EColor color, bool is_endgame)
        {
            // Таблицы позиционных весов

            int[,] pawn_table = {
                { 0,  0,  0,  0,  0,  0,  0,  0 },
                { 50, 50, 50, 50, 50, 50, 50, 50 },
                { 10, 10, 20, 30, 30, 20, 10, 10 },
                { 5,  5, 10, 25, 25, 10,  5,  5 },
                { 0,  0,  0, 20, 20,  0,  0,  0 },
                { 5, -5,-10,  0,  0,-10, -5,  5 },
                { 5, 10, 10,-20,-20, 10, 10,  5 },
                { 0,  0,  0,  0,  0,  0,  0,  0 }
            };

            int[,] knight_table = {
                {-50,-40,-30,-30,-30,-30,-40,-50 },
                {-40,-20,  0,  0,  0,  0,-20,-40 },
                {-30,  0, 10, 15, 15, 10,  0,-30 },
                {-30,  5, 15, 20, 20, 15,  5,-30 },
                {-30,  0, 15, 20, 20, 15,  0,-30 },
                {-30,  5, 10, 15, 15, 10,  5,-30 },
                {-40,-20,  0,  5,  5,  0,-20,-40 },
                {-50,-40,-30,-30,-30,-30,-40,-50 }
            };

            int adjusted_row = (color == EColor.White) ? 7 - pos.Row : pos.Row;
            int col = pos.Column;

            switch (type)
            {
                case EPiece_Type.Pawn: return pawn_table[adjusted_row, col];
                case EPiece_Type.Knight: return knight_table[adjusted_row, col];
                case EPiece_Type.Rook: return (adjusted_row == 1 || adjusted_row == 6) ? 20 : 0;
                case EPiece_Type.King: return is_endgame ? -Math.Min(Math.Min(pos.Row, 7 - pos.Row), Math.Min(pos.Column, 7 - pos.Column)) * 10 : 0;
                default: return 0;
            }
        }

        private bool Is_Endgame(ABoard board)
        {
            int total_pieces = 0;
            int queens = 0;

            foreach (APosition pos in board.Get_Piece_Positions())
            {
                total_pieces++;

                if (board[pos].Type == EPiece_Type.Queen)
                {
                    queens++;
                }
            }

            return total_pieces <= 10 || queens == 0;
        }

        private bool Is_King_Exposed(ABoard board, EColor color)
        {
            return board.Is_In_Check(color);
        }

    }
}
