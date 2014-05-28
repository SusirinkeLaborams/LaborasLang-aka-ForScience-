System.Void klass::dummy()
// Method begins at RVA 0x2050
// Code size 102 (0x66)
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
IL_0016: brfalse.s IL_0027

IL_0018: ldstr "{0} is greater than {1}."
IL_001d: ldloc.0
IL_001e: ldloc.1
IL_001f: tail.
IL_0021: call void [mscorlib]System.Console::WriteLine(string, object, object)
IL_0026: ret

IL_0027: ldloc.0
IL_0028: ldloc.1
IL_0029: call int32 [mscorlib]System.String::CompareOrdinal(string, string)
IL_002e: ldc.i4.0
IL_002f: clt
IL_0031: brfalse.s IL_0042

IL_0033: ldstr "{0} is less than {1}."
IL_0038: ldloc.0
IL_0039: ldloc.1
IL_003a: tail.
IL_003c: call void [mscorlib]System.Console::WriteLine(string, object, object)
IL_0041: ret

IL_0042: ldloc.0
IL_0043: ldloc.1
IL_0044: ceq
IL_0046: brfalse.s IL_0057

IL_0048: ldstr "{0} and {1} are equal."
IL_004d: ldloc.0
IL_004e: ldloc.1
IL_004f: tail.
IL_0051: call void [mscorlib]System.Console::WriteLine(string, object, object)
IL_0056: ret

IL_0057: ldstr "We're screwed."
IL_005c: ldloc.0
IL_005d: ldloc.1
IL_005e: tail.
IL_0060: call void [mscorlib]System.Console::WriteLine(string, object, object)
IL_0065: ret