using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Logic
{
    public class AMove_Pawn_Promotion: AMove
    {
        public override EMove_Type Move_Type => EMove_Type.Pawn_Promotion;
        public override APosition From_Position { get; }
        public override APosition To_Position { get; }

        private readonly EPiece_Type New_Piece_Type;

        public AMove_Pawn_Promotion(APosition from_pos, APosition to_pos, EPiece_Type new_piece_type)
        {
            From_Position = from_pos;
            To_Position = to_pos;
            New_Piece_Type = new_piece_type;
        }

        public override bool Act(ABoard board)
        {
            APiece pawn, promotion_piece; 
            pawn = board[From_Position];

            board[From_Position] = null;

            promotion_piece = Create_Promotion_Piece(pawn.Color);

            board[To_Position] = promotion_piece;

            return true;
        }

        private APiece Create_Promotion_Piece(EColor color)
        {
            switch (New_Piece_Type)
            {
                case EPiece_Type.Knight:
                    return new AKnight(color, true);
                case EPiece_Type.Bishop:
                    return new ABishop(color, true);
                case EPiece_Type.Rook:
                    return new ARook(color, true);
                case EPiece_Type.Queen:
                default:
                    return new AQueen(color, true);
            }
        }
    }
}
