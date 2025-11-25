using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Engine.Core
{
    public enum EPiece_Type
    {
        None,

        Pawn,
        Knight,
        Bishop,
        Rook,
        Queen,
        King
    }

    public class APiece
    {

        public const int White = 0;
        public const int Black = 8;

        // Pieces
        public const int White_Pawn = (int)EPiece_Type.Pawn | White; // 1
        public const int White_Knight = (int)EPiece_Type.Knight | White; // 2
        public const int White_Bishop = (int)EPiece_Type.Bishop | White; // 3
        public const int White_Rook = (int)EPiece_Type.Rook | White; // 4
        public const int White_Queen = (int)EPiece_Type.Queen | White; // 5
        public const int White_King = (int)EPiece_Type.King | White; // 6

        public const int Black_Pawn = (int)EPiece_Type.Pawn | Black; // 9
        public const int Black_Knight= (int)EPiece_Type.Knight | Black; // 10
        public const int Black_Bishop= (int)EPiece_Type.Bishop | Black; // 11
        public const int Black_Rook= (int)EPiece_Type.Rook | Black; // 12
        public const int Black_Queen= (int)EPiece_Type.Queen | Black; // 13
        public const int Black_King= (int)EPiece_Type.King | Black; // 14

        public const int Max_Piece_Index = Black_King;

        public static readonly int[] Piece_Indices = new int[]
        {
            White_Pawn, White_Knight, White_Bishop, White_Rook, White_Queen, White_King,
            Black_Pawn, Black_Knight, Black_Bishop, Black_Rook, Black_Queen, Black_King
        };

        private const int Type_Mask = 0b0111;
        private const int Color_Mask = 0b1000;

        public static int Make_Piece(EPiece_Type piece_type, int piece_color)
        {
            return (int)piece_type | piece_color;
        }

        public static int Make_Piece(EPiece_Type piece_type, bool piece_is_white)
        {
            return Make_Piece(piece_type, piece_is_white ? White : Black);
        }

        public static bool Is_Color(int piece, int color)
        {
            return (piece & Color_Mask) == color && piece != 0;
        }

        public static bool Is_White(int piece)
        {
            return Is_Color(piece, White);
        }

        public static int Get_Piece_Color(int piece)
        {
            return piece & Color_Mask;
        }

        public static EPiece_Type Get_Piece_Type(int piece)
        {
            return (EPiece_Type)(piece & Type_Mask);
        }

        // Rook or Queen
        public static bool Is_Orthogonal_Slider(int piece)
        {
            EPiece_Type piece_type = Get_Piece_Type(piece);

            return piece_type == EPiece_Type.Queen || piece_type == EPiece_Type.Rook;
        }

        // Queen or Bishop
        public static bool Is_Diagonal_Slider(int piece)
        {
            EPiece_Type piece_type = Get_Piece_Type(piece);

            return piece_type == EPiece_Type.Queen || piece_type == EPiece_Type.Bishop;
        }

        public static bool Is_Sliding_Piece(int piece)
        {
            return Is_Orthogonal_Slider(piece) || Is_Diagonal_Slider(piece);
        }

        public static char Get_Piece_Symbol(int piece)
        {
            EPiece_Type piece_type = Get_Piece_Type(piece);
            char symbol;

            switch (piece_type)
            {
                case EPiece_Type.Pawn: symbol = 'P'; break;
                case EPiece_Type.Knight: symbol = 'N'; break;
                case EPiece_Type.Bishop: symbol = 'B'; break;
                case EPiece_Type.Rook: symbol = 'R'; break;
                case EPiece_Type.Queen: symbol = 'Q'; break;
                case EPiece_Type.King: symbol = 'K'; break;
                default: symbol = ' '; break;
            }

            symbol = Is_White(piece) ? symbol : char.ToLower(symbol);

            return symbol;
        }

        public static EPiece_Type Get_Piece_Type_From_Symbol(char symbol)
        {
            symbol = char.ToUpper(symbol);

            switch (symbol)
            {
                case 'P': return EPiece_Type.Pawn;
                case 'N': return EPiece_Type.Knight;
                case 'B': return EPiece_Type.Bishop;
                case 'R': return EPiece_Type.Rook;
                case 'Q': return EPiece_Type.Queen;
                case 'K': return EPiece_Type.King;
                default: return EPiece_Type.None;
            }
        }
    }
}
