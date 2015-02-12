using LaborasLangCompiler.Parser.Impl.Wrappers;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaborasLangCompiler.Parser.Utils;
using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Common;

namespace LaborasLangCompiler.Parser.Impl
{
    abstract class MemberNode : ExpressionNode
    {
        public Context Scope { get; private set; }
        public TypeReference DeclaringType { get; private set; }
        public MemberReference Member { get; private set; }
        public IExpressionNode ObjectInstance { get { return Instance; } }

        protected ExpressionNode Instance { get; private set; }

        protected MemberNode(MemberReference member, ExpressionNode instance, Context scope, SequencePoint point)
            :base(point)
        {
            Scope = scope;
            DeclaringType = member.DeclaringType;
            Member = member;
            TypeUtils.VerifyAccessible(Member, scope.GetClass().TypeReference, point);
            Instance = instance;
        }

        protected static ExpressionNode GetInstance(MemberReference member, ExpressionNode specifiedInstance, Context context, SequencePoint point)
        {
            if (specifiedInstance != null)
            {
                return specifiedInstance;
            }

            if (member.IsStatic())
            {
                return null;
            }

            if (!context.IsStaticContext() && context.GetClass().TypeReference.IsAssignableTo(member.DeclaringType))
            {
                return ThisNode.Create(context, null);
            }

            ErrorCode.MissingInstance.ReportAndThrow(point, "Cannot access non-static member {0} from a static context", member.FullName);
            return null;//unreachable
        }
    }
}