using System;
using System.Collections;
using System.Collections.Generic;
namespace Lexer.Containers
{
    public unsafe struct AstNodeList : IEnumerable<AstNode>
    {
        private const int kInitialCapacity = 6;

        private AstNode* m_Nodes;
        private int m_Count;
        private int m_Capacity;
        
        public int Count { get { return m_Count; } }

        public AstNode this[int index] 
        {
            get
            {
#if DEBUG
                if (index < 0 || index >= m_Count)
                {
                    throw new IndexOutOfRangeException(String.Format("AstNodeList index out of range, index {0}, count {1}", index, m_Count));
                }
#endif
                return m_Nodes[index];
            } 
        }

        internal void Add(RootNode rootNode, AstNode child)
        {
            EnsureThereIsSpace(rootNode);
            
            m_Nodes[m_Count] = child;
            m_Count++;
        }

        private void EnsureThereIsSpace(RootNode rootNode)
        {
            if (m_Capacity == 0)
            {
                m_Capacity = kInitialCapacity;
                m_Nodes = (AstNode*)rootNode.NodePool.ProvideNodeArrayPtr(kInitialCapacity);
            }
            else if (m_Count == m_Capacity)
            {
                var oldNodes = m_Nodes;

                m_Capacity *= 2;
                m_Nodes = (AstNode*)rootNode.NodePool.ProvideNodeArrayPtr(m_Capacity);
                var dst = m_Nodes;

                for (int i = 0; i < m_Count; i++)
                {
                    *dst++ = *oldNodes++;
                }
            }
        }

        public void Initialize()
        {
            m_Count = 0;
            m_Capacity = 0;
        }

        public IEnumerator<AstNode> GetEnumerator()
        {
            return new AstNodeEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
