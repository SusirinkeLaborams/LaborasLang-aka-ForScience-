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
            var symbols = new char[] { ' ', '\t', '\'', '"', '+', '-', '!', '~', '&', '^', '|', '<', '>', '/', '*', '=', '\\', '%', '{', '}', '(', ')', '\n', '\r', ',', '.', '\0', ';', '[', ']' };
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
            var source = new SourceReader(file);
            var builder = ClaimFreeStringBuilder();
#if DEBUG
            var lastLocation = new Location(-1, -1);
#endif

            while (source.Peek() != '\0')
            {
#if DEBUG
                if (lastLocation == source.Location)
                {
                    throw new Exception(String.Format("infinite loop found at line {} column {} symbol {}", lastLocation.Row, lastLocation.Column, source.Peek()));
                }
                else
                {
                    lastLocation = source.Location;
                }
#endif

                switch (source.Peek())
                {
                    #region Whitespace
                    case '\n':
                    case '\r':
                    case ' ':
                    case '\t':
                        {
                            // Whitespaces are ignored
                            source.Pop();
                            break;
                        }
                    #endregion
                    #region StringLiteral
                    case '\'':
                        {
                            var token = CreateString(rootNode, source, builder, source.Peek(), TokenType.CharLiteral);
                            tokens.Add(token);
                            break;
                        }
                    case '"':
                        {
                            var token = CreateString(rootNode, source, builder, source.Peek(), TokenType.StringLiteral);
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
                            token.Start = source.Location;
                            builder.Append(source.Pop());
                            switch (source.Peek())
                            {
                                case '+':
                                    {
                                        builder.Append(source.Pop());
                                        token.Type = TokenType.PlusPlus;
                                        break;
                                    }
                                case '=':
                                    {
                                        builder.Append(source.Pop());
                                        token.Type = TokenType.PlusEqual;
                                        break;
                                    }
                                default:
                                    {
                                        token.Type = TokenType.Plus;
                                        break;
                                    }
                            }
                            token.End = source.Location;
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
                            token.Start = source.Location;
                            builder.Append(source.Pop());
                            switch (source.Peek())
                            {
                                case '-':
                                    {
                                        builder.Append(source.Pop());
                                        token.Type = TokenType.MinusMinus;
                                        break;
                                    }
                                case '=':
                                    {
                                        builder.Append(source.Pop());
                                        token.Type = TokenType.MinusEqual;
                                        break;
                                    }
                                default:
                                    {
                                        token.Type = TokenType.Minus;
                                        break;
                                    }
                            }
                            token.End = source.Location;
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
                            token.Start = source.Location;
                            builder.Append(source.Pop());
                            switch (source.Peek())
                            {
                                case '=':
                                    {
                                        builder.Append(source.Pop());
                                        token.Type = TokenType.NotEqual;
                                        break;
                                    }
                                default:
                                    {
                                        token.Type = TokenType.Not;
                                        break;
                                    }
                            }
                            token.End = source.Location;
                            token.Content = new FastString(rootNode, builder);
                            tokens.Add(token);
                            break;
                        }
                    #endregion
                    #region BitwiseComplement
                    case '~':
                        {
                            tokens.Add(WrapSymbol(rootNode, source, builder, TokenType.BitwiseComplement));
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
                            token.Start = source.Location;
                            builder.Append(source.Pop());
                            switch (source.Peek())
                            {
                                case '&':
                                    {
                                        builder.Append(source.Pop());

                                        if (source.Peek() == '=')
                                        {
                                            builder.Append(source.Pop());
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
                                        builder.Append(source.Pop());
                                        token.Type = TokenType.BitwiseAndEqual;
                                        break;
                                    }
                            }
                            token.End = source.Location;
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
                            token.Start = source.Location;
                            builder.Append(source.Pop());
                            switch (source.Peek())
                            {
                                case '=':
                                    {
                                        builder.Append(source.Pop());
                                        token.Type = TokenType.BitwiseXorEqual;
                                        break;
                                    }
                                default:
                                    {
                                        token.Type = TokenType.BitwiseXor;
                                        break;
                                    }
                            }
                            token.End = source.Location;
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
                            token.Start = source.Location;
                            builder.Append(source.Pop());
                            switch (source.Peek())
                            {
                                case '|':
                                    {
                                        builder.Append(source.Pop());

                                        if (source.Peek() == '=')
                                        {
                                            builder.Append(source.Pop());
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
                                        builder.Append(source.Pop());
                                        token.Type = TokenType.BitwiseOrEqual;
                                        break;
                                    }
                            }
                            token.End = source.Location;
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
                            token.Start = source.Location;
                            builder.Append(source.Pop());
                            switch (source.Peek())
                            {
                                case '<':
                                    {
                                        builder.Append(source.Pop());
                                        token.Type = TokenType.LeftShift;
                                        if (source.Peek() == '=')
                                        {
                                            token.Type = TokenType.LeftShiftEqual;
                                            builder.Append(source.Pop());
                                        }
                                        break;
                                    }
                                case '=':
                                    {
                                        builder.Append(source.Pop());
                                        token.Type = TokenType.LessOrEqual;
                                        break;
                                    }
                            }
                            token.End = source.Location;
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
                            token.Start = source.Location;
                            builder.Append(source.Pop());
                            switch (source.Peek())
                            {
                                case '>':
                                    {
                                        builder.Append(source.Pop());
                                        token.Type = TokenType.RightShift;
                                        if (source.Peek() == '=')
                                        {
                                            token.Type = TokenType.RightShiftEqual;
                                            builder.Append(source.Pop());
                                        }
                                        break;
                                    }
                                case '=':
                                    {
                                        builder.Append(source.Pop());
                                        token.Type = TokenType.MoreOrEqual;
                                        break;
                                    }
                            }
                            token.End = source.Location;
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
                            token.Start = source.Location;
                            builder.Append(source.Pop());
                            switch (source.Peek())
                            {
                                case '=':
                                    {
                                        builder.Append(source.Pop());
                                        token.Type = TokenType.DivideEqual;
                                        break;
                                    }
                                case '/':
                                    {
                                        // Single line comment
                                        while (source.Pop() != '\n') ;
                                        // No token to return
                                        continue;
                                    }
                                case '*':
                                    {
                                        /* multiline comment */
                                        source.Pop();
                                        var last = source.Pop();
                                        while (true)
                                        {
                                            var current = source.Pop();
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
                            token.End = source.Location;
                            token.Content = new FastString(rootNode, builder);
                            tokens.Add(token);
                            break;
                        }
                    #endregion
                    #region Period
                    case '.':
                        {
                            tokens.Add(WrapSymbol(rootNode, source, builder, TokenType.Period));
                            break;
                        }
                    #endregion
                    #region Comma
                    case ',':
                        {
                            tokens.Add(WrapSymbol(rootNode, source, builder, TokenType.Comma));
                            break;
                        }
                    #endregion
                    #region Multiplication
                    case '*':
                        {
                            var token = rootNode.ProvideToken();
                            builder.Clear();
                            token.Type = TokenType.Multiply;
                            token.Start = source.Location;
                            builder.Append(source.Pop());
                            if (source.Peek() == '=')
                            {
                                builder.Append(source.Pop());
                                token.Type = TokenType.MultiplyEqual;
                            }
                            token.End = source.Location;
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
                            token.Start = source.Location;
                            builder.Append(source.Pop());
                            if (source.Peek() == '=')
                            {
                                builder.Append(source.Pop());
                                token.Type = TokenType.RemainderEqual;
                            }
                            token.End = source.Location;
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
                            token.Start = source.Location;
                            builder.Append(source.Pop());
                            if (source.Peek() == '=')
                            {
                                builder.Append(source.Pop());
                                token.Type = TokenType.Equal;
                            }
                            token.End = source.Location;
                            token.Content = new FastString(rootNode, builder);
                            tokens.Add(token);
                            break;
                        }
                    #endregion
                    #region LeftCurlyBracket
                    case '{':
                        {
                            tokens.Add(WrapSymbol(rootNode, source, builder, TokenType.LeftCurlyBrace));
                            break;
                        }
                    #endregion
                    #region RightCurlyBracket
                    case '}':
                        {
                            tokens.Add(WrapSymbol(rootNode, source, builder, TokenType.RightCurlyBrace));
                            break;
                        }
                    #endregion
                    #region LeftBracket
                    case '(':
                        {
                            tokens.Add(WrapSymbol(rootNode, source, builder, TokenType.LeftParenthesis));
                            break;
                        }
                    #endregion
                    #region RightBracket
                    case ')':
                        {
                            tokens.Add(WrapSymbol(rootNode, source, builder, TokenType.RightParenthesis));
                            break;
                        }
                    #endregion
                    #region EndOfLine
                    case ';':
                        {
                            tokens.Add(WrapSymbol(rootNode, source, builder, TokenType.EndOfLine));
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
                            token.Start = source.Location;
                            token.Type = TokenType.Integer;
                            // Consume to a symbol. Check for dot is required as it will not stop a number (0.1)
                            while (!IsSymbol(source.Peek()) || source.Peek() == '.')
                            {
                                char c = source.Pop();

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
                            token.End = source.Location;
                            token.Content = new FastString(rootNode, builder);
                            tokens.Add(token);
                            break;
                        }
                    #endregion
                    #region Brackets
                    case '[':
                        {
                            tokens.Add(WrapSymbol(rootNode, source, builder, TokenType.LeftBracket));
                            break;
                        }
                    case ']':
                        {
                            tokens.Add(WrapSymbol(rootNode, source, builder, TokenType.RightBracket));
                            break;
                        }
                    #endregion
                    #region Symbol
                    default:
                        {
                            var token = rootNode.ProvideToken();
                            token.Start = source.Location;
                            builder.Clear();

                            while (!IsSymbol(source.Peek()))
                            {
                                builder.Append(source.Pop());
                            }

                            token.Content = new FastString(rootNode, builder);
                            token.Type = GetKeywordType(builder);
                            token.End = source.Location;

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

        private static Token WrapSymbol(RootNode rootNode, SourceReader Source, FastStringBuilder builder, TokenType type)
        {
            var token = rootNode.ProvideToken();
            builder.Clear();
            token.Type = type;
            token.Start = Source.Location;
            builder.Append(Source.Pop());
            token.End = Source.Location;
            token.Content = new FastString(rootNode, builder);
            return token;
        }

        private static Token CreateString(RootNode rootNode, SourceReader Source, FastStringBuilder builder, char quote, TokenType literalType)
        {
            var token = rootNode.ProvideToken();
            builder.Clear();

            token.Type = literalType;

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
            keywordTypeMap[(FastStringBuilder)"for"] = (int)TokenType.For;
            keywordTypeMap[(FastStringBuilder)"in"] = (int)TokenType.In;
            keywordTypeMap[(FastStringBuilder)"null"] = (int)TokenType.Null;
            
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
