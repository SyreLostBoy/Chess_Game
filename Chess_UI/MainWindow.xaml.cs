using Chess_Logic;
using System.Reflection.Emit;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Chess_UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AsGame_Engine Game_Engine;
        private APosition Selected_Position = null;
        private Color Highlight_Color = Color.FromArgb(150, 125, 255, 125);
        private bool Is_Dragging = false;
        private Point Drag_Start_Point;
        private Image Dragged_Piece_Image, Dragged_Piece_Image_Clone;
        private Canvas Drag_Canvas;
        private APosition Drag_Start_Position;

        private readonly Image[,] Piece_Images = new Image[8, 8];
        private readonly Rectangle[,] Highlights = new Rectangle[8, 8];
        private readonly Dictionary<APosition, AMove> Move_Cache = new Dictionary<APosition, AMove>();

        public MainWindow()
        {
            InitializeComponent();
            Initialize_Board();
            Initialize_Drag_Canvas();

            Game_Engine = new AsGame_Engine(EColor.White, ABoard.Get_Initial_Board() );
            Draw_Board(Game_Engine.Board);

            Set_Cursor(Game_Engine.Current_Player_Color);
        }

        private void Initialize_Board()
        {
            Image image;
            Rectangle highlight_rectangle;

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    image = new Image();
                    Piece_Images[row, col] = image;
                    Piece_Grid.Children.Add(image);

                    highlight_rectangle = new Rectangle();
                    Highlights[row, col] = highlight_rectangle;
                    Highlight_Grid.Children.Add(highlight_rectangle);

                }
            }
        }

        private void Initialize_Drag_Canvas()
        {
            Drag_Canvas = new Canvas();

            Drag_Canvas.IsHitTestVisible = false; // Чтобы не перехватывал события мыши

            Board_Grid.Children.Add(Drag_Canvas);
        }

        private void Draw_Board(ABoard board)
        {
            APiece current_piece;

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    current_piece = board[row, col];

                    Piece_Images[row, col].Source = AsImages.Get_Image(current_piece);
                }
            }
        }

        private void On_Window_Key_Down(object sender, KeyEventArgs event_args)
        {
            if (!Is_Menu_On_Screen() && event_args.Key == Key.Escape)
            {
                Show_Pause_Menu();
            }
            else if (!Game_Engine.Is_Game_Over() && event_args.Key == Key.Escape)
            {
                Menu_Container.Content = null;
            }
        }

        private void On_Board_Grid_Mouse_Down(object sender, MouseButtonEventArgs event_args)
        {
            Point point;
            APosition pos;
            APiece piece;
            
            if (event_args.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            if (Is_Menu_On_Screen() )
            {
                return;
            }

            point = event_args.GetPosition(Board_Grid);
            pos = To_Square_Position(point);

            piece = Game_Engine.Board[pos];

            if (piece != null && piece.Color == Game_Engine.Current_Player_Color)
            {
                Start_Dragging(pos, point);
            }

        }
        private void On_Board_Grid_Mouse_Move(object sender, MouseEventArgs event_args)
        {
            Point current_point;

            if (Is_Dragging && event_args.LeftButton == MouseButtonState.Pressed)
            {
                current_point = event_args.GetPosition(Board_Grid);
                Continue_Dragging(current_point);
            }
        }

        private void On_Board_Grid_Mouse_Up(object sender, MouseButtonEventArgs event_args)
        {
            Point end_point;
            APosition end_pos;

            if (Is_Dragging)
            {
                end_point = event_args.GetPosition(Board_Grid);
                end_pos = To_Square_Position(end_point);

                End_Dragging(end_pos);
            }
        }

        private void Start_Dragging(APosition pos, Point point)
        {
            Is_Dragging = true;
            Drag_Start_Position = pos;
            Drag_Start_Point = point;

            Dragged_Piece_Image = Piece_Images[pos.Row, pos.Column];

            if (Dragged_Piece_Image.Source == null)
            {
                return;
            }

            // Cоздаем визуальное представление для перетаскивания
            Dragged_Piece_Image_Clone = new Image();
            Dragged_Piece_Image_Clone.Source = Dragged_Piece_Image.Source;
            Dragged_Piece_Image_Clone.Width = Dragged_Piece_Image.ActualWidth;
            Dragged_Piece_Image_Clone.Height = Dragged_Piece_Image.ActualHeight;
            Dragged_Piece_Image_Clone.Opacity = 1;

            Dragged_Piece_Image.Opacity = 0.5;

            // Устанавливаем позицию
            Canvas.SetLeft(Dragged_Piece_Image_Clone, point.X - Dragged_Piece_Image_Clone.Width / 2);
            Canvas.SetTop(Dragged_Piece_Image_Clone, point.Y - Dragged_Piece_Image_Clone.Height / 2);

            Drag_Canvas.Children.Add(Dragged_Piece_Image_Clone);

            On_From_Position_Selected(pos);

            Board_Grid.CaptureMouse();
        }

        private void Continue_Dragging(Point current_point)
        {
            if (Dragged_Piece_Image_Clone == null)
            {
                return;
            }

            Canvas.SetLeft(Dragged_Piece_Image_Clone, current_point.X - Dragged_Piece_Image_Clone.Width / 2);
            Canvas.SetTop(Dragged_Piece_Image_Clone, current_point.Y - Dragged_Piece_Image_Clone.Height / 2);

        }

        private void End_Dragging(APosition end_pos)
        {
            Is_Dragging = false;
            Board_Grid.ReleaseMouseCapture();

            if (Dragged_Piece_Image_Clone != null)
            {
                Drag_Canvas.Children.Remove(Dragged_Piece_Image_Clone);
                Dragged_Piece_Image_Clone = null;
            }

            if (Dragged_Piece_Image != null)
            {
                Dragged_Piece_Image.Opacity = 1.0;

                if (end_pos != null && end_pos != Drag_Start_Position)
                {
                    On_To_Position_Selected(end_pos);
                }

            }

            Dragged_Piece_Image = null;
            Selected_Position = null;
            Hide_Highlights();
            Move_Cache.Clear();
        }

        private APosition To_Square_Position(Point point)
        {
            double square_size = Board_Grid.ActualWidth / 8;
            int row = (int)(point.Y / square_size);
            int col = (int)(point.X / square_size);

            return new APosition(row, col);
        }

        private void On_From_Position_Selected(APosition pos)
        {
            IEnumerable<AMove> moves = Game_Engine.Get_Legal_Moves_For_Piece(pos);

            if (moves.Any() )
            {
                Selected_Position = pos;
                Cache_Moves(moves);
                Show_Highlights();
            }
        }

        private void On_To_Position_Selected(APosition pos)
        {
            AMove move;

            Selected_Position = null;
            Hide_Highlights();

            if (Move_Cache.TryGetValue(pos, out move) )
            {
                if (move.Move_Type == EMove_Type.Pawn_Promotion)
                {
                    Handle_Promotion(move.From_Position, move.To_Position);
                }
                else
                {
                    Handle_Move(move);
                }
            }
        }

        private void Handle_Promotion(APosition from_pos, APosition to_pos)
        {
            Promotion_Menu promotion_menu;
            EColor current_player_color = Game_Engine.Current_Player_Color;

            Piece_Images[to_pos.Row, to_pos.Column].Source = AsImages.Get_Image(current_player_color, EPiece_Type.Pawn);
            Piece_Images[to_pos.Row, to_pos.Column].Source = null;

            promotion_menu = new Promotion_Menu(current_player_color);
            Menu_Container.Content = promotion_menu;

            promotion_menu.Piece_Selected += piece_type =>
            {
                AMove promotion_move;

                Menu_Container.Content = null;
                promotion_move = new AMove_Pawn_Promotion(from_pos, to_pos, piece_type);
                Handle_Move(promotion_move);
            };
        }

        private void Handle_Move(AMove move)
        {
            Game_Engine.Act_Move(move);
            Draw_Board(Game_Engine.Board);
            Set_Cursor(Game_Engine.Current_Player_Color);

            if (Game_Engine.Is_Game_Over() )
            {
                Show_Game_Over();
            }
        }

        private void Cache_Moves(IEnumerable<AMove> moves)
        {
            Move_Cache.Clear();

            foreach (AMove move in moves)
            {
                Move_Cache[move.To_Position] = move;
            }
        }

        private void Show_Highlights()
        {
            foreach(APosition to_pos in Move_Cache.Keys)
            {
                Highlights[to_pos.Row, to_pos.Column].Fill = new SolidColorBrush(Highlight_Color);
            }
        }
        private void Hide_Highlights()
        {
            foreach (APosition to_pos in Move_Cache.Keys)
            {
                Highlights[to_pos.Row, to_pos.Column].Fill = Brushes.Transparent;
            }
        }

        private void Set_Cursor(EColor player_color)
        {
            if (player_color == EColor.White)
            {
                Cursor = Chess_Cursors.White_Cursor;
            }
            else
            {
                Cursor = Chess_Cursors.Black_Cursor;
            }
        }

        private bool Is_Menu_On_Screen()
        {
            return Menu_Container.Content != null;
        }

        private void Show_Game_Over()
        {
            Game_Over_Menu game_over_menu = new Game_Over_Menu(Game_Engine.Result, Game_Engine.Current_Player_Color);
            Menu_Container.Content = game_over_menu;

            game_over_menu.Option_Selected += option =>
            {
                if (option == EOption.Restart)
                {
                    Menu_Container.Content = null;
                    Restart_Game();
                }
                else
                {
                    Application.Current.Shutdown();
                }
            };
        }

        private void Show_Pause_Menu()
        {
            PauseMenu pause_menu = new PauseMenu();
            Menu_Container.Content = pause_menu;

            pause_menu.Option_Selected += option =>
            {
                Menu_Container.Content = null;

                if (option == EOption.Restart)
                { 
                    Restart_Game();
                }
                // else - Menu_Container.Content = null;
            };
        }

        private void Restart_Game()
        {
            Selected_Position = null;
            Hide_Highlights();
            Move_Cache.Clear();
            Game_Engine.Restart();
            Draw_Board(Game_Engine.Board);
            Set_Cursor(Game_Engine.Current_Player_Color);
        }
    }
}