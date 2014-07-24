using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Lexer
{


    [Serializable]
    public class Token
    {
        public Token()
        {
            Content = "";
            bool a = TokenType.Unknown.IsTerminal();
        }
        [DataMember]
        private TokenType m_Type;
        [DataMember]
        private string m_Content;
        [DataMember]
        private Location m_Start;
        [DataMember]
        private Location m_End;

        #region Public parameters
        public TokenType Type
        {
            get
            {
                return m_Type;
            }
            internal set
            {
                m_Type = value;
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
                return m_Start;
            }
            internal set
            {
                m_Start = value;
            }
        }
        public Location End
        {
            get
            {
                return m_End;
            }
            internal set
            {
                m_End = value;
            }
        }
        #endregion Public parameters

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
    }
}
