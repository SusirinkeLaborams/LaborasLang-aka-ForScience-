System.Void klass::dummy()
// Method begins at RVA 0x2050
// Code size 30 (0x1e)
.maxstack 2
.entrypoint
.locals (
	[0] int32
)

IL_0000: ldc.i4.0
IL_0001: stloc.0
// loop start (head: IL_0002)
	IL_0002: nop
	IL_0003: ldloc.0
	IL_0004: ldc.i4.s 10
	IL_0006: clt
	IL_0008: brfalse.s IL_001c

	IL_000a: ldstr "The loop has looped {0} time(s)."
	IL_000f: ldloc.0
	IL_0010: box [mscorlib]System.Int32
	IL_0015: call void [mscorlib]System.Console::WriteLine(string, object)
	IL_001a: br.s IL_0002
// end loop

IL_001c: nop
IL_001d: ret