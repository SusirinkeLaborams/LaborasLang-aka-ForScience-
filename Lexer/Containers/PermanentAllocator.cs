using Lexer.Utils;
using System;

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
            // Filling 18th container would mean having items in memory occupying over 4 GB RAM
            m_MemoryContainers = new MemoryContainer[18];
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
            private readonly byte* m_Memory;
            private readonly byte* m_End;
            private byte* m_Current;

            public MemoryContainer(int capacity)
            {
                do
                {
                    m_Current = m_Memory = (byte*)NativeFunctions.AllocateProcessMemory(capacity).ToPointer();
                    m_End = m_Current + capacity;
                }
                while (m_Memory == null && (capacity /= 2) > 64);

                if (m_Memory == null)
                {
                    throw new OutOfMemoryException();
                }
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
                NativeFunctions.FreeProcessMemory(new IntPtr(m_Memory));
            }
        }
    }
}
