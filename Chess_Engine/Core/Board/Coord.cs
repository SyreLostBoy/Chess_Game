using Chess_Engine.Helpers;

namespace Chess_Engine.Core
{

    // Структура для представления клеток на шахматной доске в виде пар целых чисел вертикали/ранга.
    // (0, 0) = a1, (7, 7) = h8.
    // Координаты также могут использоваться в качестве смещений. Например, хотя координата (-1, 0) не является
    // допустимой клеткой, ее можно использовать для представления концепции перемещения на 1 клетку влево.
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
