using Chess_Engine;
using Chess_Engine.Helpers;
using Tests;

public class AChessTestBase
{
    protected ABot _bot;
    protected string _lastBestMove;
    protected ManualResetEvent _moveReceivedEvent;

    public AChessTestBase()
    {
        _bot = new ABot();
        _moveReceivedEvent = new ManualResetEvent(false);
        _bot.On_Move_Chosen += OnMoveChosen;
    }

    protected void OnMoveChosen(string move)
    {
        _lastBestMove = move;
        _moveReceivedEvent.Set();
    }

    protected string WaitForMove(int timeoutMs = 7000)
    {
        _moveReceivedEvent.Reset();
        bool moveReceived = _moveReceivedEvent.WaitOne(timeoutMs);

        if (!moveReceived)
        {
            throw new TimeoutException("Move not received within timeout");
        }

        return _lastBestMove;
    }

    protected void TestPosition(string fen, string expectedMove, string description, int thinkTimeMs = 5000)
    {
        try
        {
            Console.WriteLine($"\n🧪 {description}");
            Console.WriteLine($"FEN: {fen}");

            _bot.Set_Position(fen);
            _bot.Think_Timed(thinkTimeMs);

            string foundMove = WaitForMove();

            Console.WriteLine($"Expected: {expectedMove}");
            Console.WriteLine($"Found:    {foundMove}");

            if (foundMove == expectedMove)
            {
                Console.WriteLine("✅ PASS");
            }
            else
            {
                Console.WriteLine("❌ FAIL");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 ERROR: {ex.Message}");
        }
    }

    protected void TestPositionMultiple(string fen, string[] expectedMoves, string description, int thinkTimeMs = 2000)
    {
        try
        {
            Console.WriteLine($"\n🧪 {description}");
            Console.WriteLine($"FEN: {fen}");

            _bot.Set_Position(fen);
            _bot.Think_Timed(thinkTimeMs);

            string foundMove = WaitForMove();

            Console.WriteLine($"Expected one of: {string.Join(", ", expectedMoves)}");
            Console.WriteLine($"Found:           {foundMove}");

            if (expectedMoves.Contains(foundMove))
            {
                Console.WriteLine("✅ PASS");
            }
            else
            {
                Console.WriteLine("❌ FAIL");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 ERROR: {ex.Message}");
        }
    }
}

public class ATacticalTests : AChessTestBase
{
    public void Run_All_Tactical_Tests()
    {
        Console.WriteLine("🎯 === TACTICAL TESTS ===");

        Test_Knight_Forks();
        Test_Pawn_Forks();
        Test_Queen_Forks();
        Test_Skewers();
        Test_Pins();
        Test_Discovered_Attacks();
        Test_Exchanges();
    }

    // 1. ВИЛКИ КОНЕМ
    private void Test_Knight_Forks()
    {
        Console.WriteLine("\n🐴 --- Knight Fork Tests ---");

        // Вилка короля и ферзя
        TestPosition(
            "r1bqk1nr/pppp1ppp/2n5/4p3/4P3/5N2/PPPP1PPP/RNBQKB1R w KQkq - 0 1",
            "e5",
            "Knight fork: king + queen"
        );

        // Вилка двух ладей
        TestPosition(
            "r3k2r/ppp2ppp/2n5/4N3/8/8/PPPP1PPP/R3K2R b KQkq - 0 1",
            "c6",
            "Knight fork: two rooks"
        );
    }

    // 2. ВИЛКИ ПЕШКОЙ
    private void Test_Pawn_Forks()
    {
        Console.WriteLine("\n♙ --- Pawn Fork Tests ---");

        // Вилка двух фигур пешкой
        TestPosition(
            "rnbqkb1r/ppp1pppp/5n2/3p4/4P3/8/PPPP1PPP/RNBQKBNR w KQkq - 0 1",
            "d5",
            "Pawn fork: knight + pawn"
        );
    }

    // 3. ВИЛКИ ФЕРЗЕМ
    private void Test_Queen_Forks()
    {
        Console.WriteLine("\n♕ --- Queen Fork Tests ---");

        // Вилка двух ладей ферзем
        TestPosition(
            "r3k2r/ppp2ppp/2n5/8/3Q4/8/PPPP1PPP/R3K2R b KQkq - 0 1",
            "a7",
            "Queen fork: two rooks"
        );
    }

    // 4. СКВОЗНЫЕ АТАКИ (SKEWERS)
    private void Test_Skewers()
    {
        Console.WriteLine("\n🎯 --- Skewer Tests ---");

        // Сквозная атака ладьи (король-ладья)
        TestPosition(
            "4k3/8/8/8/3R4/8/8/4K2R b K - 0 1",
            "e4",
            "Rook skewer: king-rook"
        );

        // Сквозная атака слона (король-ферзь)
        TestPosition(
            "4k3/8/8/8/3B4/8/8/4K2Q b - - 0 1",
            "g7",
            "Bishop skewer: king-queen"
        );
    }

    // 5. СВЯЗКИ (PINS)
    private void Test_Pins()
    {
        Console.WriteLine("\n📌 --- Pin Tests ---");

        // Связка ферзя слоном
        TestPosition(
            "r1bqk1nr/pppp1ppp/2n5/2b1p3/2B1P3/3P4/PPP2PPP/RNBQK1NR b KQkq - 0 1",
            "f7",
            "Bishop pin: queen is pinned"
        );
    }

    // 6. ВСКРЫТЫЕ НАПАДЕНИЯ
    private void Test_Discovered_Attacks()
    {
        Console.WriteLine("\n⚡ --- Discovered Attack Tests ---");

        // Вскрытый шах с атакой на ферзя
        TestPosition(
            "r1bqk2r/pppp1ppp/2n2n2/2b1p3/2B1P3/3P1N2/PPP2PPP/RNBQK2R w KQkq - 0 1",
            "g1",
            "Discovered attack on queen"
        );
    }

    // 7. ТЕСТЫ НА РАЗМЕНЫ
    private void Test_Exchanges()
    {
        Console.WriteLine("\n🔄 --- Exchange Tests ---");

        // Выгодный размен (ферзь за ферзя)
        TestPosition(
            "r1bqkbnr/pppp1ppp/2n5/4p3/4P3/3P4/PPP2PPP/RNBQKBNR w KQkq - 0 1",
            "d8",
            "Queen exchange"
        );
    }
}

public class AMateTests : AChessTestBase
{
    public void Run_Mate_Tests()
    {
        Console.WriteLine("👑 === MATE TESTS ===");

        Test_Mate_In_One();
        Test_Mate_In_Two();
        Test_Mate_In_Three();
        Test_Defense_Against_Mate();
    }

    private void Test_Mate_In_One()
    {
        Console.WriteLine("\n👑 --- Mate in 1 Tests ---");

        // Простой мат ферзем
        TestPosition(
            "r1bqk1nr/pppp1ppp/2n5/2b1p3/2B1P3/5Q2/PPPP1PPP/RNB1K1NR w KQkq - 0 1",
            "f7",
            "Queen mate in 1",
            3000
        );

        // Мат ладьей
        TestPosition(
            "4k3/8/8/8/8/8/4R3/4K3 b - - 0 1",
            "e8",
            "Rook mate in 1",
            3000
        );
    }

    private void Test_Mate_In_Two()
    {
        Console.WriteLine("\n👑👑 --- Mate in 2 Tests ---");

        // Классический мат в 2 хода
        TestPosition(
            "r5rk/5p1p/5R2/4B3/8/8/7P/7K w - - 0 1",
            "f8",
            "Classic mate in 2 (1st move)",
            5000
        );
    }

    private void Test_Mate_In_Three()
    {
        Console.WriteLine("\n👑👑👑 --- Mate in 3 Tests ---");

        // Более сложный мат в 3 хода
        TestPosition(
            "r3k2r/ppp2ppp/2n5/4N3/8/8/PPPP1PPP/R3K2R b KQkq - 0 1",
            "c6",
            "Mate in 3 sequence start",
            5000
        );
    }

    private void Test_Defense_Against_Mate()
    {
        Console.WriteLine("\n🛡️ --- Mate Defense Tests ---");

        // Позиция где нужно защищаться от мата
        TestPosition(
            "r1bqk1nr/pppp1ppp/2n5/2b1p3/2B1P3/5Q2/PPPP1PPP/RNB1K1NR b KQkq - 0 1",
            "f8",
            "Defend against mate threat",
            3000
        );
    }
}

public class APositionalTests : AChessTestBase
{
    public void Run_Positional_Tests()
    {
        Console.WriteLine("🏰 === POSITIONAL TESTS ===");

        Test_Center_Control();
        Test_Development();
        Test_King_Safety();
        Test_Pawn_Structure();
    }

    private void Test_Center_Control()
    {
        Console.WriteLine("\n🎯 --- Center Control Tests ---");

        // Должен предпочесть контроль центра
        TestPositionMultiple(
            "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1",
            new[] { "e4", "d4", "c4", "f4" },
            "Should control center"
        );

        // Атака на центр
        TestPositionMultiple(
            "r1bqkbnr/pppp1ppp/2n5/4p3/4P3/5N2/PPPP1PPP/RNBQKB1R w KQkq - 0 1",
            new[] { "e5", "d4" },
            "Attack center"
        );
    }

    private void Test_Development()
    {
        Console.WriteLine("\n♘ --- Development Tests ---");

        // Должен развивать фигуры
        TestPositionMultiple(
            "rnbqkbnr/pppp1ppp/8/4p3/4P3/8/PPPP1PPP/RNBQKBNR w KQkq - 0 1",
            new[] { "f3", "c4", "c3", "f4" },
            "Develop pieces"
        );
    }

    private void Test_King_Safety()
    {
        Console.WriteLine("\n♔ --- King Safety Tests ---");

        // Должен рокировать для безопасности короля
        TestPositionMultiple(
            "r1bqkbnr/pppp1ppp/2n5/4p3/4P3/5N2/PPPP1PPP/RNBQKB1R w KQkq - 0 1",
            new[] { "g1", "e1" },
            "King safety - castle"
        );
    }

    private void Test_Pawn_Structure()
    {
        Console.WriteLine("\n♙ --- Pawn Structure Tests ---");

        // Избегать изолированных пешек
        TestPositionMultiple(
            "rnbqkbnr/ppp1pppp/8/3p4/4P3/8/PPPP1PPP/RNBQKBNR w KQkq - 0 1",
            new[] { "d4", "c3" },
            "Avoid isolated pawns"
        );
    }
}

public class AChessTestRunner
{
    public void Run_Complete_Test_Suite()
    {
        Console.WriteLine("🚀 STARTING CHESS ENGINE TACTICAL TESTS");
        Console.WriteLine("========================================\n");

        try
        {
            // Тактические тесты
            var tacticalTests = new ATacticalTests();
            tacticalTests.Run_All_Tactical_Tests();

            // Матовые тесты
            var mateTests = new AMateTests();
            mateTests.Run_Mate_Tests();

            // Позиционные тесты
            var positionalTests = new APositionalTests();
            positionalTests.Run_Positional_Tests();

            Console.WriteLine("\n🎉 ALL TESTS COMPLETED");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 TEST SUITE FAILED: {ex.Message}");
        }
    }

    // Метод для запуска отдельных тестов
    public void Run_Specific_Test(string testType)
    {
        switch (testType.ToLower())
        {
            case "tactical":
                new ATacticalTests().Run_All_Tactical_Tests();
                break;
            case "mate":
                new AMateTests().Run_Mate_Tests();
                break;
            case "positional":
                new APositionalTests().Run_Positional_Tests();
                break;
            default:
                Console.WriteLine("Unknown test type. Use: tactical, mate, positional");
                break;
        }
    }
}

namespace Program
{

    static class Program
    {
        static void Main(string[] args)
        {
            Perft_Test perft_test = new Perft_Test();

            perft_test.Run_Standard_Tests();

            perft_test.Performance_Test();

            // Для Position 5  
            //perft_test.Debug_Position_5_Sequences();
        }
    }


}

