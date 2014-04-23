System.Void klass::dummy()
// Method begins at RVA 0x2050
// Code size 18 (0x12)
.maxstack 8
.entrypoint

IL_0000: ldstr "Your input is: \"{0}\"."
IL_0005: call string klass::ReadInput()
IL_000a: tail.
IL_000c: call void [mscorlib]System.Console::WriteLine(string, object)
IL_0011: ret


System.String klass::ReadInput()
// Method begins at RVA 0x2064
// Code size 8 (0x8)
.maxstack 8

IL_0000: tail.
IL_0002: call string [mscorlib]System.Console::ReadLine()
IL_0007: ret