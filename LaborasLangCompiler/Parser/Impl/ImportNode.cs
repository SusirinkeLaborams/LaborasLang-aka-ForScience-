using LaborasLangCompiler.Common;
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
        public static void Parse(Parser parser, Context parent, AstNode lexerNode)
        {
            var namespaze = DotOperatorNode.Parse(parser, parent, lexerNode.Children[1]) as NamespaceNode;
            var point = parser.GetSequencePoint(lexerNode);
            if (namespaze != null)
            {
                parent.GetClass().AddImport(namespaze, point);
            }
            else
            {
                Errors.ReportAndThrow(ErrorCode.SymbolNotFound, point, String.Format("Namespace {0} not found", lexerNode.Children[1].FullContent));
            }
        }
        public override string ToString(int indent)
        {
            throw new InvalidOperationException();
        }
    }
}
