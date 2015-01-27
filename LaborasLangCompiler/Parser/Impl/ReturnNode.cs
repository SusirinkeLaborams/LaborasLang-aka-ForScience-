using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.Parser.Exceptions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Lexer.Containers;

namespace LaborasLangCompiler.Parser.Impl
{
    class ReturnNode : ParserNode, IReturnNode, ReturningNode
    {
        public override NodeType Type { get { return NodeType.ReturnNode; } }
        public IExpressionNode Expression { get { return expression; } }
        public bool Returns { get { return true; } }

        private ExpressionNode expression;
        protected ReturnNode(SequencePoint point) : base(point) { }

        public static ReturnNode Parse(Parser parser, Context parent, AstNode lexerNode)
        {
            var returnType = parent.GetMethod().MethodReturnType;
            var instance = new ReturnNode(parser.GetSequencePoint(lexerNode));
            if (returnType == parser.Void && lexerNode.ChildrenCount != 2)
                throw new TypeException(instance.SequencePoint, "Cannot return value in a void method");

            if (lexerNode.Children.Count == 3)
            {
                instance.expression = ExpressionNode.Parse(parser, parent, lexerNode.Children[1], returnType);
                if (!instance.expression.TypeWrapper.IsAssignableTo(returnType) || !instance.expression.IsGettable) 
                    throw new TypeException(instance.SequencePoint, "Function returns {0}, cannot return {1}", returnType, instance.expression.TypeWrapper);
            }
            return instance;
        }

        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("Return:");
            builder.Indent(indent + 1).AppendLine("Expression:");
            builder.AppendLine(expression.ToString(indent + 2));
            return builder.ToString();
        }
    }
}
