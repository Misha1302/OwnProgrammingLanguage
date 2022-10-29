.assembly extern System.Private.CoreLib {}
.assembly extern System.Console {}
.assembly extern mscorlib {}
.assembly extern System.Runtime {}

.assembly Program
{
  .hash algorithm 0x00008004
  .ver 1:0:0:0
}
.module Program.dll


.class public abstract auto ansi sealed beforefieldinit Program extends [mscorlib]System.Object
{
  .method public hidebysig static void Main(string[] args) cil managed
  {
    .entrypoint
    .locals ([0] string s, [1] float32 a)
call string  [System.Console]System.Console::ReadLine()
stloc.s s
ldloc.s s
call float32  [System.Private.CoreLib]System.Convert::ToSingle(string)
stloc.s a
ldloc.s a
ldc.i4 0
conv.r4
ceq
brfalse.s else1

ldstr "Ты //ввел 0"
call void  [System.Console]System.Console::WriteLine(string)
br.s out1

else1:

ldloc.s a
ldc.i4 1
conv.r4
ceq
brfalse.s else2

ldstr "Ты ввел 1"
call void  [System.Console]System.Console::WriteLine(string)
br.s out2

else2:

ldloc.s a
ldc.i4 2
conv.r4
ceq
brfalse.s else3

ldstr "Ты ввел 2"
call void  [System.Console]System.Console::WriteLine(string)
br.s out3

else3:

ldloc.s a
ldc.i4 3
conv.r4
ceq
brfalse.s else4

ldstr "Ты ввел 3"
call void  [System.Console]System.Console::WriteLine(string)
br.s out4

else4:

ldstr "Ты ввел другое число"
call void  [System.Console]System.Console::WriteLine(string)
out4:

out3:

ldstr "Ты увидел надпись \"Ты ввел 2\" или \"Ты ввел 3\" или \"Ты ввел другое число\""
call void  [System.Console]System.Console::WriteLine(string)
out2:

out1:

    call void Program::EndOfMain()
    ret
  }  
  
  .method private static void EndOfMain() cil managed
  {
    .locals init ([0] valuetype [mscorlib]System.ConsoleKeyInfo key)
    ldstr "Press enter to exit..."
    call void [mscorlib]System.Console::WriteLine(string)
    
    loopPoint5412378:
    ldc.i4.1
    call         valuetype [mscorlib]System.ConsoleKeyInfo [mscorlib]System.Console::ReadKey(bool)
    stloc.0

    ldloca.s     key
    call         instance valuetype [mscorlib]System.ConsoleKey [mscorlib]System.ConsoleKeyInfo::get_Key()
    ldc.i4.s     13
    bne.un.s     loopPoint5412378

    ret
  }
}
