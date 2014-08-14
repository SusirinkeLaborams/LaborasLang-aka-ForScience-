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
        public override TypeReference TypeReference { get { return classNode.TypeReference; } }

        public override string FullName { get { return classNode.FullName; } }

        private ClassNode classNode;

        public InternalType(AssemblyEmitter assembly, ClassNode classNode) : base(assembly)
        {
            this.classNode = classNode;
        }

        public override TypeWrapper GetContainedType(string name)
        {
            return null;
        }
        public override FieldWrapper GetField(string name)
        {
            return classNode.GetField(name);
        }
        public override MethodWrapper GetMethod(string name)
        {
            return classNode.GetMethod(name);
        }
        public override IEnumerable<MethodWrapper> GetMethods(string name)
        {
            return classNode.GetMethods(name);
        }
    }
}
