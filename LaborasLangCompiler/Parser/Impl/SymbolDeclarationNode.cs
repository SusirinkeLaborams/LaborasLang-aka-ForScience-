using LaborasLangCompiler.Parser.Exceptions;
using LaborasLangCompiler.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaborasLangCompiler.ILTools;
using Mono.Cecil.Cil;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Lexer.Containers;

namespace LaborasLangCompiler.Parser.Impl
{
    class SymbolDeclarationNode : ParserNode, ISymbolDeclarationNode
    {
        public override NodeType Type { get { return NodeType.SymbolDeclaration; } }
        public ILocalVariableNode DeclaredSymbol { get { return declaredSymbol; } }
        public IExpressionNode Initializer { get { return initializer; } }

        private ExpressionNode initializer;
        private LocalVariableNode declaredSymbol;
        private SymbolDeclarationNode(LocalVariableNode symbol, ExpressionNode init, SequencePoint point)
            : base(point)
        {
            this.declaredSymbol = symbol;
            this.initializer = init;
        }
        public static SymbolDeclarationNode Parse(Parser parser, ContainerNode parent, AstNode lexerNode)
        {
            LocalVariableNode symbol = null;
            var info = DeclarationInfo.Parse(parser, lexerNode);
            var name = info.SymbolName.GetSingleSymbolOrThrow();
            var declaredType = TypeNode.Parse(parser, parent, info.Type);
            ExpressionNode initializer = info.Initializer.IsNull ? null : ExpressionNode.Parse(parser, parent, info.Initializer);

            if (info.Modifiers != 0)
                throw new NotImplementedException("Modifiers not implemented for local variables");

            if (declaredType == null && (initializer == null || initializer.TypeWrapper == null))
                throw new TypeException(parser.GetSequencePoint(lexerNode), "Type inference requires initialization");

            if (initializer != null)
            {
                if (declaredType != null && initializer is AmbiguousNode)
                {
                    initializer = ((AmbiguousNode)initializer).RemoveAmbiguity(parser, declaredType);
                    if(initializer.TypeWrapper == null)
                    {
                        throw new ParseException(initializer.SequencePoint, "Ambiguous result, {0}", initializer);
                    }
                }

                if (declaredType == null)
                {
                    declaredType = initializer.TypeWrapper;
                }
                else if (!initializer.TypeWrapper.IsAssignableTo(declaredType))
                {
                    throw new TypeException(parser.GetSequencePoint(lexerNode), "Type mismatch, type " + declaredType.FullName + " initialized with " + initializer.ExpressionReturnType.FullName);
                }
            }

            if (parent is CodeBlockNode)
            {
                symbol = ((CodeBlockNode)parent).AddVariable(declaredType, name, parser.GetSequencePoint(lexerNode));
            }
            else
            {
                throw new ParseException(parser.GetSequencePoint(lexerNode), "SymbolDeclarationNode somehow parsed not in a code block");
            }

            return new SymbolDeclarationNode(symbol, initializer, parser.GetSequencePoint(lexerNode));
        }

        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("VariableDeclaration:");
            builder.Indent(indent + 1).AppendLine("Symbol:");
            builder.Indent(indent + 2).AppendFormat("{0} {1}", declaredSymbol.ExpressionReturnType, declaredSymbol.Name).AppendLine();
            if(initializer != null)
            {
                builder.Indent(indent + 1).AppendLine("Initializer:");
                builder.AppendLine(initializer.ToString(indent + 2));
            }
            return builder.ToString();
        }
    }
}
