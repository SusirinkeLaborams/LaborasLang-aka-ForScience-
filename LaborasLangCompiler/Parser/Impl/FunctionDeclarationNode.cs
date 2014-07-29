using LaborasLangCompiler.ILTools.Methods;
using LaborasLangCompiler.ILTools.Types;
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
    class FunctionDeclarationNode : RValueNode, IFunctionNode, IContainerNode
    {
        public IExpressionNode ObjectInstance { get { return null; } }
        public MethodReference Function { get; private set; }
        public override RValueNodeType RValueType { get { return RValueNodeType.Function; } }
        public override TypeReference ReturnType { get; set; }
        private CodeBlockNode body;
        private MethodEmitter emitter;
        public TypeReference FunctionReturnType { get; private set; }
        private ClassNode parent;
        public IReadOnlyList<FunctionArgumentNode> Args;
        public void Emit(bool entry = false)
        {
            emitter.ParseTree(body);
            if(entry)
                emitter.SetAsEntryPoint();
        }
        private FunctionDeclarationNode(IContainerNode parent, SequencePoint point)
            : base(point)
        {
            this.parent = parent.GetClass();
        }
        public FunctionDeclarationNode GetFunction() { return this; }
        public ClassNode GetClass() { return parent.GetClass(); }
        public LValueNode GetSymbol(string name) { return parent.GetSymbol(name); }
        public static FunctionDeclarationNode Parse(Parser parser, IContainerNode parent, AstNode lexerNode, string name = null)
        {
            var instance = new FunctionDeclarationNode(parent, parser.GetSequencePoint(lexerNode));
            var header = FunctionHeader.Parse(parser, parent, lexerNode.Children[0]);
            if (name == null)
                name = instance.parent.NewFunctionName();
            instance.ReturnType = header.FunctionType;
            instance.FunctionReturnType = header.ReturnType;
            instance.Args = header.Args;
            instance.body = CodeBlockNode.Parse(parser, instance, lexerNode.Children[1]);
            if (instance.FunctionReturnType.FullName != "System.Void" && !instance.body.Returns)
                throw new ParseException(instance.SequencePoint, "Not all control paths return a value");
            instance.emitter = new MethodEmitter(instance.parent.TypeEmitter, "$" + name, header.ReturnType, MethodAttributes.Static | MethodAttributes.Private);
            foreach (var arg in header.Args)
                instance.emitter.AddArgument(arg.Param);
            instance.Function = instance.emitter.Get();
            return instance;
        }
        public static TypeReference ParseType(Parser parser, IContainerNode parent, AstNode lexerNode)
        {
            return FunctionHeader.Parse(parser, parent, lexerNode.Children[0]).FunctionType;
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
