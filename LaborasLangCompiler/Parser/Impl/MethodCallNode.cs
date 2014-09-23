using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.Parser.Exceptions;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Lexer.Containers;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    class MethodCallNode : RValueNode, IFunctionCallNode
    {
        public override RValueNodeType RValueType { get { return RValueNodeType.Call; } }
        public override TypeWrapper TypeWrapper { get { return type; } }
        public IReadOnlyList<IExpressionNode> Args { get; private set; }
        public IExpressionNode Function { get; private set; }

        private TypeWrapper type;
        public MethodCallNode(IExpressionNode function, TypeWrapper returnType, IReadOnlyList<IExpressionNode> args, SequencePoint point)
            : base(point)
        {
            Function = function;
            Args = args;
            this.type = returnType;
        }
        public static new MethodCallNode Parse(Parser parser, ContainerNode parent, AstNode lexerNode)
        {
            if (lexerNode.Children[0].Type == Lexer.TokenType.New)
                throw new NotImplementedException("Create not implemented");
            if (lexerNode.Children.Count(x => x.Type == Lexer.TokenType.FunctionArgumentsList) > 1)
                throw new NotImplementedException("Calling returned functions not supported");
            var function = ExpressionNode.Parse(parser, parent, lexerNode.Children[0]);
            var args = new List<ExpressionNode>();
            foreach (var node in lexerNode.Children[1].Children)
            {
                switch(node.Type)
                {
                    case Lexer.TokenType.LeftParenthesis:
                    case Lexer.TokenType.RightParenthesis:
                    case Lexer.TokenType.Comma:
                        break;
                    case Lexer.TokenType.Value:
                        args.Add(ExpressionNode.Parse(parser, parent, node));
                        break;
                    default:
                        throw new ParseException(parser.GetSequencePoint(node), "Unexpected node type {0} in call", node.Type);
                }
                
            }

            var method = AsMethod(parser, function, args);
            if (method == null)
                method = AsFunctor(function, args);

            if (method == null)
                throw new TypeException(function.SequencePoint, "Method expected");

            var returnType = method.TypeWrapper.FunctorReturnType;
            return new MethodCallNode(method, returnType, args, parser.GetSequencePoint(lexerNode));
        }
        private static ExpressionNode AsFunctor(ExpressionNode node, IEnumerable<ExpressionNode> args)
        {
            if(node.TypeWrapper.IsFunctorType())
            {
                if (node.TypeWrapper.MatchesArgumentList(args.Select(a => a.TypeWrapper)))
                    return node;
                else
                    return null;
            }
            else
            {
                return null;
            }
        }
        private static ExpressionNode AsMethod(Parser parser, ExpressionNode node, IEnumerable<ExpressionNode> args)
        {
            if (node is MethodNode)
                return node;

            var ambiguous = node as AmbiguousMethodNode;
            if (ambiguous == null)
                return null;

            return ambiguous.RemoveAmbiguity(parser, new FunctorTypeWrapper(parser.Assembly, null, args.Select(a => a.TypeWrapper)));
        }
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder("(MethodCall: Return: ");
            builder.Append(ExpressionReturnType)
                .Append(" Args: ");
            string delim = "";
            foreach(var arg in Args)
            {
                builder.Append(delim).Append(arg);
                delim = ", ";
            }
            builder.Append(" Function: ").Append(Function).Append(")");
            return builder.ToString();
        }
    }
}
