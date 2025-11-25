using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chess_Engine.Helpers;

namespace Chess_Engine.Core
{
    internal class AsPiece_Square_Table
    {
        public static int Read(int[] table, int square, bool is_white)
        {
            if (is_white)
            {
                int file = AsBoard_Helper.File_Index(square);
                int rank = AsBoard_Helper.Rank_Index(square);
                rank = 7 - rank;
                square = AsBoard_Helper.Index_From_Coord(file, rank);
            }

            return table[square];
        }

        public static int Read(int piece, int square)
        {
            if (APiece.Get_Piece_Type(piece) == EPiece_Type.None)
            {
                return 0;
            }

            return Tables[piece][square];
        }

        public static readonly int[] Pawns = {
             0,   0,   0,   0,   0,   0,   0,   0,
            50,  50,  50,  50,  50,  50,  50,  50,
            10,  10,  20,  30,  30,  20,  10,  10,
             5,   5,  10,  25,  25,  10,   5,   5,
             0,   0,   0,  20,  20,   0,   0,   0,
             5,  -5, -10,   0,   0, -10,  -5,   5,
             5,  10,  10, -20, -20,  10,  10,   5,
             0,   0,   0,   0,   0,   0,   0,   0
        };

        public static readonly int[] Pawns_End = {
             0,   0,   0,   0,   0,   0,   0,   0,
            80,  80,  80,  80,  80,  80,  80,  80,
            50,  50,  50,  50,  50,  50,  50,  50,
            30,  30,  30,  30,  30,  30,  30,  30,
            20,  20,  20,  20,  20,  20,  20,  20,
            10,  10,  10,  10,  10,  10,  10,  10,
            10,  10,  10,  10,  10,  10,  10,  10,
             0,   0,   0,   0,   0,   0,   0,   0
        };

        public static readonly int[] Rooks =  {
            0,  0,  0,  0,  0,  0,  0,  0,
            5, 10, 10, 10, 10, 10, 10,  5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            0,  0,  0,  5,  5,  0,  0,  0
        };
        public static readonly int[] Knights = {
            -50,-40,-30,-30,-30,-30,-40,-50,
            -40,-20,  0,  0,  0,  0,-20,-40,
            -30,  0, 10, 15, 15, 10,  0,-30,
            -30,  5, 15, 20, 20, 15,  5,-30,
            -30,  0, 15, 20, 20, 15,  0,-30,
            -30,  5, 10, 15, 15, 10,  5,-30,
            -40,-20,  0,  5,  5,  0,-20,-40,
            -50,-40,-30,-30,-30,-30,-40,-50,
        };
        public static readonly int[] Bishops =  {
            -20,-10,-10,-10,-10,-10,-10,-20,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -10,  0,  5, 10, 10,  5,  0,-10,
            -10,  5,  5, 10, 10,  5,  5,-10,
            -10,  0, 10, 10, 10, 10,  0,-10,
            -10, 10, 10, 10, 10, 10, 10,-10,
            -10,  5,  0,  0,  0,  0,  5,-10,
            -20,-10,-10,-10,-10,-10,-10,-20,
        };
        public static readonly int[] Queens =  {
            -20,-10,-10, -5, -5,-10,-10,-20,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -10,  0,  5,  5,  5,  5,  0,-10,
            -5,   0,  5,  5,  5,  5,  0, -5,
            0,    0,  5,  5,  5,  5,  0, -5,
            -10,  5,  5,  5,  5,  5,  0,-10,
            -10,  0,  5,  0,  0,  0,  0,-10,
            -20,-10,-10, -5, -5,-10,-10,-20
        };
        public static readonly int[] King_Start =
        {
            -80, -70, -70, -70, -70, -70, -70, -80,
            -60, -60, -60, -60, -60, -60, -60, -60,
            -40, -50, -50, -60, -60, -50, -50, -40,
            -30, -40, -40, -50, -50, -40, -40, -30,
            -20, -30, -30, -40, -40, -30, -30, -20,
            -10, -20, -20, -20, -20, -20, -20, -10,
            20,  20,  -5,  -5,  -5,  -5,  20,  20,
            20,  30,  10,   0,   0,  10,  30,  20
        };

        public static readonly int[] King_End =
        {
            -20, -10, -10, -10, -10, -10, -10, -20,
            -5,   0,   5,   5,   5,   5,   0,  -5,
            -10, -5,   20,  30,  30,  20,  -5, -10,
            -15, -10,  35,  45,  45,  35, -10, -15,
            -20, -15,  30,  40,  40,  30, -15, -20,
            -25, -20,  20,  25,  25,  20, -20, -25,
            -30, -25,   0,   0,   0,   0, -25, -30,
            -50, -30, -30, -30, -30, -30, -30, -50
        };
        public static readonly int[][] Tables;

        static AsPiece_Square_Table()
        {
            Tables = new int[APiece.Max_Piece_Index + 1][];
            Tables[APiece.Make_Piece(EPiece_Type.Pawn, APiece.White)] = Pawns;
            Tables[APiece.Make_Piece(EPiece_Type.Rook, APiece.White)] = Rooks;
            Tables[APiece.Make_Piece(EPiece_Type.Knight, APiece.White)] = Knights;
            Tables[APiece.Make_Piece(EPiece_Type.Bishop, APiece.White)] = Bishops;
            Tables[APiece.Make_Piece(EPiece_Type.Queen, APiece.White)] = Queens;

            Tables[APiece.Make_Piece(EPiece_Type.Pawn, APiece.Black)] = Get_Flipped_Table(Pawns);
            Tables[APiece.Make_Piece(EPiece_Type.Rook, APiece.Black)] = Get_Flipped_Table(Rooks);
            Tables[APiece.Make_Piece(EPiece_Type.Knight, APiece.Black)] = Get_Flipped_Table(Knights);
            Tables[APiece.Make_Piece(EPiece_Type.Bishop, APiece.Black)] = Get_Flipped_Table(Bishops);
            Tables[APiece.Make_Piece(EPiece_Type.Queen, APiece.Black)] = Get_Flipped_Table(Queens);
        }

        static int[] Get_Flipped_Table(int[] table)
        {
            int[] flipped_table = new int[table.Length];

            for (int i = 0; i < table.Length; i++)
            {
                SCoordinate coord = new SCoordinate(i);
                SCoordinate flippedCoord = new SCoordinate(coord.File_Index, 7 - coord.Rank_Index);
                flipped_table[flippedCoord.Square_Index] = table[i];
            }
            return flipped_table;
        }
    }
}
