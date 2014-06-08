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
            Tokens.Peek().Type = TokenType.StartOfFile;
            while (Source.Peek() != '\0')
            {
                switch (Source.Peek())
                {
                    #region Whitespace
                    case ' ':
                    case '\t':
                        {
                            // Whitespaces are just appended to last token, they don't do anything significant
                            Tokens.Peek().TrailingContent += Source.Pop();
                            break;
                        }
                    #endregion
                    #region StringLiteral
                    case '\'':
                        {
                            // String literal, scan to next ' that is not going after a \
                            var token = new Token();
                            token.Type = TokenType.StringLiteral;
                            while(Source.Peek() != '\'')
                            {                        
                                if (Source.Peek() == '\\')
                                {
                                    token.Content += Source.Pop();
                                }
                                token.Content += Source.Pop();
                            }
                            Tokens.Push(token);
                            break;
                        }
                    case '"':
                        {
                            // Single quote string, scan to next " that is not going after a \
                            var token = new Token();
                            token.Type = TokenType.StringLiteral;
                            while (Source.Peek() != '"')
                            {
                                if (Source.Peek() == '\\')
                                {
                                    token.Content += Source.Pop();
                                }
                                token.Content += Source.Pop();
                            }
                            Tokens.Push(token);
                            break;
                        }
                    #endregion
                    #region Plus
                    case '+':
                        {
                            // ++ += +
                            var token = new Token();
                            token.Content += Source.Pop();
                            switch (Source.Peek())
                            {
                                case '+':
                                    {
                                        token.Content += Source.Pop();
                                        token.Type = TokenType.PlusPlus;
                                        break;
                                    }
                                case '=':
                                    {
                                        token.Content += Source.Pop();
                                        token.Type = TokenType.PlusEqual;
                                        break;
                                    }
                                default:
                                    {
                                        token.Type = TokenType.Plus;
                                        break;
                                    }
                            }
                            Tokens.Push(token);
                            break;
                        }
                    #endregion
                    #region Minus
                    case '-':
                        {
                            // -- -= -
                            var token = new Token();
                            token.Content += Source.Pop();
                            switch (Source.Peek())
                            {
                                case '-':
                                    {
                                        token.Content += Source.Pop();
                                        token.Type = TokenType.MinusMinus;
                                        break;
                                    }
                                case '=':
                                    {
                                        token.Content += Source.Pop();
                                        token.Type = TokenType.MinusEqual;
                                        break;
                                    }
                                default:
                                    {
                                        token.Type = TokenType.Minus;
                                        break;
                                    }
                            }
                            Tokens.Push(token);
                            break;
                        }
                    #endregion
                    #region Not
                    case '!':
                        {
                            // ! !=
                            var token = new Token();
                            token.Content += Source.Pop();
                            switch (Source.Peek())
                            {
                                case '=':
                                    {
                                        token.Content += Source.Pop();
                                        token.Type = TokenType.NotEqual;
                                        break;
                                    }
                                default:
                                    {
                                        token.Type = TokenType.Not;
                                        break;
                                    }
                            }
                            Tokens.Push(token);
                            break;
                        }
                    #endregion
                    #region BitwiseComplement
                    case '~':
                        {
                            // ~ ~=
                            var token = new Token();
                            token.Content += Source.Pop();
                            switch (Source.Peek())
                            {
                                case '=':
                                    {
                                        token.Content += Source.Pop();
                                        token.Type = TokenType.BitwiseComplementEqual;
                                        break;
                                    }
                                default:
                                    {
                                        token.Type = TokenType.BitwiseComplement;
                                        break;
                                    }
                            }
                            Tokens.Push(token);
                            break;
                        }
                    #endregion
                    #region And
                    case '&':
                        {
                            // & && &= &&=
                            throw new NotImplementedException();
                            break;
                        }
                    #endregion
                    #region XOR
                    case '^':
                        {
                            // ^ ^=
                            throw new NotImplementedException();
                            break;
                        }
                    #endregion
                    #region Or
                    case '|':
                        {
                            // | |= || ||=
                            throw new NotImplementedException();
                            break;
                        }
                    #endregion
                    #region LessThan
                    case '<':
                        {
                            // < <= <<
                            throw new NotImplementedException();
                            break;
                        }
                    #endregion
                    #region MoreThan
                    case '>':
                        {
                            // > >= >>
                            throw new NotImplementedException();
                            break;
                        }
                    #endregion
                    #region Division
                    case '/':
                        {
                            // // / /= /* ... */
                            throw new NotImplementedException();
                            break;
                        }
                    #endregion
                    #region Multiplication
                    case '*':
                        {
                            // * *=
                            throw new NotImplementedException();
                            break;
                        }
                    #endregion
                    #region Remainder
                    case '%':
                        {
                            // % %=
                            throw new NotImplementedException();
                            break;
                        }
                    #endregion
                    #region Equal
                    case '=':
                        {
                            // == =
                            throw new NotImplementedException();
                            break;
                        }
                    #endregion
                    #region LeftCurlyBracket
                    case '{':
                        {
                            // {
                            throw new NotImplementedException();
                            break;
                        }
                    #endregion
                    #region RightCurlyBracket
                    case '}':
                        {
                            // }
                            throw new NotImplementedException();
                            break;
                        }
                    #endregion
                    #region LeftBracket
                    case '(':
                        {
                            // (
                            throw new NotImplementedException();
                            break;
                        }
                    #endregion
                    #region RightBracket
                    case ')':
                        {
                            // )
                            throw new NotImplementedException();
                            break;
                        }
                    #endregion
                    #region Default
                    default:
                        {
                            throw new NotImplementedException();
                        }
                    #endregion
                }
            }

            throw new NotImplementedException();
        }
    }
}
