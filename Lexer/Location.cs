using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexer
{
    public struct Location
    {
        public int Column { get; internal set; }
        public int Row { get; internal set; }

        public Location(int column, int row) : this()
        {
            Column = column;
            Row = row;
        }
    }
}
