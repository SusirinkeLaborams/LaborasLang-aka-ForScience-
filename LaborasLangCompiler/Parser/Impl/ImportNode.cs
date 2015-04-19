using LaborasLangCompiler.Common;
using LaborasLangCompiler.Codegen;

using Lexer.Containers;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lexer;
using System.Diagnostics.Contracts;

namespace LaborasLangCompiler.Parser.Impl
{
    class ImportNode : ParserNode
    {
        public override NodeType Type { get { return NodeType.ParserInternal; } }
        private ImportNode(SequencePoint point) : base(point) { }

        public static void Parse(ContextNode context, IAbstractSyntaxTree lexerNode)
        {
            Contract.Requires(lexerNode.Type == TokenType.UseNode);
            var namespaze = DotOperatorNode.Parse(context, lexerNode.Children[1]) as NamespaceNode;
            var point = context.Parser.GetSequencePoint(lexerNode);
            if (namespaze != null)
            {
                context.GetClass().AddImport(namespaze, point);
            }
            else
            {
                ErrorCode.SymbolNotFound.ReportAndThrow(point, "Namespace {0} not found", lexerNode.Children[1].FullContent);
            }
        }

        public override string ToString(int indent)
        {
            throw new InvalidOperationException();
        }
    }
}
