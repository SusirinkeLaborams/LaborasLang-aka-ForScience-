using LaborasLangCompiler.Parser.Exceptions;
using LaborasLangCompiler.Parser;
using NPEG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaborasLangCompiler.LexingTools;

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
        public static new SymbolDeclarationNode Parse(Parser parser, ClassNode parentClass, CodeBlockNode parentBlock, AstNode lexerNode)
        {
            ILValueNode symbol = null;
            IExpressionNode initializer = null;
            string type = lexerNode.Token.Name;
            if (type == Lexer.Declaration || type == Lexer.DeclarationAndAssignment)
            {
                try
                {
                    var declaredType = parser.ParseType(lexerNode.Children[0]);
                    var name = parser.GetNodeValue(lexerNode.Children[1]);

                    if (type == Lexer.DeclarationAndAssignment)
                        initializer = ExpressionNode.Parse(parser, parentClass, parentBlock, lexerNode.Children[2]);

                    if (declaredType == null && initializer == null)
                        throw new TypeException("Type inference requires initialization");

                    if(initializer != null)
                    {
                        if (declaredType == null)
                            declaredType = initializer.ReturnType;
                        else if(!Parser.CompareTypes(declaredType, initializer.ReturnType))
                            throw new TypeException("Type mismatch, type " + declaredType.FullName + " initialized with " + initializer.ReturnType.FullName);
                    }
                    symbol = parentBlock.AddSymbol(declaredType, name);
                }
                catch(IndexOutOfRangeException e)
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
        public override bool Equals(ParserNode obj)
        {
            if (!(obj is SymbolDeclarationNode))
                return false;
            var that = (SymbolDeclarationNode)obj;
            if (Initializer != null && that.Initializer != null)
            {
                if (!Initializer.Equals(that.Initializer))
                    return false;
            }
            else
            {
                if (Initializer != null || that.Initializer != null)
                    return false;
            }
            return base.Equals(obj) && DeclaredSymbol.Equals(that.DeclaredSymbol);
        }
    }
}
