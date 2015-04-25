using LaborasLangCompiler.Codegen.Types;
using LaborasLangCompiler.Parser;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;

namespace LaborasLangCompiler.Codegen.Methods
{
    internal class ConstructorEmitter : MethodEmitter
    {
        private const MethodAttributes InstanceAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
        private const MethodAttributes StaticAttributes = MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
        private readonly bool isStatic;

        private readonly List<Instruction> epilogue = new List<Instruction>();
        
        public ConstructorEmitter(TypeEmitter declaringType, bool isStatic) :
            this(declaringType, isStatic, isStatic ? StaticAttributes : InstanceAttributes)
        {
        }

        public ConstructorEmitter(TypeEmitter declaringType, bool isStatic, MethodAttributes attributes) :
            base(declaringType, isStatic ? ".cctor" : ".ctor", declaringType.Assembly.TypeSystem.Void, attributes)
        {
            this.isStatic = isStatic;

            if (!isStatic && !declaringType.IsValueType)
            {
                var objectCtor = AssemblyRegistry.GetCompatibleMethod(declaringType.Assembly, declaringType.Assembly.TypeSystem.Object, ".ctor", new TypeReference[0]);

                Ldarg(0);
                Call(methodDefinition.DeclaringType, objectCtor);
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
                Emit(initializer, EmissionType.Value);
                Stsfld(field);
            }
            else
            {
                Ldarg(0);
                Emit(initializer, EmissionType.Value);
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

            Emit(initializer, EmissionType.Value);
            Call(methodDefinition.DeclaringType, setter);

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
