System.Void klass::.ctor()
// Method begins at RVA 0x2050
// Code size 14 (0xe)
.maxstack 8

IL_0000: ldarg.0
IL_0001: ldc.i4.2
IL_0002: stfld int32 klass::testField
IL_0007: ldarg.0
IL_0008: call instance void [mscorlib]System.Object::.ctor()
IL_000d: ret


System.Void klass::dummy()
// Method begins at RVA 0x2060
// Code size 1 (0x1)
.maxstack 8
.entrypoint

IL_0000: ret