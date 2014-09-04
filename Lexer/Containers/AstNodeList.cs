using System;
using System.Collections;
using System.Collections.Generic;
namespace Lexer.Containers
{
    public unsafe struct AstNodeList
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
                m_Capacity = -1;
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
#if DEBUG
            if (m_Capacity == -1)
            {
                throw new NotSupportedException("Can't add to list after accessing it via indexer!");
            }
#endif

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

        public AstNodeEnumerator GetEnumerator()
        {
            return new AstNodeEnumerator(this);
        }

        public void Initialize()
        {
            m_Count = 0;
            m_Capacity = 0;
        }
    }
}
