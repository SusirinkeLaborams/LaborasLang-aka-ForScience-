using LaborasLangCompiler.Parser.Exceptions;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Lexer.Containers;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    class ThisNode : ExpressionNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.This; } }
        public override bool IsGettable { get { return true; } }
        public override bool IsSettable { get { return false; } }
        public override TypeWrapper TypeWrapper { get { return type; } }

        private TypeWrapper type;

        public ThisNode(TypeWrapper type, SequencePoint point) : base(point)
        {
            this.type = type;
        }

        public static ThisNode Parse(Parser parser, Context parent, AstNode lexerNode)
        {
            if (parent.IsStaticContext())
            {
                throw new ParseException(parser.GetSequencePoint(lexerNode), "Cannot use this inside a static context");
            }
            else
            {
                return new ThisNode(parent.GetClass().TypeWrapper, parser.GetSequencePoint(lexerNode));
            }
        }

        public override string ToString(int indent)
        {
            throw new NotImplementedException();
        }

        public static ExpressionNode GetAccessingInstance(MemberWrapper member, ExpressionNode instance, Context context, SequencePoint point)
        {
            if (instance != null)
            {
                return instance;
            }

            if(member.IsStatic)
            {
                return null;
            }

            if(!context.IsStaticContext() && context.GetClass().TypeWrapper.IsAssignableTo(member.DeclaringType))
            {
                return new ThisNode(member.DeclaringType, point);
            }
            else
            {
                throw new ParseException(point, "Cannot access non-static member {0} from a static context", member.MemberReference.Name);
            }
        }
    }
}