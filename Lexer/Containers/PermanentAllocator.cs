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
    internal sealed class PermanentAllocator
    {
        private const int kInitialCapacity = 10000;
        private MemoryContainer[] m_MemoryContainers;
        private MemoryContainer m_CurrentContainer; // PERF: Cache current container

        private int m_ContainerCount;
        private int m_AllocationCapacity = kInitialCapacity;

        public PermanentAllocator()
        {
            // Filling 15th container would mean having tokens occupy over 4 GB RAM at 20 bytes per item
            m_MemoryContainers = new MemoryContainer[15];
            m_MemoryContainers[0] = m_CurrentContainer = new MemoryContainer(m_AllocationCapacity);
            m_ContainerCount = 1;
        }

        private void EnsureMemoryIsAvailable(int size)
        {
            if (!m_CurrentContainer.HasFreeSpace(size))
            {
                m_AllocationCapacity *= 2;
                m_MemoryContainers[m_ContainerCount++] = m_CurrentContainer = new MemoryContainer(m_AllocationCapacity);
            }
        }

        public unsafe byte* ProvideMemory(int size)
        {
            EnsureMemoryIsAvailable(size);
            return m_CurrentContainer.GetMemory(size);
        }

        public void Cleanup()
        {
            for (int i = 0; i < m_ContainerCount; i++)
            {
                m_MemoryContainers[i].Cleanup();
            }

            m_MemoryContainers = null;
        }

        private unsafe struct MemoryContainer
        {
            private byte* m_Memory;
            private byte* m_Current;
            private byte* m_End;

            public MemoryContainer(int capacity)
            {
                m_Current = m_Memory = (byte*)Marshal.AllocHGlobal(capacity).ToPointer();
                m_End = m_Current + capacity;
            }

            public bool HasFreeSpace(int size)
            {
                return m_Current + size < m_End;
            }

            public byte* GetMemory(int size)
            {
                var ptr = m_Current;
                m_Current += size;
                return ptr;
            }

            public void Cleanup()
            {
                Marshal.FreeHGlobal(new IntPtr(m_Memory));
            }
        }
    }
}
