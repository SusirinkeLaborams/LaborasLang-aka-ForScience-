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
    class AmbiguousMethodNode : RValueNode, IFunctionNode
    {
        public override TypeReference ReturnType { get { return null; } }
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.Intermediate; } }
        public override RValueNodeType RValueType { get { return RValueNodeType.Function; } }
        public MethodReference Function { get { return null; } }
        public IExpressionNode ObjectInstance { get; private set; }
        private List<MethodReference> methods;
        public AmbiguousMethodNode(List<MethodReference> methods, IExpressionNode instance, SequencePoint sequencePoint)
            : base(sequencePoint)
        {
            this.methods = methods;
            ObjectInstance = instance;
        }
        public MethodNode RemoveAmbiguity(Parser parser, List<TypeReference> argTypes)
        {
            MethodReference method = null;
            try
            {
                method = AssemblyRegistry.GetCompatibleMethod(methods, argTypes);
                //TODO: make this lazy
                return new MethodNode(new ExternalMethod(method, parser.Assembly), ObjectInstance, SequencePoint);
            }
            catch (Exception)
            {
                throw new TypeException(SequencePoint, "Ambiguous method result");
            }
        }
    }
}
