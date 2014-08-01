using LaborasLangCompiler.ILTools.Methods;
using LaborasLangCompiler.ILTools.Types;
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
    class FunctionDeclarationNode : RValueNode, IFunctionNode, IContainerNode, MethodWrapper
    {
        public IExpressionNode ObjectInstance { get { return null; } }
        public MethodReference Function { get { return emitter.Get(); } }
        public MethodReference MethodReference { get { return emitter.Get(); } }
        public override RValueNodeType RValueType { get { return RValueNodeType.Function; } }
        public override TypeReference ReturnType { get { return ILTools.AssemblyRegistry.GetFunctorType(parser.Assembly, MethodReference); } }
        public IEnumerable<TypeReference> ArgumentTypes { get { return MethodReference.Parameters.Select(p => p.ParameterType); } }
        public TypeReference ResultType { get { return ReturnType; } }
        private CodeBlockNode body;
        private MethodEmitter emitter;
        public TypeReference MethodReturnType { get; private set; }
        private ClassNode parent;
        private Dictionary<string, ParameterDefinition> symbols;
        private Parser parser;
        public void Emit(bool entry = false)
        {
            emitter.ParseTree(body);
            if(entry)
                emitter.SetAsEntryPoint();
        }
        public FunctionDeclarationNode(Parser parser, IContainerNode parent, SequencePoint point, AstNode header, string name = null)
            : base(point)
        {
            this.parent = parent.GetClass();
            this.symbols = new Dictionary<string, ParameterDefinition>();
            this.parser = parser;
            ParseHeader(header, name != null ? name : this.parent.NewFunctionName());
        }

        private void ParseHeader(AstNode lexerNode, string name)
        {
            MethodReturnType = TypeNode.Parse(parser, parent, lexerNode.Children[0]);
            emitter = new MethodEmitter(parent.TypeEmitter, name, MethodReturnType, MethodAttributes.Static | MethodAttributes.Private);
            for(int i = 1; i < lexerNode.Children.Count; i++)
            {
                emitter.AddArgument(ParseParameter(parent, lexerNode.Children[i]));
            }
            parent.AddMethod(this, name);
            symbols = MethodReference.Parameters.ToDictionary(param => param.Name);
        }
        private ParameterDefinition ParseParameter(IContainerNode parent, AstNode lexerNode)
        {
            var type = TypeNode.Parse(parser, parent, lexerNode.Children[0]);
            var name = parser.ValueOf(lexerNode.Children[1]);
            return new ParameterDefinition(name, ParameterAttributes.None, type);
        }
        public void ParseBody(AstNode body)
        {
            this.body = CodeBlockNode.Parse(parser, this, body);
            if(MethodReturnType.FullName != "System.Void" && !this.body.Returns)
                throw new ParseException(SequencePoint, "Not all control paths return a value");
        }
        public FunctionDeclarationNode GetFunction() { return this; }
        public ClassNode GetClass() { return parent.GetClass(); }
        public LValueNode GetSymbol(string name, SequencePoint point)
        {
            if (symbols.ContainsKey(name))
                return new FunctionArgumentNode(symbols[name], true, point);

            return parent.GetSymbol(name, point); 
        }
        public static FunctionDeclarationNode Parse(Parser parser, IContainerNode parent, AstNode lexerNode, string name = null)
        {
            var instance = new FunctionDeclarationNode(parser, parent, parser.GetSequencePoint(lexerNode), lexerNode.Children[0], name);
            instance.ParseBody(lexerNode.Children[1]);
            return instance;
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
