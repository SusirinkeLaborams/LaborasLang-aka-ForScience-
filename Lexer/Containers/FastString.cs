using Lexer.Utils;
using System;

namespace Lexer.Containers
{
    internal unsafe struct FastString
    {
        private static readonly char* kPointerToNullChar;
        private static readonly FastString kEmpty;

        private const int kSizeOfChar = sizeof(char);
        private const int kSizeOfIntOverChar = sizeof(int) / sizeof(char);
        private char* m_Buffer;

        internal static FastString Empty { get { return kEmpty; } }

        static FastString()
        {
            kPointerToNullChar = (char*)NativeFunctions.AllocateProcessMemory(1);
            *kPointerToNullChar = '\0';
            kEmpty.m_Buffer = kPointerToNullChar;
        }

        internal FastString(RootNode rootNode, char character)
        {
            m_Buffer = (char*)rootNode.Allocator.ProvideMemory(2 * kSizeOfChar);
            m_Buffer[0] = character;
            m_Buffer[1] = '\0';
        }

        internal FastString(RootNode rootNode, FastStringBuilder str)
        {
            var strLength = str.Length;
            m_Buffer = (char*)rootNode.Allocator.ProvideMemory((strLength + 2) * kSizeOfChar);

            var count = (strLength + 1) / kSizeOfIntOverChar;
            var src = (int*)str.Ptr;
            var dst = (int*)m_Buffer;

            for (int i = 0; i < count; i++)
            {
                dst[i] = src[i];
            }

            m_Buffer[str.Length] = '\0';
        }
        
        public static bool operator ==(FastString a, FastString b)
        {
            var aBuffer = a.m_Buffer;
            var bBuffer = b.m_Buffer;

            while (*aBuffer != '\0' || *bBuffer != '\0')
            {
                if (*aBuffer != *bBuffer)
                {
                    return false;
                }
                
                aBuffer++;
                bBuffer++;
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
            var ptr = m_Buffer;

            while (*ptr != '\0')
            {
                hash = (hash << 5) + hash + ((hash >> 8) ^ *ptr);
                ptr++;
            }

            return hash;
        }

        public override string ToString()
        {
            return new string(m_Buffer);
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
            m_Ptr = (char*)NativeFunctions.AllocateProcessMemory(kSizeOfChar * (m_Capacity + 1));
        }

        public FastStringBuilder(string str)
        {
            var strLength = str.Length;
            m_Capacity = m_Length = strLength;
            m_Ptr = (char*)NativeFunctions.AllocateProcessMemory(kSizeOfChar * (m_Capacity + 1));

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
            m_Ptr = (char*)NativeFunctions.AllocateProcessMemory(kSizeOfChar * (m_Capacity + 1));    // +2 part comes so FastString could copy 4 bytes at a time

            for (int i = 0; i < m_Length; i++)
            {
                m_Ptr[i] = oldPtr[i];
            }

            NativeFunctions.FreeProcessMemory((IntPtr)oldPtr);
        }
    }
}
