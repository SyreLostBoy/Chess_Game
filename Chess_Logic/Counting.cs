using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Logic
{
    public class ACounting
    {
        public int Total_Count { get; private set; }

        private readonly Dictionary<EPiece_Type, int> White_Count = new Dictionary<EPiece_Type, int>();
        private readonly Dictionary<EPiece_Type, int> Black_Count = new Dictionary<EPiece_Type, int>();

        public ACounting()
        {
            foreach (EPiece_Type piece_type in Enum.GetValues(typeof(EPiece_Type)))
            {
                White_Count[piece_type] = 0;
                Black_Count[piece_type] = 0;
            }
        }

        public void Increment(EColor player_color, EPiece_Type piece_type)
        {
            if (player_color == EColor.White)
            {
                White_Count[piece_type]++;
            }
            else if (player_color == EColor.Black)
            {
                Black_Count[piece_type]++;
            }

            Total_Count++;
        }

        public int Get_White_Count(EPiece_Type piece_type)
        {
            return White_Count[piece_type];
        }

        public int Get_Black_Count(EPiece_Type piece_type)
        {
            return Black_Count[piece_type];
        }
    }
}
