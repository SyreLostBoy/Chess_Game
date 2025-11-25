namespace Chess_Logic
{
    public class AMove_Double_Pawn : AMove
    {
        public override EMove_Type Move_Type => EMove_Type.Double_Pawn_Move;
        public override APosition From_Position { get; }
        public override APosition To_Position { get; }

        private readonly APosition Skipped_Position;

        public AMove_Double_Pawn(APosition from_pos, APosition to_pos)
        {
            From_Position = from_pos;
            To_Position = to_pos;
            Skipped_Position = new APosition( (from_pos.Row + to_pos.Row) / 2, from_pos.Column);
        }

        public override bool Act(ABoard board)
        {
            EColor player_color = board[From_Position].Color;

            board.Set_Pawn_Skip_Position(player_color, Skipped_Position);

            new AMove_Normal(From_Position, To_Position).Act(board);

            return true;
        }
    }
}
