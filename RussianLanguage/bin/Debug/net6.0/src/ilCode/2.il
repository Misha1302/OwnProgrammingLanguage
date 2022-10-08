  
  
  .method private static void EndOfMain() cil managed
  {
    .locals init ([0] valuetype [System.Console]System.ConsoleKeyInfo key)
    ldstr "Press enter to exit"
    call void [mscorlib]System.Console::WriteLine(string)
    
    // считываем 
    loopPoint:
    ldc.i4.1
    call         valuetype [System.Console]System.ConsoleKeyInfo [System.Console]System.Console::ReadKey(bool)
    stloc.0

    ldloca.s     key
    call         instance valuetype [System.Console]System.ConsoleKey [System.Console]System.ConsoleKeyInfo::get_Key()
    ldc.i4.s     13 // enter
    bne.un.s     loopPoint

    ret
  }