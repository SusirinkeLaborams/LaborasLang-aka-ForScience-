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
            if (!m_CurrentContainer.HasFreeSpaceForOneNode())
            {
                AddContainer();
            }
        }

        private void EnsureMemoryIsAvailable(int byteCount)
        {
            if (!m_CurrentContainer.HasFreeSpace(byteCount))
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
        
        public unsafe AstNode.InternalNode** ProvideNodeArrayPtr(int count)
        {
            EnsureMemoryIsAvailable(count);
            return m_CurrentContainer.GetNodeArray(count);
        }

        public unsafe void FreeMemory(AstNode.InternalNode* ptr)
        {
            for (; ;)
            {
                if (m_CurrentContainer.IsMyPtr((byte*)ptr))
                {
                    m_CurrentContainer.ResetTo((byte*)ptr);
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
            private static readonly int kNodeSize = sizeof(Token.InternalToken);
            private static readonly int kNodePointerSize = sizeof(Token.InternalToken*);
            
            private byte* m_Nodes;
            private byte* m_NextNode;
            private byte* m_End;

            public AstNodeContainer(int capacity)
            {
                var byteCount = capacity * kNodeSize;
                var ptr = Marshal.AllocHGlobal(byteCount);

                m_NextNode = m_Nodes = (byte*)ptr;
                m_End = m_Nodes + capacity * kNodeSize;
            }

            public AstNode.InternalNode* GetNode()
            {
                var node = (AstNode.InternalNode*)m_NextNode;
                m_NextNode += kNodeSize;
                return node;
            }

            public AstNode.InternalNode** GetNodeArray(int count)
            {
                var array = (AstNode.InternalNode**)m_NextNode;
                m_NextNode += kNodePointerSize * count;
                return array;
            }

            public bool HasFreeSpace(int byteCount)
            {
                return m_NextNode + byteCount <= m_End;
            }

            public bool HasFreeSpaceForOneNode()
            {
                return m_NextNode < m_End;
            }

            public bool IsMyPtr(byte* ptr)
            {
                return !(ptr < m_Nodes || ptr >= m_End);
            }

            public void ResetToZero()
            {
                m_NextNode = m_Nodes;
            }

            public void ResetTo(byte* ptr)
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
