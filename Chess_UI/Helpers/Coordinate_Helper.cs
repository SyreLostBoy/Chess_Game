using System.Windows;

namespace Chess_UI.Helpers
{
    public static class AsCoordinate_Helper
    {
        public static int Get_Square_From_Position(Point point, double board_width, bool board_flipped)
        {
            double square_size = board_width / 8;
            int ui_file = (int)(point.X / square_size);
            int ui_rank = (int)(point.Y / square_size);

            int board_file = board_flipped ? 7 - ui_file : ui_file;
            int board_rank = board_flipped ? ui_rank : 7 - ui_rank;

            int square = board_rank * 8 + board_file;
            return (square >= 0 && square < 64) ? square : -1;
        }

        public static int Get_UI_Index_From_Board_Square(int board_square, bool board_flipped)
        {
            int board_rank = board_square / 8;
            int board_file = board_square % 8;

            int ui_rank = board_flipped ? board_rank : 7 - board_rank;
            int ui_file = board_flipped ? 7 - board_file : board_file;

            return ui_rank * 8 + ui_file;
        }
    }
}
