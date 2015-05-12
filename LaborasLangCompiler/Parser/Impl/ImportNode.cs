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
using LaborasLangCompiler.Parser.Utils;

namespace LaborasLangCompiler.Parser.Impl
{
    class ImportNode : ParserNode
    {
        public override NodeType Type { get { return NodeType.ParserInternal; } }
        private ImportNode(SequencePoint point) : base(point) { }

        public static void Parse(ContextNode context, IAbstractSyntaxTree lexerNode)
        {
            Contract.Requires(lexerNode.Type == TokenType.UseNode);
            string ns = AstUtils.GetFullSymbolTextContent(lexerNode.Children[1]);
            var point = context.Parser.GetSequencePoint(lexerNode);
            if (AssemblyRegistry.IsNamespaceKnown(ns))
            {
                context.GetClass().AddImport(ns, point);
            }
            else
            {
                ErrorCode.SymbolNotFound.ReportAndThrow(point, "Namespace {0} not found", ns);
            }
        }

        public override string ToString(int indent)
        {
            throw new InvalidOperationException();
        }
    }
}
