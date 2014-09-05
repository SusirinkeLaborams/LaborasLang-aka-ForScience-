using LaborasLangCompiler.Parser.Exceptions;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Lexer.Containers;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    class DeclaraionInfo : ParserNode
    {
        public override NodeType Type { get { return NodeType.ParserInternal; } }

        public string TypeName { get; private set; }
        public string SymbolName { get; private set; }
        public AstNode ?Initializer { get; private set; }

        protected DeclaraionInfo(string name, string type, AstNode ?init, SequencePoint point) : base(point)
        {
            this.Initializer = init;
            this.SymbolName = name;
            this.TypeName = type;
        }

        public static DeclaraionInfo Parse(Parser parser, AstNode lexerNode)
        {
            string type = null;
            string name = null;
            AstNode ?init = null;

            foreach(var node in lexerNode.Children)
            {
                switch (node.Type)
                {
                    case Lexer.TokenType.VariableModifier:
                        throw new NotImplementedException("Modifiers not implemented");
                    case Lexer.TokenType.Type:
                        if (type == null)
                            type = node.Content.ToString();
                        else
                            throw new ParseException(parser.GetSequencePoint(node), "Type declared twice: {0}", node.Content.ToString());
                        break;
                    case Lexer.TokenType.FullSymbol:
                        if (name == null)
                            name = node.Content.ToString();
                        else
                            throw new ParseException(parser.GetSequencePoint(node), "Name declared twice: {0}", node.Content.ToString());
                        break;
                    case Lexer.TokenType.Value:
                        if (init == null)
                            init = node;
                        else
                            throw new ParseException(parser.GetSequencePoint(node), "Initializer declared twice: {0}", node.Content.ToString());
                        break;
                    default:
                        throw new ParseException(parser.GetSequencePoint(node), "Unexpected node in declaration: {0}", node.Type);
                }
            }

            if(name == null || type == null)
                throw new ParseException(parser.GetSequencePoint(lexerNode), "Missing elements in declaration {0}, lexer messed up", lexerNode.Content);

            return new DeclaraionInfo(name, type, init, parser.GetSequencePoint(lexerNode));
        }
    }
}
