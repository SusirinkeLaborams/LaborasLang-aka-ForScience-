using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.ILTools
{
    internal class DelegateEmitter : TypeEmitter
    {
        private const TypeAttributes DelegateTypeAttributes = TypeAttributes.NestedPublic | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.Sealed;

        private const MethodAttributes ConstructorAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
        private const MethodAttributes DelegateMethodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;

        public static TypeDefinition Create(AssemblyRegistry assemblyRegistry, AssemblyEmitter assembly, string @namespace, 
            TypeReference returnType, IReadOnlyList<TypeReference> arguments) 
        {
            return new DelegateEmitter(assemblyRegistry, assembly, @namespace, returnType, arguments).typeDefinition;
        }

        private DelegateEmitter(AssemblyRegistry assemblyRegistry, AssemblyEmitter assembly, string @namespace, 
            TypeReference returnType, IReadOnlyList<TypeReference> arguments) :
            base(assembly, ComputeClassName(returnType, arguments), @namespace, DelegateTypeAttributes, 
                assemblyRegistry.GetType("System.MulticastDelegate"))
        {
            var voidType = Module.Import(typeof(void));
            var objectType = Module.Import(typeof(object));
            var nativeIntType = Module.Import(typeof(System.IntPtr));
            var asyncResultType = Module.Import(typeof(IAsyncResult));
            var asyncCallbackType = Module.Import(typeof(AsyncCallback));

            var constructor = new MethodDefinition(".ctor", ConstructorAttributes, voidType);
            constructor.Parameters.Add(new ParameterDefinition("objectInstance", ParameterAttributes.None, objectType));
            constructor.Parameters.Add(new ParameterDefinition("functionPtr", ParameterAttributes.None, nativeIntType));

            var beginInvoke = new MethodDefinition("BeginInvoke", DelegateMethodAttributes, asyncResultType);
            foreach (var argument in arguments)
            {
                beginInvoke.Parameters.Add(new ParameterDefinition(assembly.Import(argument)));
            }

            beginInvoke.Parameters.Add(new ParameterDefinition("callback", ParameterAttributes.None, asyncCallbackType));
            beginInvoke.Parameters.Add(new ParameterDefinition("object", ParameterAttributes.None, objectType));

            var endInvoke = new MethodDefinition("EndInvoke", DelegateMethodAttributes, returnType);
            endInvoke.Parameters.Add(new ParameterDefinition("result", ParameterAttributes.None, asyncResultType));

            var invoke = new MethodDefinition("Invoke", DelegateMethodAttributes, returnType);
            foreach (var argument in arguments)
            {
                invoke.Parameters.Add(new ParameterDefinition(assembly.Import(argument)));
            }
            
            AddMethod(constructor);
            AddMethod(beginInvoke);
            AddMethod(endInvoke);
            AddMethod(invoke);
        }
        
        private static string ComputeClassName(TypeReference returnType, IReadOnlyList<TypeReference> arguments)
        {
            var name = new StringBuilder("$Delegate$" + returnType.FullName + "$");

            for (int i = 0; i < arguments.Count; i++)
            {
                if (i != 0)
                {
                    name.Append("_");
                }

                name.Append(arguments[i].FullName);
            }

            return name.ToString();
        }
    }
}
