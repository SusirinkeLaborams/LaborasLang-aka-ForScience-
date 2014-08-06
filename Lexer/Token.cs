using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Diagnostics;

namespace Lexer
{
    public unsafe struct Token
    {
        private int m_Index;
        private string m_Content;

        internal Token(int index) : this()
        {
            m_Index = index;
        }

        public TokenType Type
        {
            get
            {
                return (TokenPool.Ptr + m_Index)->type;
            }
            internal set
            {
                (TokenPool.Ptr + m_Index)->type = value;
            }
        }

        public string Content
        {
            get
            {
                return m_Content;
            }
            internal set
            {
                m_Content = value;
            }
        }

        public Location Start
        {
            get
            {
                return (TokenPool.Ptr + m_Index)->start;
            }
            internal set
            {
                (TokenPool.Ptr + m_Index)->start = value;
            }
        }

        public Location End
        {
            get
            {
                return (TokenPool.Ptr + m_Index)->end;
            }
            internal set
            {
                (TokenPool.Ptr + m_Index)->end = value;
            }
        }

        public static bool operator ==(Token a, Token b)
        {
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            return a.Type == b.Type &&
                a.Content == b.Content &&
                a.Start == b.Start &&
                a.End == b.End;
        }

        public static bool operator !=(Token a, Token b)
        {
            return !(a == b);
        }

        public bool Equals(Token other)
        {
            return this == other;
        }

        [Serializable, DebuggerDisplay("Token, type = {m_Type}")]
        internal struct InternalToken
        {
            [DataMember]
            public TokenType type;
            [DataMember]
            public Location start;
            [DataMember]
            public Location end;
        }
    }
}
