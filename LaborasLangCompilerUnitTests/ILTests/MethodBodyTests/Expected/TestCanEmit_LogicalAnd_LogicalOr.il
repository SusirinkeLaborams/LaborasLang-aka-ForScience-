System.Void klass::dummy()
// Method begins at RVA 0x2050
// Code size 83 (0x53)
.maxstack 4
.entrypoint
.locals (
	[0] bool,
	[1] bool
)

IL_0000: ldc.i4.1
IL_0001: stloc.0
IL_0002: ldc.i4.0
IL_0003: stloc.1
IL_0004: ldstr "{0} && {1} == {2}."
IL_0009: ldloc.0
IL_000a: box [mscorlib]System.Boolean
IL_000f: ldloc.1
IL_0010: box [mscorlib]System.Boolean
IL_0015: ldloc.0
IL_0016: brfalse.s IL_001b

IL_0018: ldloc.1
IL_0019: br.s IL_0020

IL_001b: ldc.i4 0

IL_0020: nop
IL_0021: box [mscorlib]System.Boolean
IL_0026: call void [mscorlib]System.Console::WriteLine(string, object, object, object)
IL_002b: ldstr "{0} || {1} == {2}."
IL_0030: ldloc.0
IL_0031: box [mscorlib]System.Boolean
IL_0036: ldloc.1
IL_0037: box [mscorlib]System.Boolean
IL_003c: ldloc.0
IL_003d: brtrue.s IL_0042

IL_003f: ldloc.1
IL_0040: br.s IL_0047

IL_0042: ldc.i4 1

IL_0047: nop
IL_0048: box [mscorlib]System.Boolean
IL_004d: call void [mscorlib]System.Console::WriteLine(string, object, object, object)
IL_0052: ret