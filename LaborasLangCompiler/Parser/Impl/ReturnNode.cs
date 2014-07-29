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

namespace LaborasLangCompiler.Parser.Impl
{
    class ReturnNode : ParserNode, IReturnNode, IReturning
    {
        public override NodeType Type { get { return NodeType.ReturnNode; } }
        public IExpressionNode Expression { get; private set; }
        public bool Returns { get { return true; } }
        protected ReturnNode(SequencePoint point) : base(point) { }
        public static ReturnNode Parse(Parser parser, IContainerNode parent, AstNode lexerNode)
        {
            var instance = new ReturnNode(parser.GetSequencePoint(lexerNode));
            TypeReference retType = parser.Primitives[Parser.Void];
            if (lexerNode.Children.Count > 0)
            {
                instance.Expression = ExpressionNode.Parse(parser, parent, lexerNode.Children[0]);
                retType = instance.Expression.ReturnType;
            }
            var functionReturn = parent.GetFunction().FunctionReturnType;
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
