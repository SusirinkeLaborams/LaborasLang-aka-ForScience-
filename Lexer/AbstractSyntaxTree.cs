using Lexer.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexer
{
    public class AbstractSyntaxTree
    {
        public List<AbstractSyntaxTree> Children { get; set; }
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
        public Location Start {
            get
            {
                return Node.Start;
            }
            set
            {
                Node.Start = value;
            }
        }
        public Location End
        {
            get
            {
                return Node.End;
            }
            set
            {
                Node.End = value;
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
