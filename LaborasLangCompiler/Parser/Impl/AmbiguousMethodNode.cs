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

namespace LaborasLangCompiler.Parser.Impl
{
    class AmbiguousMethodNode : SymbolNode, IAmbiguousNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.ParserInternal; } }

        private IEnumerable<MethodReference> methods;
        private ExpressionNode instance;

        private AmbiguousMethodNode(IEnumerable<MethodReference> methods, ExpressionNode instance, ContextNode context, SequencePoint sequencePoint)
            : base(null, context, sequencePoint)
        {
            this.methods = methods;
            this.instance = instance;
        }

        public ExpressionNode RemoveAmbiguity(ContextNode context, TypeReference expectedType)
        {
            if (!expectedType.IsFunctorType())
            {
                return this;
            }
            var paramz = MetadataHelpers.GetFunctorParamTypes(context.Parser.Assembly, expectedType);
            var method = AssemblyRegistry.GetCompatibleMethod(methods.ToList(), paramz);
            return new MethodNode(method, instance, context, SequencePoint);
        }

        public MethodNode RemoveAmbiguity(ContextNode context, IEnumerable<TypeReference> args)
        {
            var method = AssemblyRegistry.GetCompatibleMethod(methods.ToList(), args.ToList());
            if (method == null)
                return null;
            return new MethodNode(method, instance, context, SequencePoint);
        }

        public static ExpressionNode Create(IEnumerable<MethodReference> methods, ContextNode context, ExpressionNode instance, SequencePoint sequencePoint)
        {
            if(methods.Count() == 0)
            {
                //should never happen
                ErrorCode.SymbolNotFound.ReportAndThrow(sequencePoint, "Method not found");
                return null;//unreachable
            }
            else if(methods.Count() == 1)
            {
                return new MethodNode(methods.Single(), instance, context, sequencePoint);
            }
            else
            {
                return new AmbiguousMethodNode(methods, instance, context, sequencePoint);
            }
        }

        public override string ToString(int indent)
        {
            throw new InvalidOperationException();
        }
    }
}
