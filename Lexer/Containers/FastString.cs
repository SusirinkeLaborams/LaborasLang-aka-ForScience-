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
        private const int charSize = sizeof(char);
        private char* buffer;

        public FastString(RootNode rootNode, char character) : this()
        {
            buffer = (char*)rootNode.Allocator.ProvideMemory(2 * charSize);
            buffer[0] = character;
            buffer[1] = '\0';
        }

        public FastString(RootNode rootNode, StringBuilder str) : this()
        {
            buffer = (char*)rootNode.Allocator.ProvideMemory((str.Length + 1) * charSize);

            for (int i = 0; i < str.Length; i++)
            {
                buffer[i] = str[i];
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
}
