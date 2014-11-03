using LaborasLangCompiler.Parser.Exceptions;
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

namespace LaborasLangCompiler.Parser.Impl
{
    class LiteralNode : ExpressionNode, ILiteralNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.Literal; } }
        public dynamic Value { get; private set; }
        public override TypeWrapper TypeWrapper { get { return returnType; } }
        public override bool IsGettable
        {
            get { return true; }
        }
        public override bool IsSettable
        {
            get { return false; }
        }

        private TypeWrapper returnType;
        private Type type;
        private LiteralNode(Parser parser, dynamic value, Type type, SequencePoint point)
            : base(point)
        {
            this.type = type;
            this.Value = value;
            this.returnType = new ExternalType(parser.Assembly, type);
        }
        public static new LiteralNode Parse(Parser parser, ContainerNode parentBlock, AstNode lexerNode)
        {
            lexerNode = lexerNode.Children[0];
            var point = parser.GetSequencePoint(lexerNode);
            var type = ParseLiteralType(parser, lexerNode);
            dynamic value = ParseValue(parser, lexerNode.Content.ToString(), type, point);
            return new LiteralNode(parser, value, type, point);
        }

        private static Type ParseLiteralType(Parser parser, AstNode lexerNode)
        {
            switch(lexerNode.Type)
            {
                case Lexer.TokenType.Integer:
                    return typeof(Int32);
                case Lexer.TokenType.StringLiteral:
                    return typeof(string);
                case Lexer.TokenType.Float:
                    return typeof(float);
                case Lexer.TokenType.True:
                case Lexer.TokenType.False:
                    return typeof(bool);
                case Lexer.TokenType.Double:
                    return typeof(double);
                default:
                    throw new ParseException(parser.GetSequencePoint(lexerNode), "Unknown lexer type {0}", lexerNode.Type);
            }
        }

        private static dynamic ParseValue(Parser parser, string value, Type type, SequencePoint point)
        {
            try
            {
                if (type == typeof(string))
                    return value.Substring(1, value.Length - 2);
#if DEBUG
                if (!parser.Primitives.Values.Any(t => t.FullName == type.FullName))
                    throw new ParseException(point, "Type {0} is not a primite LaborasLang type", type.FullName);
#endif
                return Convert.ChangeType(value, type);
            }
            catch(OverflowException)
            {
                throw new ParseException(point, "Could not parse {0} as {1}, overflow", value, type.FullName);
            }
            catch(FormatException)
            {
                throw new ParseException(point, "Could not parse {0} as {1}, format error", value, type.FullName);
            }
        }

        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("Literal:");
            builder.Indent(indent + 1).AppendLine("Type:");
            builder.Indent(indent + 2).AppendLine(TypeWrapper.FullName);
            builder.Indent(indent + 1).AppendLine("Value:");
            builder.Indent(indent + 2).AppendLine(Value.ToString());
            return builder.ToString();
        }
    }
}
