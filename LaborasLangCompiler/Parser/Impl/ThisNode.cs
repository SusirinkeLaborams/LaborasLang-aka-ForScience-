using LaborasLangCompiler.Common;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Lexer.Containers;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Parser.Utils;
using Lexer;
using System.Diagnostics.Contracts;

namespace LaborasLangCompiler.Parser.Impl
{
    class ThisNode : ExpressionNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.This; } }
        public override bool IsGettable { get { return true; } }
        public override bool IsSettable { get { return false; } }
        public override TypeReference ExpressionReturnType { get { return type; } }

        private readonly TypeReference type;

        private ThisNode(TypeReference type, SequencePoint point)
            : base(point)
        {
            this.type = type;
        }

        public static ThisNode Parse(ContextNode parent, IAbstractSyntaxTree lexerNode)
        {
            var point = parent.Parser.GetSequencePoint(lexerNode);
            return Create(parent, point);
        }

        public static ThisNode Create(ContextNode scope, SequencePoint point)
        {
            if (scope.IsStaticContext())
            {
                ErrorCode.MissingInstance.ReportAndThrow(point, "Cannot use 'this' inside a static context");
                return null;//unreachable
            }
            else
            {
                return new ThisNode(scope.GetClass().TypeReference, point);
            }
        }

        public override string ToString(int indent)
        {
            throw new NotImplementedException();
        }
    }
}