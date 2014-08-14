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
        public ILValueNode DeclaredSymbol { get { return declaredSymbol; } }
        public IExpressionNode Initializer { get { return initializer; } }

        private ExpressionNode initializer;
        private LValueNode declaredSymbol;
        private SymbolDeclarationNode(LValueNode symbol, ExpressionNode init, SequencePoint point)
            : base(point)
        {
            this.declaredSymbol = symbol;
            this.initializer = init;
        }
        public static SymbolDeclarationNode Parse(Parser parser, IContainerNode parent, AstNode lexerNode)
        {
            LValueNode symbol = null;
            ExpressionNode initializer = null;
            string type = lexerNode.Token.Name;

            var declaredType = TypeNode.Parse(parser, parent, lexerNode.Children[0]);
            var name = parser.ValueOf(lexerNode.Children[1]);

            if (type == Lexer.DeclarationAndAssignment)
                initializer = ExpressionNode.Parse(parser, parent, lexerNode.Children[2]);

            if (declaredType == null && initializer == null)
                throw new TypeException(parser.GetSequencePoint(lexerNode), "Type inference requires initialization");

            if (initializer != null)
            {
                if (declaredType == null)
                    declaredType = initializer.TypeWrapper;
                else if (!initializer.TypeWrapper.IsAssignableTo(declaredType))
                    throw new TypeException(parser.GetSequencePoint(lexerNode), "Type mismatch, type " + declaredType.FullName + " initialized with " + initializer.ReturnType.FullName);
            }
            if (parent is CodeBlockNode)
                symbol = ((CodeBlockNode)parent).AddVariable(declaredType, name, parser.GetSequencePoint(lexerNode));
            else
                throw new ParseException(parser.GetSequencePoint(lexerNode), "SymbolDeclarationNode somehow parsed in a class");

            return new SymbolDeclarationNode(symbol, initializer, parser.GetSequencePoint(lexerNode));
        }
        public override string ToString()
        {
            return String.Format("(Declaration: {0} = {1})", DeclaredSymbol.ToString(), Initializer != null ? Initializer.ToString() : "");
        }
    }
}
