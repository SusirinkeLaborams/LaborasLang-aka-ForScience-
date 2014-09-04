using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexer.Containers
{
    public class AstNodeEnumerator : IEnumerator<AstNode>
    {
        private AstNodeList m_NodeList;
        private AstNode m_Current;
        private int m_Position;

        internal AstNodeEnumerator(AstNodeList list)
        {
            m_NodeList = list;
            m_Position = -1;
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public AstNode Current 
        { 
            get 
            { 
                if(IsValid())
                {
                    return m_Current;
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
            if(IsValid())
            {
                m_Current = m_NodeList[m_Position];
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsValid()
        {
            return m_Position >= 0 && m_Position < m_NodeList.Count;
        }

        public void Reset()
        {
            m_Position = -1;
        }

        public void Dispose() { /*nothing to dispose*/}
    }
}
