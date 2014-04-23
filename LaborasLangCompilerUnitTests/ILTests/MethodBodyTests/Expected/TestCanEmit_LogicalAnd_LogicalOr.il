System.Void klass::dummy()
// Method begins at RVA 0x2050
// Code size 75 (0x4b)
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

IL_001c: box [mscorlib]System.Boolean
IL_0021: call void [mscorlib]System.Console::WriteLine(string, object, object, object)
IL_0026: ldstr "{0} || {1} == {2}."
IL_002b: ldloc.0
IL_002c: box [mscorlib]System.Boolean
IL_0031: ldloc.1
IL_0032: box [mscorlib]System.Boolean
IL_0037: ldloc.0
IL_0038: brtrue.s IL_003d

IL_003a: ldloc.1
IL_003b: br.s IL_003e

IL_003d: ldc.i4.1

IL_003e: box [mscorlib]System.Boolean
IL_0043: tail.
IL_0045: call void [mscorlib]System.Console::WriteLine(string, object, object, object)
IL_004a: ret