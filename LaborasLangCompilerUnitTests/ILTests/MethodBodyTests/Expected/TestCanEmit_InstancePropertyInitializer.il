System.Void klass::set_testProperty(System.String)
// Method begins at RVA 0x2050
// Code size 8 (0x8)
.maxstack 8

IL_0000: ldarg.0
IL_0001: ldarg.1
IL_0002: stfld string klass::testProperty_backingField
IL_0007: ret


System.Void klass::.ctor()
// Method begins at RVA 0x205c
// Code size 18 (0x12)
.maxstack 8

IL_0000: ldarg.0
IL_0001: call instance void [mscorlib]System.Object::.ctor()
IL_0006: ldarg.0
IL_0007: ldstr "aaa"
IL_000c: call instance void klass::set_testProperty(string)
IL_0011: ret


System.Void klass::dummy()
// Method begins at RVA 0x2070
// Code size 1 (0x1)
.maxstack 8
.entrypoint

IL_0000: ret