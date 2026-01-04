using Chess_Engine.Core;
using System.Windows.Controls;
using System.Windows.Input;
using Chess_UI.Helpers;

namespace Chess_UI
{
    /// <summary>
    /// Interaction logic for Promotion_Menu.xaml
    /// </summary>
    public partial class Promotion_Menu : UserControl
    {
        public event Action<EPiece_Type> OnPieceSelected;

        private readonly bool _isWhite;

        public Promotion_Menu(bool isWhite)
        {
            InitializeComponent();
            _isWhite = isWhite;
            UpdatePieceImages();
        }

        private void UpdatePieceImages()
        {
            int color = _isWhite ? APiece.White : APiece.Black;
            Queen_Image.Source = AsImage_Helper.Get_Image(color, EPiece_Type.Queen);
            Bishop_Image.Source = AsImage_Helper.Get_Image(color, EPiece_Type.Bishop);
            Rook_Image.Source = AsImage_Helper.Get_Image(color, EPiece_Type.Rook);
            Knight_Image.Source = AsImage_Helper.Get_Image(color, EPiece_Type.Knight);
        }

        private void On_Queen_Image_Mouse_Down(object sender, MouseButtonEventArgs e)
        {
            OnPieceSelected?.Invoke(EPiece_Type.Queen);
        }

        private void On_Bishop_Image_Mouse_Down(object sender, MouseButtonEventArgs e)
        {
            OnPieceSelected?.Invoke(EPiece_Type.Bishop);
        }

        private void On_Rook_Image_Mouse_Down(object sender, MouseButtonEventArgs e)
        {
            OnPieceSelected?.Invoke(EPiece_Type.Rook);
        }

        private void On_Knight_Image_Mouse_Down(object sender, MouseButtonEventArgs e)
        {
            OnPieceSelected?.Invoke(EPiece_Type.Knight);
        }
    }
}
