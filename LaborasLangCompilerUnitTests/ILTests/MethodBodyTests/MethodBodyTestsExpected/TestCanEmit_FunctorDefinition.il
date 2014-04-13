System.Void klass::dummy()
// Method begins at RVA 0x2050
// Code size 1 (0x1)
.maxstack 8
.entrypoint

IL_0000: ret


System.Void $Functors.$System_Void::.ctor(System.Object,System.IntPtr)
// Method begins at RVA 0x2054
// Code size 15 (0xf)
.maxstack 8

IL_0000: ldarg.0
IL_0001: ldarg.1
IL_0002: stfld object $Functors.$System_Void::objectInstance
IL_0007: ldarg.0
IL_0008: ldarg.2
IL_0009: stfld native int $Functors.$System_Void::functionPtr
IL_000e: ret


System.Void $Functors.$System_Void::Invoke()
// Method begins at RVA 0x2064
// Code size 35 (0x23)
.maxstack 8

IL_0000: ldarg.0
IL_0001: ldfld object $Functors.$System_Void::objectInstance
IL_0006: dup
IL_0007: brtrue.s IL_0016

IL_0009: pop
IL_000a: ldarg.0
IL_000b: ldfld native int $Functors.$System_Void::functionPtr
IL_0010: calli System.Void()
IL_0015: ret

IL_0016: nop
IL_0017: ldarg.0
IL_0018: ldfld native int $Functors.$System_Void::functionPtr
IL_001d: calli System.Void()
IL_0022: ret


$Functors.$System_Void/$Delegate$System_Void $Functors.$System_Void::AsDelegate()
// Method begins at RVA 0x2088
// Code size 18 (0x12)
.maxstack 8

IL_0000: ldarg.0
IL_0001: ldfld object $Functors.$System_Void::objectInstance
IL_0006: ldarg.0
IL_0007: ldfld native int $Functors.$System_Void::functionPtr
IL_000c: newobj instance void $Functors.$System_Void/$Delegate$System_Void::.ctor(object, native int)
IL_0011: ret