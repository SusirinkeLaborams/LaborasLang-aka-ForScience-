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
        public override NodeType Type { get { return NodeType.ImportNode; } }
        protected ImportNode(SequencePoint point) : base(point) { }
        public static void Parse(Parser parser, IContainerNode parent, AstNode lexerNode)
        {
            string namespaze = parser.ValueOf(lexerNode.Children[0]);
            string name = null;
            if (lexerNode.Children.Count != 1)
                name = parser.ValueOf(lexerNode.Children[1]);
            if (!AssemblyRegistry.IsNamespaceKnown(namespaze))
                throw new ParseException(parser.GetSequencePoint(lexerNode), "Unknown namespace {0}", namespaze);
            parent.GetClass().AddImport(namespaze, name, parser.GetSequencePoint(lexerNode));
        }
    }
}
