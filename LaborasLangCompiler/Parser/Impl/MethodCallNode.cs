using LaborasLangCompiler.Common;
using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Parser.Utils;
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
        public override TypeReference ExpressionReturnType { get { return type; } }
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

        private TypeReference type;
        private List<ExpressionNode> args;
        private ExpressionNode function;

        public MethodCallNode(ExpressionNode function, TypeReference returnType, List<ExpressionNode> args, SequencePoint point)
            : base(point)
        {
            this.function = function;
            this.args = args;
            this.type = returnType;
        }

        public static ExpressionNode Parse(Parser parser, Context parent, AstNode lexerNode)
        {
            var function = ExpressionNode.Parse(parser, parent, lexerNode.Children[0]);
            for(int i = 1; i < lexerNode.Children.Count; i++)
            {
                var args = ParseArgList(parser, parent, lexerNode.Children[i]);
                function = Call(parser, function, args);
            }
            return function;
        }

        private static List<ExpressionNode> ParseArgList(Parser parser, Context parent, AstNode lexerNode)
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
                        ErrorCode.InvalidStructure.ReportAndThrow(parser.GetSequencePoint(node), "Unexpected node type {0} in call", node.Type);
                        break;
                }

            }
            return args;
        }

        private static ExpressionNode Call(Parser parser, ExpressionNode function, List<ExpressionNode> args)
        {
            var point = Parser.GetSequencePoint(function.SequencePoint, args.Count == 0 ? function.SequencePoint : args.Last().SequencePoint);
            var method = AsObjectCreation(parser, function, args, point);
            if (method != null)
                return method;

            foreach(var arg in args)
            {
                if (!arg.IsGettable)
                {
                    ErrorCode.NotAnRValue.ReportAndThrow(arg.SequencePoint, "Arguments must be gettable");
                }
            }

            method = AsMethod(parser, function, args, point);
            if (method != null)
                return method;

            method = AsFunctor(parser, function, args, point);
            if (method != null)
                return method;

            if (method == null)
                ErrorCode.NotCallable.ReportAndThrow(point, "Unable to call symbol");

            return method;
        }

        private static ExpressionNode AsFunctor(Parser parser, ExpressionNode node, IEnumerable<ExpressionNode> args, SequencePoint point)
        {
            if (node.ExpressionReturnType == null)
                return null;
            if (node.ExpressionReturnType.IsFunctorType())
            {
                if (node.ExpressionReturnType.MatchesArgumentList(parser.Assembly, args.Select(a => a.ExpressionReturnType).ToList()))
                    return new MethodCallNode(node, MetadataHelpers.GetFunctorReturnType(parser.Assembly, node.ExpressionReturnType), args.ToList(), point);
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
                return new MethodCallNode(node, MetadataHelpers.GetFunctorReturnType(parser.Assembly, node.ExpressionReturnType), args.ToList(), point);

            var ambiguous = node as AmbiguousMethodNode;
            if (ambiguous == null)
                return null;

            var method = ambiguous.RemoveAmbiguity(parser, args.Select(a => a.ExpressionReturnType));
            return new MethodCallNode(method, MetadataHelpers.GetFunctorReturnType(parser.Assembly, method.ExpressionReturnType), args.ToList(), point);
        }

        private static ExpressionNode AsObjectCreation(Parser parser, ExpressionNode node, IEnumerable<ExpressionNode> args, SequencePoint point)
        {
            var type = node as TypeNode;
            if (type == null)
                return null;

            var method = AssemblyRegistry.GetCompatibleConstructor(parser.Assembly, type.ParsedType, args.Select(a => a.ExpressionReturnType).ToList());
            if (method == null)
                return null;

            return new ObjectCreationNode(args.ToList(), method, type.Scope, point);
        }

        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("MethodCall:");
            builder.Indent(indent + 1).AppendFormat("ReturnType: {0}", type).AppendLine();
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
