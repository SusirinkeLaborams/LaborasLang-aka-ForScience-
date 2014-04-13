System.Void klass::dummy()
// Method begins at RVA 0x2050
// Code size 17 (0x11)
.maxstack 8
.entrypoint

IL_0000: ldstr "Hello, world!"
IL_0005: call void [mscorlib]System.Console::WriteLine(string)
IL_000a: call valuetype [mscorlib]System.ConsoleKeyInfo [mscorlib]System.Console::ReadKey()
IL_000f: pop
IL_0010: ret