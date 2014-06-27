using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexer
{
    public  class Tokenizer
    {
        private static char[] Symbols = { ' ', '\t', '\'', '"', '+', '-', '!', '~', '&', '^', '|', '<', '>', '/', '*', '=', '\\', '%', '{', '}', '(', ')', '\n', '\r', ',', '.', '\0' };
        private static char[] Digits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        public static IEnumerable<Token> Tokenize(string file)
        {
            SourceReader Source = new SourceReader(file);
            Location lastLocation = new Location(-1, -1);
            while (Source.Peek() != '\0')
            {
                if (lastLocation == Source.Location)
                {
                    throw new Exception(String.Format("infinite loop found at line {} column {} symbol {}", lastLocation.Row, lastLocation.Column, Source.Peek()));
                }
                else
                {
                    lastLocation = Source.Location;
                }

                switch (Source.Peek())
                {
                    #region Whitespace
                    case '\n':
                    case '\r':
                    case ' ':
                    case '\t':
                        {
                            // Whitespaces are ignored
                            Source.Pop();
                            break;
                        }
                    #endregion
                    #region StringLiteral
                    case '\'':
                        {
                            // String literal, scan to next ' that is not going after a \
                            var token = new Token();
                            token.Type = TokenType.StringLiteral;
                            
                            // Only peeked at the source, should save location after first pop or just increment collumn
                            var location = Source.Location;
                            location.Column = location.Column + 1;
                            token.Start = location;

                            do
                            {                        
                                if (Source.Peek() == '\\')
                                {
                                    token.Content += Source.Pop();
                                }
                                token.Content += Source.Pop();
                            } while (Source.Peek() != '\'');

                            token.Content += Source.Pop();
                            token.End = Source.Location;
                            yield return token;
                            break;
                        }
                    case '"':
                        {
                            // Duble quote string, scan to next " that is not going after a \
                            var token = new Token();
                            token.Type = TokenType.StringLiteral;

                            // Only peeked at the source, should save location after first pop or just increment collumn
                            var location = Source.Location;
                            location.Column = location.Column + 1;
                            token.Start = location;

                            do
                            {
                                if (Source.Peek() == '\\')
                                {
                                    token.Content += Source.Pop();
                                }
                                token.Content += Source.Pop();
                            } while (Source.Peek() != '"');

                            token.Content += Source.Pop();
                            token.End = Source.Location;
                            yield return token;
                            break;
                        }
                    #endregion
                    #region Plus
                    case '+':
                        {
                            // ++ += +
                            var token = new Token();
                            token.Content += Source.Pop();
                            token.Start = Source.Location;
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
                            token.End = Source.Location;
                            yield return token;
                            break;
                        }
                    #endregion
                    #region Minus
                    case '-':
                        {
                            // -- -= -
                            var token = new Token();
                            token.Content += Source.Pop();
                            token.Start = Source.Location;
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
                            token.End = Source.Location;
                            yield return token;
                            break;
                        }
                    #endregion
                    #region Not
                    case '!':
                        {
                            // ! !=
                            var token = new Token();
                            token.Content += Source.Pop();
                            token.Start = Source.Location;
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
                            token.End = Source.Location;
                            yield return token;
                            break;
                        }
                    #endregion
                    #region BitwiseComplement
                    case '~':
                        {
                            // ~ ~=
                            var token = new Token();
                            token.Content += Source.Pop();
                            token.Start = Source.Location;
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
                            token.End = Source.Location;
                            yield return token;
                            break;
                        }
                    #endregion
                    #region And
                    case '&':
                        {
                            // & && &=
                            var token = new Token();
                            token.Type = TokenType.BitwiseAnd;
                            token.Content += Source.Pop();
                            token.Start = Source.Location;
                            switch (Source.Peek())
                            {
                                case '&':
                                    {
                                        token.Content += Source.Pop();
                                        token.Type = TokenType.And;
                                        break;
                                    }
                                case '=':
                                    {
                                        token.Content += Source.Pop();
                                        token.Type = TokenType.BitwiseAndEqual;
                                        break;
                                    }
                            }
                            token.End = Source.Location;
                            yield return token;
                            break;
                        }
                    #endregion
                    #region Xor
                    case '^':
                        {
                            // ^ ^=
                            var token = new Token();
                            token.Content += Source.Pop();
                            token.Start = Source.Location;
                            switch (Source.Peek())
                            {
                                case '=':
                                    {
                                        token.Content += Source.Pop();
                                        token.Type = TokenType.BitwiseXorEqual;
                                        break;
                                    }
                                default:
                                    {
                                        token.Type = TokenType.BitwiseXor;
                                        break;
                                    }
                            }
                            token.End = Source.Location;
                            yield return token;
                            break;
                        }
                    #endregion
                    #region Or
                    case '|':
                        {
                            // | |= || 
                            var token = new Token();
                            token.Type = TokenType.BitwiseOr;
                            token.Content += Source.Pop();
                            token.Start = Source.Location;
                            switch (Source.Peek())
                            {
                                case '|':
                                    {
                                        token.Content += Source.Pop();
                                        token.Type = TokenType.Or;
                                        break;
                                    }
                                case '=':
                                    {
                                        token.Content += Source.Pop();
                                        token.Type = TokenType.BitwiseOrEqual;
                                        break;
                                    }
                            }
                            token.End = Source.Location;
                            yield return token;
                            break;
                        }
                    #endregion
                    #region LessThan
                    case '<':
                        {
                            // < <= << <<=
                            var token = new Token();
                            token.Type = TokenType.Less;
                            token.Content += Source.Pop();
                            token.Start = Source.Location;
                            switch (Source.Peek())
                            {
                                case '<':
                                    {
                                        token.Content += Source.Pop();
                                        token.Type = TokenType.LeftShift;
                                        if (Source.Peek() == '=')
                                        {
                                            token.Type = TokenType.LeftShiftEqual;
                                            token.Content += Source.Pop();
                                        }
                                        break;
                                    }
                                case '=':
                                    {
                                        token.Content += Source.Pop();
                                        token.Type = TokenType.LessOrEqual;
                                        break;
                                    }
                            }
                            token.End = Source.Location;
                            yield return token;
                            break;
                        }
                    #endregion
                    #region MoreThan
                    case '>':
                        {
                            // > >= >>
                            var token = new Token();
                            token.Type = TokenType.More;
                            token.Content += Source.Pop();
                            token.Start = Source.Location;
                            switch (Source.Peek())
                            {
                                case '>':
                                    {
                                        token.Content += Source.Pop();
                                        token.Type = TokenType.RightShift;
                                        if (Source.Peek() == '=')
                                        {
                                            token.Type = TokenType.RightShiftEqual;
                                            token.Content += Source.Pop();
                                        }
                                        break;
                                    }
                                case '=':
                                    {
                                        token.Content += Source.Pop();
                                        token.Type = TokenType.MoreOrEqual;
                                        break;
                                    }
                            }
                            token.End = Source.Location;
                            yield return token;
                            break;
                        }
                    #endregion
                    #region Division
                    case '/':
                        {
                            // // / /= /* ... */
                            var token = new Token();
                            token.Type = TokenType.Divide;
                            token.Content += Source.Pop();
                            token.Start = Source.Location;
                            switch (Source.Peek())
                            {
                                case '=':
                                    {
                                        token.Content += Source.Pop();
                                        token.Type = TokenType.DivideEqual;
                                        break;
                                    }
                                case '/':
                                    {
                                        // Single line comment
                                        while (Source.Pop() != '\n') ;
                                        // No token to return
                                        continue; 
                                    }
                                case '*':
                                    {
                                        /* multiline comment */
                                        Source.Pop();
                                        var last = Source.Pop();
                                        while (true)
                                        {
                                            var current = Source.Pop();
                                            if (last == '*' && current == '/')
                                            {
                                                break;
                                            }
                                            else
                                            {
                                                last = current;
                                            }
                                        }
                                        // No token to return
                                        continue;
                                    }
                            }
                            token.End = Source.Location;
                            yield return token;
                            break;
                        }
                    #endregion
                    #region Multiplication
                    case '*':
                        {
                            var token = new Token();
                            token.Type = TokenType.Multiply;
                            token.Content += Source.Pop();
                            token.Start = Source.Location;
                            if (Source.Peek() == '=')
                            {
                                token.Content += Source.Pop();
                                token.Type = TokenType.MultiplyEqual;
                            }
                            token.End = Source.Location;
                            yield return token;
                            break;
                        }
                    #endregion
                    #region Remainder
                    case '%':
                        {
                            var token = new Token();
                            token.Type = TokenType.Remainder;
                            token.Content += Source.Pop();
                            token.Start = Source.Location;
                            if (Source.Peek() == '=')
                            {
                                token.Content += Source.Pop();
                                token.Type = TokenType.RemainderEqual;
                            }
                            token.End = Source.Location;
                            yield return token;
                            break;
                        }
                    #endregion
                    #region Equal
                    case '=':
                        {
                            var token = new Token();
                            token.Type = TokenType.Assignment;
                            token.Content += Source.Pop();
                            token.Start = Source.Location;
                            if (Source.Peek() == '=')
                            {
                                token.Content += Source.Pop();
                                token.Type = TokenType.Equal;
                            }
                            token.End = Source.Location;
                            yield return token;
                            break;
                        }
                    #endregion
                    #region LeftCurlyBracket
                    case '{':
                        {
                            var token = new Token();
                            token.Type = TokenType.LeftCurlyBracket;
                            token.Content += Source.Pop();
                            token.Start = Source.Location;
                            token.End = Source.Location;
                            yield return token;
                            break;
                        }
                    #endregion
                    #region RightCurlyBracket
                    case '}':
                        {
                            var token = new Token();
                            token.Type = TokenType.RightCurlyBracket;
                            token.Content += Source.Pop();
                            token.Start = Source.Location;
                            token.End = Source.Location;
                            yield return token;
                            break;
                        }
                    #endregion
                    #region LeftBracket
                    case '(':
                        {
                            var token = new Token();
                            token.Type = TokenType.LeftBracket;
                            token.Content += Source.Pop();
                            token.Start = Source.Location;
                            token.End = Source.Location;
                            yield return token;
                            break;
                        }
                    #endregion
                    #region RightBracket
                    case ')':
                        {
                            var token = new Token();
                            token.Type = TokenType.RightBracket;
                            token.Content += Source.Pop();
                            token.Start = Source.Location;
                            token.End = Source.Location;
                            yield return token;
                            break;
                        }
                    #endregion
                    #region EndOfLine
                    case ';':
                        {
                            var token = new Token();
                            token.Type = TokenType.EndOfLine;
                            token.Content += Source.Pop();
                            token.Start = Source.Location;
                            token.End = Source.Location;
                            yield return token;
                            break;
                        }
                    #endregion
                    #region Number
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        {
                            var token = new Token();
                            token.Start = Source.Location;
                            token.Type = TokenType.Integer;
                            // Consume to a symbol. Check for dot is required as it will not stop a number (0.1)
                            while (!Symbols.Contains(Source.Peek()) || Source.Peek() == '.')
                            {
                                char c = Source.Pop();
                                
                                token.Content += c;
                                if(c == 'F' || c == 'f')
                                {                                    
                                    if(token.Type == TokenType.Integer || token.Type == TokenType.Double)
                                    {
                                       token.Type = TokenType.Float;
                                    }
                                    else
                                    {
                                        token.Type = TokenType.MalformedToken;
                                    }
                                } 
                                else if(c == 'l' || c == 'L')
                                {
                                    if(token.Type == TokenType.Integer)
                                    {
                                        token.Type = TokenType.Long;
                                    }
                                    else
                                    {
                                        token.Type = TokenType.MalformedToken;
                                    }
                                }
                                else if(c == '.')
                                {
                                    
                                    if(token.Type == TokenType.Integer)
                                    {
                                        token.Type = TokenType.Double;
                                    }
                                    else
                                    {
                                        token.Type = TokenType.MalformedToken;
                                    }
                                }
                                else if (!Digits.Contains(c))
                                {
                                    token.Type = TokenType.MalformedToken;
                                }
                            }
                            token.End = Source.Location;
                            yield return token;
                            break;
                        }
                    #endregion
                    #region Dot
                    case '.':
                        {
                            var token = new Token();
                            token.Type = TokenType.Dot;
                            token.Content += Source.Pop();
                            token.Start = Source.Location;
                            token.End = Source.Location;
                            yield return token;
                            break;
                        }
                    #endregion
                    #region Symbol
                    default:
                        {
                            var token = new Token();
                            token.Type = TokenType.Symbol;
                            while (!Symbols.Contains(Source.Peek()))
                            {
                                token.Content += Source.Pop();
                            }
                            yield return token;
                            break;
                        }
                    #endregion
                }
            }
            yield break;
        }
    }
}
