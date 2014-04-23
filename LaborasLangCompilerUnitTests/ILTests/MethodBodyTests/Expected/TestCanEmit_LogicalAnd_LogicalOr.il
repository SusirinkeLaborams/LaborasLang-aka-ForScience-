System.Void klass::dummy()
// Method begins at RVA 0x2050
// Code size 77 (0x4d)
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
IL_0019: br.s IL_001c

IL_001b: ldc.i4.0

IL_001c: nop
IL_001d: box [mscorlib]System.Boolean
IL_0022: call void [mscorlib]System.Console::WriteLine(string, object, object, object)
IL_0027: ldstr "{0} || {1} == {2}."
IL_002c: ldloc.0
IL_002d: box [mscorlib]System.Boolean
IL_0032: ldloc.1
IL_0033: box [mscorlib]System.Boolean
IL_0038: ldloc.0
IL_0039: brtrue.s IL_003e

IL_003b: ldloc.1
IL_003c: br.s IL_003f

IL_003e: ldc.i4.1

IL_003f: nop
IL_0040: box [mscorlib]System.Boolean
IL_0045: tail.
IL_0047: call void [mscorlib]System.Console::WriteLine(string, object, object, object)
IL_004c: ret