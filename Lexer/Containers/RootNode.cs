using System;

namespace Lexer.Containers
{
    public unsafe sealed class RootNode : IDisposable
    {
        private static readonly int kTokenSize = sizeof(Token.InternalToken);
        private AstNode m_Node;
        private bool m_Disposed = false;

        internal PermanentAllocator Allocator { get; private set; }
        internal AstNodePool NodePool { get; private set; }
        public AstNode Node { get { return m_Node; } }
        
        internal RootNode()
        {
            Allocator = new PermanentAllocator();
            NodePool = new AstNodePool();
        }

        internal unsafe Token ProvideToken()
        {
            return new Token((Token.InternalToken*)Allocator.ProvideMemory(kTokenSize));
        }

        internal void SetNode(AstNode node)
        {
            m_Node = node;
        }

        public void Dispose()
        {
            if (!m_Disposed)
            {
                GC.SuppressFinalize(this);
                m_Disposed = true;
                Allocator.Cleanup();
                NodePool.Cleanup();

                Allocator = null;
                NodePool = null;
            }
        }

        ~RootNode()
        {
            Dispose();
        }
    }
}
