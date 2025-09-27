using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Chess_Logic;

namespace Chess_UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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

        private void Board_Grid_Mouse_Down(object sender, MouseButtonEventArgs event_args)
        {
            Point point = event_args.GetPosition(Board_Grid);
            APosition pos = To_Square_Position(point);
            
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

        private void On_From_Position_Selected(APosition pos)
        {
            IEnumerable<AMove> moves = Game_Engine.Get_Legal_Moves_For_Piece(pos);

            if (moves.Any() )
            {
                Selected_Position = pos;
                Cache_Moves(moves);
                Show_Highlights();
            }
            else
            {
                int yy = 13;
            }
        }

        private void On_To_Position_Selected(APosition pos)
        {
            Selected_Position = null;
            Hide_Highlights();

            if (Move_Cache.TryGetValue(pos, out AMove move))
            {
                Handle_Move(move);
            }
        }

        private void Handle_Move(AMove move)
        {
            Game_Engine.Act_Move(move);
            Draw_Board(Game_Engine.Board);
            Set_Cursor(Game_Engine.Current_Player_Color);
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

        private AsGame_Engine Game_Engine;
        private APosition Selected_Position = null;
        private Color Highlight_Color = Color.FromArgb(150, 125, 255, 125);

        private readonly Image[,] Piece_Images = new Image[8, 8];
        private readonly Rectangle[,] Highlights = new Rectangle[8, 8];
        private readonly Dictionary<APosition, AMove> Move_Cache = new Dictionary<APosition, AMove>();
    }
}