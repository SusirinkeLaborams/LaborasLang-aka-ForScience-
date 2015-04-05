using LaborasLangCompiler.Common;
using Lexer;
using Lexer.Containers;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;

namespace LaborasLangCompiler.Parser.Impl
{
    [Flags]
    enum Modifiers
    {
        Public     = 1 << 0,
        Private    = 1 << 1,
        Protected  = 1 << 2,
        Const      = 1 << 3,
        Mutable    = 1 << 4,
        NoInstance = 1 << 5,
        Entry      = 1 << 6
    }
    static class ModifierUtils
    {
        public static Modifiers GetAccess()
        {
            return Modifiers.Private | Modifiers.Protected | Modifiers.Public;
        }

        public static Modifiers AddModifier(this Modifiers modifiers, Parser parser, IAbstractSyntaxTree node)
        {
            var toAdd = FromToken[node.Children[0].Type];
            if((modifiers & toAdd) != 0)
            {
                ErrorCode.DuplicateMods.ReportAndThrow(parser.GetSequencePoint(node), "Cannot add modifier {0} twice", toAdd);
            }
            return modifiers | toAdd;
        }

        public static bool HasAccess(this Modifiers modifiers)
        {
            return (modifiers & (GetAccess())) != 0;
        }

        public static bool HasStorage(this Modifiers modifiers)
        {
            return (modifiers & Modifiers.NoInstance) != 0;
        }

        public static bool HasMutability(this Modifiers modifiers)
        {
            return (modifiers & (Modifiers.Const | Modifiers.Mutable)) != 0;
        }

        public static Dictionary<Lexer.TokenType, Modifiers> FromToken = new Dictionary<Lexer.TokenType, Modifiers>()
        {
            { Lexer.TokenType.Public,     Modifiers.Public },
            { Lexer.TokenType.Private,    Modifiers.Private },
            { Lexer.TokenType.Protected,  Modifiers.Protected },
            { Lexer.TokenType.Const,      Modifiers.Const },
            { Lexer.TokenType.Entry,      Modifiers.Entry },
            { Lexer.TokenType.Mutable,    Modifiers.Mutable },
            { Lexer.TokenType.NoInstance, Modifiers.NoInstance }
        };
    }
}