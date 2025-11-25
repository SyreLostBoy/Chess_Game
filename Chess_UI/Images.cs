using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Chess_Logic;

namespace Chess_UI
{
    public static class AsImages
    {
        public static ImageSource Get_Image(EColor color, EPiece_Type piece_type)
        {
            switch (color)
            {
                case EColor.White:
                    return White_Pieces_Sources[piece_type];

                case EColor.Black:
                    return Black_Pieces_Sources[piece_type];

                default:
                    return null;
            };
        }

        public static ImageSource Get_Image(APiece piece)
        {
            if (piece == null)
            {
                return null;
            }

            return Get_Image(piece.Color, piece.Type);
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
