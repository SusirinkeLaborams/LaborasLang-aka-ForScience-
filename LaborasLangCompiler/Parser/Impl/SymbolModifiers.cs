using LaborasLangCompiler.Parser.Exceptions;
using Lexer.Containers;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;

namespace LaborasLangCompiler.Parser.Impl
{
    [Flags]
    enum Modifier
    {
        Public    = 1 << 0,
        Private   = 1 << 1,
        Protected = 1 << 2,
        Const     = 1 << 3,
        Mutable   = 1 << 4,
        Static    = 1 << 5,
        Entry     = 1 << 6
    }
    static class ModifierUtils
    {
        public static Modifier AddModifier(this Modifier modifiers, Parser parser, AstNode node)
        {
            var toAdd = FromToken[node.Type];
            if((modifiers & toAdd) != 0)
            {
                throw new ParseException(parser.GetSequencePoint(node), "Cannot add modifier {0} twice", toAdd);
            }
            return modifiers | toAdd;
        }

        public static Dictionary<Lexer.TokenType, Modifier> FromToken = new Dictionary<Lexer.TokenType, Modifier>()
        {
            { Lexer.TokenType.Public,    Modifier.Public },
            { Lexer.TokenType.Private,   Modifier.Private },
            { Lexer.TokenType.Protected, Modifier.Protected },
            { Lexer.TokenType.Const,     Modifier.Const },
            { Lexer.TokenType.Static,    Modifier.Static },
            { Lexer.TokenType.Entry,     Modifier.Entry }
        };
    }
}