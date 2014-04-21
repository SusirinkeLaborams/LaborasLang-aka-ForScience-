using LaborasLangCompiler.ILTools.Methods;
using LaborasLangCompiler.ILTools.Types;
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
    class FunctionDeclarationNode : RValueNode, IFunctionNode
    {
        public IExpressionNode ObjectInstance { get { return null; } }
        public MethodReference Function { get; private set; }
        public override RValueNodeType RValueType { get { return RValueNodeType.Function; } }
        public override TypeReference ReturnType { get; set; }
        private CodeBlockNode body;
        private MethodEmitter emitter;
        public TypeReference FunctionReturnType { get; private set; }
        public ClassNode ParentClass { get; private set; }
        public IReadOnlyList<FunctionArgumentNode> Args;
        public void Emit(bool entry = false)
        {
            emitter.ParseTree(body);
            if(entry)
                emitter.SetAsEntryPoint();
        }
        public static FunctionDeclarationNode Parse(Parser parser, ClassNode parentClass, CodeBlockNode parentBlock, AstNode lexerNode, string name = null)
        {
            if (name == null)
                name = parentClass.NewFunctionName();
            var instance = new FunctionDeclarationNode();
            var header = FunctionHeader.Parse(parser, parentClass, null, lexerNode.Children[0]);
            instance.body = CodeBlockNode.Parse(parser, parentClass, null, lexerNode.Children[1], header.Args);
            instance.ReturnType = header.FunctionType;
            instance.FunctionReturnType = header.ReturnType;
            instance.ParentClass = parentClass;
            instance.Args = header.Args;
            instance.emitter = new MethodEmitter(parentClass.TypeEmitter, "$" + name, header.ReturnType, MethodAttributes.Static | MethodAttributes.Private);
            foreach (var arg in header.Args)
                instance.emitter.AddArgument(arg.Param);
            instance.Function = instance.emitter.Get();
            return instance;
        }
        public static TypeReference ParseType(Parser parser, ClassNode parentClass, CodeBlockNode parentBlock, AstNode lexerNode)
        {
            return FunctionHeader.Parse(parser, parentClass, parentBlock, lexerNode.Children[0]).FunctionType;
        }
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder("(Function: ");
            builder.Append(ReturnType).Append("(");
            string delim = "";
            foreach(var arg in emitter.Get().Parameters)
            {
                builder.Append(String.Format("{0}{1} {2}", delim, arg.ParameterType, arg.Name));
                delim = ", ";
            }
            builder.Append(")").Append(body.ToString()).Append(")");
            return builder.ToString();
        }
    }
}
