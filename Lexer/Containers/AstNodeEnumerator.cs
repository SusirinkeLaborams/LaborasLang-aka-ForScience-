using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexer.Containers
{
    public struct AstNodeEnumerator : IEnumerator<AstNode>
    {
        private AstNodeList m_NodeList;
        private int m_Position;

        internal AstNodeEnumerator(AstNodeList list)
        {
            m_NodeList = list;
            m_Position = -1;
        }

        public AstNode Current 
        { 
            get 
            { 
                if(IsValid())
                {
                    return m_NodeList[m_Position];
                }
                else
                {
                    throw new InvalidOperationException("AstNodeEnumerator is invalid");
                }
            } 
        }

        public bool MoveNext()
        {
            m_Position++;
            return IsValid();
        }

        private bool IsValid()
        {
            return m_Position >= 0 && m_Position < m_NodeList.Count;
        }

        public void Reset()
        {
            m_Position = -1;
        }

        public void Dispose()
        {
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }
    }
}
