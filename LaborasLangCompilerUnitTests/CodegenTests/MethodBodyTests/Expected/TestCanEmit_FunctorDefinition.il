System.Void klass::dummy()
// Method begins at RVA 0x2050
// Code size 1 (0x1)
.maxstack 8
.entrypoint

IL_0000: ret


System.Void klass/WriteLine$Invoker::.ctor()
// Method begins at RVA 0x2054
// Code size 7 (0x7)
.maxstack 8

IL_0000: ldarg.0
IL_0001: call instance void $Functors.$System_Void?::.ctor()
IL_0006: ret


System.Void klass/WriteLine$Invoker::Invoke()
// Method begins at RVA 0x205c
// Code size 8 (0x8)
.maxstack 8

IL_0000: tail.
IL_0002: call void [mscorlib]System.Console::WriteLine()
IL_0007: ret


$Functors.$System_Void?/Delegate klass/WriteLine$Invoker::AsDelegate()
// Method begins at RVA 0x2068
// Code size 13 (0xd)
.maxstack 8

IL_0000: ldnull
IL_0001: ldftn void [mscorlib]System.Console::WriteLine()
IL_0007: newobj instance void $Functors.$System_Void?/Delegate::.ctor(object, native int)
IL_000c: ret


System.Void $Functors.$System_Void?::.ctor()
// Method begins at RVA 0x2078
// Code size 7 (0x7)
.maxstack 8

IL_0000: ldarg.0
IL_0001: call instance void [mscorlib]System.Object::.ctor()
IL_0006: ret