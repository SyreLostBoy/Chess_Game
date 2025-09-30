using Chess_Logic;
using System.Windows.Controls;
using System.Windows.Input;


namespace Chess_UI
{
    /// <summary>
    /// Interaction logic for Promotion_Menu.xaml
    /// </summary>
    public partial class Promotion_Menu : UserControl
    {
        public Promotion_Menu(EColor player_color)
        {
            InitializeComponent();

            Queen_Image.Source = AsImages.Get_Image(player_color, EPiece_Type.Queen);
            Bishop_Image.Source = AsImages.Get_Image(player_color, EPiece_Type.Bishop);
            Rook_Image.Source = AsImages.Get_Image(player_color, EPiece_Type.Rook);
            Knight_Image.Source = AsImages.Get_Image(player_color, EPiece_Type.Knight);
        }

        public event Action<EPiece_Type> Piece_Selected;

        private void On_Queen_Image_Mouse_Down(object sender, MouseButtonEventArgs event_args)
        {
            Piece_Selected?.Invoke(EPiece_Type.Queen);
        }

        private void On_Bishop_Image_Mouse_Down(object sender, MouseButtonEventArgs event_args)
        {
            Piece_Selected?.Invoke(EPiece_Type.Bishop);
        }

        private void On_Rook_Image_Mouse_Down(object sender, MouseButtonEventArgs event_args)
        {
            Piece_Selected?.Invoke(EPiece_Type.Rook);
        }

        private void On_Knight_Image_Mouse_Down(object sender, MouseButtonEventArgs event_args)
        {
            Piece_Selected?.Invoke(EPiece_Type.Knight);
        }
    }
}
