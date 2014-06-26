using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Lexer
{
    [Serializable]
    public struct Location
    {
        public int Column { get; internal set; }
        public int Row { get; internal set; }

        public Location(int column, int row) : this()
        {
            Column = column;
            Row = row;
        }

        public static bool operator ==(Location a, Location b)
        {
            return a.Column == b.Column && a.Row == b.Row;
        }

        public static bool operator !=(Location a, Location b)
        {
            return !(a == b);
        }
    }
}
