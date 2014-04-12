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

                var nodes = parser.FindChildren(new string[] { "Type", "FunctionType", "Symbol", "Function", "Value" }, lexerNode);
                if (!nodes.ContainsKey("Symbol"))
                    throw new ParseException("Symbol not found in declaration");
                string name = parser.GetNodeValue(nodes["Symbol"]);
                if(nodes.ContainsKey("Type"))
                {
                    symbol = parent.AddSymbol(parser.ParseType(nodes["Type"]), name);
                }
                else if (nodes.ContainsKey("FunctionType"))
                {
                    symbol = parent.AddSymbol(parser.ParseType(nodes["FunctionType"]), name);
                }
                else
                {
                    throw new ParseException("Type not defined in declaration");
                }

                AstNode init = null;
                if (nodes.ContainsKey("Value"))
                {
                    init = nodes["Value"];
                }
                else if (nodes.ContainsKey("Function"))
                {
                    init = nodes["Function"];
                }

                if (init != null)
                    initializer = ExpressionNode.Parse(parser, parent, init);
            }
            else
            {
                throw new ParseException("Declaration expected, " + lexerNode.Token.Name + " received");
            }
            return new SymbolDeclarationNode(symbol, initializer);
        }
    }
}
