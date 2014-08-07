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
        private static readonly Dictionary<string, int> KeywordTypeMap;

        static Tokenizer()
        {
            var symbols = new char[] { ' ', '\t', '\'', '"', '+', '-', '!', '~', '&', '^', '|', '<', '>', '/', '*', '=', '\\', '%', '{', '}', '(', ')', '\n', '\r', ',', '.', '\0', ';' };
            SymbolMap = new bool[char.MaxValue];

            for (int i = 0; i < symbols.Length; i++)
            {
                SymbolMap[symbols[i]] = true;
            }
            
            SetupTokenTypeMap(out KeywordTypeMap);
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

        private static bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private static bool IsSymbol(char c)
        {
            return SymbolMap[c];
        }

        private static TokenType GetKeywordType(string symbol)
        {
            int tokenType;

            if (KeywordTypeMap.TryGetValue(symbol, out tokenType))
            {
                return (TokenType)tokenType;
            }

            return TokenType.Symbol;
        }

        public static void SetupTokenTypeMap(out Dictionary<string, int> keywordTypeMap)
        {
            keywordTypeMap = new Dictionary<string, int>();

            keywordTypeMap["abstract"] = (int)TokenType.Abstract;
            keywordTypeMap["as"] = (int)TokenType.As;
            keywordTypeMap["base"] = (int)TokenType.Base;
            keywordTypeMap["break"] = (int)TokenType.Break;
            keywordTypeMap["case"] = (int)TokenType.Case;
            keywordTypeMap["catch"] = (int)TokenType.Catch;
            keywordTypeMap["class"] = (int)TokenType.Class;
            keywordTypeMap["const"] = (int)TokenType.Const;
            keywordTypeMap["continue"] = (int)TokenType.Continue;
            keywordTypeMap["default"] = (int)TokenType.Default;
            keywordTypeMap["do"] = (int)TokenType.Do;
            keywordTypeMap["extern"] = (int)TokenType.Extern;
            keywordTypeMap["else"] = (int)TokenType.Else;
            keywordTypeMap["enum"] = (int)TokenType.Enum;
            keywordTypeMap["false"] = (int)TokenType.False;
            keywordTypeMap["finally"] = (int)TokenType.Finally;
            keywordTypeMap["for"] = (int)TokenType.For;
            keywordTypeMap["goto"] = (int)TokenType.Goto;
            keywordTypeMap["if"] = (int)TokenType.If;
            keywordTypeMap["interface"] = (int)TokenType.Interface;
            keywordTypeMap["internal"] = (int)TokenType.Internal;
            keywordTypeMap["is"] = (int)TokenType.Is;
            keywordTypeMap["new"] = (int)TokenType.New;
            keywordTypeMap["null"] = (int)TokenType.Null;
            keywordTypeMap["namespace"] = (int)TokenType.Namespace;
            keywordTypeMap["out"] = (int)TokenType.Out;
            keywordTypeMap["override"] = (int)TokenType.Override;
            keywordTypeMap["private"] = (int)TokenType.Private;
            keywordTypeMap["protected"] = (int)TokenType.Protected;
            keywordTypeMap["public"] = (int)TokenType.Public;
            keywordTypeMap["ref"] = (int)TokenType.Ref;
            keywordTypeMap["return"] = (int)TokenType.Return;
            keywordTypeMap["switch"] = (int)TokenType.Switch;
            keywordTypeMap["struct"] = (int)TokenType.Struct;
            keywordTypeMap["sealed"] = (int)TokenType.Sealed;
            keywordTypeMap["static"] = (int)TokenType.Static;
            keywordTypeMap["this"] = (int)TokenType.This;
            keywordTypeMap["throw"] = (int)TokenType.Throw;
            keywordTypeMap["true"] = (int)TokenType.True;
            keywordTypeMap["try"] = (int)TokenType.Try;
            keywordTypeMap["using"] = (int)TokenType.Using;
            keywordTypeMap["virtual"] = (int)TokenType.Virtual;
            keywordTypeMap["while"] = (int)TokenType.While;
        }
    }
}
