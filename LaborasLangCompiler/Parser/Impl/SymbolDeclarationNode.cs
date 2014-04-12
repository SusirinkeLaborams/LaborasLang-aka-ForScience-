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
                    var nodes = parser.FindChildren(new string[] { "Type", "FunctionType", "Symbol", "Function", "Value" }, lexerNode);
                    string name = parser.GetNodeValue(nodes["Symbol"][0]);
                    if(nodes.ContainsKey("Type"))
                    {
                        symbol = parent.AddSymbol(parser.ParseType(nodes["Type"][0]), name);
                    }
                    else if (nodes.ContainsKey("FunctionType"))
                    {
                        symbol = parent.AddSymbol(parser.ParseType(nodes["FunctionType"][0]), name);
                    }
                    else
                    {
                        throw new ParseException("Type not defined in declaration");
                    }

                    AstNode init = null;
                    if (nodes.ContainsKey("Value"))
                        init = nodes["Value"][0];
                    else if (nodes.ContainsKey("Function"))
                        init = nodes["Function"][0];

                    if (init != null)
                        initializer = ExpressionNode.Parse(parser, parent, init);
                }
                catch(IndexOutOfRangeException e)
                {
                    throw new ParseException("Less than one subnode in type/functiontype/value/function", e);
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
