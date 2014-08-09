using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lexer.Containers
{
    public unsafe struct FastString
    {
        private const int kSizeOfChar = sizeof(char);
        private const int kSizeOfIntOverChar = sizeof(int) / sizeof(char);
        private char* buffer;

        internal void Set(RootNode rootNode, char character)
        {
            buffer = (char*)rootNode.Allocator.ProvideMemory(2 * kSizeOfChar);
            buffer[0] = character;
            buffer[1] = '\0';
        }

        internal void Set(RootNode rootNode, FastStringBuilder str)
        {
            var strLength = str.Length;
            buffer = (char*)rootNode.Allocator.ProvideMemory((strLength + 2) * kSizeOfChar);

            var count = (strLength + 1) / kSizeOfIntOverChar;
            var src = (int*)str.Ptr;
            var dst = (int*)buffer;

            for (int i = 0; i < count; i++)
            {
                dst[i] = src[i];
            }

            buffer[str.Length] = '\0';
        }
        
        public static bool operator ==(FastString a, FastString b)
        {
            var aBuffer = a.buffer;
            var bBuffer = b.buffer;

            while (*aBuffer != '\0' || *bBuffer != '\0')
            {
                if (*aBuffer != *bBuffer)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool operator !=(FastString a, FastString b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (obj is FastString)
            {
                return this == (FastString)obj;
            }

            return false;
        }

        public override int GetHashCode()
        {
            int hash = 0;
            var ptr = buffer;

            while (*ptr != '\0')
            {
                hash = hash << 5 + hash + ((hash >> 8) ^ *ptr);
            }

            return hash;
        }
    }
    
    internal unsafe class FastStringBuilder
    {
        private const int kSizeOfChar = sizeof(char);
        private const int kInitialCapacity = 20;
        private char* m_Ptr;
        private int m_Length;
        private int m_Capacity;

        public char* Ptr { get { return m_Ptr; } }
        public int Length { get { return m_Length; } }

        public FastStringBuilder()
        {
            m_Capacity = kInitialCapacity;
            m_Ptr = (char*)Marshal.AllocHGlobal(kSizeOfChar * (m_Capacity + 1));
        }

        public FastStringBuilder(string str)
        {
            var strLength = str.Length;
            m_Capacity = m_Length = strLength;
            m_Ptr = (char*)Marshal.AllocHGlobal(kSizeOfChar * (m_Capacity + 1));

            fixed (char* src = str)
            {
                for (int i = 0; i < strLength; i++)
                {
                    m_Ptr[i] = src[i];
                }
            }
        }
        
        public void Append(char c)
        {
            if (m_Length == m_Capacity)
            {
                Grow();
            }

            m_Ptr[m_Length++] = c;
        }

        public void Append(string str)
        {
            if (m_Length + str.Length > m_Capacity)
            {
                Grow(m_Length + str.Length);
            }

            fixed (char* strPtr = str)
            {
                for (int i = 0; i < str.Length; i++)
                {
                    m_Ptr[m_Length++] = strPtr[i];
                }
            }
        }

        public void Clear()
        {
            m_Length = 0;
        }

        public override string ToString()
        {
            throw new NotSupportedException();
        }

        public override int GetHashCode()
        {            
            int hash = 0;

            for (int i = 0; i < m_Length; i++)
            {
                hash = (hash << kSizeOfChar) + (hash ^ m_Ptr[i]);
            }

            return hash;
        }
       
        public override bool Equals(object obj)
        {
            var other = obj as FastStringBuilder;

            if (other != null)
            {
                if (other.m_Length != m_Length)
                {
                    return false;
                }

                for (int i = 0; i < m_Length; i++)
                {
                    if (m_Ptr[i] != other.m_Ptr[i])
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        public static explicit operator FastStringBuilder(string str)
        {
            return new FastStringBuilder(str);
        }

        private void Grow()
        {
            m_Capacity *= 2;
            Reallocate();
        }

        private void Grow(int targetSize)
        {
            do
            {
                m_Capacity *= 2;
            }
            while (targetSize > m_Capacity);

            Reallocate();
        }

        private void Reallocate()
        {
            var oldPtr = m_Ptr;
            m_Ptr = (char*)Marshal.AllocHGlobal(kSizeOfChar * (m_Capacity + 1));    // +2 part comes so FastString could copy 4 bytes at a time

            for (int i = 0; i < m_Length; i++)
            {
                m_Ptr[i] = oldPtr[i];
            }

            Marshal.FreeHGlobal((IntPtr)oldPtr);
        }
    }
}
