System.Void klass::dummy()
// Method begins at RVA 0x2050
// Code size 28 (0x1c)
.maxstack 2
.entrypoint
.locals (
	[0] int32
)

IL_0000: ldc.i4.0
IL_0001: stloc.0
// loop start (head: IL_0002)
	IL_0002: ldloc.0
	IL_0003: ldc.i4.s 10
	IL_0005: clt
	IL_0007: brfalse.s IL_001b

	IL_0009: ldstr "The loop has looped {0} time(s)."
	IL_000e: ldloc.0
	IL_000f: box [mscorlib]System.Int32
	IL_0014: call void [mscorlib]System.Console::WriteLine(string, object)
	IL_0019: br.s IL_0002
// end loop

IL_001b: ret