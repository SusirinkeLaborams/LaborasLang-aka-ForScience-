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
        private int m_Column;
        private int m_Row;
        #region Public properties
        public int Column
        {
            get
            {
                return m_Column;
            }
            internal set
            {
                m_Column = value;
            }
        }
        public int Row
        {
            get
            {
                return m_Row;
            }
            internal set
            {
                m_Row = value;
            }
        }
        #endregion Public properties

        public Location(int column, int row)
            : this()
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

        public override bool Equals(object obj)
        {
            if (obj is Location)
            {
                return this == (Location)obj;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return m_Row << 16 + m_Column;
        }

        public override string ToString()
        {
            return String.Format("{0}, {1}", Row, Column);
        }
    }
}
