using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Logic
{
    public abstract class APiece
    {
        public abstract APiece Copy();

        public abstract EPiece_Type Type { get; }
        public abstract EColor Color { get; }
        public bool Has_Moved { get; set; } = false;

    }
}
