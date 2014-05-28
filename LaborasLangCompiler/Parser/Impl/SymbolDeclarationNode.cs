using LaborasLangCompiler.Parser.Exceptions;
using LaborasLangCompiler.Parser;
using NPEG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaborasLangCompiler.LexingTools;
using LaborasLangCompiler.ILTools;
using Mono.Cecil.Cil;

namespace LaborasLangCompiler.Parser.Impl
{
    class SymbolDeclarationNode : ParserNode, ISymbolDeclarationNode
    {
        public override NodeType Type { get { return NodeType.SymbolDeclaration; } }
        public ILValueNode DeclaredSymbol { get; private set; }
        public IExpressionNode Initializer { get; private set; }
        private SymbolDeclarationNode(ILValueNode symbol, IExpressionNode init, SequencePoint point)
            : base(point)
        {
            DeclaredSymbol = symbol;
            Initializer = init;
        }
        public static SymbolDeclarationNode Parse(Parser parser, IContainerNode parent, AstNode lexerNode)
        {
            ILValueNode symbol = null;
            IExpressionNode initializer = null;
            string type = lexerNode.Token.Name;

            var declaredType = TypeNode.Parse(parser, parent, lexerNode.Children[0]);
            var name = parser.ValueOf(lexerNode.Children[1]);

            if (type == Lexer.DeclarationAndAssignment)
                initializer = ExpressionNode.Parse(parser, parent, lexerNode.Children[2]);

            if (declaredType == null && initializer == null)
                throw new TypeException("Type inference requires initialization");

            if (initializer != null)
            {
                if (declaredType == null)
                    declaredType = initializer.ReturnType;
                else if (!ILHelpers.IsAssignableTo(initializer.ReturnType, declaredType))
                    throw new TypeException("Type mismatch, type " + declaredType.FullName + " initialized with " + initializer.ReturnType.FullName);
                if(initializer is FunctionDeclarationNode)
                {
                    parent.GetClass().AddMethod((FunctionDeclarationNode)initializer, name + "_local");
                }
            }
            if (parent is CodeBlockNode)
                symbol = ((CodeBlockNode)parent).AddVariable(declaredType, name, parser.GetSequencePoint(lexerNode));
            else
                throw new ParseException("SymbolDeclarationNode somehow parsed in a class");

            return new SymbolDeclarationNode(symbol, initializer, parser.GetSequencePoint(lexerNode));
        }
        public override string ToString()
        {
            return String.Format("(Declaration: {0} = {1})", DeclaredSymbol.ToString(), Initializer != null ? Initializer.ToString() : "");
        }
    }
}
