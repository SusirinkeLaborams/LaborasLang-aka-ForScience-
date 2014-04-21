System.Void klass::dummy()
// Method begins at RVA 0x2050
// Code size 7 (0x7)
.maxstack 1
.entrypoint
.locals (
	[0] class klass
)

IL_0000: newobj instance void klass::.ctor()
IL_0005: stloc.0
IL_0006: ret


System.Void klass::.ctor()
// Method begins at RVA 0x2064
// Code size 7 (0x7)
.maxstack 8

IL_0000: ldarg.0
IL_0001: call instance void [mscorlib]System.Object::.ctor()
IL_0006: ret