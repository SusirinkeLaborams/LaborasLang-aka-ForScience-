System.Void klass::dummy()
// Method begins at RVA 0x2050
// Code size 91 (0x5b)
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
IL_0008: brfalse.s IL_0022

IL_000a: ldstr "{0} is greater than {1}."
IL_000f: ldloc.0
IL_0010: box [mscorlib]System.Int32
IL_0015: ldloc.1
IL_0016: box [mscorlib]System.Int32
IL_001b: call void [mscorlib]System.Console::WriteLine(string, object, object)
IL_0020: br.s IL_0059

IL_0022: nop
IL_0023: ldloc.0
IL_0024: ldloc.1
IL_0025: clt
IL_0027: brfalse.s IL_0041

IL_0029: ldstr "{0} is less than {1}."
IL_002e: ldloc.0
IL_002f: box [mscorlib]System.Int32
IL_0034: ldloc.1
IL_0035: box [mscorlib]System.Int32
IL_003a: call void [mscorlib]System.Console::WriteLine(string, object, object)
IL_003f: br.s IL_0058

IL_0041: nop
IL_0042: ldstr "{0} and {1} are equal."
IL_0047: ldloc.0
IL_0048: box [mscorlib]System.Int32
IL_004d: ldloc.1
IL_004e: box [mscorlib]System.Int32
IL_0053: call void [mscorlib]System.Console::WriteLine(string, object, object)

IL_0058: nop

IL_0059: nop
IL_005a: ret