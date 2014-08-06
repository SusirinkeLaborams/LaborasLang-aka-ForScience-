using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lexer
{
    unsafe class TokenPool
    {
        private static GCHandle m_GCHandle;
        private static Token.InternalToken* m_Ptr;

        private static Token.InternalToken[] m_Tokens;
        private static int m_FirstUnused;

        private const int InitialCapacity = 10000;

        static TokenPool()
        {
            Resize(InitialCapacity);
        }
        
        private static void EnsureTokenIsAvailable()
        {
            if (m_FirstUnused == m_Tokens.Length)
            {
                Resize(2 * m_Tokens.Length);
            }
        }

        private static void Resize(int newSize)
        {
            var oldTokens = m_Tokens;
            m_Tokens = new Token.InternalToken[newSize];

            if (oldTokens != null)
            {
                Array.Copy(oldTokens, m_Tokens, oldTokens.Length);
            }

            if (m_GCHandle.IsAllocated)
            {
                m_GCHandle.Free();
            }

            m_GCHandle = GCHandle.Alloc(m_Tokens, GCHandleType.Pinned);
            m_Ptr = (Token.InternalToken*)m_GCHandle.AddrOfPinnedObject().ToPointer();
        }

        public static Token ProvideToken()
        {
            EnsureTokenIsAvailable();

            var token = new Token(m_FirstUnused);
            m_FirstUnused++;
            return token;
        }

        public static Token.InternalToken* Ptr { get { return m_Ptr; } }
    }
}
