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
        public override TypeWrapper TypeWrapper { get { return null; } }
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.ParserInternal; } }
        public override RValueNodeType RValueType { get { return RValueNodeType.Function; } }
        public MethodReference Function { get { return null; } }
        public IExpressionNode ObjectInstance { get; private set; }
        private IEnumerable<MethodWrapper> methods;
        public AmbiguousMethodNode(IEnumerable<MethodWrapper> methods, IExpressionNode instance, SequencePoint sequencePoint)
            : base(sequencePoint)
        {
            this.methods = methods;
            ObjectInstance = instance;
        }
        public MethodNode RemoveAmbiguity(Parser parser, IEnumerable<TypeWrapper> argTypes)
        {
            MethodReference method = null;
            try
            {
                method = AssemblyRegistry.GetCompatibleMethod(methods.Select(m => m.MethodReference), argTypes.Select(t => t.TypeReference).ToList());
                //TODO: make this lazy
                return new MethodNode(new ExternalMethod(parser.Assembly, method), ObjectInstance, SequencePoint);
            }
            catch (Exception)
            {
                throw new TypeException(SequencePoint, "Ambiguous method result");
            }
        }
    }
}
