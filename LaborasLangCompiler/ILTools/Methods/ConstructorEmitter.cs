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
        private const MethodAttributes InstanceAttributes = MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
        private const MethodAttributes StaticAttributes = MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
        private readonly bool isStatic;

        private List<Instruction> epilogue = new List<Instruction>();
        
        public ConstructorEmitter(TypeEmitter declaringType, bool isStatic) :
            base(declaringType, isStatic ? ".cctor" : ".ctor",
            AssemblyRegistry.ImportType(typeof(void)), isStatic ? StaticAttributes : InstanceAttributes)
        {
            this.isStatic = isStatic;

            if (!isStatic)
            {
                var objectCtor = AssemblyRegistry.GetMethods("System.Object", ".ctor").Single(x => x.Parameters.Count == 0);

                epilogue.Add(Instruction.Create(OpCodes.Ldarg_0));
                epilogue.Add(Instruction.Create(OpCodes.Call, Import(objectCtor)));
            }

            Parsed = true;
            epilogue.Add(Instruction.Create(OpCodes.Ret));

            AddEpilogue();
        }

        public void AddFieldInitializer(FieldDefinition field, IExpressionNode initializer)
        {
            RemoveEpilogue();

            if (isStatic)
            {
                Emit(initializer);
                Stsfld(field);
            }
            else
            {
                Ldarg(0);
                Emit(initializer);
                Stfld(field);
            }

            AddEpilogue();
        }

        public void AddPropertyInitializer(PropertyDefinition property, IExpressionNode initializer)
        {
            var setter = property.SetMethod;

            if (setter == null)
            {
                throw new ArgumentException(string.Format("Property {0} has no setter.", property.FullName));
            }

            RemoveEpilogue();

            Emit(initializer);

            if (!isStatic)
            {
                Ldarg(0);
            }

            Call(Import(setter));

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
