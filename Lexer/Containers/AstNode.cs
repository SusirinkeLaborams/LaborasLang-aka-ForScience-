using System.Text;

namespace Lexer.Containers
{
    public unsafe struct AstNode
    {
        private InternalNode* m_NodePtr;

        #region Public parameters

        public AstNodeList Children
        {
            get { return m_NodePtr->children; }
        }

        public int ChildrenCount
        {
            get { return m_NodePtr->children.Count; }
        }

        public Token Token
        {
            get { return m_NodePtr->content; }
            internal set { m_NodePtr->content = value; }
        }

        public FastString Content
        {
            get { return Token.Content; }
        }

        public TokenType Type
        {
            get { return m_NodePtr->content.Type; }
            internal set { m_NodePtr->content.Type = value; }
        }

        #endregion

        internal AstNode(InternalNode* nodePtr)
        {
            m_NodePtr = nodePtr;
            m_NodePtr->children.Initialize();
        }

        private AstNode(RootNode rootNode, Token content, TokenType type)
            : this()
        {
            m_NodePtr = rootNode.NodePool.ProvideNodePtr();
            m_NodePtr->children.Initialize();
            Token = content;
            Type = type;
        }

        internal void AddChild(RootNode rootNode, AstNode child)
        {
            m_NodePtr->children.Add(rootNode, child);
        }

        internal void AddTerminal(RootNode rootNode, Token child)
        {
            AddChild(rootNode, new AstNode(rootNode, child, child.Type));
        }

        internal void Cleanup(RootNode rootNode)
        {
            rootNode.NodePool.FreeMemory(m_NodePtr);
        }

        internal struct InternalNode
        {
            public AstNodeList children;
            public Token content;
        }

        internal bool IsNull()
        {
            return m_NodePtr == null;
        }

        private string ToString(int indentation)
        {
            var builder = new StringBuilder();
            var indentStr = new string('\t', indentation);

            builder.AppendFormat("{0}Content: {1}\r\n", indentStr, Token != null ? Token.ToString() : string.Empty);
            builder.AppendFormat("{0}Type: {1}\r\n", indentStr, Type);
            builder.AppendFormat("{0}Children:\r\n", indentStr);

            for (int i = 0; i < ChildrenCount - 1; i++)
            {
                builder.Append(Children[i].ToString(indentation + 1));
                builder.Append("\r\n");
            }

            if (ChildrenCount > 0)
            {
                builder.Append(Children[ChildrenCount - 1].ToString(indentation + 1));
            }

            return builder.ToString();
        }

        public override string ToString()
        {
            return ToString(0);
        }
    }
}
