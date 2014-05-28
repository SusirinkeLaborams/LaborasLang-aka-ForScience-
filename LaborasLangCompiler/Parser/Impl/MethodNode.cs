using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.LexingTools;
using LaborasLangCompiler.Parser.Exceptions;
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
    class MethodNode : RValueNode, IFunctionNode
    {
        public override RValueNodeType RValueType { get { return RValueNodeType.Function; } }
        public override TypeReference ReturnType { get; set; }
        public IExpressionNode ObjectInstance { get; private set; }
        public MethodReference Function { get; private set; }
        public MethodNode(MethodReference method, TypeReference type, IExpressionNode instance, SequencePoint point)
            : base(point)
        {
            Function = method;
            ReturnType = type;
            ObjectInstance = instance;
        }
        public override string ToString()
        {
            return String.Format("(Method: Instance: {0}, Name: {1})", ObjectInstance == null ? "null" : ObjectInstance.ToString(), Function.FullName);
        }
    }
}
