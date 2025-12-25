using Chess_Engine.Core;
using Chess_Engine.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Tests
{
    public class Perft_Test
    {
        private ABoard Board;
        private AMove_Generator Move_Generator;

        public Perft_Test()
        {
            Board = new ABoard();
            Move_Generator = new AMove_Generator();
        }

        /// <summary>
        /// Запускает Perft test для заданной позиции и глубины
        /// </summary>
        public Perft_Result Run_Perft(string fen, int depth)
        {
            Board.Load_Position(fen);
            var result = new Perft_Result();
            var stopwatch = Stopwatch.StartNew();

            result.Total_Nodes = Perft_Recursive(depth, depth, result, false);

            stopwatch.Stop();
            result.Time_Ms = stopwatch.ElapsedMilliseconds;
            result.Nodes_Per_Second = result.Time_Ms > 0 ? (result.Total_Nodes * 1000) / (ulong)result.Time_Ms : 0;

            return result;
        }

        /// <summary>
        /// Запускает разделенный Perft test с детализацией по ходам
        /// </summary>
        public Perft_Result Run_Divided_Perft(string fen, int depth)
        {
            Board.Load_Position(fen);
            var result = new Perft_Result();
            var stopwatch = Stopwatch.StartNew();

            result.Total_Nodes = Perft_Recursive(depth, depth, result, true);

            stopwatch.Stop();
            result.Time_Ms = stopwatch.ElapsedMilliseconds;
            result.Nodes_Per_Second = result.Time_Ms > 0 ? (result.Total_Nodes * 1000) / (ulong)result.Time_Ms : 0;

            return result;
        }

        private ulong Perft_Recursive(int depth, int initial_Depth, Perft_Result result, bool divided)
        {
            if (depth == 0)
                return 1;

            ulong nodes = 0;
            var moves = Move_Generator.Generate_Moves(Board);

            foreach (var move in moves)
            {
                Board.Make_Move(move, true);
                ulong child_Nodes = Perft_Recursive(depth - 1, initial_Depth, result, divided);
                Board.Unmake_Move(move, true);

                nodes += child_Nodes;

                if (divided && depth == initial_Depth)
                {
                    result.Move_Nodes.Add(new Move_Node_Count
                    {
                        Move = move,
                        Nodes = child_Nodes
                    });
                }

                if (depth == 1)
                {
                    Update_Move_Statistics(move, result);
                }
            }

            return nodes;
        }

        /// <summary>
        /// Обновляет статистику по типам ходов
        /// </summary>
        private void Update_Move_Statistics(SMove move, Perft_Result result)
        {
            result.Total_Leaf_Nodes++;

            if (move.Move_Flag == SMove.En_Passant_Capture_Flag)
            {
                result.En_Passants++;
                result.Captures++;
            }
            else if (move.Move_Flag == SMove.Castle_Flag)
            {
                result.Castles++;
            }
            else if (move.Is_Promotion)
            {
                result.Promotions++;
                
                if (Board.Square[move.Target_Square] != (int)EPiece_Type.None)
                {
                    result.Captures++;
                }
            }
            else
            {
                if (Board.Square[move.Target_Square] != (int)EPiece_Type.None)
                {
                    result.Captures++;
                }
                else
                {
                    result.Quiet_Moves++;
                }
            }

            if (Board.Is_In_Check())
            {
                result.Checks++;

                var opponent_Moves = Move_Generator.Generate_Moves(Board);
                if (opponent_Moves.Length == 0)
                {
                    result.Checkmates++;
                }
            }
        }

        /// <summary>
        /// Запускает серию стандартных Perft тестов с правильными ожидаемыми значениями
        /// </summary>
        public void Run_Standard_Tests()
        {
            var test_Positions = new[]
            {
                new
                {
                    Name = "Initial Position",
                    Fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1",
                    Expected_Nodes = new ulong[] { 20, 400, 8902, 197281, 4865609 }
                },
                new
                {
                    Name = "Kiwipete",
                    Fen = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1",
                    Expected_Nodes = new ulong[] { 48, 2039, 97862, 4085603 }
                },
                new
                {
                    Name = "Position 3",
                    Fen = "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1",
                    Expected_Nodes = new ulong[] { 14, 191, 2812, 43238 }
                },
                new
                {
                    Name = "Position 4",
                    Fen = "r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1",
                    Expected_Nodes = new ulong[] { 6, 264, 9467, 422333 }
                },
                new
                {
                    Name = "Position 5",
                    Fen = "rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8",
                    Expected_Nodes = new ulong[] { 44, 1486, 62379, 2103487 }
                }
            };

            Console.WriteLine("Running Standard Perft Tests...");
            Console.WriteLine("==========================================");

            int total_Tests = 0;
            int passed_Tests = 0;

            foreach (var test in test_Positions)
            {
                Console.WriteLine($"\nTest: {test.Name}");
                Console.WriteLine($"FEN: {test.Fen}");
                Console.WriteLine("Depth | Expected | Actual | Status");

                for (int depth = 1; depth <= 6; depth++)
                {
                    if (depth - 1 >= test.Expected_Nodes.Length)
                        break;

                    total_Tests++;
                    var result = Run_Perft(test.Fen, depth);
                    bool passed = result.Total_Nodes == test.Expected_Nodes[depth - 1];
                    string status = passed ? "PASS" : "FAIL";

                    if (passed) passed_Tests++;

                    Console.WriteLine($"{depth,5} | {test.Expected_Nodes[depth - 1],8} | {result.Total_Nodes,6} | {status}");

                    if (!passed)
                    {
                        Console.WriteLine($"  Difference: {(long)result.Total_Nodes - (long)test.Expected_Nodes[depth - 1]:+##;-##;0}");

                        if (depth == 1)
                        {
                            Run_Divided_Perft_For_Debug(test.Fen, depth);
                        }
                    }
                }
            }

            Console.WriteLine($"\n=== SUMMARY ===");
            Console.WriteLine($"Tests passed: {passed_Tests}/{total_Tests}");
            Console.WriteLine($"Success rate: {(double)passed_Tests / total_Tests * 100:0.0}%");
        }

        /// <summary>
        /// Запускает разделенный Perft для отладки расхождений
        /// </summary>
        private void Run_Divided_Perft_For_Debug(string fen, int depth)
        {
            Console.WriteLine($"\nDebug divided perft for depth {depth}:");
            var result = Run_Divided_Perft(fen, depth);

            Console.WriteLine("Move    | Nodes");
            Console.WriteLine("--------|-------");
            foreach (var move_Node in result.Move_Nodes)
            {
                string move_String = Move_To_String(move_Node.Move);
                Console.WriteLine($"{move_String,-7} | {move_Node.Nodes}");
            }

            Console.WriteLine($"Total moves found: {result.Move_Nodes.Count}");
        }

        public void Debug_Position_4_Detailed()
        {
            string fen = "r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1";
            Board.Load_Position(fen);

            Console.WriteLine("=== DETAILED DEBUG POSITION 4 ===");
            Console.WriteLine("Board state:");
            Console.WriteLine(AsBoard_Helper.CreateDiagram(Board, true, false, false));

            var moves = Move_Generator.Generate_Moves(Board);
            Console.WriteLine($"\nTotal moves found: {moves.Length} (expected: 6)");

            // Анализируем каждую фигуру и её возможные ходы
            Console.WriteLine("\n=== PIECE-BY-PIECE ANALYSIS ===");

            // 1. Король на g1
            Analyze_Piece_Moves("King", "g1", new[] { "h1", "f1", "f2" });

            // 2. Ладья на f1  
            Analyze_Piece_Moves("Rook", "f1", new[] { "f2", "f3", "f4", "f5", "f6", "f7", "e1", "d1", "c1", "b1", "a1" });

            // 3. Слон на b4
            Analyze_Piece_Moves("Bishop", "b4", new[] { "c5", "a5", "c3", "a3" });

            // 4. Слон на c4
            Analyze_Piece_Moves("Bishop", "c4", new[] { "d5", "b5", "d3", "b3", "e6", "f7" });

            // 5. Конь на f3
            Analyze_Piece_Moves("Knight", "f3", new[] { "d4", "e5", "g5", "h4", "g1" });

            // 6. Конь на h6
            Analyze_Piece_Moves("Knight", "h6", new[] { "g8", "f7", "f5", "g4" });

            // 7. Ферзь на d1
            Analyze_Piece_Moves("Queen", "d1", new[] { "d2", "d3", "e2", "c2", "e1", "c1" });

            // 8. Пешки
            Analyze_Pawn_Moves();

            Console.WriteLine("\n=== FOUND MOVES ===");
            foreach (var move in moves)
            {
                string moveStr = Move_To_String(move);
                string piece = APiece.Get_Piece_Symbol(Board.Square[move.Start_Square]).ToString();
                Console.WriteLine($"  {moveStr} ({piece})");
            }

            Console.WriteLine("\n=== MISSING MOVE ANALYSIS ===");
            Check_Missing_Moves_Position_4();
        }

        private void Analyze_Piece_Moves(string pieceName, string square, string[] expectedMoves)
        {
            int squareIndex = AsBoard_Helper.Square_Index_From_Name(square);
            int piece = Board.Square[squareIndex];

            Console.WriteLine($"\n{pieceName} on {square}:");

            var moves = Move_Generator.Generate_Moves(Board);
            var pieceMoves = moves.ToArray().Where(m => m.Start_Square == squareIndex).ToList();

            Console.WriteLine($"  Found {pieceMoves.Count} moves:");
            foreach (var move in pieceMoves)
            {
                Console.WriteLine($"    {Move_To_String(move)}");
            }

            Console.WriteLine($"  Expected moves: {string.Join(", ", expectedMoves)}");
        }

        private void Analyze_Pawn_Moves()
        {
            Console.WriteLine("\nPawns:");

            // Пешка b5
            Analyze_Piece_Moves("Pawn", "b5", new[] { "b6", "a6", "c6" });

            // Пешка e4
            Analyze_Piece_Moves("Pawn", "e4", new[] { "e5" });

            // Пешка g2
            Analyze_Piece_Moves("Pawn", "g2", new[] { "g3", "g4" });

            // Пешка h2  
            Analyze_Piece_Moves("Pawn", "h2", new[] { "h3", "h4" });
        }

        private void Check_Missing_Moves_Position_4()
        {
            Board.Load_Position("r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1");

            // Кандидаты на пропущенный ход:
            var candidateMoves = new[]
            {
                "b5a6", // взятие пешкой
                "b5c6", // взятие пешкой  
                "d1d2", // ход ферзем
                "d1e2", // ход ферзем
                "d1c1", // ход ферзем
                "f1e1", // ход ладьей
                "f1d1", // ход ладьей
                "f1c1", // ход ладьей
                "f1b1", // ход ладьей
                "f1a1", // ход ладьей
                "c4f7", // взятие слоном
                "h6g8", // взятие конем
                "h6f7", // взятие конем
                "f3g1"  // ход конем
            };

            foreach (var moveStr in candidateMoves)
            {
                SMove move = AsMove_Utility.Get_Move_From_UCI_Name(moveStr, Board);
                if (Is_Move_Legal(move))
                {
                    Console.WriteLine($"POSSIBLE MISSING MOVE: {moveStr}");
                }
            }
        }

        private void Check_Promotion_Variants(string fen)
        {
            Console.WriteLine("\n=== CHECKING PROMOTION VARIANTS ===");

            var promotionTypes = new[] { 'q', 'r', 'n', 'b' };

            foreach (char promo in promotionTypes)
            {
                string moveStr = $"d7c8{promo}";
                Board.Load_Position(fen);
                SMove move = AsMove_Utility.Get_Move_From_UCI_Name(moveStr, Board);

                if (Is_Move_Legal(move))
                {
                    Board.Make_Move(move, true);
                    ulong nodes = Simple_Perft_Recursive(2);
                    Board.Unmake_Move(move, true);

                    Console.WriteLine($"  {moveStr}: {nodes} nodes at depth 2");
                }
            }
        }

        private ulong Simple_Perft_Recursive(int depth)
        {
            if (depth == 0)
                return 1;

            ulong nodes = 0;
            var moves = Move_Generator.Generate_Moves(Board);

            foreach (var move in moves)
            {
                Board.Make_Move(move, true);
                ulong childNodes = Simple_Perft_Recursive(depth - 1);
                Board.Unmake_Move(move, true);
                nodes += childNodes;
            }

            return nodes;
        }

        public void Debug_Position_5_Sequences()
        {
            string fen = "rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8";

            Console.WriteLine("=== DEBUG POSITION 5 SEQUENCES ===");

            Board.Load_Position(fen);
            var moves = Move_Generator.Generate_Moves(Board);

            Console.WriteLine($"Total moves at depth 1: {moves.Length}");

            var moveNodes = new List<(string, ulong)>();

            foreach (var move in moves)
            {
                string moveStr = Move_To_String(move);
                Board.Make_Move(move, true);
                ulong nodes = Simple_Perft_Recursive(2);
                Board.Unmake_Move(move, true);

                moveNodes.Add((moveStr, nodes));
            }

            var orderedMoves = moveNodes.OrderBy(m => m.Item2).ToList();

            Console.WriteLine("\nMoves with lowest node counts (potential issues):");
            foreach (var (moveStr, nodes) in orderedMoves.Take(10))
            {
                Console.WriteLine($"  {moveStr}: {nodes} nodes");
            }

            Console.WriteLine("\nMoves with highest node counts:");
            foreach (var (moveStr, nodes) in orderedMoves.TakeLast(10))
            {
                Console.WriteLine($"  {moveStr}: {nodes} nodes");
            }

            Check_Specific_Problem_Moves_Position_5(fen);
        }

        private void Check_Specific_Problem_Moves_Position_5(string fen)
        {
            Console.WriteLine("\n=== CHECKING SPECIFIC PROBLEM MOVES ===");

            var problematicMoves = new[]
            {
                "d7c8q", // превращение пешки
                "d7c8n", // превращение пешки в коня
                "e1g1",  // рокировка
                "c4f7",  // взятие слоном
                "e2f4",  // ход конем (возможное взятие на f4)
            };

            foreach (var moveStr in problematicMoves)
            {
                Board.Load_Position(fen);
                SMove move = AsMove_Utility.Get_Move_From_UCI_Name(moveStr, Board);

                if (!Is_Move_Legal(move))
                {
                    Console.WriteLine($"ILLEGAL MOVE: {moveStr}");
                    continue;
                }

                Board.Make_Move(move, true);
                var responseMoves = Move_Generator.Generate_Moves(Board);
                Board.Unmake_Move(move, true);

                Console.WriteLine($"Move {moveStr}: leads to {responseMoves.Length} response moves");

                if (moveStr.StartsWith("d7c8"))
                {
                    Check_Promotion_Variants(fen);
                }
            }
        }

        private bool Is_Move_Legal(SMove move)
        {
            if (move.Is_Null) return false;

            int start = move.Start_Square;
            int target = move.Target_Square;

            if (start < 0 || start > 63 || target < 0 || target > 63) return false;
            if (Board.Square[start] == (int)EPiece_Type.None) return false;

            int pieceColor = APiece.Is_White(Board.Square[start]) ? APiece.White : APiece.Black;
            int currentColor = Board.Is_White_To_Move ? APiece.White : APiece.Black;
            if (pieceColor != currentColor) return false;

            if (Board.Square[target] != (int)EPiece_Type.None)
            {
                int targetColor = APiece.Is_White(Board.Square[target]) ? APiece.White : APiece.Black;
                if (targetColor == currentColor) return false;
            }

            return true;
        }


        /// <summary>
        /// Конвертирует ход в строку для отображения
        /// </summary>
        private string Move_To_String(SMove move)
        {
            if (move.Is_Null)
                return "null";

            string start_Square = Square_To_String(move.Start_Square);
            string target_Square = Square_To_String(move.Target_Square);

            if (move.Is_Promotion)
            {
                char promotion_Char = move.Move_Flag switch
                {
                    SMove.Promote_To_Queen_Flag => 'q',
                    SMove.Promote_To_Rook_Flag => 'r',
                    SMove.Promote_To_Bishop_Flag => 'b',
                    SMove.Promote_To_Knight_Flag => 'n',
                    _ => '?'
                };
                return $"{start_Square}{target_Square}{promotion_Char}";
            }

            return $"{start_Square}{target_Square}";
        }

        /// <summary>
        /// Конвертирует индекс клетки в строку (a1, e4, etc)
        /// </summary>
        private string Square_To_String(int square_Index)
        {
            int file = square_Index % 8;
            int rank = square_Index / 8;
            char file_Char = (char)('a' + file);
            char rank_Char = (char)('1' + rank);
            return $"{file_Char}{rank_Char}";
        }

        /// <summary>
        /// Быстрый тест для проверки базовой функциональности
        /// </summary>
        public void Quick_Smoke_Test()
        {
            Console.WriteLine("Quick Smoke Test");
            Console.WriteLine("================");

            var tests = new[]
            {
                new { Name = "Initial Position", Fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", Depth = 3, Expected = 8902UL },
                new { Name = "Kiwipete Depth 2", Fen = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1", Depth = 2, Expected = 2039UL }
            };

            foreach (var test in tests)
            {
                var result = Run_Perft(test.Fen, test.Depth);
                bool passed = result.Total_Nodes == test.Expected;
                Console.WriteLine($"{test.Name}: {result.Total_Nodes} (expected {test.Expected}) - {(passed ? "PASS" : "FAIL")}");
            }
        }

        /// <summary>
        /// Тест производительности для базовой позиции
        /// </summary>
        public void Performance_Test()
        {
            Console.WriteLine("\nPerformance Test");
            Console.WriteLine("================");

            string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

            for (int depth = 1; depth <= 5; depth++)
            {
                var stopwatch = Stopwatch.StartNew();
                var result = Run_Perft(fen, depth);
                stopwatch.Stop();

                Console.WriteLine($"Depth {depth}: {result.Total_Nodes:N0} nodes in {stopwatch.ElapsedMilliseconds}ms ({result.Nodes_Per_Second:N0} nps)");
            }
        }
    }

    /// <summary>
    /// Результаты Perft test
    /// </summary>
    public class Perft_Result
    {
        public ulong Total_Nodes { get; set; }
        public ulong Total_Leaf_Nodes { get; set; }
        public ulong Captures { get; set; }
        public ulong Promotions { get; set; }
        public ulong En_Passants { get; set; }
        public ulong Castles { get; set; }
        public ulong Checks { get; set; }
        public ulong Checkmates { get; set; }
        public ulong Quiet_Moves { get; set; }
        public long Time_Ms { get; set; }
        public ulong Nodes_Per_Second { get; set; }
        public List<Move_Node_Count> Move_Nodes { get; set; } = new List<Move_Node_Count>();

        public void Print_Result()
        {
            Console.WriteLine($"Total Nodes: {Total_Nodes:N0}");
            Console.WriteLine($"Time: {Time_Ms} ms");
            Console.WriteLine($"Nodes/Sec: {Nodes_Per_Second:N0}");
            if (Total_Leaf_Nodes > 0)
            {
                Console.WriteLine($"Captures: {Captures}");
                Console.WriteLine($"Promotions: {Promotions}");
                Console.WriteLine($"En Passants: {En_Passants}");
                Console.WriteLine($"Castles: {Castles}");
                Console.WriteLine($"Checks: {Checks}");
                Console.WriteLine($"Checkmates: {Checkmates}");
                Console.WriteLine($"Quiet Moves: {Quiet_Moves}");
            }
        }
    }

    /// <summary>
    /// Количество узлов для конкретного хода
    /// </summary>
    public class Move_Node_Count
    {
        public SMove Move { get; set; }
        public ulong Nodes { get; set; }
    }
}