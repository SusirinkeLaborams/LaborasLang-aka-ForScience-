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
        private TypeReference functionReturnType;
        public void Emit(TypeEmitter klass, string name)
        {
            var emitter = new MethodEmitter(klass, name + "_method", functionReturnType, MethodAttributes.Static | MethodAttributes.Private);
            emitter.ParseTree(body);
            emitter.SetAsEntryPoint();
            Function = emitter.Get();
        }
        public static new FunctionDeclarationNode Parse(Parser parser, ClassNode parentClass, CodeBlockNode parentBlock, AstNode lexerNode)
        {
            var instance = new FunctionDeclarationNode();
            var header = FunctionHeader.Parse(parser, parentClass, null, lexerNode.Children[0]);
            instance.body = CodeBlockNode.Parse(parser, parentClass, null, lexerNode.Children[1], header.Args);
            instance.ReturnType = header.FunctionType;
            instance.functionReturnType = header.ReturnType;
            return instance;
        }
        public static TypeReference ParseType(Parser parser, ClassNode parentClass, CodeBlockNode parentBlock, AstNode lexerNode)
        {
            return FunctionHeader.Parse(parser, parentClass, parentBlock, lexerNode.Children[0]).FunctionType;
        }
    }
}
