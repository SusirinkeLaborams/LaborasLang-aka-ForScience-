using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Diagnostics;

namespace Lexer.Containers
{
    public unsafe struct Token
    {
        private InternalToken* m_TokenPtr;

        internal Token(InternalToken* tokenPtr) : this()
        {
            m_TokenPtr = tokenPtr;
        }

        public TokenType Type
        {
            get
            {
                return m_TokenPtr->type;
            }
            internal set
            {
                m_TokenPtr->type = value;
            }
        }

        public FastString Content
        {
            get
            {
                return m_TokenPtr->content;
            }
            internal set
            {
                m_TokenPtr->content = value;
            }
        }

        public Location Start
        {
            get
            {
                return m_TokenPtr->start;
            }
            internal set
            {
                m_TokenPtr->start = value;
            }
        }

        public Location End
        {
            get
            {
                return m_TokenPtr->end;
            }
            internal set
            {
                m_TokenPtr->end = value;
            }
        }

        public static bool operator ==(Token a, Token b)
        {
            return a.Type == b.Type &&
                a.Content == b.Content &&
                a.Start == b.Start &&
                a.End == b.End;
        }

        public static bool operator !=(Token a, Token b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            return string.Format("Type: {0}, Start: {1}, End: {2}, Content: \"{3}\"", Type, Start, End, Content);
        }

        public override bool Equals(object obj)
        {
            if (obj is Token)
            {
                return this == (Token)obj;
            }

            return false;
        }

        public override int GetHashCode()
        {
            var typeHash = (int)m_TokenPtr->type;
            var contentHash = m_TokenPtr->content.GetHashCode();
            var startHash = m_TokenPtr->start.GetHashCode();
            var endHash = m_TokenPtr->end.GetHashCode();
            
            return (typeHash << 24) ^ (contentHash << 16) + (startHash ^ endHash);
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
            [DataMember]
            public FastString content;
        }
    }
}
