System.Void klass::dummy()
// Method begins at RVA 0x2050
// Code size 22 (0x16)
.maxstack 8
.entrypoint

IL_0000: ldsflda valuetype $Functors.$System_Int32$System_String$System_Single klass::myFunction
IL_0005: call string klass::GetString()
IL_000a: ldc.r4 3.5
IL_000f: call instance int32 $Functors.$System_Int32$System_String$System_Single::Invoke(string, float32)
IL_0014: pop
IL_0015: ret


System.String klass::GetString()
// Method begins at RVA 0x2068
// Code size 6 (0x6)
.maxstack 8

IL_0000: ldstr "Str"
IL_0005: ret


System.Void $Functors.$System_Int32$System_String$System_Single::.ctor(System.Object,System.IntPtr)
// Method begins at RVA 0x2070
// Code size 15 (0xf)
.maxstack 8

IL_0000: ldarg.0
IL_0001: ldarg.1
IL_0002: stfld object $Functors.$System_Int32$System_String$System_Single::objectInstance
IL_0007: ldarg.0
IL_0008: ldarg.2
IL_0009: stfld native int $Functors.$System_Int32$System_String$System_Single::functionPtr
IL_000e: ret


System.Int32 $Functors.$System_Int32$System_String$System_Single::Invoke(System.String,System.Single)
// Method begins at RVA 0x2080
// Code size 39 (0x27)
.maxstack 8

IL_0000: ldarg.0
IL_0001: ldfld object $Functors.$System_Int32$System_String$System_Single::objectInstance
IL_0006: dup
IL_0007: brtrue.s IL_0018

IL_0009: pop
IL_000a: ldarg.1
IL_000b: ldarg.2
IL_000c: ldarg.0
IL_000d: ldfld native int $Functors.$System_Int32$System_String$System_Single::functionPtr
IL_0012: calli System.Int32(System.String,System.Single)
IL_0017: ret

IL_0018: nop
IL_0019: ldarg.1
IL_001a: ldarg.2
IL_001b: ldarg.0
IL_001c: ldfld native int $Functors.$System_Int32$System_String$System_Single::functionPtr
IL_0021: calli System.Int32(System.String,System.Single)
IL_0026: ret


$Functors.$System_Int32$System_String$System_Single/$Delegate$System_Int32$System_String$System_Single $Functors.$System_Int32$System_String$System_Single::AsDelegate()
// Method begins at RVA 0x20a8
// Code size 18 (0x12)
.maxstack 8

IL_0000: ldarg.0
IL_0001: ldfld object $Functors.$System_Int32$System_String$System_Single::objectInstance
IL_0006: ldarg.0
IL_0007: ldfld native int $Functors.$System_Int32$System_String$System_Single::functionPtr
IL_000c: newobj instance void $Functors.$System_Int32$System_String$System_Single/$Delegate$System_Int32$System_String$System_Single::.ctor(object, native int)
IL_0011: ret