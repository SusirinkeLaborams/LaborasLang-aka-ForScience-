System.Void klass::dummy()
// Method begins at RVA 0x2050
// Code size 33 (0x21)
.maxstack 2
.entrypoint
.locals (
	[0] valuetype $Functors.$System_Void$System_Int32$System_String
)

IL_0000: call valuetype $Functors.$System_Void$System_Int32$System_String klass::get_MyFunction()
IL_0005: stloc.0
IL_0006: ldloca.s 0
IL_0008: ldfld object $Functors.$System_Void$System_Int32$System_String::objectInstance
IL_000d: ldloca.s 0
IL_000f: ldfld native int $Functors.$System_Void$System_Int32$System_String::functionPtr
IL_0014: newobj instance void klass/$Delegate$System_Void$System_Int32$System_String::.ctor(object, native int)
IL_0019: tail.
IL_001b: call void klass::set_MyDelegate(class klass/$Delegate$System_Void$System_Int32$System_String)
IL_0020: ret


System.Void klass::set_MyFunction($Functors.$System_Void$System_Int32$System_String)
// Method begins at RVA 0x2080
// Code size 7 (0x7)
.maxstack 8

IL_0000: ldarg.0
IL_0001: stsfld valuetype $Functors.$System_Void$System_Int32$System_String klass::myFunction_BackingField
IL_0006: ret


$Functors.$System_Void$System_Int32$System_String klass::get_MyFunction()
// Method begins at RVA 0x2088
// Code size 6 (0x6)
.maxstack 8

IL_0000: ldsfld valuetype $Functors.$System_Void$System_Int32$System_String klass::myFunction_BackingField
IL_0005: ret


System.Void klass::set_MyDelegate(klass/$Delegate$System_Void$System_Int32$System_String)
// Method begins at RVA 0x2090
// Code size 7 (0x7)
.maxstack 8

IL_0000: ldarg.0
IL_0001: stsfld class klass/$Delegate$System_Void$System_Int32$System_String klass::myDelegate_BackingField
IL_0006: ret


klass/$Delegate$System_Void$System_Int32$System_String klass::get_MyDelegate()
// Method begins at RVA 0x2098
// Code size 6 (0x6)
.maxstack 8

IL_0000: ldsfld class klass/$Delegate$System_Void$System_Int32$System_String klass::myDelegate_BackingField
IL_0005: ret


System.Void $Functors.$System_Void$System_Int32$System_String::.ctor(System.Object,System.IntPtr)
// Method begins at RVA 0x20a0
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
// Method begins at RVA 0x20b0
// Code size 43 (0x2b)
.maxstack 8

IL_0000: ldarg.0
IL_0001: ldfld object $Functors.$System_Void$System_Int32$System_String::objectInstance
IL_0006: dup
IL_0007: brtrue.s IL_001a

IL_0009: pop
IL_000a: ldarg.1
IL_000b: ldarg.2
IL_000c: ldarg.0
IL_000d: ldfld native int $Functors.$System_Void$System_Int32$System_String::functionPtr
IL_0012: tail.
IL_0014: calli System.Void(System.Int32,System.String)
IL_0019: ret

IL_001a: nop
IL_001b: ldarg.1
IL_001c: ldarg.2
IL_001d: ldarg.0
IL_001e: ldfld native int $Functors.$System_Void$System_Int32$System_String::functionPtr
IL_0023: tail.
IL_0025: calli System.Void(System.Int32,System.String)
IL_002a: ret


$Functors.$System_Void$System_Int32$System_String/$Delegate$System_Void$System_Int32$System_String $Functors.$System_Void$System_Int32$System_String::AsDelegate()
// Method begins at RVA 0x20dc
// Code size 18 (0x12)
.maxstack 8

IL_0000: ldarg.0
IL_0001: ldfld object $Functors.$System_Void$System_Int32$System_String::objectInstance
IL_0006: ldarg.0
IL_0007: ldfld native int $Functors.$System_Void$System_Int32$System_String::functionPtr
IL_000c: newobj instance void $Functors.$System_Void$System_Int32$System_String/$Delegate$System_Void$System_Int32$System_String::.ctor(object, native int)
IL_0011: ret