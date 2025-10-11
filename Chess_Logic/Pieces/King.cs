using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Logic
{
    public class AKing : APiece
    {
        public override EPiece_Type Type => EPiece_Type.King;
        public override EColor Color { get; }

        private static readonly ADirection[] Directions = new ADirection[]
        {
           ADirection.North,
           ADirection.East,
           ADirection.West,
           ADirection.South,
           ADirection.North_West,
           ADirection.North_East,
           ADirection.South_West,
           ADirection.South_East
        };

        public AKing(EColor color, bool has_moved = false)
        {
            Color = color;
            Has_Moved = has_moved;
        }
        public override APiece Copy()
        {
            return new AKing(Color, Has_Moved);
        }
        public override IEnumerable<AMove> Get_Moves(APosition from_pos, ABoard board)
        {
            foreach (APosition to_pos in Get_Move_Positions(from_pos, board) )
            {
                yield return new AMove_Normal(from_pos, to_pos);
            }

            if (Can_Castle_King_Side(from_pos, board) )
            {
                yield return new AMove_Castle(EMove_Type.Castle_KS, from_pos);
            }

            if (Can_Castle_Queen_Side(from_pos, board))
            {
                yield return new AMove_Castle(EMove_Type.Castle_QS, from_pos);
            }
        }
        public override bool Can_Capture_King(APosition from_pos, ABoard board)
        {
            APiece piece;

            foreach (AMove move in Get_Moves(from_pos, board) )
            {
                piece = board[move.To_Position];

                if (piece != null && piece.Type == EPiece_Type.King)
                {
                    return true;
                }
            }

            return false;
        }
        public static bool Is_Unmoved_Rook(APosition pos, ABoard board)
        {
            APiece piece;

            if (board.Is_Empty(pos))
            {
                return false;
            }

            piece = board[pos];

            return piece.Type == EPiece_Type.Rook && !piece.Has_Moved;
        }

        public static bool All_Empty(IEnumerable<APosition> possitions, ABoard board)
        {
            return possitions.All(pos => board.Is_Empty(pos));
        }

        private bool Can_Castle_King_Side(APosition from, ABoard board)
        {
            int current_row;
            APosition rook_pos;
            APosition[] positions_between;

            if (Has_Moved)
            {
                return false;
            }

            current_row = from.Row;

            rook_pos = new APosition(current_row, 7);
            positions_between = new APosition[] { new APosition(current_row, 5), new APosition(current_row, 6) };

            return Is_Unmoved_Rook(rook_pos, board) && All_Empty(positions_between, board);
        }

        private bool Can_Castle_Queen_Side(APosition from, ABoard board)
        {
            int current_row;
            APosition rook_pos;
            APosition[] positions_between;

            if (Has_Moved)
            {
                return false;
            }

            current_row = from.Row;

            rook_pos = new APosition(current_row, 0);
            positions_between = new APosition[] { new APosition(current_row, 1), new APosition(current_row, 2), new APosition(current_row, 3) };

            return Is_Unmoved_Rook(rook_pos, board) && All_Empty(positions_between, board);
        }

        private IEnumerable<APosition> Get_Move_Positions(APosition from_pos, ABoard board)
        {
            APosition to_pos;

            foreach (ADirection dir in Directions)
            {
                to_pos = from_pos + dir;

                if (!ABoard.Is_Inside_Board(to_pos))
                {
                    continue;
                }

                if (board.Is_Empty(to_pos) || board[to_pos].Color != this.Color)
                {
                    yield return to_pos;
                }
            }
        }
    }
}
