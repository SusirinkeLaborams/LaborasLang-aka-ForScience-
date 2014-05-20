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
    class AmbiguousMethodNode : RValueNode
    {
        public override TypeReference ReturnType { get; set; }
        public override RValueNodeType RValueType { get { return RValueNodeType.Function; } }
        private List<MethodReference> methods;
        public AmbiguousMethodNode(List<MethodReference> methods)
        {
            this.methods = methods;
            ReturnType = null;
        }
        public static MethodNode RemoveAmbiguity(Parser parser, AmbiguousMethodNode methods, List<TypeReference> argTypes)
        {
            MethodReference method = null;
            try
            {
                method = AssemblyRegistry.GetBestMatch(argTypes, methods.methods);
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
