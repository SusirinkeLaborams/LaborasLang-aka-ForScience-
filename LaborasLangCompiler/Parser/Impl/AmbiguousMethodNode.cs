using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.Parser.Exceptions;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    class AmbiguousMethodNode : RValueNode, IFunctionNode
    {
        public override TypeReference ReturnType { get; set; }
        public override RValueNodeType RValueType { get { return RValueNodeType.Function; } }
        public MethodReference Function { get { return null; } }
        public IExpressionNode ObjectInstance { get; private set; }
        private List<MethodReference> methods;
        public AmbiguousMethodNode(List<MethodReference> methods, IExpressionNode instance)
        {
            this.methods = methods;
            ReturnType = null;
            ObjectInstance = instance;
        }
        public MethodNode RemoveAmbiguity(Parser parser, List<TypeReference> argTypes)
        {
            MethodReference method = null;
            try
            {
                method = AssemblyRegistry.GetBestMatch(argTypes, methods);
                //TODO: make this lazy
                return new MethodNode(method, AssemblyRegistry.GetFunctorType(parser.Assembly, method));
            }
            catch(Exception e)
            {
                throw new TypeException("Ambiguous method result", e);
            }
        }
    }
}
