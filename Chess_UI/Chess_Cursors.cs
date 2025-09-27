using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Chess_UI
{
    public static class Chess_Cursors
    {
        private static Cursor Load_Cursor(string file_path)
        {
            Stream stream = Application.GetResourceStream(new Uri(file_path, UriKind.Relative)).Stream;

            return new Cursor(stream, true);
        }

        public static readonly Cursor White_Cursor = Load_Cursor("Assets/CursorW.cur");
        public static readonly Cursor Black_Cursor = Load_Cursor("Assets/CursorB.cur");
    }
}
