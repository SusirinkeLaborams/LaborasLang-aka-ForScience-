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
    class FunctionDeclarationNode : CodeBlockNode, IFunctionNode
    {
        public IExpressionNode ObjectInstance { get { return null; } }
        public MethodReference Function { get; set; }
        public RValueNodeType RValueType { get { return RValueNodeType.Function; } }
        public ExpressionNodeType ExpressionType { get { return ExpressionNodeType.RValue; } }
        public TypeReference ReturnType { get; set; }
        private FunctionDeclarationNode(CodeBlockNode parent) : base(parent)
        {
        }
        public static new FunctionDeclarationNode Parse(Parser parser, ClassNode parentClass, CodeBlockNode parentBlock, AstNode lexerNode)
        {
            var instance = new FunctionDeclarationNode(parentBlock);
            var header = FunctionHeader.Parse(parser, parentClass, parentBlock, lexerNode.Children[0]);
            throw new NotImplementedException();
        }
        public static TypeReference ParseType(Parser parser, ClassNode parentClass, CodeBlockNode parentBlock, AstNode lexerNode)
        {
            return FunctionHeader.Parse(parser, parentClass, parentBlock, lexerNode.Children[0]).FunctionType;
        }
    }
}
