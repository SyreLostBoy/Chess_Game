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

        private readonly Image[,] Piece_Images = new Image[8, 8];
        private readonly Rectangle[,] Highlights = new Rectangle[8, 8];
        private readonly Dictionary<APosition, AMove> Move_Cache = new Dictionary<APosition, AMove>();

        public MainWindow()
        {
            InitializeComponent();
            Initialize_Board();

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

        private void On_Board_Grid_Mouse_Down(object sender, MouseButtonEventArgs event_args)
        {
            Point point;
            APosition pos;
            
            if (Is_Menu_On_Screen() )
            {
                return;
            }

            point = event_args.GetPosition(Board_Grid);
            pos = To_Square_Position(point);

            if (Selected_Position == null)
            {
                On_From_Position_Selected(pos);
            }
            else
            {
                On_To_Position_Selected(pos);
            }

        }

        private APosition To_Square_Position(Point point)
        {
            double square_size = Board_Grid.ActualWidth / 8;
            int row = (int)(point.Y / square_size);
            int col = (int)(point.X / square_size);

            return new APosition(row, col);
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