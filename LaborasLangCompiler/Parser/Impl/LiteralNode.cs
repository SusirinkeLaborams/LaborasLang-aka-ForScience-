using LaborasLangCompiler.Parser.Exceptions;
using LaborasLangCompiler.Parser;
using Mono.Cecil;
using NPEG;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaborasLangCompiler.LexingTools;
using Mono.Cecil.Cil;

namespace LaborasLangCompiler.Parser.Impl
{
    class LiteralNode : RValueNode, ILiteralNode
    {
        public override RValueNodeType RValueType { get { return RValueNodeType.Literal; } }
        public dynamic Value { get; private set; }
        public override TypeReference ReturnType { get { return returnType; } }

        private TypeReference returnType;
        private LiteralNode(dynamic value, TypeReference type, SequencePoint point)
            : base(point)
        {
            this.returnType = type;
            this.Value = value;
        }
        public static new LiteralNode Parse(Parser parser, IContainerNode parentBlock, AstNode lexerNode)
        {
            lexerNode = lexerNode.Children[0];
            string type = lexerNode.Token.Name;
            string value = parser.ValueOf(lexerNode);
            var point = parser.GetSequencePoint(lexerNode);
            try
            {
                switch (type)
                {
                    case Lexer.StringLiteral:
                        return new LiteralNode(value.Substring(1, value.Length - 2), parser.Primitives[Parser.String], point);
                    case Lexer.IntegerLiteral:
                        return new LiteralNode(Convert.ToInt32(value), parser.Primitives[Parser.Int], point);
                    case Lexer.FloatLiteral:
                        return new LiteralNode(Convert.ToSingle(value, CultureInfo.InvariantCulture.NumberFormat), parser.Primitives[Parser.Float], point);
                    case Lexer.BooleanLiteral:
                        bool val = value == "true" ? true : false;
                        return new LiteralNode(val, parser.Primitives[Parser.Bool], point);
                    default:
                        throw new ParseException(point, "Literal expected, " + type + " received");
                }
            }
            catch(FormatException e)
            {
                throw new ParseException(point, "Could not parse " + value + " as an " + type, e);
            }
            catch(OverflowException e)
            {
                throw new ParseException(point, "Could not fit " + value + " in " + type, e);
            }
        }
        public override string ToString()
        {
            return "(Literal: " + ReturnType.FullName + " " + Value + ")";
        }
    }
}
