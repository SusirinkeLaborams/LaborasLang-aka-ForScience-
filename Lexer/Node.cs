using Lexer.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexer
{
    public class Node
    {
        public TokenType Type { get; set; }
        public Location Start { get; set; }
        public Location End { get; set; }
        public string Content { get; set; }

        public Node()
        {
        }

        internal Node(Token token)
        {
            this.Type = token.Type;
            this.Start = token.Start;
            this.End = token.End;
            this.Content = token.Content.ToString();
        }

        public Node(TokenType tokenType)
        {
            this.Type = tokenType;
        }

        public override string ToString()
        {
            return string.Format("Type: {0}, Start: {1}, End: {2}, Content: \"{3}\"", Type, Start, End, Content);
        }
    }
}
