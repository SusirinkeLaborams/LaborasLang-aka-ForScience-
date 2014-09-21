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

namespace LaborasLangCompiler.Parser.Impl
{
    class DeclarationInfo
    {
        public AstNode Type { get; private set; }
        public AstNode Initializer { get; private set; }
        public AstNode SymbolName { get; private set; }

        protected DeclarationInfo(AstNode name, AstNode type, AstNode init, SequencePoint point)
        {
            this.Initializer = init;
            this.SymbolName = name;
            this.Type = type;
        }

        public static DeclarationInfo Parse(Parser parser, AstNode lexerNode)
        {
            AstNode type = new AstNode();
            AstNode name = new AstNode();
            AstNode init = new AstNode();

            foreach(var node in lexerNode.Children)
            {
                switch (node.Type)
                {
                    case Lexer.TokenType.VariableModifier:
                        throw new NotImplementedException("Modifiers not implemented");
                    case Lexer.TokenType.Symbol:
                        name = node;
                        break;
                    case Lexer.TokenType.Type:
                        type = node;
                        break;
                    case Lexer.TokenType.Value:
                        init = node;
                        break;
                    case Lexer.TokenType.Assignment:
                    case Lexer.TokenType.EndOfLine:
                        break;
                    default:
                        throw new ParseException(parser.GetSequencePoint(node), "Unexpected node in declaration: {0}", node.Type);
                }
            }

            if(name.IsNull || type.IsNull)
                throw new ParseException(parser.GetSequencePoint(lexerNode), "Missing elements in declaration {0}, lexer messed up", lexerNode.Content);

            return new DeclarationInfo(name, type, init, parser.GetSequencePoint(lexerNode));
        }
    }
}
