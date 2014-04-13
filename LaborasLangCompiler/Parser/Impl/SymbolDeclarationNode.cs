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
            ILValueNode symbol = null;
            IExpressionNode initializer = null;
            string type = lexerNode.Token.Name;
            if (type == "Declaration" || type == "DeclarationAndAssignment")
            {
                try
                {
                    var declaredType = parser.ParseType(lexerNode.Children[0]);
                    var name = parser.GetNodeValue(lexerNode.Children[1]);
                    symbol = parent.AddSymbol(declaredType, name);
                    if (type == "DeclarationAndAssignment")
                        initializer = ExpressionNode.Parse(parser, parent, lexerNode.Children[2]);
                }
                catch(Exception e)
                {
                    throw new ParseException("Failed to parse declaration " + parser.GetNodeValue(lexerNode), e);
                }
            }
            else
            {
                throw new ParseException("Declaration expected, " + lexerNode.Token.Name + " received");
            }
            return new SymbolDeclarationNode(symbol, initializer);
        }
    }
}
