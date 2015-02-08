System.Void klass::dummy()
// Method begins at RVA 0x2050
// Code size 1 (0x1)
.maxstack 8
.entrypoint

IL_0000: ret


System.Void klass::.ctor()
// Method begins at RVA 0x2054
// Code size 14 (0xe)
.maxstack 8

IL_0000: ldarg.0
IL_0001: call instance void [mscorlib]System.Object::.ctor()
IL_0006: ldarg.0
IL_0007: ldc.i4.2
IL_0008: stfld int32 klass::testField
IL_000d: ret