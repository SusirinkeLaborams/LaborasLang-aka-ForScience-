using LaborasLangCompiler.Parser.Exceptions;
using LaborasLangCompiler.Parser.Tree;
using NPEG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    class SymbolDeclarationNode : ParserNode, ISymbolDeclarationNode
    {
        public override NodeType Type { get { return NodeType.SymbolDeclaration; } }
        public ILValueNode DeclaredSymbol { get; private set; }
        public IExpressionNode Initializer { get; private set; }
        public SymbolDeclarationNode(ILValueNode symbol, IExpressionNode init)
        {
            DeclaredSymbol = symbol;
            Initializer = init;
        }
        public static new SymbolDeclarationNode Parse(Parser parser, CodeBlockNode parent, AstNode lexerNode)
        {
            if(lexerNode.Token.Name == "Declaration")
            {
                throw new NotImplementedException();
            }
            else if(lexerNode.Token.Name == "DeclarationAndAssignment")
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new ParseException("Declaration expected, " + lexerNode.Token.Name + " received");
            }
        }
    }
}
