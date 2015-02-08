System.Void klass::dummy()
// Method begins at RVA 0x2050
// Code size 23 (0x17)
.maxstack 8
.entrypoint

IL_0000: ldstr "testing string addition: "
IL_0005: ldc.i4.s 22
IL_0007: box [mscorlib]System.Int32
IL_000c: call string [mscorlib]System.String::Concat(object, object)
IL_0011: stsfld string klass::myField
IL_0016: ret