using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.LexingTools;
using LaborasLangCompiler.Parser.Exceptions;
using Mono.Cecil;
using NPEG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    class MethodNode : RValueNode, IFunctionNode
    {
        public override RValueNodeType RValueType { get { return RValueNodeType.Function; } }
        public override TypeReference ReturnType { get; set; }
        public IExpressionNode ObjectInstance { get; private set; }
        public MethodReference Function { get; private set; }
        public MethodNode(MethodReference method, TypeReference type)
        {
            Function = method;
            ReturnType = type;
        }
        public static MethodNode Parse(Parser parser, IContainerNode parent, AstNode lexerNode, List<TypeReference> args)
        {
            var full = parser.ValueOf(lexerNode);
            var index = full.LastIndexOf('.');
            var type = full.Substring(0, index);
            var name = full.Substring(index + 1);
            var method = AssemblyRegistry.GetCompatibleMethod(parser.Assembly, type.ToString(), name, args);
            return new MethodNode(method, AssemblyRegistry.GetFunctorType(parser.Assembly, method));
        }
        public override string ToString()
        {
            return String.Format("(Method: {0})", Function.FullName);
        }
    }
}
