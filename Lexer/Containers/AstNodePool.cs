using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lexer.Containers
{
    // This class provides a fast, GC-free LIFO style allocator that
    // is designed for rapid creation and destruction of AstNode structs in LIFO manner
    // The lexer itself allocates root node, then tries to figure out its children,
    // and if it fails, discarting everything up to the root node works as all children are allocated
    // after the root node.
    internal sealed class AstNodePool
    {
        private const int kInitialCapacity = 10000;
        private AstNodeContainer[] m_AstNodeContainers;
        private AstNodeContainer m_CurrentContainer; // PERF: Cache current container

        private int m_ContainerCount;
        private int m_CurrentContainerIndex;

        private int m_AllocationCapacity = kInitialCapacity;

        public AstNodePool()
        {
            // Filling 21st container would mean having tokens occupy 4 GB RAM
            m_AstNodeContainers = new AstNodeContainer[15];
            
            m_AstNodeContainers[0] = m_CurrentContainer = new AstNodeContainer(m_AllocationCapacity);

            m_CurrentContainerIndex = 0;
            m_ContainerCount = 1;
        }

        private void AddContainer()
        {
            m_CurrentContainerIndex++;

            if (m_ContainerCount == m_CurrentContainerIndex)
            {
                m_AllocationCapacity *= 2;
                m_AstNodeContainers[m_ContainerCount++] = m_CurrentContainer = new AstNodeContainer(m_AllocationCapacity);
            }
            else
            {
                m_CurrentContainer = m_AstNodeContainers[m_CurrentContainerIndex];
            }
        }

        private void EnsureOneNodeIsAvailable()
        {
            if (!m_CurrentContainer.HasFreeSpaceForOne())
            {
                AddContainer();
            }
        }

        private void EnsureNodeIsAvailable(int count)
        {
            if (!m_CurrentContainer.HasFreeSpace(count))
            {
                AddContainer();
            }
        }
        
        public unsafe AstNode.InternalNode* ProvideNodePtr()
        {
            EnsureOneNodeIsAvailable();
            return m_CurrentContainer.GetNode();
        }

        public unsafe AstNode ProvideNode()
        {
            return new AstNode(ProvideNodePtr());
        }
        
        public unsafe AstNode.InternalNode* ProvideMemory(int count)
        {
            EnsureNodeIsAvailable(count);
            return m_CurrentContainer.GetNode();
        }

        public unsafe void FreeMemory(AstNode.InternalNode* ptr)
        {
            for (; ;)
            {
                if (m_CurrentContainer.IsMyPtr(ptr))
                {
                    m_CurrentContainer.ResetTo(ptr);
                    return;
                }
                else
                {
                    m_CurrentContainer.ResetToZero();
                    m_CurrentContainerIndex--;
                    m_CurrentContainer = m_AstNodeContainers[m_CurrentContainerIndex];
                }
            }
        }

        public void Cleanup()
        {
            for (int i = 0; i < m_ContainerCount; i++)
            {
                m_AstNodeContainers[i].Cleanup();
            }

            m_AstNodeContainers = null;
        }

        private unsafe struct AstNodeContainer
        {
            private static readonly int TokenSize = sizeof(Token.InternalToken);
            
            private AstNode.InternalNode* m_Nodes;
            private AstNode.InternalNode* m_NextNode;
            private AstNode.InternalNode* m_End;

            public AstNodeContainer(int capacity)
            {
                var byteCount = capacity * TokenSize;
                var ptr = Marshal.AllocHGlobal(byteCount);

                m_NextNode = m_Nodes = (AstNode.InternalNode*)ptr;
                m_End = m_Nodes + capacity;
            }

            public AstNode.InternalNode* GetNode()
            {
                return m_NextNode++;
            }

            public AstNode.InternalNode* GetNodes(int count)
            {
                var node = m_NextNode;
                m_NextNode += count;
                return node;
            }

            public bool HasFreeSpace(int count)
            {
                return m_NextNode + count <= m_End;
            }

            public bool HasFreeSpaceForOne()
            {
                return m_NextNode < m_End;
            }

            public bool IsMyPtr(AstNode.InternalNode* ptr)
            {
                return !(ptr < m_Nodes || ptr >= m_End);
            }

            public void ResetToZero()
            {
                m_NextNode = m_Nodes;
            }

            public void ResetTo(AstNode.InternalNode* ptr)
            {
                m_NextNode = ptr;
            }

            public void Cleanup()
            {
                Marshal.FreeHGlobal(new IntPtr(m_Nodes));
            }
        }
    }
}
