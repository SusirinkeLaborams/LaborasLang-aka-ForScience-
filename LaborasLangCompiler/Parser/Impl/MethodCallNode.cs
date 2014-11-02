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
    class MethodCallNode : ExpressionNode, IFunctionCallNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.Call; } }
        public override TypeWrapper TypeWrapper { get { return type; } }
        public IReadOnlyList<IExpressionNode> Args { get { return args; } }
        public IExpressionNode Function { get { return function; } }
        public override bool IsSettable { get { return false; } }
        public override bool IsGettable
        {
            get
            {
                return ExpressionReturnType.FullName != "System.Void";
            }
        }

        private TypeWrapper type;
        private List<ExpressionNode> args;
        private ExpressionNode function;

        public MethodCallNode(ExpressionNode function, TypeWrapper returnType, List<ExpressionNode> args, SequencePoint point)
            : base(point)
        {
            this.function = function;
            this.args = args;
            this.type = returnType;
        }

        public static new ExpressionNode Parse(Parser parser, ContainerNode parent, AstNode lexerNode)
        {
            var function = ExpressionNode.Parse(parser, parent, lexerNode.Children[0]);
            for(int i = 1; i < lexerNode.Children.Count; i++)
            {
                var args = ParseArgList(parser, parent, lexerNode.Children[i]);
                function = Call(parser, function, args);
            }
            return function;
        }

        private static List<ExpressionNode> ParseArgList(Parser parser, ContainerNode parent, AstNode lexerNode)
        {
            var args = new List<ExpressionNode>();
            foreach (var node in lexerNode.Children)
            {
                switch (node.Type)
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
            return args;
        }

        private static ExpressionNode Call(Parser parser, ExpressionNode function, List<ExpressionNode> args)
        {
            var point = parser.GetSequencePoint(function.SequencePoint, args.Count == 0 ? function.SequencePoint : args.Last().SequencePoint);
            var method = AsObjectCreation(parser, function, args, point);
            if (method != null)
                return method;

            foreach(var arg in args)
            {
                if (!arg.IsGettable)
                    throw new TypeException(arg.SequencePoint, "Arguments must be gettable");
            }

            method = AsMethod(parser, function, args, point);
            if (method != null)
                return method;

            method = AsFunctor(function, args, point);
            if (method != null)
                return method;

            if (method == null)
                throw new TypeException(point, "Unable to call as a method or constructor");

            return method;
        }

        private static ExpressionNode AsFunctor(ExpressionNode node, IEnumerable<ExpressionNode> args, SequencePoint point)
        {
            if (node.TypeWrapper == null)
                return null;
            if(node.TypeWrapper.IsFunctorType())
            {
                if (node.TypeWrapper.MatchesArgumentList(args.Select(a => a.TypeWrapper)))
                    return new MethodCallNode(node, node.TypeWrapper.FunctorReturnType, args.ToList(), point);
                else
                    return null;
            }
            else
            {
                return null;
            }
        }

        private static ExpressionNode AsMethod(Parser parser, ExpressionNode node, IEnumerable<ExpressionNode> args, SequencePoint point)
        {
            if (node is MethodNode)
                return new MethodCallNode(node, node.TypeWrapper.FunctorReturnType, args.ToList(), point);

            var ambiguous = node as AmbiguousMethodNode;
            if (ambiguous == null)
                return null;

            var method = ambiguous.RemoveAmbiguity(parser, new FunctorTypeWrapper(parser.Assembly, null, args.Select(a => a.TypeWrapper)));
            return new MethodCallNode(method, method.TypeWrapper.FunctorReturnType, args.ToList(), point);
        }

        private static ExpressionNode AsObjectCreation(Parser parser, ExpressionNode node, IEnumerable<ExpressionNode> args, SequencePoint point)
        {
            var type = node as TypeNode;
            if (type == null)
                return null;

            var method = AssemblyRegistry.GetCompatibleConstructor(parser.Assembly, type.ParsedType.TypeReference, args.Select(a => a.ExpressionReturnType).ToList());
            if (method == null)
                return null;

            return new ObjectCreationNode(type.ParsedType, args.ToList(), new ExternalMethod(parser.Assembly, method), type.Scope, point);
        }

        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("MethodCall:");
            builder.Indent(indent + 1).AppendFormat("ReturnType: {0}", TypeWrapper).AppendLine();
            builder.Indent(indent + 1).Append("Args:").AppendLine();
            foreach(var arg in args)
            {
                builder.AppendLine(arg.ToString(indent + 2));
            }
            builder.Indent(indent + 1).AppendLine("Function:");
            builder.AppendLine(function.ToString(indent + 2));
            return builder.ToString();
        }
    }
}
