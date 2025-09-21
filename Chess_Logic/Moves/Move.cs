using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Logic
{
    public abstract class AMove
    {
        public abstract void Act(AsBoard board);

        public abstract EMove_Type Move_Type { get;  }
        public abstract APosition From_Position { get; }
        public abstract APosition To_Position { get; }

    }
}
