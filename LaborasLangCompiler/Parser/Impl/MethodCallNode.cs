using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.LexingTools;
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
        public MethodCallNode(IExpressionNode function, TypeReference returnType, IReadOnlyList<IExpressionNode> args)
        {
            Function = function;
            Arguments = args;
            ReturnType = returnType;
        }
        //tmp code kol lexer neveikia su dot
        public static new MethodCallNode Parse(Parser parser, IContainerNode parent, AstNode lexerNode)
        {
            var function = DotOperatorNode.Parse(parser, parent, lexerNode.Children[0]);
            var args = new List<IExpressionNode>();
            for (int i = 1; i < lexerNode.Children.Count; i++)
            {
                args.Add(ExpressionNode.Parse(parser, parent, lexerNode.Children[i]));
            }
            var method = function.ExtractMethod(args.Select(x => x.ReturnType).ToList());
            var nvm = new List<TypeReference>();
            var returnType = ILTools.ILHelpers.GetFunctorReturnTypeAndArguments(parser.Assembly, method.ReturnType, out nvm);
            return new MethodCallNode(method, returnType, args);
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
