using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Codegen.Methods;
using LaborasLangCompiler.Codegen.Types;
using LaborasLangCompiler.Parser.Utils;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using LaborasLangCompiler.Parser;
using Lexer.Containers;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaborasLangCompiler.Common;

namespace LaborasLangCompiler.Parser.Impl
{
    class FunctionDeclarationNode : ContextNode
    {
        public MethodReference MethodReference { get { return emitter.Get(); } }
        public override NodeType Type { get { return NodeType.ParserInternal; } }
        public IEnumerable<TypeReference> ParamTypes { get; private set; }
        public TypeReference MethodReturnType { get; private set; }

        private readonly AstNode body;
        private CodeBlockNode parsedBody;
        private IMethodEmitter emitter;
        private readonly Dictionary<string, ParameterDefinition> symbols = new Dictionary<string, ParameterDefinition>();
        private Modifiers modifiers;

        private FunctionDeclarationNode(ContextNode parent, Modifiers modifiers, string name, AstNode method)
            : base(parent.Parser, parent, parent.Parser.GetSequencePoint(method))
        {
            this.body = method.Children[1];
            ParseHeader(modifiers, method.Children[0], name);
        }

        public void Emit()
        {
            parsedBody = CodeBlockNode.Parse(this, body);
            if (MethodReturnType.FullName != Parser.Void.FullName && !parsedBody.Returns)
                ErrorCode.MissingReturn.ReportAndThrow(SequencePoint, "Not all control paths return a value");
            emitter.ParseTree(parsedBody);
        }

        private void ParseHeader(Modifiers mods, AstNode lexerNode, string methodName)
        {
            var point = Parser.GetSequencePoint(lexerNode);
            var builder = new TypeNode.TypeBuilder(Parent);
            int count = lexerNode.ChildrenCount;
            var paramz = ParseParams(Parser, lexerNode.Children[count - 1]);
            for (int i = 0; i < count - 1; i++)
            {
                builder.Append(lexerNode.Children[i]);
            }

            MethodReturnType = builder.Type;
            emitter = Parser.Emitters.CreateMethod(GetClass().TypeEmitter, methodName, MethodReturnType, AttributesFromModifiers(Parser.GetSequencePoint(lexerNode), mods));

            foreach(var p in paramz)
            {
                var param = ParseParameter(Parent, p.Type, p.Name);
                if (param.ParameterType.IsVoid())
                    ErrorCode.IllegalMethodParam.ReportAndThrow(point, "Illegal method parameter type void");
                emitter.AddArgument(param);
                symbols.Add(param.Name, param);
            }
            ParamTypes = symbols.Values.Select(p => p.ParameterType);

            if (mods.HasFlag(Modifiers.Entry))
            {
                if (MethodReturnType != Parser.Int32 && MethodReturnType != Parser.UInt32 && MethodReturnType != Parser.Void)
                    InvalidEntryReturn(SequencePoint, MethodReturnType);
                emitter.SetAsEntryPoint();

                if (!ValidParametersForMain(ParamTypes))
                {
                    InvalidEntryParams(point, ParamTypes);
                }
            }
        }

        private static bool ValidParametersForMain(IEnumerable<TypeReference> parameterTypes)
        {
            var parameterCount = parameterTypes.Count();

            if (parameterCount > 1)
                return false;

            if (parameterCount == 0)
                return true;

            var arrayType = parameterTypes.First() as ArrayType;

            if (arrayType != null && arrayType.ElementType.MetadataType == MetadataType.String)
                return true;

            return false;
        }

        private ParameterDefinition ParseParameter(ContextNode parent, AstNode typeNode, AstNode nameNode)
        {
            var type = TypeNode.Parse(parent, typeNode);
            var name = nameNode.GetSingleSymbolOrThrow();
            return new ParameterDefinition(name, ParameterAttributes.None, type);
        }

        public override FunctionDeclarationNode GetMethod() { return this; }

        public override ExpressionNode GetSymbol(string name, ContextNode scope, SequencePoint point)
        {
            if (symbols.ContainsKey(name))
                return new ParameterNode(symbols[name], point);

            return Parent.GetSymbol(name, scope, point); 
        }

        public override bool IsStaticContext()
        {
            return MethodReference.IsStatic();
        }

        public override ClassNode GetClass()
        {
            return Parent.GetClass();
        }

        public static FunctionDeclarationNode ParseAsFunctor(ContextNode context, AstNode function)
        {
            var instance = new FunctionDeclarationNode(context, Modifiers.NoInstance | Modifiers.Private, context.GetClass().NewFunctionName(), function);
            context.GetClass().AddLambda(instance);
            instance.Emit();
            return instance;
        }

        public static FunctionDeclarationNode ParseAsMethod(ClassNode parent, DeclarationInfo declaration)
        {
            var instance = new FunctionDeclarationNode(parent, declaration.Modifiers, declaration.SymbolName.GetSingleSymbolOrThrow(), declaration.Initializer.Children[0]);
            return instance;
        }

        public static TypeReference ParseFunctorType(ContextNode context, AstNode lexerNode)
        {
            var builder = new TypeNode.TypeBuilder(context);
            int count = lexerNode.ChildrenCount;
            for (int i = 0; i < count - 1; i++)
            {
                builder.Append(lexerNode.Children[i]);
            }
            builder.Append(ParseParams(context.Parser, lexerNode.Children[count - 1]).Select(p => TypeNode.Parse(context, p.Type)));

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
                    TooManyAccessMods(point, modifiers);
                else
                    ret |= MethodAttributes.Public;
            }
            else if(modifiers.HasFlag(Modifiers.Protected))
            {
                if (modifiers.HasFlag(Modifiers.Private | Modifiers.Public))
                    TooManyAccessMods(point, modifiers);
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

        private static List<FunctionParamInfo> ParseParams(Parser parser, AstNode lexerNode)
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
                            ErrorCode.InvalidStructure.ReportAndThrow(parser.GetSequencePoint(lexerNode), "Not a valid method definition, {0}", lexerNode.FullContent);
                        else
                            ret.Add(new FunctionParamInfo(param, next));
                        i += 2;
                        break;
                    default:
                        ErrorCode.InvalidStructure.ReportAndThrow(parser.GetSequencePoint(lexerNode), "Unexpected node {0} in {1}", param.Type, lexerNode.FullContent);
                        break;
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

        private static void InvalidEntryReturn(SequencePoint point, TypeReference type)
        {
            ErrorCode.InvalidEntryReturn.ReportAndThrow(point, "Illegal entrypoint return type {0}, must be int, uint or void", type.FullName);
        }

        private static void InvalidEntryParams(SequencePoint point, IEnumerable<TypeReference> paramz)
        {
            ErrorCode.InvalidEntryParams.ReportAndThrow(point,
                "Illegal entrypoint parameter types {0}, must be string[] or without params", String.Join(", ", paramz.Select(p => p.FullName)));
        }

        private static void TooManyAccessMods(SequencePoint point, Modifiers mods)
        {
            var all = ModifierUtils.GetAccess();
            ErrorCode.InvalidMethodMods.ReportAndThrow(point, "Only one of {0} is allowed, {1} found", all, mods | all);
        }

        private static void InvalidMethodDefinition(Parser parser, AstNode lexerNode)
        {
            ErrorCode.InvalidStructure.ReportAndThrow(parser.GetSequencePoint(lexerNode), "Not a valid method definition, {0}", lexerNode.FullContent);
        }
    }
}