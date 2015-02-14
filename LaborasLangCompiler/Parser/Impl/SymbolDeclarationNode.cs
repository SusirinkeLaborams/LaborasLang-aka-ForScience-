using LaborasLangCompiler.Parser.Utils;
using LaborasLangCompiler.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaborasLangCompiler.Codegen;
using Mono.Cecil.Cil;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Lexer.Containers;
using LaborasLangCompiler.Common;
using Mono.Cecil;

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

        public static SymbolDeclarationNode Parse(Parser parser, ContextNode parent, AstNode lexerNode)
        {
            var info = DeclarationInfo.Parse(parser, lexerNode);
            var name = info.SymbolName.GetSingleSymbolOrThrow();
            var declaredType = TypeNode.Parse(parser, parent, info.Type);
            var point = parser.GetSequencePoint(lexerNode);
            ExpressionNode initializer = info.Initializer.IsNull ? null : ExpressionNode.Parse(parser, parent, info.Initializer, declaredType);

            return Create(parser, parent, info.Modifiers, declaredType, name, initializer, point);
        }

        private static bool ParseModifiers(Modifiers mods, SequencePoint point)
        {
            Modifiers mask = ~(Modifiers.Const | Modifiers.Mutable);
            if ((mods & mask) != 0)
                ErrorCode.InvalidVariableMods.ReportAndThrow(point, "Only const and mutable modifiers are allowed for local varialbes");

            return mods.HasFlag(Modifiers.Const);
        }

        public static SymbolDeclarationNode Create(Parser parser, ContextNode parent, Modifiers mods, TypeReference type, string name, ExpressionNode initializer, SequencePoint point)
        {
            if (type.IsVoid())
                ErrorCode.VoidLValue.ReportAndThrow(point, "Cannot declare a variable of type void");

            bool isConst = ParseModifiers(mods, point);

            if (isConst && initializer == null)
                ErrorCode.MissingInit.ReportAndThrow(point, "Const variables require initialization");

            if (type.IsAuto() && (initializer == null || initializer.ExpressionReturnType == null))
                ErrorCode.MissingInit.ReportAndThrow(point, "Type inference requires initialization");

            if (type.IsAuto())
            {
                if (initializer == null)
                    ErrorCode.MissingInit.ReportAndThrow(point, "Type inference requires initialization");
                else if (initializer.ExpressionReturnType == null)
                    ErrorCode.MissingInit.ReportAndThrow(point, "Initiliazer type is not defined, inferrence unavailable");
            }

            if (initializer != null)
            {
                if (type.IsAuto())
                {
                    type = initializer.ExpressionReturnType;
                }
                else if (!initializer.ExpressionReturnType.IsAssignableTo(type))
                {
                    ErrorCode.TypeMissmatch.ReportAndThrow(initializer.SequencePoint,
                        "Variable of type {0} initialized with {1}", type, initializer.ExpressionReturnType);
                }
            }

            return new SymbolDeclarationNode(new VariableDefinition(name, type), isConst, initializer, point);
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
