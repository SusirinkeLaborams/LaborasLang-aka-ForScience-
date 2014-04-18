System.Void klass::dummy()
// Method begins at RVA 0x2050
// Code size 31 (0x1f)
.maxstack 2
.entrypoint
.locals (
	[0] valuetype $Functors.$System_Void$System_Int32$System_String
)

IL_0000: ldsfld valuetype $Functors.$System_Void$System_Int32$System_String klass::myFunction
IL_0005: stloc.0
IL_0006: ldloca.s 0
IL_0008: ldfld object $Functors.$System_Void$System_Int32$System_String::objectInstance
IL_000d: ldloca.s 0
IL_000f: ldfld native int $Functors.$System_Void$System_Int32$System_String::functionPtr
IL_0014: newobj instance void klass/$Delegate$System_Void$System_Int32$System_String::.ctor(object, native int)
IL_0019: stsfld class klass/$Delegate$System_Void$System_Int32$System_String klass::myDelegate
IL_001e: ret


System.Void $Functors.$System_Void$System_Int32$System_String::.ctor(System.Object,System.IntPtr)
// Method begins at RVA 0x207c
// Code size 15 (0xf)
.maxstack 8

IL_0000: ldarg.0
IL_0001: ldarg.1
IL_0002: stfld object $Functors.$System_Void$System_Int32$System_String::objectInstance
IL_0007: ldarg.0
IL_0008: ldarg.2
IL_0009: stfld native int $Functors.$System_Void$System_Int32$System_String::functionPtr
IL_000e: ret


System.Void $Functors.$System_Void$System_Int32$System_String::Invoke(System.Int32,System.String)
// Method begins at RVA 0x208c
// Code size 39 (0x27)
.maxstack 8

IL_0000: ldarg.0
IL_0001: ldfld object $Functors.$System_Void$System_Int32$System_String::objectInstance
IL_0006: dup
IL_0007: brtrue.s IL_0018

IL_0009: pop
IL_000a: ldarg.1
IL_000b: ldarg.2
IL_000c: ldarg.0
IL_000d: ldfld native int $Functors.$System_Void$System_Int32$System_String::functionPtr
IL_0012: calli System.Void(System.Int32,System.String)
IL_0017: ret

IL_0018: nop
IL_0019: ldarg.1
IL_001a: ldarg.2
IL_001b: ldarg.0
IL_001c: ldfld native int $Functors.$System_Void$System_Int32$System_String::functionPtr
IL_0021: calli System.Void(System.Int32,System.String)
IL_0026: ret


$Functors.$System_Void$System_Int32$System_String/$Delegate$System_Void$System_Int32$System_String $Functors.$System_Void$System_Int32$System_String::AsDelegate()
// Method begins at RVA 0x20b4
// Code size 18 (0x12)
.maxstack 8

IL_0000: ldarg.0
IL_0001: ldfld object $Functors.$System_Void$System_Int32$System_String::objectInstance
IL_0006: ldarg.0
IL_0007: ldfld native int $Functors.$System_Void$System_Int32$System_String::functionPtr
IL_000c: newobj instance void $Functors.$System_Void$System_Int32$System_String/$Delegate$System_Void$System_Int32$System_String::.ctor(object, native int)
IL_0011: ret