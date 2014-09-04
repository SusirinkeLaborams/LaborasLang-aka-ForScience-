using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.Parser.Exceptions;
using Lexer.Containers;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    class ImportNode : ParserNode
    {
        public override NodeType Type { get { return NodeType.ParserInternal; } }
        protected ImportNode(SequencePoint point) : base(point) { }
        public static void Parse(Parser parser, ContainerNode parent, AstNode lexerNode)
        {
            string namespaze = lexerNode.Children[0].Content.ToString();
            
            parent.GetClass().AddImport(namespaze, parser.GetSequencePoint(lexerNode));
        }
    }
}
