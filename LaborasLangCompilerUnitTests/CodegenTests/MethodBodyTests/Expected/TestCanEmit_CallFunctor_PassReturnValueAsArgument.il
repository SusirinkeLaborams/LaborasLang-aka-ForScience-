System.Void klass::dummy()
// Method begins at RVA 0x2050
// Code size 22 (0x16)
.maxstack 8
.entrypoint

IL_0000: ldsfld class '$Functors.$System_Int32*System_String*System_Single?' klass::myFunction
IL_0005: call string klass::GetString()
IL_000a: ldc.r4 3.5
IL_000f: callvirt instance int32 '$Functors.$System_Int32*System_String*System_Single?'::Invoke(string, float32)
IL_0014: pop
IL_0015: ret


System.String klass::GetString()
// Method begins at RVA 0x2068
// Code size 6 (0x6)
.maxstack 8

IL_0000: ldstr "Str"
IL_0005: ret


System.Void $Functors.$System_Int32*System_String*System_Single?::.ctor()
// Method begins at RVA 0x2070
// Code size 7 (0x7)
.maxstack 8

IL_0000: ldarg.0
IL_0001: call instance void [mscorlib]System.Object::.ctor()
IL_0006: ret