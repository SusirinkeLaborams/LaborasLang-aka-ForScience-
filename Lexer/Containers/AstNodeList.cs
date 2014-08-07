using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexer.Containers
{
    public unsafe struct AstNodeList
    {
        private const int kInitialCapacity = 12; // Must be multiple of 4

        private AstNode.InternalNode* m_Nodes;
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
                return new AstNode(m_Nodes + index, false); 
            } 
        }

        internal void Add(RootNode rootNode, AstNode child)
        {
            EnsureThereIsSpace(rootNode);

            var dst = m_Nodes + m_Count;
            dst->children = child.Children;
            dst->content = child.Content;
            dst->type = child.Type;

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
                m_Nodes = rootNode.NodePool.ProvideMemory(kInitialCapacity);
            }
            else if (m_Count == m_Capacity)
            {
                var oldNodes = m_Nodes;

                m_Capacity *= 2;
                m_Nodes = rootNode.NodePool.ProvideMemory(m_Capacity);

                int* src = (int*)oldNodes;
                int* dst = (int*)m_Nodes;
                int count = m_Count * sizeof(AstNode.InternalNode) / 4;

                for (int i = 0; i < count; i++)
                {
                    *dst++ = *src++;
                }
            }
        }

        public void Initialize()
        {
            m_Count = 0;
            m_Capacity = 0;
        }
    }
}
