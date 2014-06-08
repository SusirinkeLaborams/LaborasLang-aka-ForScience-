using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexer
{
    public class Token
    {
        public enum TokenType
        {
            StartOfFile,
            EndOfLine,            
            EndOfFile,
            Comment,
    
            Dot,
            Plus,
            PlusPlus,
            Minus,
            MinusMinus,
            Whitespace,
            

        }

        public TokenType Type { get; internal set; }
        public string Content { get; internal set; }
        public string TrailingContent { get; internal set; }
    }
}
