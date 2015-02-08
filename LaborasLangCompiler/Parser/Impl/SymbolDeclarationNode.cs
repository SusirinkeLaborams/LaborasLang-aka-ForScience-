
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
using LaborasLangCompiler.Common;

namespace LaborasLangCompiler.Parser.Impl
{
    class SymbolDeclarationNode : ParserNode, ISymbolDeclarationNode
    {
        public override NodeType Type { get { return NodeType.SymbolDeclaration; } }
        public VariableDefinition Variable { get; private set; }
        public IExpressionNode Initializer { get { return initializer; } }
        public bool IsConst { get; private set; }

        private ExpressionNode initializer;

        private SymbolDeclarationNode(VariableDefinition variable, bool isConst, ExpressionNode init, SequencePoint point)
            : base(point)
        {
            this.Variable = variable;
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
                ErrorCode.VoidLValue.ReportAndThrow(point, "Cannot declare a variable of type void");

            bool isConst = ParseModifiers(info.Modifiers, point);

            if (isConst && initializer == null)
                ErrorCode.MissingInit.ReportAndThrow(point, "Const variables require initialization");

            if (declaredType.IsAuto() && (initializer == null || initializer.ExpressionReturnType == null))
                ErrorCode.MissingInit.ReportAndThrow(point, "Type inference requires initialization");

            if(declaredType.IsAuto())
            {
                if(initializer == null)
                    ErrorCode.MissingInit.ReportAndThrow(point, "Type inference requires initialization");
                else if (initializer.ExpressionReturnType == null)
                    ErrorCode.MissingInit.ReportAndThrow(point, "Initiliazer type is not defined, inferrence unavailable");
            }

            if (initializer != null)
            {
                if (declaredType.IsAuto())
                {
                    declaredType = initializer.ExpressionReturnType;
                }
                else if (!initializer.ExpressionReturnType.IsAssignableTo(declaredType))
                {
                    ErrorCode.TypeMissmatch.ReportAndThrow(initializer.SequencePoint,
                        "Variable of type {0} initialized with {1}", declaredType, initializer.ExpressionReturnType);
                }
            }

            SymbolDeclarationNode node = new SymbolDeclarationNode(new VariableDefinition(name, declaredType), isConst, initializer, point);

            return node;
        }

        private static bool ParseModifiers(Modifiers mods, SequencePoint point)
        {
            Modifiers mask = ~(Modifiers.Const | Modifiers.Mutable);
            if ((mods & mask) != 0)
                ErrorCode.InvalidVariableMods.ReportAndThrow(point, "Only const and mutable modifiers are allowed for local varialbes");

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
            return String.Format("{0}{1} {2}", IsConst ? "const " : "", Variable.VariableType, Variable.Name);
        }
    }
}
