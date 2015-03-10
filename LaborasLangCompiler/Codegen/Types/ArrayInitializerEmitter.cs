using Mono.Cecil;

namespace LaborasLangCompiler.Codegen.Types
{
    internal class ArrayInitializerEmitter : TypeEmitter
    {
        private const TypeAttributes ArrayInitializerTypeAttributes = TypeAttributes.NestedPublic | TypeAttributes.ExplicitLayout | TypeAttributes.Sealed;
        private const FieldAttributes ArrayInitializerFieldAttributes = FieldAttributes.Static | FieldAttributes.Assembly | FieldAttributes.HasFieldRVA;

        private static int arrayInitializerCounter;

        public static FieldDefinition Emit(AssemblyEmitter assembly, byte[] arrayInitializerBytes)
        {
            var className = "__ArrayInitializer_" + arrayInitializerCounter;
            var emitter = new ArrayInitializerEmitter(assembly, className);
            return emitter.EmitInitializerField(arrayInitializerBytes);
        }

        private ArrayInitializerEmitter(AssemblyEmitter assembly, string className) :
            base(assembly, className, "", ArrayInitializerTypeAttributes, AssemblyRegistry.FindType(assembly, "System.ValueType"), false)
        {
            var moduleType = assembly.MainModule.GetType("", "<Module>");
            moduleType.NestedTypes.Add(typeDefinition);
        }

        private FieldDefinition EmitInitializerField(byte[] arrayInitializerBytes)
        {
            typeDefinition.ClassSize = arrayInitializerBytes.Length;
            typeDefinition.PackingSize = 1;

            var field = new FieldDefinition("__InitializerField", ArrayInitializerFieldAttributes, typeDefinition);
            field.InitialValue = arrayInitializerBytes;
            typeDefinition.DeclaringType.Fields.Add(field);
            return field;
        }
    }
}
