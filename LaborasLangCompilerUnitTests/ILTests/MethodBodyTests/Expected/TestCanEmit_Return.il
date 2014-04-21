System.Void klass::dummy()
// Method begins at RVA 0x2050
// Code size 16 (0x10)
.maxstack 8
.entrypoint

IL_0000: ldstr "Your input is: \"{0}\"."
IL_0005: call string klass::ReadInput()
IL_000a: call void [mscorlib]System.Console::WriteLine(string, object)
IL_000f: ret


System.String klass::ReadInput()
// Method begins at RVA 0x2064
// Code size 6 (0x6)
.maxstack 8

IL_0000: call string [mscorlib]System.Console::ReadLine()
IL_0005: ret