using LaborasLangCompiler.ILTools.Methods;
using LaborasLangCompiler.ILTools.Types;
using LaborasLangCompiler.Parser.Exceptions;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Lexer.Containers;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    class FunctionDeclarationNode : ParserNode, ContainerNode, MethodWrapper
    {
        public MethodReference MethodReference { get { return emitter.Get(); } }
        public override NodeType Type { get { return NodeType.ParserInternal; } }
        public FunctorTypeWrapper FunctorType 
        {
            get
            { 
                if(functorType == null)
                {
                    functorType = new FunctorTypeWrapper(parser.Assembly, MethodReturnType, ParamTypes);
                }
                return functorType;
            }
        }
        public bool IsStatic { get { return true; } }
        public IEnumerable<TypeWrapper> ParamTypes { get; private set; }
        public TypeWrapper MethodReturnType { get; private set; }

        private CodeBlockNode body;
        private MethodEmitter emitter;
        private ClassNode parent;
        private Dictionary<string, ParameterWrapper> symbols;
        private Parser parser;
        private FunctorTypeWrapper functorType;

        private FunctionDeclarationNode(Parser parser, ContainerNode parent, SequencePoint point, AstNode header, string name = null)
            : base(point)
        {
            this.parent = parent.GetClass();
            this.symbols = new Dictionary<string, ParameterWrapper>();
            this.parser = parser;
            ParseHeader(header, name != null ? name : this.parent.NewFunctionName());
        }

        public void Emit(bool entry)
        {
            emitter.ParseTree(body);
            if(entry)
                emitter.SetAsEntryPoint();
        }

        private void ParseHeader(AstNode lexerNode, string methodName)
        {
            var info = new FunctionDeclarationInfo(parser, lexerNode);
            MethodReturnType = TypeNode.Parse(parser, parent, info.ReturnType); 
            emitter = new MethodEmitter(parent.TypeEmitter, methodName, MethodReturnType.TypeReference, MethodAttributes.Static | MethodAttributes.Private);
            foreach(var p in info.Params)
            {
                var param = ParseParameter(parent, p.Type, p.Name);
                emitter.AddArgument(param.ParameterDefinition);
                symbols.Add(param.Name, param);
            }
            ParamTypes = symbols.Values.Select(p => p.TypeWrapper);
            parent.AddMethod(this, methodName);
        }

        private ParameterWrapper ParseParameter(ContainerNode parent, AstNode typeNode, AstNode nameNode)
        {
            var type = TypeNode.Parse(parser, parent, typeNode);
            var name = nameNode.GetSingleSymbolOrThrow();
            return new ParameterWrapper(name, ParameterAttributes.None, type);
        }

        public void ParseBody(AstNode body)
        {
            this.body = CodeBlockNode.Parse(parser, this, body);
            if(MethodReturnType.FullName != parser.Void.FullName && !this.body.Returns)
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

        public static FunctionDeclarationNode Parse(Parser parser, ContainerNode parent, AstNode lexerNode, string name = null)
        {
            var instance = new FunctionDeclarationNode(parser, parent, parser.GetSequencePoint(lexerNode), lexerNode.Children[0], name);
            instance.ParseBody(lexerNode.Children[1]);
            return instance;
        }

        public static FunctorTypeWrapper ParseFunctorType(Parser parser, ContainerNode parent, AstNode lexerNode)
        {
            var info = new FunctionDeclarationInfo(parser, lexerNode.Children[0]);
            var ret = TypeNode.Parse(parser, parent, info.ReturnType);
            var args = info.Params.Select(p => TypeNode.Parse(parser, parent, p.Type));
            return new FunctorTypeWrapper(parser.Assembly, ret, args);
        }

        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("Method:");
            builder.Indent(indent + 1).AppendLine("Name:");
            builder.Indent(indent + 2).AppendLine(MethodReference.Name);
            builder.Indent(indent + 1).AppendLine("Return:");
            builder.Indent(indent + 2).AppendLine(MethodReturnType.FullName);
            builder.Indent(indent + 1).AppendLine("Params:");
            foreach(var param in MethodReference.Parameters)
            {
                builder.Indent(indent + 2).AppendFormat("{0} {1}{2}", param.ParameterType.FullName, param.Name, Environment.NewLine);
            }
            builder.Indent(indent + 1).AppendLine("Body:");
            builder.AppendLine(body.ToString(indent + 2));
            return builder.ToString();
        }

        class FunctionDeclarationInfo
        {
            public AstNode ReturnType { get; private set; }
            public List<FunctionParamInfo> Params { get; private set; }

            public FunctionDeclarationInfo(Parser parser, AstNode lexerNode)
            {
                ReturnType = lexerNode.Children[0];
                Params = new List<FunctionParamInfo>();
                int i = 1;
                while (i < lexerNode.ChildrenCount)
                {
                    var param = lexerNode.Children[i];
                    switch(param.Type)
                    {
                        case Lexer.TokenType.LeftParenthesis:
                        case Lexer.TokenType.RightParenthesis:
                        case Lexer.TokenType.Comma:
                            i++;
                            break;
                        case Lexer.TokenType.Type:
                            var next = lexerNode.Children[i + 1];
                            if(next.Type != Lexer.TokenType.Symbol)
                                throw new ParseException(parser.GetSequencePoint(lexerNode), "Not a valid method definition, {0}", lexerNode.FullContent);
                            else
                                Params.Add(new FunctionParamInfo(param, next));
                            i += 2;
                            break;
                        default:
                            throw new ParseException(parser.GetSequencePoint(lexerNode), "Unexpected node type, {0} in {1}", param.Type, lexerNode.FullContent);
                    }
                }
            }
        }
        struct FunctionParamInfo
        {
            public AstNode Type { get; private set; }
            public AstNode Name { get; private set; }

            public FunctionParamInfo(AstNode type, AstNode name)
            {
                Type = type;
                Name = name;
            }
        }
    }
}