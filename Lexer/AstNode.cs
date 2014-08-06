using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;


namespace Lexer
{
    [Serializable]
    [DataContract(IsReference = false)]
    
    public struct AstNode
    {
        [DataMember]
        private List<AstNode> m_Children;
        [DataMember]
        private Token m_Content;
        [DataMember]
        private TokenType m_Type;

#region Public parameters
        public List<AstNode> Children
        { 
            get
            {
                return m_Children;
            }
            internal set
            {
                m_Children = value;
            }
        }

        public Token Content
        {
            get
            {
                return m_Content;
            }
            internal set
            {
                m_Content = value;
            }
        }

        public TokenType Type
        {
            get
            {
                return m_Type;
            }
            internal set
            {
                m_Type = value;
            }
        }
#endregion

        public AstNode(Token content, TokenType type) : this()
        {
            Content = content;
            Type = type;
        }

        internal void AddChild(AstNode child)
        {
            // PERF: Many AstNodes end up empty, so don't create children List until it's needed
            if (m_Children == null)
            {
                m_Children = new List<AstNode>(3);
            }

            Children.Add(child);
        }

        internal void AddTerminal(Token child)
        {
            AddChild(new AstNode(child, child.Type));
        }
    }
}
