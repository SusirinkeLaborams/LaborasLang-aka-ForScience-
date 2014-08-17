using LaborasLangCompiler.ILTools;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl.Wrappers
{
    class InternalType : TypeWrapper
    {
        public override TypeReference TypeReference { get { return Class.TypeReference; } }

        public override string FullName { get { return Class.FullName; } }

        public override TypeWrapper FunctorReturnType
        {
            get { throw new InvalidOperationException("Type is not a functor type"); }
        }

        public override IEnumerable<TypeWrapper> FunctorArgumentTypes
        {
            get { throw new InvalidOperationException("Type is not a functor type"); }
        }

        public ClassNode Class { get; private set; }

        public InternalType(AssemblyEmitter assembly, ClassNode classNode) : base(assembly)
        {
            this.Class = classNode;
        }

        public override TypeWrapper GetContainedType(string name)
        {
            return Class.GetContainedType(name);
        }

        public override FieldWrapper GetField(string name)
        {
            return Class.GetField(name);
        }

        public override MethodWrapper GetMethod(string name)
        {
            return Class.GetMethod(name);
        }

        public override IEnumerable<MethodWrapper> GetMethods(string name)
        {
            return Class.GetMethods(name);
        }
    }
}
