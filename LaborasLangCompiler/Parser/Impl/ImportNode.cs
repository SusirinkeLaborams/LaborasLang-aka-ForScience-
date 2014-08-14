using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.Parser.Exceptions;
using Mono.Cecil.Cil;
using NPEG;
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
        public static void Parse(Parser parser, IContainerNode parent, AstNode lexerNode)
        {
            string namespaze = parser.ValueOf(lexerNode.Children[0]);
            
            parent.GetClass().AddImport(namespaze, parser.GetSequencePoint(lexerNode));
        }
    }
}
