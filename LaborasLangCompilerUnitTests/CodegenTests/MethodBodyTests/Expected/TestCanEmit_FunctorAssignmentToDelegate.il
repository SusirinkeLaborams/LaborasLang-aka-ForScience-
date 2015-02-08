System.Void klass::dummy()
// Method begins at RVA 0x2050
// Code size 27 (0x1b)
.maxstack 8
.entrypoint

IL_0000: ldsfld class '$Functors.$System_Void*System_Int32*System_String?' klass::myFunction
IL_0005: callvirt instance class '$Functors.$System_Void*System_Int32*System_String?'/Delegate '$Functors.$System_Void*System_Int32*System_String?'::AsDelegate()
IL_000a: ldftn instance void '$Functors.$System_Void*System_Int32*System_String?'/Delegate::Invoke(int32, string)
IL_0010: newobj instance void klass/Delegate::.ctor(object, native int)
IL_0015: stsfld class klass/Delegate klass::myDelegate
IL_001a: ret


System.Void $Functors.$System_Void*System_Int32*System_String?::.ctor()
// Method begins at RVA 0x206c
// Code size 7 (0x7)
.maxstack 8

IL_0000: ldarg.0
IL_0001: call instance void [mscorlib]System.Object::.ctor()
IL_0006: ret