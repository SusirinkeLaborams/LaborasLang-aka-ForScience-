using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;


namespace Lexer
{
    [Serializable]
    [DataContract(IsReference = true)]
    
    public class AstNode
    {
        [DataMember]
        private List<AstNode> m_Children;
        [DataMember]
        private AstNode m_Parent;
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

        public AstNode Parent
        {
            get
            {
                return m_Parent;
            }
            internal set
            {
                m_Parent = value;
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
        public AstNode()
        {
            Parent = null;
            Content = null;
            Type = TokenType.Unknown;
        }

        internal void AddChild(AstNode child)
        {
            // PERF: Many AstNodes end up empty, so don't create children List until it's needed
            if (m_Children == null)
            {
                m_Children = new List<AstNode>();
            }

            child.Parent = this;
            Children.Add(child);
        }

        internal void AddTerminal(Token child)
        {
            AddChild(new AstNode(child, child.Type));
        }
    }
}
