using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Chess_Engine.Core;

namespace Chess_UI.Helpers
{
    public static class AsImage_Helper
    {
        public static ImageSource Get_Image(int color, EPiece_Type piece_type)
        {
            if (piece_type == EPiece_Type.None)
            {
                return null;
            }

            if (color == APiece.White)
            {
                return White_Pieces_Sources[piece_type];
            }
            else if (color == APiece.Black)
            {
                return Black_Pieces_Sources[piece_type];
            }

            return null;
        }

        public static ImageSource Get_Image(int piece)
        {
            return Get_Image(APiece.Get_Piece_Color(piece), APiece.Get_Piece_Type(piece) );
        }

        private static ImageSource Load_Image(string file_path)
        {
            return new BitmapImage(new Uri(file_path, UriKind.Relative));
        }

        private static readonly Dictionary<EPiece_Type, ImageSource> White_Pieces_Sources = new Dictionary<EPiece_Type, ImageSource>
        {
            {EPiece_Type.Pawn, Load_Image("Assets/PawnW.png") },
            {EPiece_Type.Knight, Load_Image("Assets/KnightW.png") },
            {EPiece_Type.Bishop, Load_Image("Assets/BishopW.png") },
            {EPiece_Type.Rook, Load_Image("Assets/RookW.png") },
            {EPiece_Type.Queen, Load_Image("Assets/QueenW.png") },
            {EPiece_Type.King, Load_Image("Assets/KingW.png") }
        };

        private static readonly Dictionary<EPiece_Type, ImageSource> Black_Pieces_Sources = new Dictionary<EPiece_Type, ImageSource>
        {
            {EPiece_Type.Pawn, Load_Image("Assets/PawnB.png") },
            {EPiece_Type.Knight, Load_Image("Assets/KnightB.png") },
            {EPiece_Type.Bishop, Load_Image("Assets/BishopB.png") },
            {EPiece_Type.Rook, Load_Image("Assets/RookB.png") },
            {EPiece_Type.Queen, Load_Image("Assets/QueenB.png") },
            {EPiece_Type.King, Load_Image("Assets/KingB.png") }
        };
    }
}
