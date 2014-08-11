using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.LexingTools;
using LaborasLangCompiler.Parser.Exceptions;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Mono.Cecil;
using Mono.Cecil.Cil;
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
        public override TypeWrapper ReturnType { get { return type; } }
        public IReadOnlyList<IExpressionNode> Arguments { get; private set; }
        public IExpressionNode Function { get; private set; }

        private TypeWrapper type;
        public MethodCallNode(IExpressionNode function, TypeWrapper returnType, IReadOnlyList<IExpressionNode> args, SequencePoint point)
            : base(point)
        {
            Function = function;
            Arguments = args;
            this.type = returnType;
        }
        public static new MethodCallNode Parse(Parser parser, IContainerNode parent, AstNode lexerNode)
        {
            if (lexerNode.Children[0].Token.Name == Lexer.Create)
                throw new NotImplementedException("Create not implemented");
            if(lexerNode.Children.Count(x => x.Token.Name == Lexer.Arguments) > 1)
                throw new NotImplementedException("Calling returned functions not supported");
            var function = DotOperatorNode.Parse(parser, parent, lexerNode.Children[0]);
            var args = new List<IExpressionNode>();
            foreach (var node in lexerNode.Children[1].Children)
            {
                args.Add(ExpressionNode.Parse(parser, parent, node));
            }
            var method = function.ExtractMethod(args.Select(x => x.ReturnType).ToList());
            var nvm = new List<TypeReference>();
            var returnType = new ExternalType(parser.Assembly, ILTools.ILHelpers.GetFunctorReturnTypeAndArguments(parser.Assembly, method.ReturnType, out nvm));
            return new MethodCallNode(method, returnType, args, parser.GetSequencePoint(lexerNode));
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
