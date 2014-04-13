using LaborasLangCompiler.Parser.Tree;
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
        public static ParserNode Parse(Parser parser, CodeBlockNode parent, AstNode lexerNode)
        {
            switch (lexerNode.Token.Name)
            {
                case "CodeBlock":
                    return CodeBlockNode.Parse(parser, parent, lexerNode.Children[0]);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
