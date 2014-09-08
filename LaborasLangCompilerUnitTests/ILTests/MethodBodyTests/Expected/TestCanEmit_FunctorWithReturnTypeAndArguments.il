System.Void klass::dummy()
// Method begins at RVA 0x2050
// Code size 1 (0x1)
.maxstack 8
.entrypoint

IL_0000: ret


System.Int32 klass::MethodWithArgs(System.Boolean,System.Single,System.String)
// Method begins at RVA 0x2054
// Code size 2 (0x2)
.maxstack 8

IL_0000: ldc.i4.m1
IL_0001: ret


System.Void klass/MethodWithArgs$System_Boolean$System_Single$System_String$Invoker::.ctor()
// Method begins at RVA 0x2058
// Code size 7 (0x7)
.maxstack 8

IL_0000: ldarg.0
IL_0001: call instance void '$Functors.$System_Int32*System_Boolean*System_Single*System_String?'::.ctor()
IL_0006: ret


System.Int32 klass/MethodWithArgs$System_Boolean$System_Single$System_String$Invoker::Invoke(System.Boolean,System.Single,System.String)
// Method begins at RVA 0x2060
// Code size 11 (0xb)
.maxstack 8

IL_0000: ldarg.1
IL_0001: ldarg.2
IL_0002: ldarg.3
IL_0003: tail.
IL_0005: call int32 klass::MethodWithArgs(bool, float32, string)
IL_000a: ret


$Functors.$System_Int32*System_Boolean*System_Single*System_String?/Delegate klass/MethodWithArgs$System_Boolean$System_Single$System_String$Invoker::AsDelegate()
// Method begins at RVA 0x206c
// Code size 13 (0xd)
.maxstack 8

IL_0000: ldnull
IL_0001: ldftn int32 klass::MethodWithArgs(bool, float32, string)
IL_0007: newobj instance void '$Functors.$System_Int32*System_Boolean*System_Single*System_String?'/Delegate::.ctor(object, native int)
IL_000c: ret


System.Void $Functors.$System_Int32*System_Boolean*System_Single*System_String?::.ctor()
// Method begins at RVA 0x207c
// Code size 7 (0x7)
.maxstack 8

IL_0000: ldarg.0
IL_0001: call instance void [mscorlib]System.Object::.ctor()
IL_0006: ret