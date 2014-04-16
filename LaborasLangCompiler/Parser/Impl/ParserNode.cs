using LaborasLangCompiler.LexingTools;
using LaborasLangCompiler.Parser;
using NPEG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    abstract class ParserNode : IParserNode
    {
        public abstract NodeType Type { get; }
        public static ParserNode Parse(Parser parser, ClassNode parentClass, CodeBlockNode parentBlock, AstNode lexerNode)
        {
            switch (lexerNode.Token.Name)
            {
                case Lexer.CodeBlock:
                    return CodeBlockNode.Parse(parser, parentClass, parentBlock, lexerNode.Children[0]);
                default:
                    throw new NotImplementedException();
            }
        }
        public virtual string Print()
        {
            return "";
        }
    }
}
