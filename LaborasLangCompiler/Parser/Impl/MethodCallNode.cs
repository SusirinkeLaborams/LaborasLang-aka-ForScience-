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
using Lexer;
using System.Diagnostics.Contracts;

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
                return ExpressionReturnType.MetadataType != MetadataType.Void;
            }
        }

        private readonly TypeReference type;
        private readonly IReadOnlyList<ExpressionNode> args;
        private readonly ExpressionNode function;

        private MethodCallNode(ExpressionNode function, TypeReference returnType, IReadOnlyList<ExpressionNode> args, SequencePoint point)
            : base(point)
        {
            this.function = function;
            this.args = args;
            this.type = returnType;
        }

        public static ExpressionNode Parse(ContextNode context, IAbstractSyntaxTree lexerNode)
        {
            Contract.Requires(lexerNode.Children[1].Type == Lexer.TokenType.FunctionArgumentsList);
            var function = ExpressionNode.Parse(context, lexerNode.Children[0]);
            var args = ParseArgList(context, lexerNode.Children[1]);
            var point = context.Parser.GetSequencePoint(lexerNode.Children[1]);
            return Create(context, function, args, point);
        }

        private static List<ExpressionNode> ParseArgList(ContextNode parent, IAbstractSyntaxTree lexerNode)
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
                        args.Add(ExpressionNode.Parse(parent, node));
                        break;
                    default:
                        ErrorCode.InvalidStructure.ReportAndThrow(parent.Parser.GetSequencePoint(node), "Unexpected node type {0} in call", node.Type);
                        break;
                }

            }
            return args;
        }

        public static ExpressionNode Create(ContextNode context, ExpressionNode function, IEnumerable<ExpressionNode> args, SequencePoint point)
        {
            foreach(var arg in args)
            {
                if (!arg.IsGettable)
                {
                    ErrorCode.NotAnRValue.ReportAndThrow(arg.SequencePoint, "Arguments must be gettable");
                }
            }

            var method = AsObjectCreation(context, function, args, point);
            if (method != null)
                return method;

            method = AsMethod(context, function, args, point);
            if (method != null)
                return method;

            method = AsFunctor(context, function, args, point);
            if (method != null)
                return method;

            if (method == null)
                ErrorCode.NotCallable.ReportAndThrow(point, "Unable to call symbol");

            return method;
        }

        private static ExpressionNode AsFunctor(ContextNode context, ExpressionNode node, IEnumerable<ExpressionNode> args, SequencePoint point)
        {
            if (!node.IsGettable || !node.ExpressionReturnType.IsFunctorType())
                return null;

            if (node.ExpressionReturnType.MatchesArgumentList(context.Parser.Assembly, args.Select(a => a.ExpressionReturnType).ToList()))
            {
                return new MethodCallNode(node, MetadataHelpers.GetFunctorReturnType(context.Parser.Assembly, node.ExpressionReturnType), args.ToList(), point);
            }
            else
            {
                ErrorCode.TypeMissmatch.ReportAndThrow(point, "Cannot call functor, requires parameters ({0}), called with ({1})",
                        String.Join(", ", MetadataHelpers.GetFunctorParamTypes(context.Parser.Assembly, node.ExpressionReturnType).Select(p => p.FullName)),
                        String.Join(", ", args.Select(a => a.ExpressionReturnType.FullName)));
                return null;//unreachable
            }
        }

        private static ExpressionNode AsMethod(ContextNode context, ExpressionNode node, IEnumerable<ExpressionNode> args, SequencePoint point)
        {
            var method = node as MethodNode;
            if(method != null)
            {
                if(MetadataHelpers.MatchesArgumentList(method.Method, args.Select(arg => arg.ExpressionReturnType).ToList()))
                {
                    return new MethodCallNode(method, method.Method.ReturnType, args.ToList(), point);
                }
                else
                {
                    ErrorCode.TypeMissmatch.ReportAndThrow(point, "Cannot call method, {0} requires parameters ({1}), called with ({2})",
                        method.Method.FullName,
                        String.Join(", ", method.Method.Parameters.Select(p => p.ParameterType.FullName)),
                        String.Join(", ", args.Select(a => a.ExpressionReturnType.FullName)));
                }
            }

            var ambiguous = node as AmbiguousMethodNode;
            if (ambiguous == null)
                return null;//not a method

            method = ambiguous.RemoveAmbiguity(context, args.Select(a => a.ExpressionReturnType));
            if (method == null)
            {
                ErrorCode.TypeMissmatch.ReportAndThrow(point, "Cannot call method, {0} with arguments ({1}), none of the overloads match",
                    ambiguous.FullName,
                    String.Join(", ", args.Select(a => a.ExpressionReturnType.FullName)));
            }

            return new MethodCallNode(method, method.Method.ReturnType, args.ToList(), point);
        }

        private static ExpressionNode AsObjectCreation(ContextNode context, ExpressionNode node, IEnumerable<ExpressionNode> args, SequencePoint point)
        {
            var type = node as TypeNode;
            if (type == null)
                return null;

            var resolved = type.ParsedType.Resolve();

            if (resolved.IsAbstract)
                ErrorCode.CannotCreate.ReportAndThrow(point, "Type {0} is abstract, cannot create instance", type.ParsedType.FullName);

            var methods = AssemblyRegistry.GetConstructors(context.Parser.Assembly, type.ParsedType);
            if(methods.Count == 0)
            {
                if (resolved.IsValueType && args.Count() == 0)
                    return new ValueCreationNode(resolved, point);

                ErrorCode.CannotCreate.ReportAndThrow(point, "No constructor for {0} found, cannot create instance", type.ParsedType.FullName);
            }
            var method = AssemblyRegistry.GetCompatibleMethod(methods, args.Select(a => a.ExpressionReturnType).ToList());

            if(method == null)
            {
                if(methods.Count == 1)
                {
                    ErrorCode.TypeMissmatch.ReportAndThrow(point, "Cannot call constructor for {0} requires parameters ({1}), called with ({2})",
                        type.ParsedType.FullName,
                        String.Join(", ", methods.Single().Parameters.Select(p => p.ParameterType.FullName)),
                        String.Join(", ", args.Select(a => a.ExpressionReturnType.FullName)));
                }
                else
                {
                    ErrorCode.TypeMissmatch.ReportAndThrow(point, "Cannot call constructor for {0} with arguments ({1}), none of the overloads match",
                        type.ParsedType.FullName,
                        String.Join(", ", args.Select(a => a.ExpressionReturnType.FullName)));
                }
            }

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
