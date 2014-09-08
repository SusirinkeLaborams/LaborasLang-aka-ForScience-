System.Void klass::dummy()
// Method begins at RVA 0x2050
// Code size 1 (0x1)
.maxstack 8
.entrypoint

IL_0000: ret


System.Int32 klass::FunctionWithArgs(System.Double,System.Single)
// Method begins at RVA 0x2054
// Code size 2 (0x2)
.maxstack 8

IL_0000: ldc.i4.5
IL_0001: ret


System.Void klass::.cctor()
// Method begins at RVA 0x2058
// Code size 11 (0xb)
.maxstack 8

IL_0000: newobj instance void klass/FunctionWithArgs$System_Double$System_Single$Invoker::.ctor()
IL_0005: stsfld class '$Functors.$System_Int32*System_Double*System_Single?' klass::myFunction
IL_000a: ret


System.Void klass/FunctionWithArgs$System_Double$System_Single$Invoker::.ctor()
// Method begins at RVA 0x2064
// Code size 7 (0x7)
.maxstack 8

IL_0000: ldarg.0
IL_0001: call instance void '$Functors.$System_Int32*System_Double*System_Single?'::.ctor()
IL_0006: ret


System.Int32 klass/FunctionWithArgs$System_Double$System_Single$Invoker::Invoke(System.Double,System.Single)
// Method begins at RVA 0x206c
// Code size 10 (0xa)
.maxstack 8

IL_0000: ldarg.1
IL_0001: ldarg.2
IL_0002: tail.
IL_0004: call int32 klass::FunctionWithArgs(float64, float32)
IL_0009: ret


$Functors.$System_Int32*System_Double*System_Single?/Delegate klass/FunctionWithArgs$System_Double$System_Single$Invoker::AsDelegate()
// Method begins at RVA 0x2078
// Code size 13 (0xd)
.maxstack 8

IL_0000: ldnull
IL_0001: ldftn int32 klass::FunctionWithArgs(float64, float32)
IL_0007: newobj instance void '$Functors.$System_Int32*System_Double*System_Single?'/Delegate::.ctor(object, native int)
IL_000c: ret


System.Void $Functors.$System_Int32*System_Double*System_Single?::.ctor()
// Method begins at RVA 0x2088
// Code size 7 (0x7)
.maxstack 8

IL_0000: ldarg.0
IL_0001: call instance void [mscorlib]System.Object::.ctor()
IL_0006: ret