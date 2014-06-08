using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexer
{
    class Tokenizer
    {
        

        public IEnumerable<Token> Tokenize(string file)
        {
            SourceReader Source = new SourceReader(file);
            
            var Tokens = new Stack<Token>();
            Tokens.Push(new Token());
            Tokens.Peek().Type = Token.TokenType.StartOfFile;
            while (Source.Peek() != '\0')
            {
                switch (Source.Peek())
                {
                    case ' ':
                    case '\t':
                        {
                            Tokens.Peek().TrailingContent += Source.Pop();
                            break;
                        }
                    default:
                        {
                            throw new NotImplementedException();
                        }
                }
            }
            
            throw new NotImplementedException();
        }
    }
}
