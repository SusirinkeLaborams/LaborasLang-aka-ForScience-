System.Void klass::dummy()
// Method begins at RVA 0x2050
// Code size 81 (0x51)
.maxstack 3
.entrypoint
.locals (
	[0] string,
	[1] string
)

IL_0000: ldstr "hi"
IL_0005: stloc.0
IL_0006: ldstr "bye"
IL_000b: stloc.1
IL_000c: ldloc.0
IL_000d: ldloc.1
IL_000e: call int32 [mscorlib]System.String::CompareOrdinal(string, string)
IL_0013: ldc.i4.0
IL_0014: cgt
IL_0016: brfalse.s IL_0026

IL_0018: ldstr "{0} is greater than {1}."
IL_001d: ldloc.0
IL_001e: ldloc.1
IL_001f: call void [mscorlib]System.Console::WriteLine(string, object, object)
IL_0024: br.s IL_004f

IL_0026: nop
IL_0027: ldloc.0
IL_0028: ldloc.1
IL_0029: call int32 [mscorlib]System.String::CompareOrdinal(string, string)
IL_002e: ldc.i4.0
IL_002f: clt
IL_0031: brfalse.s IL_0041

IL_0033: ldstr "{0} is less than {1}."
IL_0038: ldloc.0
IL_0039: ldloc.1
IL_003a: call void [mscorlib]System.Console::WriteLine(string, object, object)
IL_003f: br.s IL_004e

IL_0041: nop
IL_0042: ldstr "{0} and {1} are equal."
IL_0047: ldloc.0
IL_0048: ldloc.1
IL_0049: call void [mscorlib]System.Console::WriteLine(string, object, object)

IL_004e: nop

IL_004f: nop
IL_0050: ret