using LaborasLangCompiler.ILTools;
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
    class AmbiguousMethodNode : RValueNode, AmbiguousNode
    {
        public override TypeWrapper TypeWrapper { get { return null; } }
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.RValue; } }
        public override RValueNodeType RValueType { get { return RValueNodeType.ParserInternal; } }

        private IEnumerable<MethodWrapper> methods;
        private ExpressionNode instance;
        public AmbiguousMethodNode(IEnumerable<MethodWrapper> methods, ExpressionNode instance, SequencePoint sequencePoint)
            : base(sequencePoint)
        {
            this.methods = methods;
            this.instance = instance;
        }
        public ExpressionNode RemoveAmbiguity(Parser parser, TypeWrapper expectedType = null)
        {
            if (!expectedType.IsFunctorType())
                throw new TypeException(SequencePoint, "Cannot cast functor to type {0}", expectedType.FullName);
            if (expectedType == null)
            {
                try
                {
                    var method = methods.Single();
                    return new MethodNode(method, instance, SequencePoint);
                }
                catch(InvalidOperationException)
                {
                    return this;
                }
            }
            else
            {
                try
                {
                    var method = AssemblyRegistry.GetCompatibleMethod(methods.Select(m => m.MethodReference), expectedType.FunctorParamTypes.Select(t => t.TypeReference).ToList());
                    return new MethodNode(new ExternalMethod(parser.Assembly, method), instance, SequencePoint);
                }
                catch(Exception)
                {
                    throw new TypeException(SequencePoint, "Ambiguous method result");
                }
            }
        }
    }
}
