using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.LexingTools;
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
    class MethodNode : RValueNode, IFunctionNode
    {
        public override RValueNodeType RValueType { get { return RValueNodeType.Function; } }
        public override TypeWrapper ReturnType { get { return Function.MethodReturnType; } }
        public IExpressionNode ObjectInstance { get; private set; }
        public MethodWrapper Function { get; private set; }
        public MethodNode(MethodWrapper method, IExpressionNode instance, SequencePoint point)
            : base(point)
        {
            Function = method;
            this.ObjectInstance = instance;
        }
        public static MethodNode Parse(Parser parser, IContainerNode parent, AstNode lexerNode, string name = null)
        {
            var method = FunctionDeclarationNode.Parse(parser, parent, lexerNode, name);
            return new MethodNode(method, null, method.SequencePoint);
        }
        public override string ToString()
        {
            return String.Format("(MethodNode: Instance: {0}, Method: {1})", ObjectInstance == null ? "null" : ObjectInstance.ToString(), method.MethodReference.Name);
        }
    }
}
