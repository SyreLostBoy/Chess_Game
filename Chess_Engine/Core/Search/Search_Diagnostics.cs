using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Engine.Core
{
    public struct SSearch_Diagnostics
    {
        public int Num_Completed_Iterations;
        public int Num_Positions_Evaluated;
        public ulong Num_Cut_Offs;

        public string Move_Val;
        public string Move;
        public int Eval;
        public bool Move_Is_From_Partial_Search;
        public int Num_Q_Checks;
        public int Num_Q_Mates;

        public bool Is_Book;

        public int Max_Extention_Reached_In_Search;
    }
}
