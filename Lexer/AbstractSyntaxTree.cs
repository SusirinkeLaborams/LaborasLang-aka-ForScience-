using Lexer.Containers;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexer
{
    internal class AbstractSyntaxTree : IAbstractSyntaxTree
    {
        public List<AbstractSyntaxTree> Children { get; set; }
        IReadOnlyList<IAbstractSyntaxTree> IAbstractSyntaxTree.Children { get { return Children; } }

        public Node Node { get; set; }
        #region Node shortcuts
        public TokenType Type
        {
            get
            {
                return Node.Type;
            }
            set
            {
                Node.Type = value;
            }
        }

        public string Content 
        {
            get
            {
                return Node.Content;
            }
            set
            {
                Node.Content = value;
            }
        }

        public string FullContent
        {
            get
            {
                if (Node.Type.IsTerminal())
                    return Content.ToString();

                StringBuilder builder = new StringBuilder();
                foreach(var node in Children)
                {
                    builder.Append(node.FullContent);
                }
                return builder.ToString();
            }
        }
        #endregion

        internal AbstractSyntaxTree(AstNode original)
        {
            Node = new Node(original.Token);
            Children = new List<AbstractSyntaxTree>(original.ChildrenCount);
            for (int i = 0; i < original.ChildrenCount; i++)
            {
                Children.Add(new AbstractSyntaxTree(original.Children[i]));
            }
        }

        internal AbstractSyntaxTree(Node node, List<AbstractSyntaxTree> children) 
        {
            Node = node;
            Children = children;
        }

        internal void ReplaceWith(AbstractSyntaxTree other)
        {
            this.Children = other.Children;
            this.Node = other.Node;

        }
        internal void Collapse()
        {
            Contract.Assume(Children.Count == 1);
            this.ReplaceWith(Children[0]);
        }

        private string ToString(int indentation)
        {
            var builder = new StringBuilder();
            var indentStr = new string('\t', indentation);

            builder.AppendFormat("{0}Content: {1}\r\n", indentStr, Node != null ? Node.ToString() : string.Empty);
            builder.AppendFormat("{0}Type: {1}\r\n", indentStr, Type);
            builder.AppendFormat("{0}Children:\r\n", indentStr);

            for (int i = 0; i < Children.Count - 1; i++)
            {
                builder.Append(Children[i].ToString(indentation + 1));
                builder.Append("\r\n");
            }

            if (Children.Count > 0)
            {
                builder.Append(Children[Children.Count - 1].ToString(indentation + 1));
            }

            return builder.ToString();
        }

        public override string ToString()
        {
            return ToString(0);
        }
    }
}
