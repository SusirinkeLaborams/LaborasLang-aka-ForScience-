using LaborasLangCompiler.ILTools.Types;
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
        private const TypeAttributes DelegateTypeAttributes = TypeAttributes.NestedPublic | TypeAttributes.Sealed;

        private const MethodAttributes ConstructorAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
        private const MethodAttributes DelegateMethodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.VtableLayoutMask;

        private TypeReference voidType;
        private TypeReference objectType;
        private TypeReference nativeIntType;
        private TypeReference asyncResultType;
        private TypeReference asyncCallbackType;

        public static TypeDefinition Create(AssemblyEmitter assembly, TypeDefinition declaringType,
            TypeReference returnType, IReadOnlyList<TypeReference> arguments)
        {
            return new DelegateEmitter(assembly, "Delegate", declaringType, returnType, arguments).typeDefinition;
        }
        public static TypeDefinition Create(AssemblyEmitter assembly, string delegateName, TypeDefinition declaringType,
            TypeReference returnType, IReadOnlyList<TypeReference> arguments)
        {
            return new DelegateEmitter(assembly, delegateName, declaringType, returnType, arguments).typeDefinition;
        }
        
        private DelegateEmitter(AssemblyEmitter assembly, string delegateName, TypeDefinition declaringType, TypeReference returnType,
            IReadOnlyList<TypeReference> arguments) :
            base(assembly, delegateName, "", DelegateTypeAttributes, AssemblyRegistry.FindType(assembly, "System.MulticastDelegate"), false)
        {
            if (declaringType == null)
            {
                throw new ArgumentNullException("declaringType", "Delegate class must be have a valid declaring type!");
            }

            typeDefinition.DeclaringType = declaringType;

            InitializeTypes();

            AddConstructor();
            AddBeginInvoke(arguments);
            AddEndInvoke(returnType);
            AddInvoke(returnType, arguments);
        }

        private void InitializeTypes()
        {
            voidType = Assembly.TypeToTypeReference(typeof(void));
            objectType = Assembly.TypeToTypeReference(typeof(object));
            nativeIntType = Assembly.TypeToTypeReference(typeof(IntPtr));
            asyncResultType = Assembly.TypeToTypeReference(typeof(IAsyncResult));
            asyncCallbackType = Assembly.TypeToTypeReference(typeof(AsyncCallback));
        }

        private void AddConstructor()
        {
            var constructor = new MethodDefinition(".ctor", ConstructorAttributes, voidType);
            constructor.Parameters.Add(new ParameterDefinition("objectInstance", ParameterAttributes.None, objectType));
            constructor.Parameters.Add(new ParameterDefinition("functionPtr", ParameterAttributes.None, nativeIntType));
            constructor.ImplAttributes = MethodImplAttributes.Runtime;

            AddMethod(constructor);
        }

        private void AddBeginInvoke(IReadOnlyList<TypeReference> arguments)
        {
            var beginInvoke = new MethodDefinition("BeginInvoke", DelegateMethodAttributes, asyncResultType);
            foreach (var argument in arguments)
            {
                beginInvoke.Parameters.Add(new ParameterDefinition(argument));
            }

            beginInvoke.Parameters.Add(new ParameterDefinition("callback", ParameterAttributes.None, asyncCallbackType));
            beginInvoke.Parameters.Add(new ParameterDefinition("object", ParameterAttributes.None, objectType));
            beginInvoke.ImplAttributes = MethodImplAttributes.Runtime;

            AddMethod(beginInvoke);
        }

        private void AddEndInvoke(TypeReference returnType)
        {
            var endInvoke = new MethodDefinition("EndInvoke", DelegateMethodAttributes, returnType);
            endInvoke.Parameters.Add(new ParameterDefinition("result", ParameterAttributes.None, asyncResultType));
            endInvoke.ImplAttributes = MethodImplAttributes.Runtime;

            AddMethod(endInvoke);
        }

        private void AddInvoke(TypeReference returnType, IReadOnlyList<TypeReference> arguments)
        {
            var invoke = new MethodDefinition("Invoke", DelegateMethodAttributes, returnType);

            foreach (var argument in arguments)
            {
                invoke.Parameters.Add(new ParameterDefinition(argument));
            }

            invoke.ImplAttributes = MethodImplAttributes.Runtime;
            AddMethod(invoke);
        }
    }
}
