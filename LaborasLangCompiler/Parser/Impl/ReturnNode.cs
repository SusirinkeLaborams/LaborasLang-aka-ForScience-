using NPEG;
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
            var instance = new ReturnNode(parser.GetSequencePoint(lexerNode));
            TypeWrapper retType = parser.Void;
            if (lexerNode.Children.Count > 0)
            {
                instance.expression = ExpressionNode.Parse(parser, parent, lexerNode.Children[0]);
                retType = instance.expression.TypeWrapper;
            }
            var functionReturn = parent.GetFunction().MethodReturnType;
            if (!retType.IsAssignableTo(functionReturn))
                throw new TypeException(instance.SequencePoint, "Function returns {0}, cannot return {1}",
                    functionReturn, retType);
            return instance;
        }
        public override string ToString()
        {
            return String.Format("(ReturnNode: {0})", Expression);
        }
    }
}
