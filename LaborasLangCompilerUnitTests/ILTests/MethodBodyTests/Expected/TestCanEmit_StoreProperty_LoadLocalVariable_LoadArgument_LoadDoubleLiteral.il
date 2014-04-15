System.Void klass::set_doubleProperty(System.Double)
// Method begins at RVA 0x2050
// Code size 7 (0x7)
.maxstack 8

IL_0000: ldarg.0
IL_0001: stsfld float64 klass::doubleProperty_backingField
IL_0006: ret


System.Void klass::dummy()
// Method begins at RVA 0x2058
// Code size 17 (0x11)
.maxstack 1
.entrypoint
.locals (
	[0] float64
)

IL_0000: ldc.r8 5.5
IL_0009: stloc.0
IL_000a: ldloc.0
IL_000b: call void klass::set_doubleProperty(float64)
IL_0010: ret