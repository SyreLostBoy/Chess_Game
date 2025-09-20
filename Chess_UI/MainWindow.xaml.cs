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

            Game_State = new AsGame_State(EColor.White, AsBoard.Get_Initial_Board() );
            Draw_Board(Game_State.Board);
        }

        private void Initialize_Board()
        {
            Image image;

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    image = new Image();
                    Piece_Images[row, col] = image;
                    Piece_Grid.Children.Add(image);
                }
            }
        }

        private void Draw_Board(AsBoard board)
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

        private readonly Image[,] Piece_Images = new Image[8, 8];
        private AsGame_State Game_State;
    }
}