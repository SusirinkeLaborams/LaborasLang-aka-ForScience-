using Lexer.Containers;
using System;
using System.Collections.Generic;

namespace Lexer
{
    internal static class Tokenizer
    {
        private static readonly bool[] SymbolMap;
        private static readonly Dictionary<FastStringBuilder, int> KeywordTypeMap;

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
            var builder = ClaimFreeStringBuilder();
#if DEBUG
            var lastLocation = new Location(-1, -1);
#endif

            while (Source.Peek() != '\0')
            {
#if DEBUG
                if (lastLocation == Source.Location)
                {
                    throw new Exception(String.Format("infinite loop found at line {} column {} symbol {}", lastLocation.Row, lastLocation.Column, Source.Peek()));
                }
                else
                {
                    lastLocation = Source.Location;
                }
#endif

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
                    case '"':
                        {                         
                            var token = CreateString(rootNode, Source, builder, Source.Peek());
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
                            token.Start = Source.Location;
                            builder.Append(Source.Pop());
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
                            token.Content = new FastString(rootNode, builder);
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
                            token.Start = Source.Location;
                            builder.Append(Source.Pop());
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
                            token.Content = new FastString(rootNode, builder);
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
                            token.Start = Source.Location;
                            builder.Append(Source.Pop());
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
                            token.Content = new FastString(rootNode, builder);
                            tokens.Add(token);
                            break;
                        }
                    #endregion
                    #region BitwiseComplement
                    case '~':
                        {
                            // ~
                            var token = rootNode.ProvideToken();
                            builder.Clear();
                            token.Start = Source.Location;
                            builder.Append(Source.Pop());

                            token.Type = TokenType.BitwiseComplement;
                            token.End = Source.Location;
                            token.Content = new FastString(rootNode, builder);
                            tokens.Add(token);
                            break;
                        }
                    #endregion
                    #region And
                    case '&':
                        {
                            // & && &= &&=
                            var token = rootNode.ProvideToken();
                            builder.Clear();
                            token.Type = TokenType.BitwiseAnd;
                            token.Start = Source.Location;
                            builder.Append(Source.Pop());
                            switch (Source.Peek())
                            {
                                case '&':
                                    {
                                        builder.Append(Source.Pop());

                                        if (Source.Peek() == '=')
                                        {
                                            builder.Append(Source.Pop());
                                            token.Type = TokenType.LogicalAndEqual;
                                        }
                                        else
                                        {
                                            token.Type = TokenType.LogicalAnd;
                                        }

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
                            token.Content = new FastString(rootNode, builder);
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
                            token.Start = Source.Location;
                            builder.Append(Source.Pop());
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
                            token.Content = new FastString(rootNode, builder);
                            tokens.Add(token);
                            break;
                        }
                    #endregion
                    #region Or
                    case '|':
                        {
                            // | |= || ||=
                            var token = rootNode.ProvideToken();
                            builder.Clear();
                            token.Type = TokenType.BitwiseOr;
                            token.Start = Source.Location;
                            builder.Append(Source.Pop());
                            switch (Source.Peek())
                            {
                                case '|':
                                    {
                                        builder.Append(Source.Pop());

                                        if (Source.Peek() == '=')
                                        {
                                            builder.Append(Source.Pop());
                                            token.Type = TokenType.LogicalOrEqual;
                                        }
                                        else
                                        {
                                            token.Type = TokenType.LogicalOr;
                                        }
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
                            token.Content = new FastString(rootNode, builder);
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
                            token.Start = Source.Location;
                            builder.Append(Source.Pop());
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
                            token.Content = new FastString(rootNode, builder);
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
                            token.Start = Source.Location;
                            builder.Append(Source.Pop());
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
                            token.Content = new FastString(rootNode, builder);
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
                            token.Start = Source.Location;
                            builder.Append(Source.Pop());
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
                            token.Content = new FastString(rootNode, builder);
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
                            token.Start = Source.Location;
                            builder.Append(Source.Pop());
                            token.End = Source.Location;
                            token.Content = new FastString(rootNode, builder);
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
                            token.Start = Source.Location;
                            builder.Append(Source.Pop());
                            token.End = Source.Location;
                            token.Content = new FastString(rootNode, builder);
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
                            token.Start = Source.Location;
                            builder.Append(Source.Pop());
                            if (Source.Peek() == '=')
                            {
                                builder.Append(Source.Pop());
                                token.Type = TokenType.MultiplyEqual;
                            }
                            token.End = Source.Location;
                            token.Content = new FastString(rootNode, builder);
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
                            token.Start = Source.Location;
                            builder.Append(Source.Pop());
                            if (Source.Peek() == '=')
                            {
                                builder.Append(Source.Pop());
                                token.Type = TokenType.RemainderEqual;
                            }
                            token.End = Source.Location;
                            token.Content = new FastString(rootNode, builder);
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
                            token.Start = Source.Location;
                            builder.Append(Source.Pop());
                            if (Source.Peek() == '=')
                            {
                                builder.Append(Source.Pop());
                                token.Type = TokenType.Equal;
                            }
                            token.End = Source.Location;
                            token.Content = new FastString(rootNode, builder);
                            tokens.Add(token);
                            break;
                        }
                    #endregion
                    #region LeftCurlyBracket
                    case '{':
                        {
                            var token = rootNode.ProvideToken();
                            builder.Clear();
                            token.Type = TokenType.LeftCurlyBrace;
                            token.Start = Source.Location;
                            builder.Append(Source.Pop());
                            token.End = Source.Location;
                            token.Content = new FastString(rootNode, builder);
                            tokens.Add(token);
                            break;
                        }
                    #endregion
                    #region RightCurlyBracket
                    case '}':
                        {
                            var token = rootNode.ProvideToken();
                            builder.Clear();
                            token.Type = TokenType.RightCurlyBrace;
                            token.Start = Source.Location;
                            builder.Append(Source.Pop());
                            token.End = Source.Location;
                            token.Content = new FastString(rootNode, builder);
                            tokens.Add(token);
                            break;
                        }
                    #endregion
                    #region LeftBracket
                    case '(':
                        {
                            var token = rootNode.ProvideToken();
                            builder.Clear();
                            token.Type = TokenType.LeftParenthesis;
                            token.Start = Source.Location;
                            builder.Append(Source.Pop());
                            token.End = Source.Location;
                            token.Content = new FastString(rootNode, builder);
                            tokens.Add(token);
                            break;
                        }
                    #endregion
                    #region RightBracket
                    case ')':
                        {
                            var token = rootNode.ProvideToken();
                            builder.Clear();
                            token.Type = TokenType.RightParenthesis;
                            token.Start = Source.Location;
                            builder.Append(Source.Pop());
                            token.End = Source.Location;
                            token.Content = new FastString(rootNode, builder);
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
                            token.Start = Source.Location;
                            builder.Append(Source.Pop());
                            token.End = Source.Location;
                            token.Content = new FastString(rootNode, builder);
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
                            token.Content = new FastString(rootNode, builder);
                            tokens.Add(token);
                            break;
                        }
                    #endregion
                    #region Symbol
                    default:
                        {
                            var token = rootNode.ProvideToken();
                            token.Start = Source.Location;
                            builder.Clear();

                            while (!IsSymbol(Source.Peek()))
                            {
                                builder.Append(Source.Pop());
                            }

                            token.Content = new FastString(rootNode, builder);
                            token.Type = GetKeywordType(builder);
                            token.End = Source.Location;

                            tokens.Add(token);
                            break;
                        }
                    #endregion
                }
            }

            YieldStringBuilder(builder);

            var tokenArray = new Token[tokens.Count];
            tokens.CopyTo(tokenArray);
            return tokenArray;
        }

        private static Token CreateString(RootNode rootNode, SourceReader Source, FastStringBuilder builder, char quote)
        {
            var token = rootNode.ProvideToken();
            builder.Clear();

            token.Type = TokenType.StringLiteral;

            //Opening quote
            Source.Pop();

            token.Start = Source.Location;

            while (Source.Peek() != quote)
            {
                if (Source.Peek() == '\\')
                {
                    builder.Append(Source.Pop());
                }
                builder.Append(Source.Pop());
            };

            //Closing quote
            Source.Pop();

            token.End = Source.Location;
            token.Content = new FastString(rootNode, builder);
            return token;
        }

        private static bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private static bool IsSymbol(char c)
        {
            return SymbolMap[c];
        }

        private static TokenType GetKeywordType(FastStringBuilder symbol)
        {
            int tokenType;

            if (KeywordTypeMap.TryGetValue(symbol, out tokenType))
            {
                return (TokenType)tokenType;
            }

            return TokenType.Symbol;
        }

        private static void SetupTokenTypeMap(out Dictionary<FastStringBuilder, int> keywordTypeMap)
        {
            keywordTypeMap = new Dictionary<FastStringBuilder, int>();


            keywordTypeMap[(FastStringBuilder)"else"] = (int)TokenType.Else;
            keywordTypeMap[(FastStringBuilder)"entry"] = (int)TokenType.Entry;
            keywordTypeMap[(FastStringBuilder)"false"] = (int)TokenType.False;
            keywordTypeMap[(FastStringBuilder)"if"] = (int)TokenType.If;
            keywordTypeMap[(FastStringBuilder)"internal"] = (int)TokenType.Internal;
            keywordTypeMap[(FastStringBuilder)"mutable"] = (int)TokenType.Mutable;
            keywordTypeMap[(FastStringBuilder)"const"] = (int)TokenType.Const;
            keywordTypeMap[(FastStringBuilder)"private"] = (int)TokenType.Private;
            keywordTypeMap[(FastStringBuilder)"public"] = (int)TokenType.Public;
            keywordTypeMap[(FastStringBuilder)"return"] = (int)TokenType.Return;
            keywordTypeMap[(FastStringBuilder)"noinstance"] = (int)TokenType.NoInstance;
            keywordTypeMap[(FastStringBuilder)"true"] = (int)TokenType.True;
            keywordTypeMap[(FastStringBuilder)"use"] = (int)TokenType.Use;
            keywordTypeMap[(FastStringBuilder)"virtual"] = (int)TokenType.Virtual;
            keywordTypeMap[(FastStringBuilder)"while"] = (int)TokenType.While;            
        }

        class StringBuilderCacheField
        {
            public FastStringBuilder stringBuilder;
            public bool isTaken;
        }

        private static List<StringBuilderCacheField> s_StringBuilderCache = new List<StringBuilderCacheField>();

        // FastStringBuilder allocates memory each time it needs to grow its buffer,
        // which will not be more than log(N) where N is max length
        // However, if we create a new instance every time we tokenize something,
        // we're gonna be doing it all over again every time, so we cache it instead
        private static FastStringBuilder ClaimFreeStringBuilder()
        {
            lock (s_StringBuilderCache)
            {
                for (int i = 0; i < s_StringBuilderCache.Count; i++)
                {
                    if (!s_StringBuilderCache[i].isTaken)
                    {
                        return s_StringBuilderCache[i].stringBuilder;
                    }
                }

                var newBuilder = new FastStringBuilder();
                s_StringBuilderCache.Add(new StringBuilderCacheField()
                    {
                        stringBuilder = newBuilder,
                        isTaken = true
                    });

                return newBuilder;
            }
        }

        private static unsafe void YieldStringBuilder(FastStringBuilder strBuilder)
        {
            lock (s_StringBuilderCache)
            {
                for (int i = 0; i < s_StringBuilderCache.Count; i++)
                {
                    if (strBuilder.Ptr == s_StringBuilderCache[i].stringBuilder.Ptr)
                    {
                        s_StringBuilderCache[i].isTaken = false;
                        return;
                    }
                }
            }
        }
    }
}
