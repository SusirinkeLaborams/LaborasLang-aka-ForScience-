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

namespace LaborasLangCompiler.Parser.Impl
{
    class SymbolDeclarationNode : ParserNode, ISymbolDeclarationNode
    {
        public override NodeType Type { get { return NodeType.SymbolDeclaration; } }
        public ILValueNode DeclaredSymbol { get; private set; }
        public IExpressionNode Initializer { get; private set; }
        private SymbolDeclarationNode(ILValueNode symbol, IExpressionNode init)
        {
            DeclaredSymbol = symbol;
            Initializer = init;
        }
        public static SymbolDeclarationNode Parse(Parser parser, ClassNode parentClass, CodeBlockNode parentBlock, AstNode lexerNode)
        {
            ILValueNode symbol = null;
            IExpressionNode initializer = null;
            string type = lexerNode.Token.Name;

            var declaredType = parser.ParseType(lexerNode.Children[0]);
            var name = parser.ValueOf(lexerNode.Children[1]);

            if (type == Lexer.DeclarationAndAssignment)
                initializer = ExpressionNode.Parse(parser, parentClass, parentBlock, lexerNode.Children[2]);

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
                    var method = (FunctionDeclarationNode)initializer;
                    method.Emit(parentClass.TypeEmitter, name + "local_method");
                }
            }
            symbol = parentBlock.AddVariable(declaredType, name);

            return new SymbolDeclarationNode(symbol, initializer);
        }
    }
}
