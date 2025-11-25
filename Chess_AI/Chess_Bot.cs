using Chess_Logic;
using System.ComponentModel;
using System.Security;

namespace Chess_AI
{
    public class AChess_Bot
    {
        private const int Max_Depth = 4;
        private int Nodes_Evaluated;

        private readonly Dictionary<ulong, int> Transposition_Table = new Dictionary<ulong, int>();

        private static readonly int[] Piece_Values = new int[7];
        private static readonly int[,] Pawn_Table, Knight_Table;
        private static readonly ulong[,,] Zobrist_Numbers = Initialize_Zobrist_Numbers();

        static AChess_Bot()
        {
            Piece_Values[(int)EPiece_Type.Pawn] = 100;
            Piece_Values[(int)EPiece_Type.Knight] = 320;
            Piece_Values[(int)EPiece_Type.Bishop] = 330;
            Piece_Values[(int)EPiece_Type.Rook] = 500;
            Piece_Values[(int)EPiece_Type.Queen] = 900;
            Piece_Values[(int)EPiece_Type.King] = 10000;

            Pawn_Table = new int[,] {
                { 0,  0,  0,  0,  0,  0,  0,  0 },
                { 50, 50, 50, 50, 50, 50, 50, 50 },
                { 10, 10, 20, 30, 30, 20, 10, 10 },
                { 5,  5, 10, 25, 25, 10,  5,  5 },
                { 0,  0,  0, 20, 20,  0,  0,  0 },
                { 5, -5,-10,  0,  0,-10, -5,  5 },
                { 5, 10, 10,-20,-20, 10, 10,  5 },
                { 0,  0,  0,  0,  0,  0,  0,  0 }
            };

            Knight_Table = new int[,] {
                {-50,-40,-30,-30,-30,-30,-40,-50 },
                {-40,-20,  0,  0,  0,  0,-20,-40 },
                {-30,  0, 10, 15, 15, 10,  0,-30 },
                {-30,  5, 15, 20, 20, 15,  5,-30 },
                {-30,  0, 15, 20, 20, 15,  0,-30 },
                {-30,  5, 10, 15, 15, 10,  5,-30 },
                {-40,-20,  0,  5,  5,  0,-20,-40 },
                {-50,-40,-30,-30,-30,-30,-40,-50 }
            };
        }

        public AMove Find_Best_Move(AsGame_State game_state, EColor bot_color)
        {
            AMove best_move = null;
            int best_score = int.MinValue;
            int score = 0;
            int repetition_score;
            Nodes_Evaluated = 0;
            
            var possible_moves = game_state.Get_All_Legal_Moves_For(bot_color).ToList();

            if (possible_moves.Count == 0)
                return best_move;

            if (possible_moves.Count == 1)
            {
                return possible_moves[0];
            }

            // Сортировка ходов для улучшения отсечения
            var sorted_moves = Order_Moves(possible_moves, game_state.Board, bot_color);

            foreach (var move in sorted_moves)
            {
                APooled_Game_State test_state = Create_Test_State(game_state, move);

                if (test_state.State.Check_Threefold_Repetition() )
                {
                    repetition_score = Evaluate_Threefold_Position(test_state.State, bot_color);

                    if (repetition_score > best_score)
                    {
                        best_score = repetition_score;
                        best_move = move;
                    }

                    continue;
                }

                score = -Alpha_Beta(test_state.State, Max_Depth - 1, int.MinValue + 1, int.MaxValue - 1, bot_color.Opponent());

                if (score > best_score)
                {
                    best_score = score;
                    best_move = move;
                }

                if (best_score > 9000)
                {
                    break;
                }

            }

            return best_move ?? sorted_moves[0];
        }

        private static ulong[,,] Initialize_Zobrist_Numbers()
        {
            var random = new Random();
            ulong[,,] numbers = new ulong[8, 8, 13]; // 8х8 клеток х 12 типов фигур + 1 для цвета

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    for (int k = 0; k < 13; k++)
                    {
                        byte[] buffer = new byte[8];
                        random.NextBytes(buffer);
                        numbers[i, j, k] = BitConverter.ToUInt64(buffer, 0);
                    }
                }
            }

            return numbers;
        }

        private APooled_Game_State Create_Test_State(AsGame_State original_state, AMove move)
        {
            return new APooled_Game_State(original_state, move);
        }

        private int Alpha_Beta(AsGame_State game_state, int depth, int alpha, int beta, EColor current_color)
        {
            APooled_Game_State test_state;
            int score, repetition_value;
            ulong hash;

            Nodes_Evaluated++;

            // Проверка кэша
            hash = Calculate_Position_Hash(game_state);

            if (Transposition_Table.TryGetValue(hash, out int cached_score))
            {
                return cached_score;
            }

            if (game_state.Check_Threefold_Repetition() )
            {
                repetition_value = Evaluate_Threefold_In_Search(game_state, current_color);

                Transposition_Table[hash] = repetition_value;
                
                return repetition_value;
            }

            if (depth == 0)
            {
                return Quiescence_Search(game_state, alpha, beta, current_color);
            }

            if (game_state.Is_Game_Over() )
            {
                return Evaluate_Terminal_Position(game_state, current_color);
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

                score = -Alpha_Beta(test_state.State, depth - 1, -beta, -alpha, current_color.Opponent() );

                if (score >= beta)
                {
                    Transposition_Table[hash] = beta;
                    return beta;
                }

                if (score > alpha)
                {
                    alpha = score;
                }

            }

            Transposition_Table[hash] = alpha;
            return alpha;
        }

        private int Quiescence_Search(AsGame_State game_state, int alpha, int beta, EColor current_color)
        {
            int stand_pat = Evaluate_Position(game_state, current_color);
            APooled_Game_State test_state;
            int score, repetition_value;

            if (stand_pat >= beta)
            {
                return beta;
            }
            
            if (stand_pat > alpha)
            {
                alpha = stand_pat;
            }

            // Рассматриваем только взятия
            var capture_moves = game_state.Get_All_Legal_Moves_For(current_color).Where(move => game_state.Board[move.To_Position] != null).ToList();

            var ordered_captures = Order_Moves(capture_moves, game_state.Board, current_color);

            foreach (AMove move in ordered_captures)
            {
                test_state = Create_Test_State(game_state, move);

                if (test_state.State.Check_Threefold_Repetition() )
                {
                    repetition_value = Evaluate_Threefold_In_Search(test_state.State, current_color);

                    if (repetition_value >= beta)
                    {
                        return beta;
                    }

                    if (repetition_value > alpha)
                    {
                        alpha = repetition_value;
                    }

                    continue;
                }

                score = -Quiescence_Search(test_state.State, -beta, -alpha, current_color.Opponent());

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

        private ulong Calculate_Position_Hash(AsGame_State game_state)
        {// Упрощенный Zobrist hash для кэширования
            int piece_type_index, piece_index;
            ulong hash = 0;
            APiece piece;

            // Хеш фигур на доске
            foreach (APosition pos in game_state.Board.Get_Piece_Positions())
            {
                piece = game_state.Board[pos];

                // Безопасное вычисление индекса фигуры
                piece_type_index = (int)piece.Type;
                if (piece_type_index >= 1 && piece_type_index <= 6) // Проверяем валидность типа фигуры
                {
                    piece_index = (piece_type_index - 1) * 2 + (piece.Color == EColor.White ? 0 : 1);

                    // Проверяем границы массива
                    if (pos.Row >= 0 && pos.Row < 8 && pos.Column >= 0 && pos.Column < 8 &&
                        piece_index >= 0 && piece_index < 12)
                    {
                        hash ^= Zobrist_Numbers[pos.Row, pos.Column, piece_index];
                    }
                }
            }

            // Хеш текущего игрока (используем безопасный индекс)
            if (game_state.Current_Player_Color == EColor.Black)
            {
                // Используем предопределенный безопасный индекс для цвета
                hash ^= Zobrist_Numbers[0, 0, 0]; // Всегда используем [0,0,0] для цвета
            }

            return hash;
        }

        private int Evaluate_Threefold_Position(AsGame_State game_State, EColor bot_color)
        {
            // Если мы в выигрышной позиции, троекратное повторение невыгодно
            int material_score = Evaluate_Material(game_State.Board, bot_color);

            if (material_score > 200) // Мы имеем материальное преимущество
            {
                return -500;
            }
            else if (material_score < -200) // Мы в проигрышной позиции
            {
                return 800;
            }
            else // Равная позиция
            {
                return 100;
            }
        }

        private int Evaluate_Threefold_In_Search(AsGame_State game_state, EColor bot_color)
        {
            int material_score = Evaluate_Material(game_state.Board, bot_color);

            if (material_score > 150)
            {
                return -300; // Невыгодно при преимуществе
            }
            else if (material_score < -150)
            {
                return 300; // Выгодно при прогрышном положении
            }

            return 0;
        }

        private int Evaluate_Position(AsGame_State game_state, EColor color)
        {
            int score = 0;
            ABoard board = game_state.Board;

            // Материальный баланс
            score += Evaluate_Material(board, color);

            // Позиционная оценка
            if (!Is_Simple_Endgame(board))
            {
                score += Evaluate_Piece_Positions(board, color);

            }

            // Мобильность
            if (Nodes_Evaluated < 1000)
            {
                score += Evaluate_Mobility(game_state, color);
            }

            // Безопасность короля
            score += Evaluate_King_Safety(board, color);

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
                value = Piece_Values[(int)piece.Type];

                score += (piece.Color == color) ? value : -value;

            }

            return score;
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
                APiece from_piece = board[move.From_Position];
                ABoard test_board;

                // Взятие фигур
                if (target_piece != null)
                {
                    score += 1000 + Piece_Values[(int)target_piece.Type] - Piece_Values[(int)from_piece.Type];
                }

                // Шах
                if (Is_Check_After_Move(board, move, color))
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

        private bool Is_Check_After_Move(ABoard board, AMove move, EColor color)
        {
            bool is_check = false;
            APiece original_piece = board[move.To_Position];
            
            board[move.To_Position] = board[move.From_Position];
            board[move.From_Position] = null;

            is_check = board.Is_In_Check(color.Opponent() );

            board[move.From_Position] = board[move.To_Position];
            board[move.To_Position] = original_piece;

            return is_check;
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
            int adjusted_row = (color == EColor.White) ? 7 - pos.Row : pos.Row;
            int col = pos.Column;

            switch (type)
            {
                case EPiece_Type.Pawn: return Pawn_Table[adjusted_row, col];
                case EPiece_Type.Knight: return Knight_Table[adjusted_row, col];
                case EPiece_Type.Rook: return (adjusted_row == 1 || adjusted_row == 6) ? 20 : 0;
                case EPiece_Type.King: return is_endgame ? -Math.Min(Math.Min(pos.Row, 7 - pos.Row), Math.Min(pos.Column, 7 - pos.Column)) * 10 : 0;
                default: return 0;
            }
        }

        private bool Is_Simple_Endgame(ABoard board)
        {
            ACounting counting = board.Count_Pieces();

            if (counting.Total_Count > 10)
            {
                return false;
            }

            return counting.Total_Count <= 10;
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
