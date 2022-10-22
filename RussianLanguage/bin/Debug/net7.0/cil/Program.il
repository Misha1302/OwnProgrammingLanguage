.assembly extern System.Console {}
.assembly extern mscorlib {}
.assembly extern System.Runtime {}
.assembly extern System.Console {}

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
    .locals ([0] string numberStr)
call string [System.Console]System.Console::ReadLine()
stloc.s numberStr
ldloc.s numberStr
ldstr ", World!"
call string [System.Runtime]System.String::Concat(string, string)
stloc.s numberStr
ldloc.s numberStr
call void [System.Console]System.Console::WriteLine(string)
ldstr "Конец программы :>"
call void [System.Console]System.Console::WriteLine(string)
    call void Program::EndOfMain()
    ret
  }  
  
  .method private static void EndOfMain() cil managed
  {
    .locals init ([0] valuetype [mscorlib]System.ConsoleKeyInfo key)
    ldstr "Press enter to exit"
    call void [mscorlib]System.Console::WriteLine(string)
    
    // считываем 
    loopPoint:
    ldc.i4.1
    call         valuetype [mscorlib]System.ConsoleKeyInfo [mscorlib]System.Console::ReadKey(bool)
    stloc.0

    ldloca.s     key
    call         instance valuetype [mscorlib]System.ConsoleKey [mscorlib]System.ConsoleKeyInfo::get_Key()
    ldc.i4.s     13 // enter key
    bne.un.s     loopPoint

    ret
  }
}