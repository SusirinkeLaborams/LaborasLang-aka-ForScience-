using LaborasLangCompiler.ILTools.Types;
using LaborasLangCompiler.Parser;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.ILTools.Methods
{
    internal class ConstructorEmitter : MethodEmitter
    {
        private const MethodAttributes InstanceAttributes = MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
        private const MethodAttributes StaticAttributes = MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
        private readonly bool isStatic;
        
        public ConstructorEmitter(AssemblyRegistry assemblyRegistry, TypeEmitter declaringType, bool isStatic) :
            base(assemblyRegistry, declaringType, isStatic ? ".cctor" : ".ctor", 
            assemblyRegistry.ImportType(typeof(void)), isStatic ? InstanceAttributes : StaticAttributes)
        {
            this.isStatic = isStatic;
        }

        public void AddFieldInitializer(FieldDefinition field, IExpressionNode initializer)
        {
            Emit(initializer);

            if (isStatic)
            {
                Stsfld(field);
            }
            else
            {
                Stfld(field);
            }
        }
    }
}
