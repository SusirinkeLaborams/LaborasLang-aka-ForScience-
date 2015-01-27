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
        public VariableDefinition Variable { get { return VarialbeWrapper.VariableDefinition; } }
        public VariableWrapper VarialbeWrapper { get; private set; }
        public IExpressionNode Initializer { get { return initializer; } }
        public bool IsConst { get; private set; }

        private ExpressionNode initializer;

        private SymbolDeclarationNode(VariableWrapper variable, bool isConst, ExpressionNode init, SequencePoint point)
            : base(point)
        {
            this.VarialbeWrapper = variable;
            this.initializer = init;
            this.IsConst = isConst;
        }
        public static SymbolDeclarationNode Parse(Parser parser, Context parent, AstNode lexerNode)
        {
            var info = DeclarationInfo.Parse(parser, lexerNode);
            var name = info.SymbolName.GetSingleSymbolOrThrow();
            var declaredType = TypeNode.Parse(parser, parent, info.Type);
            var point = parser.GetSequencePoint(lexerNode);
            ExpressionNode initializer = info.Initializer.IsNull ? null : ExpressionNode.Parse(parser, parent, info.Initializer, declaredType);

            if (declaredType.IsVoid())
                throw new TypeException(point, "Cannot declare a variable of type void");

            bool isConst = ParseModifiers(info.Modifiers, point);

            if (isConst && initializer == null)
                throw new ParseException(point, "Const variables require initialization");

            if (declaredType.IsAuto() && (initializer == null || initializer.ExpressionReturnType == null))
                throw new TypeException(point, "Type inference requires initialization");

            if (initializer != null)
            {
                if (declaredType.IsAuto())
                {
                    declaredType = initializer.ExpressionReturnType;
                }
                else if (!initializer.ExpressionReturnType.IsAssignableTo(declaredType))
                {
                    throw new TypeException(point, "Type mismatch, type " + declaredType.FullName + " initialized with " + initializer.ExpressionReturnType.FullName);
                }
            }

            SymbolDeclarationNode node = new SymbolDeclarationNode(new VariableWrapper(name, declaredType), isConst, initializer, point);

            if (parent is CodeBlockNode)
            {
                ((CodeBlockNode)parent).AddVariable(node);
            }
            else
            {
                throw new ParseException(point, "SymbolDeclarationNode somehow parsed not in a code block");
            }

            return node;
        }

        private static bool ParseModifiers(Modifiers mods, SequencePoint point)
        {
            Modifiers mask = ~(Modifiers.Const | Modifiers.Mutable);
            if ((mods & mask) != 0)
                throw new ParseException(point, "Only const and mutable modifiers are allowed for local varialbes");

            return mods.HasFlag(Modifiers.Const);
        }

        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("VariableDeclaration:");
            builder.Indent(indent + 1).AppendLine("Symbol:");
            builder.Indent(indent + 2).Append(GetSignature()).AppendLine();
            if(initializer != null)
            {
                builder.Indent(indent + 1).AppendLine("Initializer:");
                builder.AppendLine(initializer.ToString(indent + 2));
            }
            return builder.ToString();
        }

        public string GetSignature()
        {
            return String.Format("{0}{1} {2}", IsConst ? "const " : "", VarialbeWrapper.TypeReference, VarialbeWrapper.Name);
        }
    }
}
