using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Motor;
using InOut;
using System.Diagnostics;

namespace EngineConsole
{
  class Program
  {
    public static cCpu m_CPU = new cCpu();

    public static void FromEngine(string str)
    {
      Debug.Write(str);
    }
  
    //------------------------------------------------------------------------------
    static void Main(string[] args)
    {
      //cMotor motor = new cMotor(args);

      cMotor.m_ConfigFile.SetNivel(10);

      cUci.Init(cMotor.m_mapConfig);

      cBitBoard.Init();
      cPosicion.Init();
      cSearch.Init();
      cBonusPeones.Init();
      cEval.Init();
      cMotor.m_Threads.Init();
      cMotor.m_TablaHash.Init((ulong)cMotor.m_mapConfig["Hash"].Get());

      cPosicion posStart = new cPosicion("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", false, cMotor.m_Threads.Principal());
      cMotor.m_Consola.SetDelegateFunction(FromEngine);

      cUci.Command("uci", posStart);
      cUci.Command("setoption name Hash value 128", posStart);
      cUci.Command("setoption name Ponder value false", posStart);
      cUci.Command("ucinewgame", posStart);
      cUci.Command("isready", posStart);
      cUci.Command("position fen 2k5/1n6/1nn5/8/8/8/2NNN3/4K3 w - - 0 1", posStart);
      cUci.Command("go wtime 300000 btime 300000 winc 0 binc 0", posStart);
    }
  }
}

