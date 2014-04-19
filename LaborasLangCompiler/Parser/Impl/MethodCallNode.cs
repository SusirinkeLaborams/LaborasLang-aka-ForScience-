using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.Parser.Exceptions;
using Mono.Cecil;
using NPEG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    class MethodCallNode : RValueNode, IMethodCallNode
    {
        public override RValueNodeType RValueType { get { return RValueNodeType.Call; } }
        public override TypeReference ReturnType { get; set; }
        public IReadOnlyList<IExpressionNode> Arguments { get; private set; }
        public IExpressionNode Function { get; private set; }
        public static new MethodCallNode Parse(Parser parser, ClassNode parentClass, CodeBlockNode parentBlock, AstNode lexerNode)
        {
            var instance = new MethodCallNode();
            instance.Function = LValueNode.Parse(parser, parentClass, parentBlock, lexerNode.Children[0]);
            var type = instance.Function.ReturnType;
            if (!type.IsFunctorType())
                throw new TypeException("Type " + instance.ReturnType + " cannot be called as a function");
            List<TypeReference> argTypes;
            instance.ReturnType = ILHelpers.GetFunctorReturnTypeAndArguments(parser.Assembly, type, out argTypes);
            var args = new List<IExpressionNode>();
            for(int i = 1; i < lexerNode.Children.Count; i++)
            {
                var argument = ExpressionNode.Parse(parser, parentClass, parentBlock, lexerNode.Children[i]);
                if (argument.ReturnType.IsAssignableTo(argTypes[i - 1]))
                    args.Add(argument);
                else
                    throw new TypeException(String.Format("Argument {0}, {1} expected, {2} received", i-1, argTypes[i-1], argument.ReturnType));
            }
            instance.Arguments = args;
            return instance;
        }
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder("(MethodCall: Return: ");
            builder.Append(ReturnType)
                .Append(" Args: ");
            string delim = "";
            foreach(var arg in Arguments)
            {
                builder.Append(delim).Append(arg);
                delim = ", ";
            }
            builder.Append(" Function: ").Append(Function).Append(")");
            return builder.ToString();
        }
    }
}
