using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexer.Containers
{
    public unsafe class RootNode : IDisposable
    {
        private static readonly int kTokenSize = sizeof(Token.InternalToken);
        private AstNode m_Node;
        private bool m_Disposed = true;

        internal PermanentAllocator Allocator { get; private set; }
        internal AstNodePool NodePool { get; private set; }

        internal unsafe Token ProvideToken()
        {
            return new Token((Token.InternalToken*)Allocator.ProvideMemory(kTokenSize));
        }

        internal RootNode()
        {
            Allocator = new PermanentAllocator();
            NodePool = new AstNodePool();
        }

        public AstNode Node { get { return m_Node; } }

        internal void SetNode(AstNode node)
        {
            m_Node = node;
        }
        
        public void Dispose()
        {
            m_Disposed = true;
            Allocator.Cleanup();
            NodePool.Cleanup();

            Allocator = null;
            NodePool = null;
        }

        ~RootNode()
        {
            if (!m_Disposed)
            {
                Dispose();
            }
        }
    }
}
