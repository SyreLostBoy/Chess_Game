using Chess_Engine.Core;
using Chess_UI.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Chess_UI.Board_Renderer
{
    public class AsBoard_Renderer
    {
        private readonly Grid Board_Grid;
        private readonly UniformGrid Piece_Grid;
        private readonly UniformGrid Highlight_Grid;
        public bool Is_Board_Flipped { get; private set; }

        private List<Image> Piece_Images = new List<Image>();
        private List<Border> Highlight_Borders = new List<Border>();

        // Цвета
        public static readonly Color HighlightColor = Color.FromArgb(150, 125, 255, 125);
        public static readonly Color CaptureHighlight = Color.FromArgb(180, 255, 100, 100);
        public static readonly Color CheckHighlight = Color.FromArgb(150, 255, 0, 0);
        public static readonly Color SelectedColor = Color.FromArgb(150, 255, 255, 0);
        public static readonly Color LastMoveColor = Color.FromArgb(150, 173, 216, 230);

        public AsBoard_Renderer(Grid board_grid, UniformGrid piece_grid, UniformGrid highlight_grid)
        {
            Board_Grid = board_grid;
            Piece_Grid = piece_grid;
            Highlight_Grid = highlight_grid;

            Init();
        }

        public void Init()
        {
            Piece_Grid.Children.Clear();
            Highlight_Grid.Children.Clear();
            Piece_Images.Clear();
            Highlight_Borders.Clear();

            for (int i = 0; i < 64; i++)
            {
                Border highlight = new Border { Background = Brushes.Transparent };
                Highlight_Borders.Add(highlight);
                Highlight_Grid.Children.Add(highlight);

                Image image = new Image { Stretch = Stretch.Uniform };
                Piece_Images.Add(image);
                Piece_Grid.Children.Add(image);
            }
        }

        public void Draw_Board(ABoard board)
        {
            for (int i = 0; i < 64; i++)
            {
                int board_square = AsCoordinate_Helper.Get_UI_Index_From_Board_Square(i, Is_Board_Flipped);
                int piece = board.Square[board_square];

                Piece_Images[i].Source = AsImage_Helper.Get_Image(piece);
                Piece_Images[i].Opacity = 1.0;
            }
        }

        public void Update_Higlights(int selected_square, Dictionary<int, SMove> move_cache, ABoard board, SMove? last_move)
        {
            Hide_Highlights();

            // Подсветка выбранной клетки
            if (selected_square >= 0 && selected_square < 64)
            {
                int ui_index = AsCoordinate_Helper.Get_UI_Index_From_Board_Square(selected_square, Is_Board_Flipped);
                Highlight_Borders[ui_index].Background = new SolidColorBrush(SelectedColor);
            }

            // Подсветка возможных ходов
            foreach (int target_square in move_cache.Keys)
            {
                if (target_square >= 0 && target_square < 64)
                {
                    int ui_index = AsCoordinate_Helper.Get_UI_Index_From_Board_Square(target_square, Is_Board_Flipped);
                    Highlight_Borders[ui_index].Background = new SolidColorBrush(HighlightColor);
                }
            }

            // Подсветка последнего хода
            if (last_move.HasValue)
            {
                SMove move = last_move.Value;
                int start_ui_index = AsCoordinate_Helper.Get_UI_Index_From_Board_Square(move.Start_Square, Is_Board_Flipped);
                int target_ui_index = AsCoordinate_Helper.Get_UI_Index_From_Board_Square(move.Target_Square, Is_Board_Flipped);

                Highlight_Borders[start_ui_index].Background = new SolidColorBrush(LastMoveColor);
                Highlight_Borders[target_ui_index].Background = new SolidColorBrush(LastMoveColor);
            }

            // Подсветка шаха
            if (board.Is_In_Check() )
            {
                int king_square = board.Is_White_To_Move ? board.King_Square[ABoard.White_Index] : board.King_Square[ABoard.Black_Index];
                int ui_index = AsCoordinate_Helper.Get_UI_Index_From_Board_Square(king_square, Is_Board_Flipped);
                Border king_border = Highlight_Borders[ui_index];
                
                king_border.Background = new SolidColorBrush(CheckHighlight);
                king_border.BorderBrush = new SolidColorBrush(Colors.Red);
                king_border.BorderThickness = new Thickness(3);
            }
        }

        public void Hide_Highlights()
        {
            foreach (Border highlight in Highlight_Borders)
            {
                highlight.Background = Brushes.Transparent;
                highlight.BorderBrush = Brushes.Transparent;
                highlight.BorderThickness = new Thickness(0);
            }
        }

        public void Flip_Board(bool flipped)
        {
            Is_Board_Flipped = flipped;
        }

        public int Get_Square_From_Position(Point point)
        {
            return AsCoordinate_Helper.Get_Square_From_Position(point, Board_Grid.ActualWidth, Is_Board_Flipped);
        }
    }
}
