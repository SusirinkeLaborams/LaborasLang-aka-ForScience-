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
    class AmbiguousMethodNode : RValueNode
    {
        public override TypeWrapper TypeWrapper { get { return null; } }
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.ParserInternal; } }
        public override RValueNodeType RValueType { get { return RValueNodeType.ParserInternal; } }

        private IEnumerable<MethodWrapper> methods;
        private ExpressionNode instance;
        public AmbiguousMethodNode(IEnumerable<MethodWrapper> methods, ExpressionNode instance, SequencePoint sequencePoint)
            : base(sequencePoint)
        {
            this.methods = methods;
            this.instance = instance;
        }
        public MethodNode RemoveAmbiguity(Parser parser, IEnumerable<TypeWrapper> argTypes)
        {
            MethodReference method = null;
            try
            {
                method = AssemblyRegistry.GetCompatibleMethod(methods.Select(m => m.MethodReference), argTypes.Select(t => t.TypeReference).ToList());
                return new MethodNode(new ExternalMethod(parser.Assembly, method), instance, SequencePoint);
            }
            catch (Exception)
            {
                throw new TypeException(SequencePoint, "Ambiguous method result");
            }
        }
    }
}
