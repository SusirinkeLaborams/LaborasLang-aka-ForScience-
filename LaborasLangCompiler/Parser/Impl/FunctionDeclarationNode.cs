using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.ILTools.Methods;
using LaborasLangCompiler.ILTools.Types;
using LaborasLangCompiler.Parser.Exceptions;
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
    class FunctionDeclarationNode : ParserNode, Context
    {
        public MethodReference MethodReference { get { return emitter.Get(); } }
        public override NodeType Type { get { return NodeType.ParserInternal; } }
        public IEnumerable<TypeReference> ParamTypes { get; private set; }
        public TypeReference MethodReturnType { get; private set; }

        private AstNode body;
        private CodeBlockNode parsedBody;
        private MethodEmitter emitter;
        private ClassNode parent;
        private Dictionary<string, ParameterDefinition> symbols;
        private Parser parser;
        private Modifiers modifiers;

        private FunctionDeclarationNode(Parser parser, Context parent, Modifiers modifiers, string name, AstNode method)
            : base(parser.GetSequencePoint(method))
        {
            this.parent = parent.GetClass();
            this.symbols = new Dictionary<string, ParameterDefinition>();
            this.parser = parser;
            this.body = method.Children[1];
            ParseHeader(modifiers, method.Children[0], name);
        }

        public void Emit()
        {
            parsedBody = CodeBlockNode.Parse(parser, this, body);
            if (MethodReturnType.FullName != parser.Void.FullName && !parsedBody.Returns)
                Errors.ReportAndThrow(ErrorCode.MissingReturn, SequencePoint, "Not all control paths return a value");
            if(parser.ProjectParser.ShouldEmit)
                emitter.ParseTree(parsedBody);
        }

        private void ParseHeader(Modifiers mods, AstNode lexerNode, string methodName)
        {
            var point = parser.GetSequencePoint(lexerNode);
            var builder = new TypeNode.TypeBuilder(parser, parent);
            int count = lexerNode.ChildrenCount;
            var paramz = ParseParams(parser, lexerNode.Children[count - 1]);
            for (int i = 0; i < count - 1; i++)
            {
                builder.Append(lexerNode.Children[i]);
            }

            MethodReturnType = builder.Type;
            emitter = new MethodEmitter(parent.TypeEmitter, methodName, MethodReturnType, AttributesFromModifiers(parser.GetSequencePoint(lexerNode), mods));

            foreach(var p in paramz)
            {
                var param = ParseParameter(parent, p.Type, p.Name);
                if (Utils.IsVoid(param.ParameterType))
                    Errors.ReportAndThrow(ErrorCode.IllegalMethodParam, point, "Illegal method parameter type void");
                emitter.AddArgument(param);
                symbols.Add(param.Name, param);
            }
            ParamTypes = symbols.Values.Select(p => p.ParameterType);

            if (mods.HasFlag(Modifiers.Entry))
            {
                if (MethodReturnType != parser.Int32 && MethodReturnType != parser.UInt32 && MethodReturnType != parser.Void)
                    InvalidEntryReturn(SequencePoint, MethodReturnType);
                emitter.SetAsEntryPoint();

                if((ParamTypes.Count() > 1) || (ParamTypes.Count() == 1 && !ParamTypes.First().TypeEquals(parser.Assembly.TypeToTypeReference(typeof(string[])))))
                {
                    InvalidEntryParams(point, ParamTypes);
                }
            }

        }

        private ParameterDefinition ParseParameter(Context parent, AstNode typeNode, AstNode nameNode)
        {
            var type = TypeNode.Parse(parser, parent, typeNode);
            var name = nameNode.GetSingleSymbolOrThrow();
            return new ParameterDefinition(name, ParameterAttributes.None, type);
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
            return MethodReference.IsStatic();
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
            builder.Append(ParseParams(parser, lexerNode.Children[count - 1]).Select(p => TypeNode.Parse(parser, parent, p.Type)));

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
                            Errors.ReportAndThrow(ErrorCode.InvalidStructure, parser.GetSequencePoint(lexerNode), String.Format("Not a valid method definition, {0}", lexerNode.FullContent));
                        else
                            ret.Add(new FunctionParamInfo(param, next));
                        i += 2;
                        break;
                    default:
                        Errors.ReportAndThrow(ErrorCode.InvalidStructure, parser.GetSequencePoint(lexerNode), String.Format("Unexpected node {0} in {1}", param.Type, lexerNode.FullContent));
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
            Errors.ReportAndThrow(ErrorCode.InvalidEntryReturn, point, String.Format("Illegal entrypoint return type {0}, must be int, uint or void", type.FullName));
        }

        private static void InvalidEntryParams(SequencePoint point, IEnumerable<TypeReference> paramz)
        {
            Errors.ReportAndThrow(ErrorCode.InvalidEntryParams, point,
                String.Format("Illegal entrypoint parameter types {0}, must be string[] or without params", String.Join(", ", paramz.Select(p => p.FullName))));
        }

        private static void TooManyAccessMods(SequencePoint point, Modifiers mods)
        {
            var all = ModifierUtils.GetAccess();
            Errors.ReportAndThrow(ErrorCode.InvalidMethodMods, point, String.Format("Only one of {0} is allowed, {1} found", all, mods | all));
        }

        private static void InvalidMethodDefinition(Parser parser, AstNode lexerNode)
        {
            Errors.ReportAndThrow(ErrorCode.InvalidStructure, parser.GetSequencePoint(lexerNode), String.Format("Not a valid method definition, {0}", lexerNode.FullContent));
        }
    }
}