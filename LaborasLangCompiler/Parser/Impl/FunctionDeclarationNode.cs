using LaborasLangCompiler.ILTools;
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
    class FunctionDeclarationNode : ParserNode, Context, MethodWrapper
    {
        public MethodReference MethodReference { get { return emitter.Get(); } }
        public MemberReference MemberReference { get { return MethodReference; } }
        public override NodeType Type { get { return NodeType.ParserInternal; } }
        public TypeReference FunctorType { get { return functorType.Value; } }
        public bool IsStatic { get { return true; } }
        public IEnumerable<TypeReference> ParamTypes { get; private set; }
        public TypeReference MethodReturnType { get; private set; }
        public TypeReference DeclaringType { get; private set; }

        private AstNode body;
        private CodeBlockNode parsedBody;
        private MethodEmitter emitter;
        private ClassNode parent;
        private Dictionary<string, ParameterWrapper> symbols;
        private Parser parser;
        private Lazy<TypeReference> functorType;
        private Modifiers modifiers;

        private FunctionDeclarationNode(Parser parser, Context parent, Modifiers modifiers, string name, AstNode method)
            : base(parser.GetSequencePoint(method))
        {
            this.parent = parent.GetClass();
            this.symbols = new Dictionary<string, ParameterWrapper>();
            this.parser = parser;
            this.body = method.Children[1];
            this.DeclaringType = parent.GetClass().TypeReference;
            ParseHeader(modifiers, method.Children[0], name);
            this.functorType = new Lazy<TypeReference>(() => AssemblyRegistry.GetFunctorType(parser.Assembly, emitter.Get()));
        }

        public void Emit()
        {
            parsedBody = CodeBlockNode.Parse(parser, this, body);
            if (MethodReturnType.FullName != parser.Void.FullName && !parsedBody.Returns)
                throw new ParseException(SequencePoint, "Not all control paths return a value");
            if(parser.ShouldEmit)
                emitter.ParseTree(parsedBody);
        }

        private void ParseHeader(Modifiers mods, AstNode lexerNode, string methodName)
        {
            var builder = new TypeNode.TypeBuilder(parser, parent);
            int count = lexerNode.ChildrenCount;
            var paramz = ParseParams(parser, parent, lexerNode.Children[count - 1]);
            for (int i = 0; i < count - 1; i++)
            {
                builder.Append(lexerNode.Children[i]);
            }

            MethodReturnType = builder.Type;
            emitter = new MethodEmitter(parent.TypeEmitter, methodName, MethodReturnType, AttributesFromModifiers(parser.GetSequencePoint(lexerNode), mods));

            foreach(var p in paramz)
            {
                var param = ParseParameter(parent, p.Type, p.Name);
                if (Utils.IsVoid(param.TypeReference))
                    throw new TypeException(parser.GetSequencePoint(p.Type), "Cannot declare a parameter of type void");
                emitter.AddArgument(param.ParameterDefinition);
                symbols.Add(param.Name, param);
            }
            ParamTypes = symbols.Values.Select(p => p.TypeReference);

            if (mods.HasFlag(Modifiers.Entry))
            {
                if (MethodReturnType != parser.Int32 && MethodReturnType != parser.UInt32 && MethodReturnType != parser.Void)
                    throw new TypeException(parser.GetSequencePoint(lexerNode), "Illegal entrypoint return type {0}, must be int, uint or void", MethodReturnType.FullName);
                emitter.SetAsEntryPoint();
                if(ParamTypes.Count() == 1 && ParamTypes.First().FullName != parser.Assembly.TypeToTypeReference(typeof(String[])).FullName)
                {
                    throw new TypeException(parser.GetSequencePoint(lexerNode), "Illegal entrypoint parameter type {0}, must be parameterless or string[]", ParamTypes.First().FullName);
                }
                else if(ParamTypes.Count() > 1)
                {
                    throw new TypeException(parser.GetSequencePoint(lexerNode), "Illegal entrypoint parameter type {0}, must be parameterless or string[]", ParamTypes.First().FullName);
                }
            }

        }

        private ParameterWrapper ParseParameter(Context parent, AstNode typeNode, AstNode nameNode)
        {
            var type = TypeNode.Parse(parser, parent, typeNode);
            var name = nameNode.GetSingleSymbolOrThrow();
            return new ParameterWrapper(name, ParameterAttributes.None, type);
        }

        public FunctionDeclarationNode GetMethod() { return this; }

        public ClassNode GetClass() { return parent.GetClass(); }

        public ExpressionNode GetSymbol(string name, Context scope, SequencePoint point)
        {
            if (symbols.ContainsKey(name))
                return new FunctionArgumentNode(symbols[name], true, point);

            return parent.GetSymbol(name, scope, point); 
        }

        public bool IsStaticContext()
        {
            return IsStatic;
        }

        public static FunctionDeclarationNode ParseAsFunctor(Parser parser, Context parent, AstNode function)
        {
            var instance = new FunctionDeclarationNode(parser, parent, Modifiers.NoInstance | Modifiers.Private, parent.GetClass().NewFunctionName(), function);
            parent.GetClass().AddLambda(instance);
            instance.Emit();
            return instance;
        }

        public static FunctionDeclarationNode ParseAsMethod(Parser parser, ClassNode parent, DeclarationInfo declaration)
        {
            var instance = new FunctionDeclarationNode(parser, parent, declaration.Modifiers, declaration.SymbolName.GetSingleSymbolOrThrow(), declaration.Initializer.Children[0]);
            return instance;
        }

        public static TypeReference ParseFunctorType(Parser parser, Context parent, AstNode lexerNode)
        {
            var builder = new TypeNode.TypeBuilder(parser, parent);
            int count = lexerNode.ChildrenCount;
            for (int i = 0; i < count - 1; i++)
            {
                builder.Append(lexerNode.Children[i]);
            }
            builder.Append(ParseParams(parser, parent, lexerNode.Children[count - 1]).Select(p => TypeNode.Parse(parser, parent, p.Type)));

            return builder.Type;
        }

        private MethodAttributes AttributesFromModifiers(SequencePoint point, Modifiers modifiers)
        {
            MethodAttributes ret = 0;
            if(!modifiers.HasAccess())
            {
                modifiers |= Modifiers.Private;
            }
            if(!modifiers.HasStorage())
            {
                modifiers |= Modifiers.NoInstance;
            }

            if (modifiers.HasFlag(Modifiers.Private))
            {
                ret |= MethodAttributes.Private;
            }
            else if(modifiers.HasFlag(Modifiers.Public))
            {
                if (modifiers.HasFlag(Modifiers.Private))
                    throw new ParseException(point, "Illegal method declaration, only one access modifier allowed");
                else
                    ret |= MethodAttributes.Public;
            }
            else if(modifiers.HasFlag(Modifiers.Protected))
            {
                if (modifiers.HasFlag(Modifiers.Private | Modifiers.Public))
                    throw new ParseException(point, "Illegal method declaration, only one access modifier allowed");
                else
                    ret |= MethodAttributes.Family;
            }

            if(modifiers.HasFlag(Modifiers.NoInstance))
            {
                ret |= MethodAttributes.Static;
            }
            else
            {
                throw new NotImplementedException("Only static methods allowed");
            }

            this.modifiers = modifiers;
            return ret;
        }

        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("Method:");
            builder.Indent(indent + 1).AppendLine("Name:");
            builder.Indent(indent + 2).AppendLine(MethodReference.Name);
            builder.Indent(indent + 1).AppendLine("Modifiers:");
            builder.Indent(indent + 2).AppendLine(modifiers.ToString());
            builder.Indent(indent + 1).AppendLine("Return:");
            builder.Indent(indent + 2).AppendLine(MethodReturnType.FullName);
            builder.Indent(indent + 1).AppendLine("Params:");
            foreach(var param in MethodReference.Parameters)
            {
                builder.Indent(indent + 2).AppendFormat("{0} {1}{2}", param.ParameterType.FullName, param.Name, Environment.NewLine);
            }
            builder.Indent(indent + 1).AppendLine("Body:");
            builder.AppendLine(parsedBody.ToString(indent + 2));
            return builder.ToString();
        }

        private static List<FunctionParamInfo> ParseParams(Parser parser, Context container, AstNode lexerNode)
        {
            var ret = new List<FunctionParamInfo>();
            int i = 1;
            while (i < lexerNode.ChildrenCount)
            {
                var param = lexerNode.Children[i];
                switch (param.Type)
                {
                    case Lexer.TokenType.LeftParenthesis:
                    case Lexer.TokenType.RightParenthesis:
                    case Lexer.TokenType.Comma:
                        i++;
                        break;
                    case Lexer.TokenType.Type:
                        var next = lexerNode.Children[i + 1];
                        if (next.Type != Lexer.TokenType.Symbol)
                            throw new ParseException(parser.GetSequencePoint(lexerNode), "Not a valid method definition, {0}", lexerNode.FullContent);
                        else
                            ret.Add(new FunctionParamInfo(param, next));
                        i += 2;
                        break;
                    default:
                        throw new ParseException(parser.GetSequencePoint(lexerNode), "Unexpected node type, {0} in {1}", param.Type, lexerNode.FullContent);
                }
            }
            return ret;
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