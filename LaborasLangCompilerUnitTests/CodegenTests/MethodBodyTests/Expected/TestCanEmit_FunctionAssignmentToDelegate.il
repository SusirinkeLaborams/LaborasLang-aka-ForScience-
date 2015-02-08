System.Void klass::dummy()
// Method begins at RVA 0x2050
// Code size 18 (0x12)
.maxstack 8
.entrypoint

IL_0000: ldnull
IL_0001: ldftn void klass::dummy()
IL_0007: newobj instance void klass/MyDelegate::.ctor(object, native int)
IL_000c: stsfld class klass/MyDelegate klass::myDelegate
IL_0011: ret