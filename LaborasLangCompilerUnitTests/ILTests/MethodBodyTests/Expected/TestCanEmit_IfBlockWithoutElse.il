System.Void klass::dummy()
// Method begins at RVA 0x2050
// Code size 19 (0x13)
.maxstack 8
.entrypoint

IL_0000: ldsfld bool klass::myField
IL_0005: brfalse.s IL_0011

IL_0007: ldstr "myField is true."
IL_000c: call void [mscorlib]System.Console::WriteLine(string)

IL_0011: nop
IL_0012: ret