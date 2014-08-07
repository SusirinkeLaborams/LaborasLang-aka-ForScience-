using Lexer.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexer
{
    internal class Tokenizer
    {
        private static readonly bool[] SymbolMap;
        
        static Tokenizer()
        {
            var symbols = new char[] { ' ', '\t', '\'', '"', '+', '-', '!', '~', '&', '^', '|', '<', '>', '/', '*', '=', '\\', '%', '{', '}', '(', ')', '\n', '\r', ',', '.', '\0', ';' };
            SymbolMap = new bool[char.MaxValue];

            for (int i = 0; i < symbols.Length; i++)
            {
                SymbolMap[symbols[i]] = true;
            }
        }
       
        public static Token[] Tokenize(string file, RootNode rootNode)
        {
            var tokens = new List<Token>();
            var Source = new SourceReader(file);
            var builder = new StringBuilder();
            var lastLocation = new Location(-1, -1);

            while (Source.Peek() != '\0')
            {
#if DEBUG
                if (lastLocation == Source.Location)
                {
                    throw new Exception(String.Format("infinite loop found at line {} column {} symbol {}", lastLocation.Row, lastLocation.Column, Source.Peek()));
                }
                else
#endif
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
                            var token = rootNode.ProvideToken();
                            builder.Clear();

                            token.Type = TokenType.StringLiteral;

                            // Only peeked at the source, should save location after first pop or just increment collumn
                            var location = Source.Location;
                            location.Column = location.Column + 1;
                            token.Start = location;

                            do
                            {
                                if (Source.Peek() == '\\')
                                {
                                    builder.Append(Source.Pop());
                                }
                                builder.Append(Source.Pop());
                            } while (Source.Peek() != '\'');

                            builder.Append(Source.Pop());
                            token.End = Source.Location;
                            token.Content.Set(rootNode, builder);
                            tokens.Add(token);
                            break;
                        }
                    case '"':
                        {
                            // Duble quote string, scan to next " that is not going after a \
                            var token = rootNode.ProvideToken();
                            builder.Clear();
                            token.Type = TokenType.StringLiteral;

                            // Only peeked at the source, should save location after first pop or just increment collumn
                            var location = Source.Location;
                            location.Column = location.Column + 1;
                            token.Start = location;

                            do
                            {
                                if (Source.Peek() == '\\')
                                {
                                    builder.Append(Source.Pop());
                                }
                                builder.Append(Source.Pop());
                            } while (Source.Peek() != '"');

                            builder.Append(Source.Pop());
                            token.End = Source.Location;
                            token.Content.Set(rootNode, builder);
                            tokens.Add(token);
                            break;
                        }
                    #endregion
                    #region Plus
                    case '+':
                        {
                            // ++ += +
                            var token = rootNode.ProvideToken();
                            builder.Clear();
                            builder.Append(Source.Pop());
                            token.Start = Source.Location;
                            switch (Source.Peek())
                            {
                                case '+':
                                    {
                                        builder.Append(Source.Pop());
                                        token.Type = TokenType.PlusPlus;
                                        break;
                                    }
                                case '=':
                                    {
                                        builder.Append(Source.Pop());
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
                            token.Content.Set(rootNode, builder);
                            tokens.Add(token);
                            break;
                        }
                    #endregion
                    #region Minus
                    case '-':
                        {
                            // -- -= -
                            var token = rootNode.ProvideToken();
                            builder.Clear();
                            builder.Append(Source.Pop());
                            token.Start = Source.Location;
                            switch (Source.Peek())
                            {
                                case '-':
                                    {
                                        builder.Append(Source.Pop());
                                        token.Type = TokenType.MinusMinus;
                                        break;
                                    }
                                case '=':
                                    {
                                        builder.Append(Source.Pop());
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
                            token.Content.Set(rootNode, builder);
                            tokens.Add(token);
                            break;
                        }
                    #endregion
                    #region Not
                    case '!':
                        {
                            // ! !=
                            var token = rootNode.ProvideToken();
                            builder.Clear();
                            builder.Append(Source.Pop());
                            token.Start = Source.Location;
                            switch (Source.Peek())
                            {
                                case '=':
                                    {
                                        builder.Append(Source.Pop());
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
                            token.Content.Set(rootNode, builder);
                            tokens.Add(token);
                            break;
                        }
                    #endregion
                    #region BitwiseComplement
                    case '~':
                        {
                            // ~ ~=
                            var token = rootNode.ProvideToken();
                            builder.Clear();
                            builder.Append(Source.Pop());
                            token.Start = Source.Location;
                            switch (Source.Peek())
                            {
                                case '=':
                                    {
                                        builder.Append(Source.Pop());
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
                            token.Content.Set(rootNode, builder);
                            tokens.Add(token);
                            break;
                        }
                    #endregion
                    #region And
                    case '&':
                        {
                            // & && &=
                            var token = rootNode.ProvideToken();
                            builder.Clear();
                            token.Type = TokenType.BitwiseAnd;
                            builder.Append(Source.Pop());
                            token.Start = Source.Location;
                            switch (Source.Peek())
                            {
                                case '&':
                                    {
                                        builder.Append(Source.Pop());
                                        token.Type = TokenType.And;
                                        break;
                                    }
                                case '=':
                                    {
                                        builder.Append(Source.Pop());
                                        token.Type = TokenType.BitwiseAndEqual;
                                        break;
                                    }
                            }
                            token.End = Source.Location;
                            token.Content.Set(rootNode, builder);
                            tokens.Add(token);
                            break;
                        }
                    #endregion
                    #region Xor
                    case '^':
                        {
                            // ^ ^=
                            var token = rootNode.ProvideToken();
                            builder.Clear();
                            builder.Append(Source.Pop());
                            token.Start = Source.Location;
                            switch (Source.Peek())
                            {
                                case '=':
                                    {
                                        builder.Append(Source.Pop());
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
                            token.Content.Set(rootNode, builder);
                            tokens.Add(token);
                            break;
                        }
                    #endregion
                    #region Or
                    case '|':
                        {
                            // | |= || 
                            var token = rootNode.ProvideToken();
                            builder.Clear();
                            token.Type = TokenType.BitwiseOr;
                            builder.Append(Source.Pop());
                            token.Start = Source.Location;
                            switch (Source.Peek())
                            {
                                case '|':
                                    {
                                        builder.Append(Source.Pop());
                                        token.Type = TokenType.Or;
                                        break;
                                    }
                                case '=':
                                    {
                                        builder.Append(Source.Pop());
                                        token.Type = TokenType.BitwiseOrEqual;
                                        break;
                                    }
                            }
                            token.End = Source.Location;
                            token.Content.Set(rootNode, builder);
                            tokens.Add(token);
                            break;
                        }
                    #endregion
                    #region LessThan
                    case '<':
                        {
                            // < <= << <<=
                            var token = rootNode.ProvideToken();
                            builder.Clear();
                            token.Type = TokenType.Less;
                            builder.Append(Source.Pop());
                            token.Start = Source.Location;
                            switch (Source.Peek())
                            {
                                case '<':
                                    {
                                        builder.Append(Source.Pop());
                                        token.Type = TokenType.LeftShift;
                                        if (Source.Peek() == '=')
                                        {
                                            token.Type = TokenType.LeftShiftEqual;
                                            builder.Append(Source.Pop());
                                        }
                                        break;
                                    }
                                case '=':
                                    {
                                        builder.Append(Source.Pop());
                                        token.Type = TokenType.LessOrEqual;
                                        break;
                                    }
                            }
                            token.End = Source.Location;
                            token.Content.Set(rootNode, builder);
                            tokens.Add(token);
                            break;
                        }
                    #endregion
                    #region MoreThan
                    case '>':
                        {
                            // > >= >>
                            var token = rootNode.ProvideToken();
                            builder.Clear();
                            token.Type = TokenType.More;
                            builder.Append(Source.Pop());
                            token.Start = Source.Location;
                            switch (Source.Peek())
                            {
                                case '>':
                                    {
                                        builder.Append(Source.Pop());
                                        token.Type = TokenType.RightShift;
                                        if (Source.Peek() == '=')
                                        {
                                            token.Type = TokenType.RightShiftEqual;
                                            builder.Append(Source.Pop());
                                        }
                                        break;
                                    }
                                case '=':
                                    {
                                        builder.Append(Source.Pop());
                                        token.Type = TokenType.MoreOrEqual;
                                        break;
                                    }
                            }
                            token.End = Source.Location;
                            token.Content.Set(rootNode, builder);
                            tokens.Add(token);
                            break;
                        }
                    #endregion
                    #region Division
                    case '/':
                        {
                            // // / /= /* ... */
                            var token = rootNode.ProvideToken();
                            builder.Clear();
                            token.Type = TokenType.Divide;
                            builder.Append(Source.Pop());
                            token.Start = Source.Location;
                            switch (Source.Peek())
                            {
                                case '=':
                                    {
                                        builder.Append(Source.Pop());
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
                            token.Content.Set(rootNode, builder);
                            tokens.Add(token);
                            break;
                        }
                    #endregion
                    #region Period
                    case '.':
                        {
                            var token = rootNode.ProvideToken();
                            builder.Clear();
                            token.Type = TokenType.Period;
                            builder.Append(Source.Pop());
                            token.Start = Source.Location;
                            token.End = Source.Location;
                            token.Content.Set(rootNode, builder);
                            tokens.Add(token);
                            break;
                        }
                    #endregion
                    #region Comma
                    case ',':
                        {
                            var token = rootNode.ProvideToken();
                            builder.Clear();
                            token.Type = TokenType.Comma;
                            builder.Append(Source.Pop());
                            token.Start = Source.Location;
                            token.End = Source.Location;
                            token.Content.Set(rootNode, builder);
                            tokens.Add(token);
                            break;
                        }
                    #endregion
                    #region Multiplication
                    case '*':
                        {
                            var token = rootNode.ProvideToken();
                            builder.Clear();
                            token.Type = TokenType.Multiply;
                            builder.Append(Source.Pop());
                            token.Start = Source.Location;
                            if (Source.Peek() == '=')
                            {
                                builder.Append(Source.Pop());
                                token.Type = TokenType.MultiplyEqual;
                            }
                            token.End = Source.Location;
                            token.Content.Set(rootNode, builder);
                            tokens.Add(token);
                            break;
                        }
                    #endregion
                    #region Remainder
                    case '%':
                        {
                            var token = rootNode.ProvideToken();
                            builder.Clear();
                            token.Type = TokenType.Remainder;
                            builder.Append(Source.Pop());
                            token.Start = Source.Location;
                            if (Source.Peek() == '=')
                            {
                                builder.Append(Source.Pop());
                                token.Type = TokenType.RemainderEqual;
                            }
                            token.End = Source.Location;
                            token.Content.Set(rootNode, builder);
                            tokens.Add(token);
                            break;
                        }
                    #endregion
                    #region Equal
                    case '=':
                        {
                            var token = rootNode.ProvideToken();
                            builder.Clear();
                            token.Type = TokenType.Assignment;
                            builder.Append(Source.Pop());
                            token.Start = Source.Location;
                            if (Source.Peek() == '=')
                            {
                                builder.Append(Source.Pop());
                                token.Type = TokenType.Equal;
                            }
                            token.End = Source.Location;
                            token.Content.Set(rootNode, builder);
                            tokens.Add(token);
                            break;
                        }
                    #endregion
                    #region LeftCurlyBracket
                    case '{':
                        {
                            var token = rootNode.ProvideToken();
                            builder.Clear();
                            token.Type = TokenType.LeftCurlyBracket;
                            builder.Append(Source.Pop());
                            token.Start = Source.Location;
                            token.End = Source.Location;
                            token.Content.Set(rootNode, builder);
                            tokens.Add(token);
                            break;
                        }
                    #endregion
                    #region RightCurlyBracket
                    case '}':
                        {
                            var token = rootNode.ProvideToken();
                            builder.Clear();
                            token.Type = TokenType.RightCurlyBracket;
                            builder.Append(Source.Pop());
                            token.Start = Source.Location;
                            token.End = Source.Location;
                            token.Content.Set(rootNode, builder);
                            tokens.Add(token);
                            break;
                        }
                    #endregion
                    #region LeftBracket
                    case '(':
                        {
                            var token = rootNode.ProvideToken();
                            builder.Clear();
                            token.Type = TokenType.LeftBracket;
                            builder.Append(Source.Pop());
                            token.Start = Source.Location;
                            token.End = Source.Location;
                            token.Content.Set(rootNode, builder);
                            tokens.Add(token);
                            break;
                        }
                    #endregion
                    #region RightBracket
                    case ')':
                        {
                            var token = rootNode.ProvideToken();
                            builder.Clear();
                            token.Type = TokenType.RightBracket;
                            builder.Append(Source.Pop());
                            token.Start = Source.Location;
                            token.End = Source.Location;
                            token.Content.Set(rootNode, builder);
                            tokens.Add(token);
                            break;
                        }
                    #endregion
                    #region EndOfLine
                    case ';':
                        {
                            var token = rootNode.ProvideToken();
                            builder.Clear();
                            token.Type = TokenType.EndOfLine;
                            builder.Append(Source.Pop());
                            token.Start = Source.Location;
                            token.End = Source.Location;
                            token.Content.Set(rootNode, builder);
                            tokens.Add(token);
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
                            var token = rootNode.ProvideToken();
                            builder.Clear();
                            token.Start = Source.Location;
                            token.Type = TokenType.Integer;
                            // Consume to a symbol. Check for dot is required as it will not stop a number (0.1)
                            while (!IsSymbol(Source.Peek()) || Source.Peek() == '.')
                            {
                                char c = Source.Pop();

                                builder.Append(c);
                                if (c == 'F' || c == 'f')
                                {
                                    if (token.Type == TokenType.Integer || token.Type == TokenType.Double)
                                    {
                                        token.Type = TokenType.Float;
                                    }
                                    else
                                    {
                                        token.Type = TokenType.MalformedToken;
                                    }
                                }
                                else if (c == 'l' || c == 'L')
                                {
                                    if (token.Type == TokenType.Integer)
                                    {
                                        token.Type = TokenType.Long;
                                    }
                                    else
                                    {
                                        token.Type = TokenType.MalformedToken;
                                    }
                                }
                                else if (c == '.')
                                {

                                    if (token.Type == TokenType.Integer)
                                    {
                                        token.Type = TokenType.Double;
                                    }
                                    else
                                    {
                                        token.Type = TokenType.MalformedToken;
                                    }
                                }
                                else if (!IsDigit(c))
                                {
                                    token.Type = TokenType.MalformedToken;
                                }
                            }
                            token.End = Source.Location;
                            token.Content.Set(rootNode, builder);
                            tokens.Add(token);
                            break;
                        }
                    #endregion
                    #region Symbol
                    default:
                        {
                            var token = rootNode.ProvideToken();
                            builder.Clear();
                            while (!IsSymbol(Source.Peek()))
                            {
                                builder.Append(Source.Pop());
                            }

                            var str = builder.ToString();
                            token.Content.Set(rootNode, builder);
                            token.Type = GetKeywordType(str);
                            tokens.Add(token);
                            break;
                        }
                    #endregion
                }
            }

            var tokenArray = new Token[tokens.Count];
            tokens.CopyTo(tokenArray);
            return tokenArray;
        }

        public static bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        public static bool IsSymbol(char c)
        {
            return SymbolMap[c];
        }

        public static TokenType GetKeywordType(string symbol)
        {
            switch (symbol)
            {
                case "abstract":
                    return TokenType.Abstract;
                case "as":
                    return TokenType.As;
                case "base":
                    return TokenType.Base;
                case "break":
                    return TokenType.Break;
                case "case":
                    return TokenType.Case;
                case "catch":
                    return TokenType.Catch;
                case "class":
                    return TokenType.Class;
                case "const":
                    return TokenType.Const;
                case "continue":
                    return TokenType.Continue;
                case "default":
                    return TokenType.Default;
                case "do":
                    return TokenType.Do;
                case "extern":
                    return TokenType.Extern;
                case "else":
                    return TokenType.Else;
                case "enum":
                    return TokenType.Enum;
                case "false":
                    return TokenType.False;
                case "finally":
                    return TokenType.Finally;
                case "for":
                    return TokenType.For;
                case "goto":
                    return TokenType.Goto;
                case "if":
                    return TokenType.If;
                case "interface":
                    return TokenType.Interface;
                case "internal":
                    return TokenType.Internal;
                case "is":
                    return TokenType.Is;
                case "new":
                    return TokenType.New;
                case "null":
                    return TokenType.Null;
                case "namespace":
                    return TokenType.Namespace;
                case "out":
                    return TokenType.Out;
                case "override":
                    return TokenType.Override;
                case "private":
                    return TokenType.Private;
                case "protected":
                    return TokenType.Protected;
                case "public":
                    return TokenType.Public;
                case "ref":
                    return TokenType.Ref;
                case "return":
                    return TokenType.Return;
                case "switch":
                    return TokenType.Switch;
                case "struct":
                    return TokenType.Struct;
                case "sealed":
                    return TokenType.Sealed;
                case "static":
                    return TokenType.Static;
                case "this":
                    return TokenType.This;
                case "throw":
                    return TokenType.Throw;
                case "true":
                    return TokenType.True;
                case "try":
                    return TokenType.Try;
                case "using":
                    return TokenType.Using;
                case "virtual":
                    return TokenType.Virtual;
                case "while":
                    return TokenType.While;
            }
            return TokenType.Symbol;
        }
    }
}
