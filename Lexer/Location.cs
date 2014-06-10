using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexer
{
    struct Location
    {
        public int Collumn { get; internal set; }
        public int Row { get; internal set; }

        public Location(int collumn, int row)
        {
            Collumn = collumn;
            Row = row;
        }
    }
}
