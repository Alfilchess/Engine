using InOut;
using Motor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Types;

namespace EngineConsole
{
  class Program
  {
    //------------------------------------------------------------------------------
    public static void FromEngine(string str)
    {
      Debug.WriteLine(str);
      Console.WriteLine(str);
    }
  
    //------------------------------------------------------------------------------
    static void Main(string[] args)
    {
      cMotor motor = new cMotor(args);
    }
  }
}

