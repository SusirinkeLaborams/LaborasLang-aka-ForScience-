using LaborasLangCompiler.Parser.Impl.Wrappers;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    abstract class MemberNode : ExpressionNode
    {
        public Context Scope { get; private set; }
        public TypeWrapper DeclaringType { get; private set; }
        public abstract MemberWrapper MemberWrapper { get; }

        protected MemberNode(TypeWrapper declaringType, MemberWrapper member, Context scope, SequencePoint point)
            :base(point)
        {
            Scope = scope;
            DeclaringType = declaringType;
            Utils.VerifyAccessible(member.MemberReference, scope, point);
        }
    }
}
