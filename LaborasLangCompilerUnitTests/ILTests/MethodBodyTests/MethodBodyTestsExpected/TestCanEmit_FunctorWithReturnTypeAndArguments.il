System.Void klass::dummy()
// Method begins at RVA 0x2050
// Code size 1 (0x1)
.maxstack 8
.entrypoint

IL_0000: ret


System.Void $Functors.$System_Int32$System_Boolean$System_Single$System_String::.ctor(System.Object,System.IntPtr)
// Method begins at RVA 0x2054
// Code size 15 (0xf)
.maxstack 8

IL_0000: ldarg.0
IL_0001: ldarg.1
IL_0002: stfld object $Functors.$System_Int32$System_Boolean$System_Single$System_String::objectInstance
IL_0007: ldarg.0
IL_0008: ldarg.2
IL_0009: stfld native int $Functors.$System_Int32$System_Boolean$System_Single$System_String::functionPtr
IL_000e: ret


System.Int32 $Functors.$System_Int32$System_Boolean$System_Single$System_String::Invoke(System.Boolean,System.Single,System.String)
// Method begins at RVA 0x2064
// Code size 41 (0x29)
.maxstack 8

IL_0000: ldarg.0
IL_0001: ldfld object $Functors.$System_Int32$System_Boolean$System_Single$System_String::objectInstance
IL_0006: dup
IL_0007: brtrue.s IL_0019

IL_0009: pop
IL_000a: ldarg.1
IL_000b: ldarg.2
IL_000c: ldarg.3
IL_000d: ldarg.0
IL_000e: ldfld native int $Functors.$System_Int32$System_Boolean$System_Single$System_String::functionPtr
IL_0013: calli System.Int32(System.Boolean,System.Single,System.String)
IL_0018: ret

IL_0019: nop
IL_001a: ldarg.1
IL_001b: ldarg.2
IL_001c: ldarg.3
IL_001d: ldarg.0
IL_001e: ldfld native int $Functors.$System_Int32$System_Boolean$System_Single$System_String::functionPtr
IL_0023: calli System.Int32(System.Boolean,System.Single,System.String)
IL_0028: ret


$Functors.$System_Int32$System_Boolean$System_Single$System_String/$Delegate$System_Int32$System_Boolean$System_Single$System_String $Functors.$System_Int32$System_Boolean$System_Single$System_String::AsDelegate()
// Method begins at RVA 0x2090
// Code size 18 (0x12)
.maxstack 8

IL_0000: ldarg.0
IL_0001: ldfld object $Functors.$System_Int32$System_Boolean$System_Single$System_String::objectInstance
IL_0006: ldarg.0
IL_0007: ldfld native int $Functors.$System_Int32$System_Boolean$System_Single$System_String::functionPtr
IL_000c: newobj instance void $Functors.$System_Int32$System_Boolean$System_Single$System_String/$Delegate$System_Int32$System_Boolean$System_Single$System_String::.ctor(object, native int)
IL_0011: ret