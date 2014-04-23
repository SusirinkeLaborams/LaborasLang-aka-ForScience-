System.Void klass::dummy()
// Method begins at RVA 0x2050
// Code size 1 (0x1)
.maxstack 8
.entrypoint

IL_0000: ret


System.Void klass::.cctor()
// Method begins at RVA 0x2054
// Code size 18 (0x12)
.maxstack 8

IL_0000: ldnull
IL_0001: ldftn void klass::dummy()
IL_0007: newobj instance void $Functors.$System_Int32$System_Double$System_Single::.ctor(object, native int)
IL_000c: stsfld valuetype $Functors.$System_Int32$System_Double$System_Single klass::myFunction
IL_0011: ret


System.Void $Functors.$System_Int32$System_Double$System_Single::.ctor(System.Object,System.IntPtr)
// Method begins at RVA 0x2068
// Code size 15 (0xf)
.maxstack 8

IL_0000: ldarg.0
IL_0001: ldarg.1
IL_0002: stfld object $Functors.$System_Int32$System_Double$System_Single::objectInstance
IL_0007: ldarg.0
IL_0008: ldarg.2
IL_0009: stfld native int $Functors.$System_Int32$System_Double$System_Single::functionPtr
IL_000e: ret


System.Int32 $Functors.$System_Int32$System_Double$System_Single::Invoke(System.Double,System.Single)
// Method begins at RVA 0x2078
// Code size 43 (0x2b)
.maxstack 8

IL_0000: ldarg.0
IL_0001: ldfld object $Functors.$System_Int32$System_Double$System_Single::objectInstance
IL_0006: dup
IL_0007: brtrue.s IL_001a

IL_0009: pop
IL_000a: ldarg.1
IL_000b: ldarg.2
IL_000c: ldarg.0
IL_000d: ldfld native int $Functors.$System_Int32$System_Double$System_Single::functionPtr
IL_0012: tail.
IL_0014: calli System.Int32(System.Double,System.Single)
IL_0019: ret

IL_001a: nop
IL_001b: ldarg.1
IL_001c: ldarg.2
IL_001d: ldarg.0
IL_001e: ldfld native int $Functors.$System_Int32$System_Double$System_Single::functionPtr
IL_0023: tail.
IL_0025: calli System.Int32(System.Double,System.Single)
IL_002a: ret


$Functors.$System_Int32$System_Double$System_Single/$Delegate$System_Int32$System_Double$System_Single $Functors.$System_Int32$System_Double$System_Single::AsDelegate()
// Method begins at RVA 0x20a4
// Code size 18 (0x12)
.maxstack 8

IL_0000: ldarg.0
IL_0001: ldfld object $Functors.$System_Int32$System_Double$System_Single::objectInstance
IL_0006: ldarg.0
IL_0007: ldfld native int $Functors.$System_Int32$System_Double$System_Single::functionPtr
IL_000c: newobj instance void $Functors.$System_Int32$System_Double$System_Single/$Delegate$System_Int32$System_Double$System_Single::.ctor(object, native int)
IL_0011: ret