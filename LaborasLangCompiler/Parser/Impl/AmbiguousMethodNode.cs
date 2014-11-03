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
        public override TypeWrapper TypeWrapper { get { return null; } }
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.ParserInternal; } }

        private IEnumerable<MethodWrapper> methods;
        private ExpressionNode instance;
        public AmbiguousMethodNode(IEnumerable<MethodWrapper> methods, ExpressionNode instance, TypeReference scope, SequencePoint sequencePoint)
            : base(null, scope, sequencePoint)
        {
            this.methods = methods;
            this.instance = instance;
        }
        public ExpressionNode RemoveAmbiguity(Parser parser, TypeWrapper expectedType)
        {
            if (!expectedType.IsFunctorType())
                throw new TypeException(SequencePoint, "Cannot cast functor to type {0}", expectedType.FullName);
            try
            {
                var method = AssemblyRegistry.GetCompatibleMethod(methods.Select(m => m.MethodReference), expectedType.FunctorParamTypes.Select(t => t.TypeReference).ToList());
                return new MethodNode(new ExternalMethod(parser.Assembly, method), instance, Scope, SequencePoint);
            }
            catch (Exception)
            {
                throw new TypeException(SequencePoint, "Ambiguous method result");
            }
        }

        public override string ToString(int indent)
        {
            throw new InvalidOperationException();
        }
    }
}
