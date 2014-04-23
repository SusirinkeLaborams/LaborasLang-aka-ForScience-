System.Void klass::dummy()
// Method begins at RVA 0x2050
// Code size 33 (0x21)
.maxstack 8
.entrypoint

IL_0000: ldstr "Hi"
IL_0005: ldstr "Default value"
IL_000a: call void klass::MethodWithDefaultParameter(string, string)
IL_000f: ldstr "Hi"
IL_0014: ldstr "NonOptional"
IL_0019: tail.
IL_001b: call void klass::MethodWithDefaultParameter(string, string)
IL_0020: ret


System.Void klass::MethodWithDefaultParameter(System.String,System.String)
// Method begins at RVA 0x2074
// Code size 15 (0xf)
.maxstack 8

IL_0000: ldstr "{0}: {1}"
IL_0005: ldarg.0
IL_0006: ldarg.1
IL_0007: tail.
IL_0009: call void [mscorlib]System.Console::WriteLine(string, object, object)
IL_000e: ret