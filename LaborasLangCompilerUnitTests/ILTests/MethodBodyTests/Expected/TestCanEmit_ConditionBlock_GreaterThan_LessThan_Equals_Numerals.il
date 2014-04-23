System.Void klass::dummy()
// Method begins at RVA 0x2050
// Code size 128 (0x80)
.maxstack 3
.entrypoint
.locals (
	[0] int32,
	[1] int32
)

IL_0000: ldc.i4.5
IL_0001: stloc.0
IL_0002: ldc.i4.6
IL_0003: stloc.1
IL_0004: ldloc.0
IL_0005: ldloc.1
IL_0006: cgt
IL_0008: brfalse.s IL_0025

IL_000a: ldstr "{0} is greater than {1}."
IL_000f: ldloc.0
IL_0010: box [mscorlib]System.Int32
IL_0015: ldloc.1
IL_0016: box [mscorlib]System.Int32
IL_001b: tail.
IL_001d: call void [mscorlib]System.Console::WriteLine(string, object, object)
IL_0022: ret

IL_0023: br.s IL_007f

IL_0025: ldloc.0
IL_0026: ldloc.1
IL_0027: clt
IL_0029: brfalse.s IL_0046

IL_002b: ldstr "{0} is less than {1}."
IL_0030: ldloc.0
IL_0031: box [mscorlib]System.Int32
IL_0036: ldloc.1
IL_0037: box [mscorlib]System.Int32
IL_003c: tail.
IL_003e: call void [mscorlib]System.Console::WriteLine(string, object, object)
IL_0043: ret

IL_0044: br.s IL_007f

IL_0046: ldloc.0
IL_0047: ldloc.1
IL_0048: ceq
IL_004a: brfalse.s IL_0067

IL_004c: ldstr "{0} and {1} are equal."
IL_0051: ldloc.0
IL_0052: box [mscorlib]System.Int32
IL_0057: ldloc.1
IL_0058: box [mscorlib]System.Int32
IL_005d: tail.
IL_005f: call void [mscorlib]System.Console::WriteLine(string, object, object)
IL_0064: ret

IL_0065: br.s IL_007f

IL_0067: ldstr "We're screwed."
IL_006c: ldloc.0
IL_006d: box [mscorlib]System.Int32
IL_0072: ldloc.1
IL_0073: box [mscorlib]System.Int32
IL_0078: tail.
IL_007a: call void [mscorlib]System.Console::WriteLine(string, object, object)

IL_007f: ret