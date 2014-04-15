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
        
        public ConstructorEmitter(TypeEmitter declaringType, bool isStatic) :
            base(declaringType, isStatic ? ".cctor" : ".ctor", 
            AssemblyRegistry.ImportType(typeof(void)), isStatic ? InstanceAttributes : StaticAttributes)
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

        public void AddPropertyInitializer(PropertyDefinition property, IExpressionNode initializer)
        {
            var setter = property.SetMethod;

            if (setter == null)
            {
                throw new ArgumentException(string.Format("Property {0} has no setter.", property.FullName));
            }

            Emit(initializer);

            if (isStatic)
            {
                Ldarg(0);
            }

            Call(Import(setter));
        }
    }
}
