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
    class FunctionDeclarationNode : ParserNode, IContainerNode, MethodWrapper
    {
        public MethodReference MethodReference { get { return emitter.Get(); } }
        public override NodeType Type { get { return NodeType.ParserInternal; } }
        public TypeWrapper FunctorType 
        {
            get
            { 
                if(functorType == null)
                {
                    functorType = new FunctorTypeWrapper(parser.Assembly, MethodReturnType, ArgumentTypes);
                }
                return functorType;
            }
        }
        public bool IsStatic { get { return true; } }
        public IEnumerable<TypeWrapper> ArgumentTypes { get; private set; }
        private CodeBlockNode body;
        private MethodEmitter emitter;
        public TypeWrapper MethodReturnType { get; private set; }
        private ClassNode parent;
        private Dictionary<string, ParameterWrapper> symbols;
        private Parser parser;
        private TypeWrapper functorType;
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
            this.symbols = new Dictionary<string, ParameterWrapper>();
            this.parser = parser;
            ParseHeader(header, name != null ? name : this.parent.NewFunctionName());
        }

        private void ParseHeader(AstNode lexerNode, string name)
        {
            MethodReturnType = TypeNode.Parse(parser, parent, lexerNode.Children[0]);
            emitter = new MethodEmitter(parent.TypeEmitter, name, MethodReturnType.TypeReference, MethodAttributes.Static | MethodAttributes.Private);
            for(int i = 1; i < lexerNode.Children.Count; i++)
            {
                var param = ParseParameter(parent, lexerNode.Children[i]);
                emitter.AddArgument(param.ParameterDefinition);
                symbols.Add(param.Name, param);
            }
            ArgumentTypes = symbols.Select(arg => arg.Value.TypeWrapper);
            parent.AddMethod(this, name);
        }
        private ParameterWrapper ParseParameter(IContainerNode parent, AstNode lexerNode)
        {
            var type = TypeNode.Parse(parser, parent, lexerNode.Children[0]);
            var name = parser.ValueOf(lexerNode.Children[1]);
            return new ParameterWrapper(name, ParameterAttributes.None, type);
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
            StringBuilder builder = new StringBuilder("(Method: ");
            builder.Append(MethodReference.Name).Append(" ");
            builder.Append(MethodReturnType).Append("(");
            string delim = "";
            foreach(var arg in emitter.Get().Parameters)
            {
                builder.AppendFormat("{0}{1} {2}", delim, arg.ParameterType, arg.Name);
                delim = ", ";
            }
            builder.Append(")").Append(body.ToString()).Append(")");
            return builder.ToString();
        }
        public static FunctorTypeWrapper ParseFunctorType(Parser parser, IContainerNode parent, AstNode lexerNode)
        {
            var header = lexerNode.Children[0];
            var ret = TypeNode.Parse(parser, parent, header.Children[0]);
            var args = new List<TypeWrapper>();
            for (int i = 1; i < header.Children.Count; i++)
            {
                var arg = header.Children[i];
                args.Add(TypeNode.Parse(parser, parent, arg.Children[0]));
            }
            return new FunctorTypeWrapper(parser.Assembly, ret, args);
        }
    }
}
