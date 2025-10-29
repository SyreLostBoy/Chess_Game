namespace Chess_Logic
{
    public class ARook : APiece
    {
        public override EPiece_Type Type => EPiece_Type.Rook;
        public override EColor Color { get; }

        private static readonly ADirection[] Directions = new ADirection[]
        {
           ADirection.North,
           ADirection.East,
           ADirection.West,
           ADirection.South
        };

        public ARook(EColor color, bool has_moved = false)
        {
            Color = color;
            Has_Moved = has_moved;
        }

        public override APiece Copy()
        {
            return new ARook(Color, Has_Moved);
        }

        public override IEnumerable<AMove> Get_Moves(APosition from_pos, ABoard board)
        {
            foreach (APosition to_pos in Get_Move_Positions_In_Directions(from_pos, board, Directions))
            {
                yield return new AMove_Normal(from_pos, to_pos);
            }
        }
    }
}
