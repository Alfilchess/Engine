using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using InOut;
using Types;
//using Finales;

using System.Threading;
using System.Linq;
using System.Runtime.InteropServices;
using ComTypes = System.Runtime.InteropServices.ComTypes;
using System.Diagnostics;

using type = System.Int32;
using TableBases;

namespace Motor
{
  

  //------------------------------------------------------------------------------
  //------------------------------------------------------------------------------
  class Program
  {
    public static cCpu m_CPU = new cCpu();

    //------------------------------------------------------------------------------
    static void Main(string[] args)
    {
      cMotor motor = new cMotor(args);
    }
  }
}
