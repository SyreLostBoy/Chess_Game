using Chess_Engine.Helpers;

namespace Chess_Engine.Core
{

    // Structure for representing squares on the chess board as file/rank integer pairs.
    // (0, 0) = a1, (7, 7) = h8.
    // Coords can also be used as offsets. For example, while a Coord of (-1, 0) is not
    // a valid square, it can be used to represent the concept of moving 1 square left.
    public struct SCoordinate : IComparable<SCoordinate>
    {
        public readonly int File_Index;
        public readonly int Rank_Index;
        public int Square_Index => AsBoard_Helper.Index_From_Coord(this);

        public SCoordinate(int file_index, int rank_index)
        {
            File_Index = file_index;
            Rank_Index = rank_index;
        }

        public SCoordinate(int square_index) 
        {
            File_Index = AsBoard_Helper.File_Index(square_index);
            Rank_Index = AsBoard_Helper.Rank_Index(square_index);
        }

        public bool Is_Light_Square()
        {
            return (File_Index + Rank_Index) % 2 == 0;
        }

        public bool Is_Valid_Square()
        {
            return File_Index >= 0 && File_Index < 8 && Rank_Index >= 0 && Rank_Index < 8;
        }

        public int CompareTo(SCoordinate other)
        {
            return (File_Index == other.File_Index && Rank_Index == other.Rank_Index) ? 0 : 1;
        }

        public static SCoordinate operator +(SCoordinate a, SCoordinate b)
        {
            return new SCoordinate(a.File_Index + b.File_Index, a.Rank_Index + b.Rank_Index);
        }

        public static SCoordinate operator -(SCoordinate a, SCoordinate b)
        {
            return new SCoordinate(a.File_Index - b.File_Index, a.Rank_Index - b.Rank_Index);
        }

        public static SCoordinate operator *(SCoordinate a, int scalar)
        {
            return new SCoordinate(a.File_Index * scalar, a.Rank_Index * scalar);
        }

        public static SCoordinate operator *(int scalar, SCoordinate a)
        {
            return a * scalar;
        }
    }
}
