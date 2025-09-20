using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//------------------------------------------------------------------------------------------------------------
namespace Chess_Logic
{
    public class ADirection
    {
        public ADirection(int row_delta, int column_delta)
        {
            Row_Delta = row_delta;
            Column_Delta = column_delta;
        }

        public static ADirection operator +(ADirection dir_1, ADirection dir_2)
        {
            int row_delta = dir_1.Row_Delta + dir_2.Row_Delta;
            int column_delta = dir_1.Column_Delta + dir_2.Column_Delta;

            return new ADirection(row_delta, column_delta);
        }

        public static ADirection operator *(int scalar, ADirection dir)
        {
            return new ADirection(dir.Row_Delta * scalar, dir.Column_Delta * scalar);
        }

        public int Row_Delta { get; }
        public int Column_Delta { get; }

        // Basic Directions
        public readonly static ADirection North = new ADirection(-1, 0);
        public readonly static ADirection South = new ADirection(1, 0);
        public readonly static ADirection East = new ADirection(0, 1);
        public readonly static ADirection West = new ADirection(0, -1);

        // Diagonal Directions
        public readonly static ADirection North_East = North + East;
        public readonly static ADirection North_West = North + West;
        public readonly static ADirection South_East = South + East;
        public readonly static ADirection South_West = South + West;
    }
}
//------------------------------------------------------------------------------------------------------------