using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.Parser.Exceptions;
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
    class AmbiguousMethodNode : SymbolNode, AmbiguousNode
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
                throw new TypeException(SequencePoint, "Cannot cast functor to type {0}", expectedType.FullName);
            try
            {
                var paramz = ILHelpers.GetFunctorParamTypes(parser.Assembly, expectedType);
                var method = AssemblyRegistry.GetCompatibleMethod(methods.ToList(), paramz);
                return new MethodNode(parser, method, instance, parent, SequencePoint);
            }
            catch (Exception)
            {
                throw new TypeException(SequencePoint, "Ambiguous method result");
            }
        }

        public MethodNode RemoveAmbiguity(Parser parser, IEnumerable<TypeReference> args)
        {
            try
            {
                var method = AssemblyRegistry.GetCompatibleMethod(methods.ToList(), args.ToList());
                return new MethodNode(parser, method, instance, parent, SequencePoint);
            }
            catch (Exception)
            {
                throw new TypeException(SequencePoint, "Ambiguous method result");
            }
        }

        public static ExpressionNode Create(Parser parser, IEnumerable<MethodReference> methods, Context parent, ExpressionNode instance, SequencePoint sequencePoint)
        {
            if(methods.Count() == 1)
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
