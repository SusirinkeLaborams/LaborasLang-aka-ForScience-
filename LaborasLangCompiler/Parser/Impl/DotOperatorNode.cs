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
        public static TypeNode ParseAsType(Parser parser, IContainerNode parent, AstNode lexerNode, ISymbolNode left = null)
        {
            throw new NotImplementedException();
        }
        public static NamespaceNode ParseAsNamespace(Parser parser, IContainerNode parent, AstNode lexerNode, NamespaceNode left = null)
        {
            throw new NotImplementedException();
        }
        public static LValueNode ParseAsLValue(Parser parser, IContainerNode parent, AstNode lexerNode, ISymbolNode left = null)
        {
            throw new NotImplementedException();
        }
        public static IFunctionNode ParseAsMethod(Parser parser, IContainerNode parent, AstNode lexerNode, List<TypeReference> args = null, ISymbolNode left = null)
        {
            throw new NotImplementedException();
        }
    }
}
