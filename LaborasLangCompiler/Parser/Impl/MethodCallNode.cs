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
        public static new MethodCallNode Parse(Parser parser, ClassNode parentClass, CodeBlockNode parentBlock, AstNode lexerNode)
        {
            var instance = new MethodCallNode();
            var args = new List<IExpressionNode>();
            for (int i = 1; i < lexerNode.Children.Count; i++)
            {
                args.Add(ExpressionNode.Parse(parser, parentClass, parentBlock, lexerNode.Children[i]));
            }
            var argTypes = new List<TypeReference>();
            foreach(var arg in args)
            {
                argTypes.Add(arg.ReturnType);
            }
            instance.Arguments = args;
            instance.ParseMethod(parser, parentClass, parentBlock, lexerNode.Children[0], argTypes);
            return instance;
        }
        private void ParseMethod(Parser parser, ClassNode parentClass, CodeBlockNode parentBlock, AstNode lexerNode, List<TypeReference> args)
        {
            string type = lexerNode.Token.Name;
            ExpressionNode method;
            if (type == Lexer.Symbol || type == Lexer.FullSymbol)
            {
                if (lexerNode.Children.Count == 1)
                {
                    method = LValueNode.Parse(parser, parentClass, parentBlock, lexerNode);
                }
                else
                {
                    method = MethodNode.Parse(parser, parentClass, parentBlock, lexerNode, args);
                }
            }
            else
            {
                method = ExpressionNode.Parse(parser, parentClass, parentBlock, lexerNode);
            }

            if (!method.ReturnType.IsFunctorType())
                throw new TypeException("Type " + method.ReturnType + " cannot be called as a function");
            List<TypeReference> methodArgs;
            ReturnType = ILHelpers.GetFunctorReturnTypeAndArguments(parser.Assembly, method.ReturnType, out methodArgs);
            for (int i = 0; i < args.Count; i++)
            {
                if (!args[i].IsAssignableTo(methodArgs[i]))
                    throw new TypeException(String.Format("Argument {0}, {1} expected, {2} received", i - 1, methodArgs[i], args[i]));
            }
            Function = method;
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
