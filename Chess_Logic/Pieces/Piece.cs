using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Logic
{
    public abstract class APiece
    {
        public abstract APiece Copy();
        public abstract IEnumerable<AMove> Get_Moves(APosition from_pos, AsBoard board);
        public abstract EPiece_Type Type { get; }
        public abstract EColor Color { get; }
        public bool Has_Moved { get; set; } = false;
        protected IEnumerable<APosition> Get_Move_Positions_In_Direction(APosition from, AsBoard board, ADirection direction)
        {// Возвращает список позиций для хода в заданном направлении

            APiece piece;

            for (APosition pos = from + direction; AsBoard.Is_Inside_Board(pos); pos += direction)
            {
                if (board.Is_Empty(pos) )
                {
                    yield return pos; // Позиция доступна
                    continue;
                }

                piece = board[pos];

                if (piece.Color != this.Color)
                {
                    yield return pos; // Позиция доступна, на ней вражеская фигура
                }

                yield break; // Доступные позиции закончились
            }
        }
        
        protected IEnumerable<APosition> Get_Move_Positions_In_Directions(APosition from, AsBoard board, ADirection[] directions)
        {// Возвращает список всевозможных позиций для хода

            return directions.SelectMany(direction => Get_Move_Positions_In_Direction(from, board, direction) );
        }
    }
}
