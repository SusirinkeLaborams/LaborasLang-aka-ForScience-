using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;


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

        public Token Content
        {
            get { return m_NodePtr->content; }
            private set { m_NodePtr->content = value; }
        }

        public TokenType Type
        {
            get { return m_NodePtr->type; }
            internal set { m_NodePtr->type = value; }
        }

        #endregion

        internal AstNode(InternalNode* nodePtr, bool initialize = true)
        {
            m_NodePtr = nodePtr;
            m_NodePtr->children.Initialize();
        }

        private AstNode(RootNode rootNode, Token content, TokenType type)
            : this()
        {
            m_NodePtr = rootNode.NodePool.ProvideNodePtr();
            m_NodePtr->children.Initialize();
            Content = content;
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
            [DataMember]
            public AstNodeList children;
            [DataMember]
            public Token content;
            [DataMember]
            public TokenType type;
        }

        internal bool IsNull()
        {
            return m_NodePtr == null;
        }
    }
}
