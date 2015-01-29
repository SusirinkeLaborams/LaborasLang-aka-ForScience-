using LaborasLangCompiler.Parser.Impl.Wrappers;
using Mono.Cecil;
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
        public TypeReference DeclaringType { get; private set; }
        public MemberReference Member { get; private set; }

        protected MemberNode(MemberReference member, Context scope, SequencePoint point)
            :base(point)
        {
            Scope = scope;
            DeclaringType = member.DeclaringType;
            Member = member;
            Utils.VerifyAccessible(Member, scope.GetClass().TypeReference, point);
        }
    }
}
