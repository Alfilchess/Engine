using InOut;
using System;
using System.Collections.Generic;
using System.Text;
using Types;
using Gaviota;
using Pwned.Chess.Common;
using System.IO;
using System.Diagnostics;

namespace Motor
{
  //---------------------------------------------------------------------------------------------
  //---------------------------------------------------------------------------------------------
  public partial class cMotor
  {
    public static Dictionary<string, cOptConfig> m_mapConfig = new Dictionary<string, cOptConfig>(StringComparer.CurrentCultureIgnoreCase);
    public static cThreadPool m_Threads = new cThreadPool();
    public static cTablaHash m_TablaHash = new cTablaHash();
    public static cConsola m_Consola = new cConsola(System.Console.In, System.Console.Out);
    private static string m_strInfo = "Alfil sf Battles Engine 17.10.06 (beta)";
    private static string m_strAutor = "id author Enrique Sanchez";
    public static cConfigFile m_ConfigFile = new cConfigFile(Process.GetCurrentProcess().ProcessName +".cfg");
    public static cUci m_UCI = new cUci();

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
    public static void Init()
    {
      cMotor.m_ConfigFile.SetNivel(cConfigFile.MAX_LEVEL);

      cMotor.m_UCI.Init(cMotor.m_mapConfig);

      cBitBoard.Init();
      cPosicion.Init();
      cSearch.Init();
      cBonusPeones.Init();
      cEval.Init();
      cMotor.m_Threads.Init();
      cMotor.m_TablaHash.Init((ulong)cMotor.m_mapConfig["Hash"].Get());


#if TABLEBASES
      string strGaviotaPath = cMotor.m_mapConfig["GaviotaTbPath"].getString();
      try
      {
        if (Directory.Exists(strGaviotaPath))
          EGTB.Init(/*AppDomain.CurrentDomain.BaseDirectory*/strGaviotaPath, new WindowsStreamCreator());
      }
      catch(Exception /*ex*/)
      {
      }
#endif
    }

#if TABLEBASES

    public class WindowsStreamCreator : IStreamCreator
    {
      public Stream OpenStream(string path)
      {
        return new FileStream(path, FileMode.Open);
      }
    }
#endif
    //---------------------------------------------------------------------------------------------
    public cMotor(string[] args)
    {
      /*m_ConfigFile.SetNivel(cConfigFile.MAX_LEVEL);

      m_UCI.Init(m_mapConfig);

      cBitBoard.Init();
      cPosicion.Init();
      cSearch.Init();
      cBonusPeones.Init();
      cEval.Init();

      m_Threads.Init();
      m_TablaHash.Init((ulong)m_mapConfig["Hash"].Get());*/
      Init();



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
  }
}
