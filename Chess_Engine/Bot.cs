using Chess_Engine.Core;
using Chess_Engine.Helpers;
using System.Numerics;

namespace Chess_Engine
{
    public class ABot
    {
        public event Action<string>? On_Move_Chosen;
        public bool Is_Thinking { get; private set; }
        public bool Latest_Move_Is_Book_Move { get; private set; }

        private ASearcher Searcher;
        private readonly ABoard Board;
        private readonly AOpening_Book Opening_Book;
        private readonly AutoResetEvent Search_Wait_Handle;
        CancellationTokenSource? Cancel_Search_Timer;

        private int Current_Search_ID;
        private bool Is_Quitting;

        private const bool Use_Opening_Book = true;
        private const int Max_Book_Ply = 16;
        private const bool Use_Max_Think_Time = false;
        private const int Max_Think_Time_Ms = 5000;

        public ABot()
        {
            Board = ABoard.Create_Board();
            Searcher = new ASearcher(Board);
            Searcher.On_Search_Complete += On_Search_Completed;

            Opening_Book = Load_Opening_Book();

            Search_Wait_Handle = new(false);

            Task.Factory.StartNew(Search_Thread, TaskCreationOptions.LongRunning);
        }

        public ABot(ABoard board)
        {
            Board = board;
            Searcher = new ASearcher(Board);
            Searcher.On_Search_Complete += On_Search_Completed;
            
            Opening_Book = Load_Opening_Book();

            Search_Wait_Handle = new(false);

            Task.Factory.StartNew(Search_Thread, TaskCreationOptions.LongRunning);
        }

        public SMove Get_Best_Move()
        {
            return Searcher.Best_Move_So_Far;
        }

        public void Notify_New_Game()
        {
            ABoard search_board = ABoard.Create_Board(Board.Current_FEN);
            Searcher.Clear_For_New_Position();
        }

        public void Set_Position(string fen)
        {
            Board.Load_Position(fen);
        }

        public void Make_Move(string move_string)
        {
            SMove move = AsMove_Utility.Get_Move_From_UCI_Name(move_string, Board);
            Board.Make_Move(move);
        }

        public int Choose_Think_Time(int time_remaining_white_ms, int time_remaining_black_ms, int increment_white_ms, int increment_black_ms)
        {
            int my_time_remaining_ms = Board.Is_White_To_Move ? time_remaining_white_ms : time_remaining_black_ms;
            int my_increment_ms = Board.Is_White_To_Move ? increment_white_ms : increment_black_ms;

            double think_time_ms = my_increment_ms / 40.0;

            if (Use_Max_Think_Time)
            {
                think_time_ms = Math.Min(Max_Think_Time_Ms, think_time_ms);
            }

            if (my_time_remaining_ms > my_increment_ms * 2)
            {
                think_time_ms += my_increment_ms * 0.8;
            }

            double min_think_time = Math.Min(50, my_time_remaining_ms * 0.25);
            
            return (int)Math.Ceiling(Math.Max(min_think_time, think_time_ms));
        }

        public void Think_Timed(int time_ms)
        {
            Latest_Move_Is_Book_Move = false;
            Is_Thinking = true;
            Cancel_Search_Timer?.Cancel();

            var searchBoard = ABoard.Create_Board(Board.Current_FEN);
            Searcher = new ASearcher(searchBoard);
            Searcher.On_Search_Complete += On_Search_Completed;

            if (Try_Get_Opening_Book_Move(out SMove book_move))
            {
                Latest_Move_Is_Book_Move = true;
                On_Search_Completed(book_move);
            }
            else
            {
                Start_Search(time_ms);
            }
        }

        public void Stop_Thinking()
        {
            End_Search();
        }

        public void Quit()
        {
            Is_Quitting = true;
            End_Search();
        }

        public static string Get_Resource_Path(params string[] local_path)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "resources", Path.Combine(local_path));
        }

        public static string Read_Resource_File(string local_path)
        {
            return File.ReadAllText(Read_Resource_File(local_path));
        }

        public string Get_Board_Diagram() => Board.ToString();

        private void Start_Search(int time_ms)
        {
            Current_Search_ID++;
            Search_Wait_Handle.Set();
            Cancel_Search_Timer = new CancellationTokenSource();
            Task.Delay(time_ms, Cancel_Search_Timer.Token).ContinueWith((t) => End_Search(Current_Search_ID));
        }

        private AOpening_Book Load_Opening_Book()
        {
            string bookPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", Path.Combine("Opening_Book\\Book.txt"));
            
            if (File.Exists(bookPath))
            {
                try
                {
                    string bookContent = File.ReadAllText(bookPath);
                    var openingBook = new AOpening_Book(bookContent);
                    Console.WriteLine("Opening book loaded successfully");
                    return openingBook;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading opening book: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"Opening book file not found: {bookPath}");
            }

            return new AOpening_Book("");
        }

        private void Search_Thread()
        {
            while (!Is_Quitting)
            {
                Search_Wait_Handle.WaitOne();
                Searcher.Start_Search();
            }
        }

        private void End_Search()
        {
            Cancel_Search_Timer?.Cancel();

            if (Is_Thinking)
            {
                Searcher.End_Search();
            }
        }

        private void End_Search(int search_id)
        {
            if (Cancel_Search_Timer != null && Cancel_Search_Timer.IsCancellationRequested)
            {
                return;
            }

            if (Current_Search_ID == search_id)
            {
                End_Search();
            }
        }

        private void On_Search_Completed(SMove move)
        {
            Is_Thinking = false;
            string move_name = AsMove_Utility.Get_Move_Name_UCI(move).Replace("=", "");

            On_Move_Chosen?.Invoke(move_name);
        }

        private bool Try_Get_Opening_Book_Move(out SMove book_move)
        {
            if (Use_Opening_Book && Board.Ply_Count <= Max_Book_Ply && Opening_Book.Try_Get_Book_Move(Board, out string move_string))
            {
                book_move = AsMove_Utility.Get_Move_From_UCI_Name(move_string, Board);
                return true;
            }

            book_move = SMove.Null_Move;
            return false;
        }
    }
}
