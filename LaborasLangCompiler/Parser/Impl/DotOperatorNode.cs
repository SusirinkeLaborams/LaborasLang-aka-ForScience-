using LaborasLangCompiler.LexingTools;
using LaborasLangCompiler.Parser.Exceptions;
using Mono.Cecil;
using NPEG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    abstract class DotOperatorNode
    {
        public static IExpressionNode Parse(Parser parser, IContainerNode parent, AstNode lexerNode)
        {
            throw new NotImplementedException();
        }
        private static IExpressionNode Parse(Parser parser, IContainerNode parent, AstNode lexerNode, List<TypeReference> args = null)
        {
            throw new NotImplementedException();
        }
        public static TypeNode ParseAsType(Parser parser, IContainerNode parent, AstNode lexerNode)
        {
            var parsed = Parse(parser, parent, lexerNode);
            if (!(parsed is TypeNode))
                throw new ParseException("Type expected");
            else
                return (TypeNode)parsed;
        }
        public static NamespaceNode ParseAsNamespace(Parser parser, IContainerNode parent, AstNode lexerNode)
        {
            var parsed = Parse(parser, parent, lexerNode);
            if (!(parsed is NamespaceNode))
                throw new ParseException("Namespace expected");
            else
                return (NamespaceNode)parsed;
        }
    }
}
