using LaborasLangCompiler.Parser.Utils;
using LaborasLangCompiler.Parser;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil.Cil;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Lexer.Containers;
using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Common;
using Lexer;
using System.Diagnostics.Contracts;
using System.Numerics;

namespace LaborasLangCompiler.Parser.Impl
{
    class LiteralNode : ExpressionNode, ILiteralNode, IAmbiguousNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.Literal; } }
        public Literal Value { get; private set; }
        public override TypeReference ExpressionReturnType { get { return type; } }
        public override bool IsGettable
        {
            get { return true; }
        }
        public override bool IsSettable
        {
            get { return false; }
        }

        private readonly TypeReference type;

        private LiteralNode(IConvertible value, TypeReference type, SequencePoint point)
            : base(point)
        {
            this.type = type;
            this.Value = new Literal(value);
        }

        public static LiteralNode Create(ContextNode context, IConvertible value, SequencePoint point)
        {
            var type = MetadataHelpers.GetBuiltInTypeReference(context.Parser.Assembly, value.GetType());

            if (type == null)
            {
                Errors.ReportAndThrow(ErrorCode.TypeMissmatch, point, "Cannot create literal of type {0} with value {1}",
                    value.GetType().FullName, value);
            }

            return new LiteralNode(value, type, point);
        }

        public static LiteralNode Parse(ContextNode context, IAbstractSyntaxTree lexerNode)
        {
            Contract.Requires(lexerNode.Type == Lexer.TokenType.LiteralNode);
            lexerNode = lexerNode.Children[0];
            var point = context.Parser.GetSequencePoint(lexerNode);
            var type = ParseLiteralType(context.Parser, lexerNode);
            return ParseValue(context.Parser.ProjectParser, lexerNode.Content, type, point);
        }

        private static TypeReference ParseLiteralType(Parser parser, IAbstractSyntaxTree lexerNode)
        {
            switch(lexerNode.Type)
            {
                case Lexer.TokenType.Long:
                    return parser.Int64;
                case Lexer.TokenType.Integer:
                    return parser.Int32;
                case Lexer.TokenType.CharLiteral:
                    return parser.Char;
                case Lexer.TokenType.StringLiteral:
                    return parser.String;
                case Lexer.TokenType.Float:
                    return parser.Float;
                case Lexer.TokenType.True:
                case Lexer.TokenType.False:
                    return parser.Bool;
                case Lexer.TokenType.Double:
                    return parser.Double;
                default:
                    ContractsHelper.AssumeUnreachable("Unknown literal type");
                    return null;//unreachable
            }
        }

        private static LiteralNode ParseValue(ProjectParser parser, string value, TypeReference type, SequencePoint point)
        {
            try
            {
                switch (type.MetadataType)
                {
                    case MetadataType.String:
                    case MetadataType.Boolean:
                        return new LiteralNode(value, type, point);

                    case MetadataType.SByte:
                    case MetadataType.Byte:
                    case MetadataType.Int16:
                    case MetadataType.UInt16:
                    case MetadataType.Int32:
                    case MetadataType.UInt32:
                    case MetadataType.Int64:
                    case MetadataType.UInt64:
                        return ParseInteger(parser, value, type, point);

                    case MetadataType.Char:
                        {
                            if (value.Length > 1)
                            {
                                ErrorCode.MultipleCharacterLiteral.ReportAndThrow(point, "Character literal must not be longer than one character (found: '{0}').", value);
                            }

                            return new LiteralNode(value, type, point);
                        }

                    case MetadataType.Single:
                    case MetadataType.Double:
                        return ParseRational(parser, value, type, point);
                        
                    default:

                        ContractsHelper.AssertUnreachable(String.Format("Unexpected literal type {0}", type.FullName));
                        return null;//unreachable
                }
            }
            catch(OverflowException)
            {
                ErrorCode.InvalidStructure.ReportAndThrow(point, "Could not parse {0} as {1}, overflow", value, type.FullName);
                return null;//unreachable
            }
            catch(FormatException)
            {
                ErrorCode.InvalidStructure.ReportAndThrow(point, "Could not parse {0} as {1}, format error", value, type.FullName);
                return null;//unreachable
            }
        }

        private static LiteralNode ParseInteger(ProjectParser parser, string value, TypeReference requestedType, SequencePoint point)
        {
            try
            {
                var parsed = BigInteger.Parse(value, CultureInfo.InvariantCulture);
                if (parsed.Sign >= 0)
                {
                    var type = parser.MaxValues.Where(kv => kv.Key >= parsed).OrderBy(kv => kv.Key).FirstOrDefault().Value;
                    if (type != null)
                    {
                        return new LiteralNode((ulong)parsed, type.IsAssignableTo(requestedType) ? requestedType : type, point);
                    }
                }
                else
                {
                    var type = parser.MinValues.Where(kv => kv.Key <= parsed).OrderBy(kv => kv.Key).LastOrDefault().Value;
                    if (type != null)
                    {
                        return new LiteralNode((long)parsed, type.IsAssignableTo(requestedType) ? requestedType : type, point);
                    }
                }
                ErrorCode.IntegerOverlflow.ReportAndThrow(point, "Cannot fit {0} into an integer, use BigInteger.Parse", value);
            }
            catch (FormatException)
            {
                ContractsHelper.AssumeUnreachable("Error parsing {0} as integer", value);
            }
            return Utils.Utils.Fail<LiteralNode>();
        }

        private static LiteralNode ParseRational(ProjectParser parser, string value, TypeReference requestedType, SequencePoint point)
        {
            try
            {

                switch (requestedType.MetadataType)
                {
                    case MetadataType.Single:
                        return new LiteralNode(Convert.ToSingle(value, CultureInfo.InvariantCulture), requestedType, point);
                    case MetadataType.Double:
                        return new LiteralNode(Convert.ToDouble(value, CultureInfo.InvariantCulture), requestedType, point);
                    default:
                        ContractsHelper.AssertUnreachable("Unexpected type {0} in ParseRational", requestedType.MetadataType);
                        return Utils.Utils.Fail<LiteralNode>();
                }
            }
            catch (OverflowException)
            {
                ErrorCode.InvalidStructure.ReportAndThrow(point, "Could not parse {0} as {1}, overflow", value, requestedType.FullName);
                return Utils.Utils.Fail<LiteralNode>();
            }
            catch (FormatException)
            {
                ErrorCode.InvalidStructure.ReportAndThrow(point, "Could not parse {0} as {1}, format error", value, requestedType.FullName);
                return Utils.Utils.Fail<LiteralNode>();
            }
        }

        public ExpressionNode RemoveAmbiguity(ContextNode context, TypeReference expectedType)
        {
            if (expectedType.FullName == type.FullName)
                return this;

            var conversions = GetImplicitConversions(context.Parser, this);
            if(conversions.Any(c => c.TypeEquals(expectedType)))
            {
                return ConvertLiteral(this, expectedType);
            }
            else
            {
                //cannot convert
                return this;
            }
        }

        private static LiteralNode ConvertLiteral(LiteralNode node, TypeReference type)
        {
            return new LiteralNode(node.Value.Value, type, node.SequencePoint);
        }

        private static IEnumerable<TypeReference> GetImplicitConversions(Parser parser, LiteralNode node)
        {
            var type = node.ExpressionReturnType;
            if(type.IsIntegerType())
            {
                if(type.IsSignedInteger())
                {
                    var value = (long)node.Value;
                    if (value >= 0)
                    {
                        return parser.ProjectParser.MaxValues.Where(kv => kv.Key >= (ulong)value).Select(kv => kv.Value);
                    }
                    else
                    {
                        return parser.ProjectParser.MinValues.Where(kv => kv.Key >= value).Select(kv => kv.Value);
                    }
                }
                else
                {
                    var value = (ulong)node.Value;
                    return parser.ProjectParser.MaxValues.Where(kv => kv.Key >= value).Select(kv => kv.Value);
                }
            }
            else
            {
                return Enumerable.Empty<TypeReference>();
            }
        }

        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("Literal:");
            builder.Indent(indent + 1).AppendFormat("Type: {0}", type.FullName).AppendLine();
            builder.Indent(indent + 1).AppendFormat("Value: {0}", Value.ToString()).AppendLine();
            return builder.ToString();
        }
    }
}
