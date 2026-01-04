using Chess_Engine;
using Chess_Engine.Bot;
using Chess_Engine.Core;
using Chess_Engine.Helpers;
using Chess_UI.Bot_Controller;
using Chess_UI.Helpers;
using Sound_System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Chess_UI
{
    public partial class MainWindow : Window
    {
        private ABoard Board;
        private AMove_Generator Move_Generator;
        private AsSound_System Sound_System;
        private ABot_Controller Bot_Controller;
        private ADifficulty_Controller Difficulty_Controller;

        private bool Playing_Against_Bot = true;
        private bool Human_Is_White = true;
        private bool Board_Flipped = true;

        private int Selected_Square = -1;
        private int Check_King_Square = -1;
        private bool Is_Dragging = false;
        private Image Dragged_Piece, Dragged_Piece_Clone;
        private Canvas Drag_Canvas;
        private Point Drag_Start_Point;
        private int Drag_Start_Square;
        private Dictionary<int, SMove> Move_Cache = new Dictionary<int, SMove>();

        private List<Image> Piece_Images = new List<Image>();
        private List<Border> Highlight_Borders = new List<Border>();

        //Константы
        public static readonly Color Highlight_Color = Color.FromArgb(150, 125, 255, 125);
        public static readonly Color Capture_Highlight = Color.FromArgb(180, 255, 100, 100);
        public static readonly Color Check_Highlight = Color.FromArgb(150, 255, 0, 0);
        public static readonly Color Selected_Color = Color.FromArgb(150, 255, 255, 0);
        public static readonly Color Last_Move_Color = Color.FromArgb(150, 173, 216, 230);

        public const int Drag_Piece_Width = 80;
        public const int Drag_Piece_Height = 80;
        public const double Drag_Piece_Offset = 30;
        public const double Drag_Piece_Opacity = 0.8;
        public const double Original_Piece_Opacity_When_Dragging = 0.3;

        public MainWindow()
        {
            InitializeComponent();
            Initialize_Board();
            Initialize_Drag_Canvas();
            Initialize_Game();
        }

        protected override void OnClosed(EventArgs e)
        {
            Bot_Controller?.Dispose();
            base.OnClosed(e);
        }

        private void Initialize_Game()
        {
            Board = ABoard.Create_Board();
            Move_Generator = new AMove_Generator();
            Sound_System = new AsSound_System();

            Initialize_Bot();

            Draw_Board();
        }

        private void Initialize_Board()
        {
            Piece_Grid.Children.Clear();
            Highlight_Grid.Children.Clear();
            Piece_Images.Clear();
            Highlight_Borders.Clear();

            for (int i = 0; i < 64; i++)
            {
                var highlight = new Border { Background = Brushes.Transparent };
                Highlight_Borders.Add(highlight);
                Highlight_Grid.Children.Add(highlight);
            }

            for (int i = 0; i < 64; i++)
            {
                var image = new Image { Stretch = Stretch.Uniform };
                Piece_Images.Add(image);
                Piece_Grid.Children.Add(image);
            }
        }

        private void Initialize_Drag_Canvas()
        {
            Drag_Canvas = new Canvas();
            Drag_Canvas.IsHitTestVisible = false;
            Board_Grid.Children.Add(Drag_Canvas);
        }

        private void Initialize_Bot()
        {
            Difficulty_Controller = new ADifficulty_Controller();

            if (Playing_Against_Bot)
            {
                Bot_Controller = new ABot_Controller(Board, Difficulty_Controller);
                Bot_Controller.On_Move_Chosen += On_Bot_Move_Chosen;

                Set_Bot_Difficulty(EDifficulty.Master); //!!!
            }
            else
            {
                Bot_Controller = null;
            }
        }

        #region Mouse Event Handlers

        private void On_Board_Grid_Mouse_Down(object sender, MouseButtonEventArgs e)
        {
            if (Playing_Against_Bot && Board.Move_Color != (Human_Is_White ? APiece.White : APiece.Black))
            {
                return;
            }

            if (Is_Menu_On_Screen()) return;

            Point point = e.GetPosition(Board_Grid);
            int square = AsCoordinate_Helper.Get_Square_From_Position(point, Board_Grid.ActualWidth, Board_Flipped);

            if (square >= 0 && square < 64)
            {
                int piece = Board.Square[square];

                if (piece != (int)EPiece_Type.None && APiece.Is_Color(piece, Board.Move_Color))
                {
                    Start_Dragging(square, point);
                }
                else
                {
                    if (Selected_Square != -1 && Move_Cache.TryGetValue(square, out SMove move))
                    {
                        if (move.Is_Promotion)
                        {
                            Handle_Promotion(move.Start_Square, move.Target_Square);
                        }
                        else
                        {
                            Execute_Move(move);
                        }
                        Clear_Selection();
                    }
                    else
                    {
                        Clear_Selection();
                    }
                }
            }
        }

        private void On_Board_Grid_Mouse_Move(object sender, MouseEventArgs e)
        {
            if (Is_Dragging && e.LeftButton == MouseButtonState.Pressed && Dragged_Piece_Clone != null)
            {
                Point current_point = e.GetPosition(Board_Grid);
                Continue_Dragging(current_point);
            }
        }

        private void On_Board_Grid_Mouse_Up(object sender, MouseButtonEventArgs e)
        {
            if (Is_Dragging)
            {
                Point end_point = e.GetPosition(Board_Grid);
                int end_square = AsCoordinate_Helper.Get_Square_From_Position(end_point, Board_Grid.ActualWidth, Board_Flipped);
                End_Dragging(end_square);
            }
        }

        #endregion

        #region Drag & Drop Logic

        private void Start_Dragging(int square, Point point)
        {
            Is_Dragging = true;
            Drag_Start_Square = square;
            Drag_Start_Point = point;

            int ui_index = AsCoordinate_Helper.Get_UI_Index_From_Board_Square(square, Board_Flipped);
            Dragged_Piece = Piece_Images[ui_index];

            if (Dragged_Piece.Source == null)
            {
                Is_Dragging = false;
                return;
            }

            Dragged_Piece_Clone = new Image
            {
                Source = Dragged_Piece.Source,
                Width = Drag_Piece_Width,
                Height = Drag_Piece_Height,
                Opacity = Drag_Piece_Opacity
            };

            Canvas.SetLeft(Dragged_Piece_Clone, point.X - Drag_Piece_Offset);
            Canvas.SetTop(Dragged_Piece_Clone, point.Y - Drag_Piece_Offset);

            Drag_Canvas.Children.Add(Dragged_Piece_Clone);
            Dragged_Piece.Opacity = Original_Piece_Opacity_When_Dragging;

            On_From_Square_Selected(square);
            Board_Grid.CaptureMouse();
        }

        private void Continue_Dragging(Point current_point)
        {
            if (Dragged_Piece_Clone == null) return;

            Canvas.SetLeft(Dragged_Piece_Clone, current_point.X - Drag_Piece_Offset);
            Canvas.SetTop(Dragged_Piece_Clone, current_point.Y - Drag_Piece_Offset);
        }

        private void End_Dragging(int end_square)
        {
            Is_Dragging = false;
            Board_Grid.ReleaseMouseCapture();

            if (Dragged_Piece_Clone != null)
            {
                Drag_Canvas.Children.Remove(Dragged_Piece_Clone);
                Dragged_Piece_Clone = null;
            }

            if (Dragged_Piece != null)
            {
                int ui_index = AsCoordinate_Helper.Get_UI_Index_From_Board_Square(Drag_Start_Square, Board_Flipped);
                Dragged_Piece = Piece_Images[ui_index];
                Dragged_Piece.Opacity = 1.0;

                if (end_square == Drag_Start_Square)
                {
                    return;
                }

                if (end_square >= 0 && end_square < 64 && end_square != Drag_Start_Square)
                {
                    if (Move_Cache.TryGetValue(end_square, out SMove move))
                    {
                        if (move.Is_Promotion)
                        {
                            Handle_Promotion(move.Start_Square, move.Target_Square);
                        }
                        else
                        {
                            Execute_Move(move);
                        }
                        Clear_Selection();
                    }
                    else
                    {
                        Clear_Selection();
                    }
                }
                else
                {
                    Clear_Selection();
                }
            }
        }

        #endregion

        #region Game Logic

        private void On_From_Square_Selected(int square)
        {
            var moves = Get_Legal_Moves_For_Piece(square);
            Selected_Square = square;

            if (moves.Any())
            {
                Cache_Moves(moves);
            }
            else
            {
                Move_Cache.Clear();
            }

            Update_Highlights();
        }

        private IEnumerable<SMove> Get_Legal_Moves_For_Piece(int square)
        {
            var all_moves = Move_Generator.Generate_Moves(Board);
            var moves = all_moves.ToArray().Where(m => m.Start_Square == square);
            return moves;
        }

        private void On_Bot_Move_Chosen(string move_string)
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    if (!Playing_Against_Bot)
                    {
                        Board_Grid.IsEnabled = true;
                        return;
                    }

                    if (string.IsNullOrEmpty(move_string) || move_string == "null")
                    {
                        Check_Game_State();
                        Board_Grid.IsEnabled = true;
                        return;
                    }

                    SMove move = AsMove_Utility.Get_Move_From_UCI_Name(move_string, Board);

                    var move_generator = new AMove_Generator();
                    var legal_moves = move_generator.Generate_Moves(Board);
                    bool is_valid_move = legal_moves.ToArray().Any(m =>
                        m.Start_Square == move.Start_Square &&
                        m.Target_Square == move.Target_Square &&
                        m.Move_Flag == move.Move_Flag);

                    if (!is_valid_move)
                    {
                        if (legal_moves.Length > 0)
                        {
                            move = legal_moves[0];
                        }
                        else
                        {
                            Check_Game_State();
                            Board_Grid.IsEnabled = true;
                            return;
                        }
                    }

                    Execute_Move(move);

                    // После хода бота, если это режим человек-человек, доска уже включена в Execute_Move
                    // В режиме человек-бот доска будет включена только если очередь человека
                    if (!Playing_Against_Bot)
                    {
                        Board_Grid.IsEnabled = true;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка при выполнении хода бота: {ex.Message}");
                    Board_Grid.IsEnabled = true;
                }
            });
        }

        private void Set_Bot_Difficulty(EDifficulty difficulty)
        {
            if (Bot_Controller == null)
            {
                return;
            }

            Bot_Controller.Set_Difficulty(difficulty);
        }

        private void Make_Bot_Move()
        {
            if (Bot_Controller != null && Playing_Against_Bot && !Is_Game_Over())
            {
                Board_Grid.IsEnabled = false;
                System.Threading.Tasks.Task.Run(() =>
                {
                    Bot_Controller.Make_Move();
                });
            }
            else
            {
                Board_Grid.IsEnabled = true;
            }
        }

        private void Execute_Move(SMove move)
        {
            bool is_capture = Board.Square[move.Target_Square] != (int)EPiece_Type.None;

            Board.Make_Move(move);
            Sound_System.Play_Move_Sound(move, is_capture);

            Draw_Board();
            Check_Game_State();

            if (Playing_Against_Bot && !Is_Game_Over())
            {
                bool bot_should_move = (Human_Is_White && !Board.Is_White_To_Move) || (!Human_Is_White && Board.Is_White_To_Move);

                if (bot_should_move)
                {
                    Make_Bot_Move();
                }
                else
                {
                    Board_Grid.IsEnabled = true;
                }
            }
            else if (!Playing_Against_Bot)
            {
                Board_Grid.IsEnabled = true;
            }
        }

        private void Handle_Promotion(int from_square, int to_square)
        {
            Show_Promotion_Menu(from_square, to_square);
        }

        public void Complete_Promotion_Move(int from_square, int to_square, EPiece_Type promotion_type)
        {
            int flag = Get_Promotion_Flag(promotion_type);
            var promotion_move = new SMove(from_square, to_square, flag);

            Execute_Move(promotion_move);
            Menu_Container.Content = null;
        }

        private int Get_Promotion_Flag(EPiece_Type piece_type)
        {
            return piece_type switch
            {
                EPiece_Type.Queen => SMove.Promote_To_Queen_Flag,
                EPiece_Type.Rook => SMove.Promote_To_Rook_Flag,
                EPiece_Type.Bishop => SMove.Promote_To_Bishop_Flag,
                EPiece_Type.Knight => SMove.Promote_To_Knight_Flag,
                _ => SMove.Promote_To_Queen_Flag
            };
        }

        private void Cache_Moves(IEnumerable<SMove> moves)
        {
            Move_Cache.Clear();
            foreach (SMove move in moves)
            {
                Move_Cache[move.Target_Square] = move;
            }
        }

        private void Draw_Board()
        {
            for (int i = 0; i < 64; i++)
            {
                int board_square = AsCoordinate_Helper.Get_UI_Index_From_Board_Square(i, Board_Flipped);
                int piece = Board.Square[board_square];

                Piece_Images[i].Source = AsImage_Helper.Get_Image(piece);
                Piece_Images[i].Opacity = 1.0;
            }

            Update_Check_State();
            Update_Highlights();
        }

        private void Update_Check_State()
        {
            Check_King_Square = -1;

            if (Board.Is_In_Check())
            {
                Check_King_Square = Board.Is_White_To_Move ? Board.King_Square[ABoard.White_Index] : Board.King_Square[ABoard.Black_Index];
            }
        }

        private void Update_Highlights()
        {
            Hide_Highlights();

            if (Selected_Square >= 0 && Selected_Square < 64)
            {
                int ui_index = AsCoordinate_Helper.Get_UI_Index_From_Board_Square(Selected_Square, Board_Flipped);
                Highlight_Borders[ui_index].Background = new SolidColorBrush(Selected_Color);
            }

            foreach (int target_square in Move_Cache.Keys)
            {
                if (target_square >= 0 && target_square < 64)
                {
                    int ui_index = AsCoordinate_Helper.Get_UI_Index_From_Board_Square(target_square, Board_Flipped);
                    bool is_capture = Board.Square[target_square] != (int)EPiece_Type.None;
                    Color move_color = is_capture ? Capture_Highlight : Highlight_Color;

                    Highlight_Borders[ui_index].Background = new SolidColorBrush(move_color);
                }
            }

            if (Board.All_Game_Moves.Count > 0)
            {
                var last_move = Board.All_Game_Moves[^1];
                if (last_move.Start_Square >= 0 && last_move.Start_Square < 64)
                {
                    int ui_index = AsCoordinate_Helper.Get_UI_Index_From_Board_Square(last_move.Start_Square, Board_Flipped);
                    Highlight_Borders[ui_index].Background = new SolidColorBrush(Last_Move_Color);
                }
                if (last_move.Target_Square >= 0 && last_move.Target_Square < 64)
                {
                    int ui_index = AsCoordinate_Helper.Get_UI_Index_From_Board_Square(last_move.Target_Square, Board_Flipped);
                    Highlight_Borders[ui_index].Background = new SolidColorBrush(Last_Move_Color);
                }
            }

            if (Check_King_Square >= 0 && Check_King_Square < 64)
            {
                int ui_index = AsCoordinate_Helper.Get_UI_Index_From_Board_Square(Check_King_Square, Board_Flipped);
                var king_border = Highlight_Borders[ui_index];
                king_border.Background = new SolidColorBrush(Check_Highlight);
                king_border.BorderBrush = new SolidColorBrush(Colors.Red);
                king_border.BorderThickness = new Thickness(3);
            }
        }

        private void Hide_Highlights()
        {
            foreach (var highlight in Highlight_Borders)
            {
                highlight.Background = Brushes.Transparent;
                highlight.BorderBrush = Brushes.Transparent;
                highlight.BorderThickness = new Thickness(0);
            }
        }

        private void Clear_Selection()
        {
            Selected_Square = -1;
            Move_Cache.Clear();
            Dragged_Piece = null;
            Is_Dragging = false;
            Update_Highlights();
        }

        private void Check_Game_State()
        {
            EGame_Result game_state = AsGame_Manager.Get_Game_State(Board);

            if (game_state != EGame_Result.In_Progress)
            {
                Show_Game_Over_Menu(game_state);
            }
        }

        #endregion

        #region Menu Management

        private void New_Game_Human_vs_Human_Click(object sender, RoutedEventArgs e)
        {
            Playing_Against_Bot = false;
            Human_Is_White = true;
            Board_Flipped = false;
            Start_New_Game();
        }

        private void New_Game_Human_White_Click(object sender, RoutedEventArgs e)
        {
            Playing_Against_Bot = true;
            Human_Is_White = true;
            Board_Flipped = false;
            Start_New_Game();

            if (Bot_Controller == null)
                Initialize_Bot();
        }

        private void New_Game_Human_Black_Click(object sender, RoutedEventArgs e)
        {
            Playing_Against_Bot = true;
            Human_Is_White = false;
            Board_Flipped = true;
            Start_New_Game();

            if (Bot_Controller == null)
                Initialize_Bot();

            Make_Bot_Move();
        }

        private void Start_New_Game()
        {
            Board.Load_Start_Position();
            Clear_Selection();
            Draw_Board();

            if (Playing_Against_Bot)
            {
                // Уничтожаем старый контроллер бота если был
                Bot_Controller?.Dispose();
                Bot_Controller = null;

                // Инициализируем бота
                Initialize_Bot();

                bool bot_should_move = (!Human_Is_White && Board.Is_White_To_Move) || (Human_Is_White && !Board.Is_White_To_Move);

                if (bot_should_move)
                {
                    Board_Grid.IsEnabled = false;
                    
                    System.Threading.Tasks.Task.Run(() =>
                    {
                        Bot_Controller?.Make_Move();
                    });
                }
                else
                {
                    Board_Grid.IsEnabled = true;
                }
            }
            else
            {
                Board_Grid.IsEnabled = true;
            }
        }

        private void Show_Pause_Menu()
        {
            var pause_menu = new PauseMenu();
            pause_menu.OnContinue += On_Continue_Game;
            pause_menu.OnRestart += On_Restart_Game;
            Menu_Container.Content = pause_menu;
        }

        private void Show_Promotion_Menu(int pawn_square, int target_square)
        {
            var promotion_menu = new Promotion_Menu(Board.Is_White_To_Move);
            promotion_menu.OnPieceSelected += (piece_type) =>
            {
                Complete_Promotion_Move(pawn_square, target_square, piece_type);
            };
            Menu_Container.Content = promotion_menu;
        }

        private void Show_Game_Over_Menu(EGame_Result result)
        {
            var game_over_menu = new Game_Over_Menu();
            game_over_menu.SetResult(result);
            game_over_menu.OnRestart += On_Restart_Game;
            game_over_menu.OnExit += On_Exit_Game;
            Menu_Container.Content = game_over_menu;
        }

        private void On_Continue_Game()
        {
            Menu_Container.Content = null;
        }

        private void On_Restart_Game()
        {
            Board.Load_Start_Position();
            Clear_Selection();
            Draw_Board();
            Menu_Container.Content = null;
        }

        private void On_Exit_Game()
        {
            this.Close();
        }

        private bool Is_Menu_On_Screen()
        {
            return Menu_Container.Content != null;
        }

        #endregion

        private void On_Window_Key_Down(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (Is_Menu_On_Screen())
                {
                    Menu_Container.Content = null;
                }
                else
                {
                    Show_Pause_Menu();
                }
            }
        }

        private bool Is_Game_Over()
        {
            var game_state = AsGame_Manager.Get_Game_State(Board);
            return game_state != EGame_Result.In_Progress;
        }
    }
}