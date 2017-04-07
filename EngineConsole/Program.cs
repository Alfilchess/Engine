using Motor;
using System.Diagnostics;

namespace EngineConsole
{
  class Program
  {
    public static cCpu m_CPU = new cCpu();

    //------------------------------------------------------------------------------
    public static void FromEngine(string str)
    {
      Debug.WriteLine(str);
    }
  
    //------------------------------------------------------------------------------
    static void Main(string[] args)
    {
#if CHESSARIA
      cMotor.Init();
      
      cPosicion posStart = new cPosicion("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", false, cMotor.m_Threads.Principal());
      // or
      //cPosicion posStart = new cPosicion("rnbqkbnr/pppppppp/--------/--------/--------/--------/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

      cMotor.m_Consola.SetDelegateFunction(FromEngine);

      cMotor.m_UCI.Command("uci", ref posStart);
      cMotor.m_UCI.Command("setoption name Hash value 128", ref posStart);
      cMotor.m_UCI.Command("setoption name Ponder value false", ref posStart);
      cMotor.m_UCI.Command("setoption name Threads value 4", ref posStart); //-- From 1 to 16
      cMotor.m_UCI.Command("setoption name Level 16", ref posStart);
      //cMotor.m_UCI.Command("setoption name UCI_LimitStrength value true", ref posStart); 
      //cMotor.m_UCI.Command("setoption name UCI_Elo value 3000", ref posStart); //-- From 200 to 300
      cMotor.m_UCI.Command("ucinewgame", ref posStart);
      cMotor.m_UCI.Command("isready", ref posStart);

      //-- Without king by moment, put king into a hole
      //cMotor.m_UCI.Command("position fen 1n4nk/1r1r1r2/2r1r3/8/8/2R1R3/1R1R1R2/2N1N2K b - - 0 1", posStart);
      //posStart.SetAgujero(cCasilla.H1);
      //posStart.SetAgujero(cCasilla.H8);

      //-- FEN position
      //cMotor.m_UCI.Command("position fen -rrrbk--/--bb----/--------/--O[pass=false]O[pass=false]O[pass=false]O[pass=false]--/--O[pass=true]O[pass=true]O[pass=true]O[pass=true]--/--P-----/--PQQR--/---BK--- w - - 0 11", ref posStart);
      //cMotor.m_UCI.Command("position fen r3k2r/5ppp/8/8/8/4R3/PPP1PPPP/R3K2R b KQkq - 0 1", ref posStart);
      //-- History moves
      //cMotor.m_UCI.Command("position startpos moves e2e4", ref posStart);

      //-- Holes (agujero) and walls(obstaculo) before thinking
      /*posStart.SetAgujero(cCasilla.D4);
      posStart.SetAgujero(cCasilla.D5);
      posStart.SetAgujero(cCasilla.E4);
      posStart.SetAgujero(cCasilla.E5);
      posStart.SetWall(cCasilla.A5);
      posStart.SetWall(cCasilla.B5);
      posStart.SetWall(cCasilla.C5);
      posStart.SetWall(cCasilla.A4);
      posStart.SetWall(cCasilla.B4);
      posStart.SetWall(cCasilla.C4);
      posStart.SetWall(cCasilla.H5);
      posStart.SetWall(cCasilla.G5);
      posStart.SetWall(cCasilla.F5);
      posStart.SetWall(cCasilla.F4);
      posStart.SetWall(cCasilla.G4);
      posStart.SetWall(cCasilla.H4);
      */
      //cMotor.m_UCI.Command("position fen 2k5/1n6/1nn5/8/8/8/2NNN3/4K3 w - - 0 1", posStart);
      cMotor.m_UCI.Command("go wtime 3000000 btime 3000000 winc 0 binc 0 searchmoves e8g8", ref posStart);
      

      //-- Waiting and stop when user needs
      System.Threading.Thread.Sleep(3000);
      //Debug.Write(posStart.DibujaTablero());
      //cMotor.m_UCI.Command("stop", ref posStart);
      
#else
      cMotor motor = new cMotor(args);
#endif
    }
  }
}

