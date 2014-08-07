using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lexer.Containers
{
    // The point of this class is to provide a fast, GC-free memory allocator
    // which would last for the duration of the root lexer object
    class PermanentAllocator
    {
        private const int kInitialCapacity = 10000;
        private MemoryContainer[] m_MemoryContainers;
        private int m_ContainerCount;

        public PermanentAllocator()
        {
            // Filling 15th container would mean having tokens occupy over 4 GB RAM at 20 bytes per item
            m_MemoryContainers = new MemoryContainer[15];
            m_MemoryContainers[0] = new MemoryContainer(kInitialCapacity);
            m_ContainerCount = 1;
        }

        private void EnsureMemoryIsAvailable(int size)
        {
            var currentContainer = m_MemoryContainers[m_ContainerCount - 1];

            if (!currentContainer.HasFreeSpace(size))
            {
                m_MemoryContainers[m_ContainerCount++] = new MemoryContainer(2 * currentContainer.Capacity);
            }
        }

        public unsafe byte* ProvideMemory(int size)
        {
            EnsureMemoryIsAvailable(size);
            return m_MemoryContainers[m_ContainerCount - 1].GetMemory(size);
        }

        public void Cleanup()
        {
            for (int i = 0; i < m_ContainerCount; i++)
            {
                m_MemoryContainers[i].Cleanup();
            }

            m_MemoryContainers = null;
        }

        private unsafe class MemoryContainer
        {
            private int m_Capacity;
            private int m_Index;
            private byte* m_Memory;

            public int Capacity { get { return m_Capacity; } }

            public MemoryContainer(int capacity)
            {
                m_Capacity = capacity;
                m_Memory = (byte*)Marshal.AllocHGlobal(capacity).ToPointer();
            }

            public bool HasFreeSpace(int size)
            {
                return m_Index + size < m_Capacity;
            }

            public byte* GetMemory(int size)
            {
                var ptr = m_Memory + m_Index;
                m_Index += size;
                return ptr;
            }

            public void Cleanup()
            {
                Marshal.FreeHGlobal(new IntPtr(m_Memory));
            }
        }
    }
}
