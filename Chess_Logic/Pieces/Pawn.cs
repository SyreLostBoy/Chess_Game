namespace Chess_Logic
{
    public class APawn : APiece 
    {
        public override EPiece_Type Type => EPiece_Type.Pawn;
        public override EColor Color { get; }

        private ADirection Forward_Direction;

        public APawn(EColor color, bool has_moved = false)
        {
            Color = color;
            Has_Moved = has_moved;

            if (Color == EColor.White)
            {
                Forward_Direction = ADirection.North;
            }
            else if (Color == EColor.Black)
            {
                Forward_Direction = ADirection.South;
            }
        }

        public override APiece Copy()
        {
            return new APawn(Color, Has_Moved);
        }

        public override IEnumerable<AMove> Get_Moves(APosition from_pos, ABoard board)
        {
            return Get_Forward_Moves(from_pos, board).Concat(Get_Diagonal_Moves(from_pos, board));
        }

        public override bool Can_Capture_King(APosition from_pos, ABoard board)
        {
            APiece piece;

            foreach (AMove move in Get_Diagonal_Moves(from_pos, board) )
            {
                piece = board[move.To_Position];

                if (piece != null && piece.Type == EPiece_Type.King)
                {
                    return true;
                }
            }

            return false;
        }

        private bool Can_Capture_At(APosition pos, ABoard board)
        {
            if (!ABoard.Is_Inside_Board(pos) || board.Is_Empty(pos) )
            {
                return false;
            }

            return board[pos].Color != this.Color;
        }

        private IEnumerable<AMove> Get_Forward_Moves(APosition from_pos, ABoard board)
        {
            APosition one_move_pos = from_pos + Forward_Direction;
            APosition two_move_pos;

            if (Can_Move_To(one_move_pos, board) )
            {
                if (one_move_pos.Row == 0 || one_move_pos.Row == 7)
                {// Ход превращения пешки
                    foreach (AMove promotion_move in Get_Promotion_Moves(from_pos, one_move_pos))
                    {
                        yield return promotion_move;
                    }
                }
                else
                {
                    yield return new AMove_Normal(from_pos, one_move_pos);

                }

                two_move_pos = one_move_pos + Forward_Direction;

                if (!Has_Moved && Can_Move_To(two_move_pos, board) )
                {
                    yield return new AMove_Double_Pawn(from_pos, two_move_pos);
                }
            }
        }

        private IEnumerable<AMove> Get_Diagonal_Moves(APosition from_pos, ABoard board)
        {
            APosition to_pos;

            ADirection[] horizontal_directions = new ADirection[]
            {
                ADirection.West,
                ADirection.East
            };

            foreach (ADirection dir in horizontal_directions)
            {
                to_pos = from_pos + Forward_Direction + dir; // Получаем диагональную позицию. Forward_Direction + dir = диагональное направление относительно прямого движения пешки

                if (to_pos == board.Get_Pawn_Skip_Position(Color.Opponent() ) )
                {
                    yield return new AMove_En_Passant(from_pos, to_pos);
                }
                else if (Can_Capture_At(to_pos, board) )
                {
                    if (to_pos.Row == 0 || to_pos.Row == 7)
                    {// Ход превращения пешки
                        foreach (AMove promotion_move in Get_Promotion_Moves(from_pos, to_pos))
                        {
                            yield return promotion_move;
                        }
                    }
                    else
                    {
                        yield return new AMove_Normal(from_pos, to_pos);

                    }
                }
            }
        }

        private static bool Can_Move_To(APosition pos, ABoard board)
        {
            return ABoard.Is_Inside_Board(pos) && board.Is_Empty(pos);
        }

        private static IEnumerable<AMove> Get_Promotion_Moves(APosition from_pos, APosition to_pos)
        {
            yield return new AMove_Pawn_Promotion(from_pos, to_pos, EPiece_Type.Knight);
            yield return new AMove_Pawn_Promotion(from_pos, to_pos, EPiece_Type.Bishop);
            yield return new AMove_Pawn_Promotion(from_pos, to_pos, EPiece_Type.Rook);
            yield return new AMove_Pawn_Promotion(from_pos, to_pos, EPiece_Type.Queen);
        }

    }
}
