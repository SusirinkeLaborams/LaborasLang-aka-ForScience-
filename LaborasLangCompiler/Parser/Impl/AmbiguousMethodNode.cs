using LaborasLangCompiler.Common;
using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

namespace LaborasLangCompiler.Parser.Impl
{
    class AmbiguousMethodNode : SymbolNode, IAmbiguousNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.ParserInternal; } }

        private readonly IEnumerable<MethodReference> methods;
        private readonly ExpressionNode instance;

        private AmbiguousMethodNode(IEnumerable<MethodReference> methods, ExpressionNode instance, ContextNode context, SequencePoint sequencePoint)
            : base(GetReadableName(methods.First()), context, sequencePoint)
        {
            this.methods = methods;
            this.instance = instance;
            var first = methods.First();
        }

        public ExpressionNode RemoveAmbiguity(ContextNode context, TypeReference expectedType)
        {
            if (!expectedType.IsFunctorType())
            {
                return this;
            }
            try
            {
                var paramz = MetadataHelpers.GetFunctorParamTypes(context.Parser.Assembly, expectedType);
                var method = AssemblyRegistry.GetCompatibleMethod(methods.ToList(), paramz);
                return new MethodNode(method, instance, context, SequencePoint);
            }
            catch(AssemblyRegistry.AmbiguousMethodException)
            {
                return this;
            }
        }

        public MethodNode RemoveAmbiguity(ContextNode context, IEnumerable<TypeReference> args)
        {
            try
            {
                var method = AssemblyRegistry.GetCompatibleMethod(methods.ToList(), args.ToList());
                if (method == null)
                    return null;
                return new MethodNode(method, instance, context, SequencePoint);
            }
            catch(AssemblyRegistry.AmbiguousMethodException e)
            {
                ErrorCode.AmbiguousMethod.ReportAndThrow(SequencePoint,
                    "Method {0} is ambiguous with arguments ({1}), possible matches: \r\n {2}",
                    Name, 
                    String.Join(", ", args.Select(a => a.FullName)),
                    String.Join("\r\n", e.Matches.Select(match => match.FullName)));
                return Utils.Utils.Fail<MethodNode>();
            }
        }

        public static ExpressionNode Create(IEnumerable<MethodReference> methods, ContextNode context, ExpressionNode instance, SequencePoint sequencePoint)
        {
            Contract.Requires(methods.Any());
            Contract.Ensures(Contract.Result<ExpressionNode>() != null);
            if(methods.Count() == 1)
            {
                return new MethodNode(methods.Single(), instance, context, sequencePoint);
            }
            else
            {
                return new AmbiguousMethodNode(methods, instance, context, sequencePoint);
            }
        }

        private static string GetReadableName(MethodReference method)
        {
            return method.DeclaringType.Name + "::" + method.Name;
        }

        public override string ToString(int indent)
        {
            throw new InvalidOperationException();
        }
    }
}
