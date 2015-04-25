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
                var type = GetLowestConversion(parser, parsed);
                if(type == null)
                {
                    ErrorCode.IntegerOverlflow.ReportAndThrow(point, "Cannot fit {0} into an integer, use BigInteger.Parse", value);
                }
                else
                {
                    //return what we parsed, if requested type can't fit
                    if(type.IsAssignableTo(requestedType))
                    {
                        type = requestedType;
                    }
                }
                if (parsed.Sign >= 0)
                {
                    return new LiteralNode((ulong)parsed, type, point);
                }
                else
                {
                    return new LiteralNode((long)parsed, type, point);
                }
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
                BigInteger converted;
                if(type.IsSignedInteger())
                {
                    converted = new BigInteger((long)node.Value);
                }
                else
                {
                    converted = new BigInteger((ulong)node.Value);
                }
                return GetImplicitConversions(parser.ProjectParser, converted);
            }
            else
            {
                return Enumerable.Empty<TypeReference>();
            }
        }

        private static bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("Literal:");
            builder.Indent(indent + 1).AppendFormat("Type: {0}", type.FullName).AppendLine();
            builder.Indent(indent + 1).AppendFormat("Value: {0}", Value.ToString()).AppendLine();
            return builder.ToString();
        }

        private static IEnumerable<TypeReference> GetImplicitConversions(ProjectParser parser, BigInteger integer)
        {
            List<TypeReference> ret = new List<TypeReference>();
            if(integer >= 0)
            {
                if (integer <= sbyte.MaxValue)
                    ret.Add(parser.Int8);
                if (integer <= byte.MaxValue)
                    ret.Add(parser.UInt8);
                if (integer <= short.MaxValue)
                    ret.Add(parser.Int16);
                if (integer <= ushort.MaxValue)
                    ret.Add(parser.UInt16);
                if (integer <= char.MaxValue)
                    ret.Add(parser.Char);
                if (integer <= int.MaxValue)
                    ret.Add(parser.Int32);
                if (integer <= uint.MaxValue)
                    ret.Add(parser.UInt32);
                if (integer <= long.MaxValue)
                    ret.Add(parser.Int64);
                if (integer <= ulong.MaxValue)
                    ret.Add(parser.UInt64);
            }
            else
            {
                if (integer >= sbyte.MinValue)
                    ret.Add(parser.Int8);
                if (integer >= short.MinValue)
                    ret.Add(parser.Int16);
                if (integer >= int.MinValue)
                    ret.Add(parser.Int32);
                if (integer >= long.MinValue)
                    ret.Add(parser.Int64);
            }
            return ret;
        }
        /// <summary>
        /// Returns lowest possible type for this integer or null, unsigned if possible
        /// </summary>
        private static TypeReference GetLowestConversion(ProjectParser parser, BigInteger integer)
        {
            var conversions = GetImplicitConversions(parser, integer);
            if(integer >= 0)
            {
                return conversions.FirstOrDefault(type => type.IsUnsignedInteger());
            }
            else
            {
                return conversions.FirstOrDefault();
            }
        }
    }
}
