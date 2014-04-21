System.Void klass::dummy()
// Method begins at RVA 0x2050
// Code size 31 (0x1f)
.maxstack 8
.entrypoint

IL_0000: ldstr "Hi"
IL_0005: ldstr "Default value"
IL_000a: call void klass::MethodWithDefaultParameter(string, string)
IL_000f: ldstr "Hi"
IL_0014: ldstr "NonOptional"
IL_0019: call void klass::MethodWithDefaultParameter(string, string)
IL_001e: ret


System.Void klass::MethodWithDefaultParameter(System.String,System.String)
// Method begins at RVA 0x2070
// Code size 13 (0xd)
.maxstack 8

IL_0000: ldstr "{0}: {1}"
IL_0005: ldarg.0
IL_0006: ldarg.1
IL_0007: call void [mscorlib]System.Console::WriteLine(string, object, object)
IL_000c: ret