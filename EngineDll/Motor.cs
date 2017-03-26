using InOut;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TableBases;
using Types;

namespace Motor
{
  //---------------------------------------------------------------------------------------------
  //---------------------------------------------------------------------------------------------
  public class cMotor
  {
    public static Dictionary<string, cOptConfig> m_mapConfig = new Dictionary<string, cOptConfig>(StringComparer.CurrentCultureIgnoreCase);
    public static cThreadPool m_Threads = new cThreadPool();
    public static cTablaHash m_TablaHash = new cTablaHash();
    public static cConsola m_Consola = new cConsola(System.Console.In, System.Console.Out);
    private static string m_strInfo = "ChessEngine 17.3.15 (beta)";
    private static string m_strAutor = "id author Enrique Sanchez";
    public static cConfigFile m_ConfigFile = new cConfigFile("ChessEngine.cfg");
    public static cUci m_UCI = new cUci();

    private static string[] Defaults = new string[]
      {
        "-rrrbk--/--bb----/--------/--O[pass=false]O[pass=false]O[pass=false]O[pass=false]--/--O[pass=true]O[pass=true]O[pass=true]O[pass=true]--/--P-----/--PQQR--/---BK--- w - - 0 11",
        "rnbqkbnr/pppppppp/---O[pass=true]----/--O[pass=false,skin=obstacle_pass1]-----/--------/--------/PPPPPPPP/RNBQKBNR w KQkq - 0 1"/*,
        "3rr3/2nnnn2/2pppp2/8/8/2PPPP2/2NNNN2/3RR3 w - - 0 1",
        "2rrrr2/2pppp2/8/8/8/8/2PPPP2/2RRRR2 w - - 0 1",
        "/8/8/8/8/1P6/K5k1/8 b - - 0 1",
        "3rr1k1/p1p4p/1p4p1/3PPn2/q1P5/3Q4/PBP3PP/1R3RK1 w - - 0 22",
        "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1",
        "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 10",
        "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 11",
        "4rrk1/pp1n3p/3q2pQ/2p1pb2/2PP4/2P3N1/P2B2PP/4RRK1 b - - 7 19"*/
      };

    //---------------------------------------------------------------------------------------------
    public static string Info()
    {
      StringBuilder s = new StringBuilder(m_strInfo);
      s.Append(cTypes.LF);
      s.Append(m_strAutor);

      return s.ToString();
    }

    //---------------------------------------------------------------------------------------------
    public cMotor()
    {
    }

    //---------------------------------------------------------------------------------------------
    public static void Init(int nLevel = 10)
    {
      cMotor.m_ConfigFile.SetNivel(nLevel);

      cMotor.m_UCI.Init(cMotor.m_mapConfig);

      cBitBoard.Init();
      cPosicion.Init();
      cSearch.Init();
      cBonusPeones.Init();
      cEval.Init();
      cMotor.m_Threads.Init();
      cMotor.m_TablaHash.Init((ulong)cMotor.m_mapConfig["Hash"].Get());
    }

    //---------------------------------------------------------------------------------------------
    public cMotor(string[] args)
    {
      m_ConfigFile.SetNivel(10);

      m_UCI.Init(m_mapConfig);

      cBitBoard.Init();
      cPosicion.Init();
      cSearch.Init();
      cBonusPeones.Init();
      cEval.Init();

      m_Threads.Init();
      m_TablaHash.Init((ulong)m_mapConfig["Hash"].Get());

#if TABLEBASES
      cTablaFinales.Init(AppDomain.CurrentDomain.BaseDirectory);
#endif

      //cPosicion pos = new cPosicion("8/8/8/K2P3k/8/8/8/8 b - - 0 1", false, null);


      //cMotor.Test();
      m_UCI.Recibir(args);

      m_Threads.Close();
    }

    //---------------------------------------------------------------------------------------------
    public void Dispose()
    {
      m_Threads.Close();
    }

    //---------------------------------------------------------------------------------------------
    public static void Test()
    {
      cControlReloj limits = new cControlReloj();
      List<string> posicionesFEN = new List<string>();

      m_mapConfig["Hash"].setCurrentValue("128");
      m_TablaHash.Clear();

      m_mapConfig["Threads"].setCurrentValue("8");

      //m_mapConfig["UCI_LimitStrength"].setCurrentValue("true");
      //m_mapConfig["Level"].setCurrentValue("2");
      

      //-- Tiempo en milisegundos
      limits.movetime = 1000 * 10;

      posicionesFEN.AddRange(Defaults);

      Int64 nodes = 0;

      Stack<cPosInfo> st = new Stack<cPosInfo>();
      long elapsed = cReloj.Now();

      for(int i = 0; i < posicionesFEN.Count; ++i)
      {
        cPosicion posicion = new cPosicion(posicionesFEN[i], m_mapConfig["UCI_Chess960"].Get() != 0 ? true : false, m_Threads.Principal());

        m_Consola.Print(cTypes.LF + (i + 1).ToString() + "/" + posicionesFEN.Count.ToString() + ": ", AccionConsola.NADA);
        m_Consola.Print(posicionesFEN[i], AccionConsola.NADA);
        m_Consola.Print(cTypes.LF, AccionConsola.NADA);

        m_Threads.Analizando(posicion, limits, st);
        m_Threads.WaitAnalizando();
        nodes += (Int64)cSearch.RootPos.GetNodos();

        m_Consola.Print(posicion.DibujaTablero(), AccionConsola.NADA);
      }

      elapsed = cReloj.Now() - elapsed + 1;

      m_Consola.Print(cTypes.LF + "===========================", AccionConsola.NADA);
      m_Consola.Print(cTypes.LF + "Tiempo transcurrido: " + elapsed.ToString(), AccionConsola.NADA);
      m_Consola.Print(cTypes.LF + "Nodos analizados   : " + nodes.ToString(), AccionConsola.NADA);
    }
  }
}
