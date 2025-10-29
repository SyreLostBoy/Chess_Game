//------------------------------------------------------------------------------------------------------------
namespace Chess_Logic
{
    public class APosition
    {
        public int Row { get; }
        public int Column { get; }

        public APosition(int row, int column)
        {
            Row = row;
            Column = column;
        }

        public EColor Get_Square_Color() 
        {
            if ((Row + Column) % 2 == 0)
            {
                return EColor.White;
            }

            return EColor.Black;
        }

        public override bool Equals(object obj)
        {
            return obj is APosition position && Row == position.Row && Column == position.Column;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Row, Column);
        }

        public static bool operator ==(APosition left, APosition right)
        {
            return EqualityComparer<APosition>.Default.Equals(left, right);
        }

        public static bool operator !=(APosition left, APosition right)
        {
            return !(left == right);
        }

        public static APosition operator +(APosition pos, ADirection dir)
        {
            return new APosition(pos.Row + dir.Row_Delta, pos.Column + dir.Column_Delta);
        }
    }
}
//------------------------------------------------------------------------------------------------------------