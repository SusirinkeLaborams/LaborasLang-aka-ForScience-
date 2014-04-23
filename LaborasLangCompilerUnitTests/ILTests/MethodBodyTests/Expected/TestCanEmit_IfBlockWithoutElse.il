System.Void klass::dummy()
// Method begins at RVA 0x2050
// Code size 20 (0x14)
.maxstack 8
.entrypoint

IL_0000: ldsfld bool klass::myField
IL_0005: brfalse.s IL_0013

IL_0007: ldstr "myField is true."
IL_000c: tail.
IL_000e: call void [mscorlib]System.Console::WriteLine(string)

IL_0013: ret