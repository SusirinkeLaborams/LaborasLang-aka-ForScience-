using LaborasLangCompiler.Parser.Exceptions;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Lexer.Containers;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaborasLangCompiler.Parser;
using LaborasLangCompiler.Common;

namespace LaborasLangCompiler.Parser.Impl
{
    class DeclarationInfo
    {
        public AstNode Type { get; private set; }
        public AstNode Initializer { get; private set; }
        public AstNode SymbolName { get; private set; }
        public Modifiers Modifiers { get; private set; }

        public static DeclarationInfo Parse(Parser parser, AstNode lexerNode)
        {
            DeclarationInfo instance = new DeclarationInfo();

            foreach(var node in lexerNode.Children)
            {
                switch (node.Type)
                {
                    case Lexer.TokenType.VariableModifier:
                        instance.Modifiers = instance.Modifiers.AddModifier(parser, node);
                        break;
                    case Lexer.TokenType.Symbol:
                        instance.SymbolName = node;
                        break;
                    case Lexer.TokenType.Type:
                        instance.Type = node;
                        break;
                    case Lexer.TokenType.Value:
                        instance.Initializer = node;
                        break;
                    case Lexer.TokenType.Assignment:
                    case Lexer.TokenType.EndOfLine:
                        break;
                    default:
                        ErrorHandling.Report(ErrorCode.InvalidStructure, parser.GetSequencePoint(node), String.Format("Unexpected node in declaration: {0}", node.Type));
                        break;
                }
            }

            if (instance.SymbolName.IsNull || instance.Type.IsNull)
            {
                ErrorHandling.Report(ErrorCode.InvalidStructure, parser.GetSequencePoint(lexerNode),
                    String.Format("Missing elements in declaration {0}, lexer messed up", lexerNode.Content));
            }

            return instance;
        }
    }
}
