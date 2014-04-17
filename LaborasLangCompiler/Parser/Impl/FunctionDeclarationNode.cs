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
            instance.emitter = new MethodEmitter(parentClass.TypeEmitter, "$" + name, header.ReturnType, MethodAttributes.Static | MethodAttributes.Private);
            instance.Function = instance.emitter.Get();
            return instance;
        }
        public static TypeReference ParseType(Parser parser, ClassNode parentClass, CodeBlockNode parentBlock, AstNode lexerNode)
        {
            return FunctionHeader.Parse(parser, parentClass, parentBlock, lexerNode.Children[0]).FunctionType;
        }
    }
}
