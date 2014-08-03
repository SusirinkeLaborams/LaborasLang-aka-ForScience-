using LaborasLangCompiler.ILTools.Types;
using LaborasLangCompiler.Parser;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.ILTools.Methods
{
    internal class ConstructorEmitter : MethodEmitter
    {
        private const MethodAttributes InstanceAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
        private const MethodAttributes StaticAttributes = MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
        private readonly bool isStatic;

        private List<Instruction> epilogue = new List<Instruction>();
        
        public ConstructorEmitter(TypeEmitter declaringType, bool isStatic) :
            this(declaringType, isStatic, isStatic ? StaticAttributes : InstanceAttributes)
        {
        }

        public ConstructorEmitter(TypeEmitter declaringType, bool isStatic, MethodAttributes attributes) :
            base(declaringType, isStatic ? ".cctor" : ".ctor", declaringType.Assembly.TypeToTypeReference(typeof(void)), attributes)
        {
            this.isStatic = isStatic;

            if (!isStatic)
            {
                var objectCtor = AssemblyRegistry.GetCompatibleMethod(declaringType.Assembly, "System.Object", ".ctor", new List<TypeReference>());

                Ldarg(0);
                Call(objectCtor);
            }

            Parsed = true;
            epilogue.Add(Instruction.Create(OpCodes.Ret));

            AddEpilogue();
        }

        public void AddFieldInitializer(FieldReference field, IExpressionNode initializer)
        {
            RemoveEpilogue();

            if (isStatic)
            {
                Emit(initializer, false);
                Stsfld(field);
            }
            else
            {
                Ldarg(0);
                Emit(initializer, false);
                Stfld(field);
            }

            AddEpilogue();
        }

        public void AddPropertyInitializer(PropertyReference property, IExpressionNode initializer)
        {
            var setter = AssemblyRegistry.GetPropertySetter(Assembly, property);

            if (setter == null)
            {
                throw new ArgumentException(string.Format("Property {0} has no setter.", property.FullName));
            }

            RemoveEpilogue();

            if (!isStatic)
            {
                Ldarg(0);
            }
            
            Emit(initializer, false);
            Call(setter);

            AddEpilogue();
        }

        private void RemoveEpilogue()
        {
            for (int i = 0; i < epilogue.Count; i++)
            {
                body.Instructions.RemoveAt(body.Instructions.Count - 1);
            }
        }

        private void AddEpilogue()
        {
            foreach (var instruction in epilogue)
            {
                body.Instructions.Add(instruction);
            }
        }
    }
}
