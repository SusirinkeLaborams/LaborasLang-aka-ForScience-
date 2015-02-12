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
        private Context parent;

        private AmbiguousMethodNode(IEnumerable<MethodReference> methods, ExpressionNode instance, Context parent, SequencePoint sequencePoint)
            : base(null, parent, sequencePoint)
        {
            this.methods = methods;
            this.instance = instance;
            this.parent = parent;
        }

        public ExpressionNode RemoveAmbiguity(Parser parser, TypeReference expectedType)
        {
            if (!expectedType.IsFunctorType())
            {
                return this;
            }
            var paramz = MetadataHelpers.GetFunctorParamTypes(parser.Assembly, expectedType);
            var method = AssemblyRegistry.GetCompatibleMethod(methods.ToList(), paramz);
            return new MethodNode(parser, method, instance, parent, SequencePoint);
        }

        public MethodNode RemoveAmbiguity(Parser parser, IEnumerable<TypeReference> args)
        {
            var method = AssemblyRegistry.GetCompatibleMethod(methods.ToList(), args.ToList());
            if (method == null)
                return null;
            return new MethodNode(parser, method, instance, parent, SequencePoint);
        }

        public static ExpressionNode Create(Parser parser, IEnumerable<MethodReference> methods, Context parent, ExpressionNode instance, SequencePoint sequencePoint)
        {
            if(methods.Count() == 0)
            {
                //should never happen
                ErrorCode.SymbolNotFound.ReportAndThrow(sequencePoint, "Method not found");
                return null;//unreachable
            }
            else if(methods.Count() == 1)
            {
                return new MethodNode(parser, methods.Single(), instance, parent, sequencePoint);
            }
            else
            {
                return new AmbiguousMethodNode(methods, instance, parent, sequencePoint);
            }
        }

        public override string ToString(int indent)
        {
            throw new InvalidOperationException();
        }
    }
}
