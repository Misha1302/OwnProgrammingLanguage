  
  
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