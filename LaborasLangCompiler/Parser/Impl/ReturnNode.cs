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

        public static ReturnNode Parse(Parser parser, ContainerNode parent, AstNode lexerNode)
        {
            var returnType = parent.GetFunction().MethodReturnType;
            var instance = new ReturnNode(parser.GetSequencePoint(lexerNode));
            if (returnType == parser.Void && lexerNode.ChildrenCount != 2)
                throw new TypeException(instance.SequencePoint, "Cannot return value in a void method");

            if (lexerNode.Children.Count == 3)
            {
                instance.expression = ExpressionNode.Parse(parser, parent, lexerNode.Children[1]);
                if (!instance.expression.TypeWrapper.IsAssignableTo(returnType)) 
                    throw new TypeException(instance.SequencePoint, "Function returns {0}, cannot return {1}", returnType, instance.expression.TypeWrapper);
            }
            return instance;
        }
        public override string ToString()
        {
            return String.Format("(ReturnNode: {0})", Expression);
        }
    }
}
