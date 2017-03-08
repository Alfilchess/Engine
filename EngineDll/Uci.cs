using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;
using Types;
using Motor;

using mov = System.Int32;
using val = System.Int32;
using sq = System.Int32;

namespace InOut
{
  //---------------------------------------------------------------------------------
  public partial class cUci
  {
    //---------------------------------------------------------------------------------
    public static Stack<cPosInfo> SetupStates = new Stack<cPosInfo>();

    private static string[] strPromotionChar = new string[cColor.SIZE] { " PNBRQK", " pnbrqk" };

    //---------------------------------------------------------------------------------
    public static string Puntos(val v, val alpha, val beta)
    {
      StringBuilder ss = new StringBuilder();

      if (Math.Abs(v) < cValoresJuego.MATE_MAXIMO)
        ss.Append("cp " + (v * 100 / cValoresJuego.PEON_FINAL)); //-- En centipeones
      else
        ss.Append("mate " + ((v > 0 ? cValoresJuego.MATE - v + 1 : -cValoresJuego.MATE - v) / 2));

      return ss.ToString();
    }

    //---------------------------------------------------------------------------------
    public static string GetNombreCasilla(sq s)
    {
      char[] ch = new char[2] { cTypes.GetCharColumna(cTypes.Columna(s)), cTypes.GetCharFila(cTypes.Fila(s)) };
      return new string(ch);
    }

    //---------------------------------------------------------------------------------
    public static string GetMovimiento(mov m, bool bChess960)
    {
      sq from = cTypes.GetFromCasilla(m);
      sq to = cTypes.GetToCasilla(m);

      if (m == cMovType.MOV_NAN)
        return "(none)";

      if (m == cMovType.MOV_NULO)
        return "0000";

      if (cTypes.TipoMovimiento(m) == cMovType.ENROQUE && !bChess960)
        to = cTypes.CreaCasilla((to > from ? COLUMNA.G : COLUMNA.C), cTypes.Fila(from));

      string move = GetNombreCasilla(from) + GetNombreCasilla(to);

      if (cTypes.TipoMovimiento(m) == cMovType.PROMOCION)
        move += strPromotionChar[cColor.NEGRO][cTypes.GetPromocion(m)];

      return move;
    }

    //---------------------------------------------------------------------------------
    public static mov GetFromUCI(cPosicion pos, string str)
    {
      if (str.Length == 5)
      {
        char[] strChar = str.ToCharArray();
        strChar[4] = char.ToLower(strChar[4]);
        str = new String(strChar);
      }

      for (cReglas it = new cReglas(pos, cMovType.LEGAL); it.GetActualMov() != cMovType.MOV_NAN; ++it)
        if (str == GetMovimiento(it.GetActualMov(), pos.IsChess960() != false))
          return it.GetActualMov();

      return cMovType.MOV_NAN;
    }

    //---------------------------------------------------------------------------------
    public static void Init(Dictionary<string, cOptConfig> config)
    {
      config.Clear();

      config["Threads"] = new cOptConfig("Threads", config.Count, 1, 1, cThreadPool.MAX_THREADS, cOptConfig.OnThread, true, false);
      config["Hash"] = new cOptConfig("Hash", config.Count, 128, 1, 4096, cOptConfig.OnHash, true, false);
      config["Clear Hash"] = new cOptConfig("Clear Hash", config.Count, cOptConfig.OnClearHash, true);
      config["Ponder"] = new cOptConfig("Ponder", config.Count, false, null, true);
      config["Log"] = new cOptConfig("Log", config.Count, false, null, true);
      config["OwnBook"] = new cOptConfig("OwnBook", config.Count, true, null, true);
      config["MultiPV"] = new cOptConfig("MultiPV", config.Count, 1, 1, 3, null, true, false);
      config["UCI_Chess960"] = new cOptConfig("UCI_Chess960", config.Count, false, null, true);
      config["Level"] = new cOptConfig("Level", config.Count, 10, 1, 10, cOptConfig.OnLevel, false, false);
      config["UCI_EngineAbout"] = new cOptConfig("UCI_EngineAbout", config.Count, "www.alfilchess.com", null, true);
  
      config["MOVILIDAD_MEDIO_JUEGO"] = new cOptConfig("MOVILIDAD_MEDIO_JUEGO", config.Count, 50, 0, 200, cOptConfig.OnEval, false, true);
      config["MOVILIDAD_FINAL"] = new cOptConfig("MOVILIDAD_FINAL", config.Count, 50, 0, 200, cOptConfig.OnEval, false, true);
      config["PEON_DEFENDIDO_MEDIO_JUEGO"] = new cOptConfig("PEON_DEFENDIDO_MEDIO_JUEGO", config.Count, 50, 0, 200, cOptConfig.OnEval, false, true);
      config["PEON_DEFENDIDO_FINAL"] = new cOptConfig("PEON_DEFENDIDO_FINAL", config.Count, 50, 0, 200, cOptConfig.OnEval, false, true);
      config["PEON_PASADO_MEDIO_JUEGO"] = new cOptConfig("PEON_PASADO_MEDIO_JUEGO", config.Count, 50, 0, 200, cOptConfig.OnEval, false, true);
      config["PEON_PASADO_FINAL"] = new cOptConfig("PEON_PASADO_FINAL", config.Count, 50, 0, 200, cOptConfig.OnEval, false, true);
      config["ESPACIOS"] = new cOptConfig("ESPACIOS", config.Count, 50, 0, 200, cOptConfig.OnEval, false, true);
      config["JUEGO_AGRESIVO"] = new cOptConfig("JUEGO_AGRESIVO", config.Count, 50, 0, 200, cOptConfig.OnEval, false, true);
      config["JUEGO_DEFENSIVO"] = new cOptConfig("JUEGO_DEFENSIVO", config.Count, 50, 0, 200, cOptConfig.OnEval, false, true);

      config["PEON_VALOR_MEDIO_JUEGO"] = new cOptConfig("PEON_VALOR_MEDIO_JUEGO", config.Count, 7, 0, 3000, null, false, true);
      config["PEON_VALOR_FINAL"] = new cOptConfig("PEON_VALOR_FINAL", config.Count, 9, 0, 3000, null, false, true);
      config["CABALLO_VALOR_MEDIO_JUEGO"] = new cOptConfig("CABALLO_VALOR_MEDIO_JUEGO", config.Count, 27, 0, 3000, null, false, true);
      config["CABALLO_VALOR_FINAL"] = new cOptConfig("CABALLO_VALOR_FINAL", config.Count, 28, 0, 3000, null, false, true);
      config["ALFIL_VALOR_MEDIO_JUEGO"] = new cOptConfig("ALFIL_VALOR_MEDIO_JUEGO", config.Count, 28, 0, 3000, null, false, true);
      config["ALFIL_VALOR_FINAL"] = new cOptConfig("ALFIL_VALOR_FINAL", config.Count, 28, 0, 3000, null, false, true);
      config["TORRE_VALOR_MEDIO_JUEGO"] = new cOptConfig("TORRE_VALOR_MEDIO_JUEGO", config.Count, 42, 0, 3000, null, false, true);
      config["TORRE_VALOR_FINAL"] = new cOptConfig("TORRE_VALOR_FINAL", config.Count, 43, 0, 3000, null, false, true);
      config["DAMA_VALOR_MEDIO_JUEGO"] = new cOptConfig("DAMA_VALOR_MEDIO_JUEGO", config.Count, 84, 0, 3000, null, false, true);
      config["DAMA_VALOR_FINAL"] = new cOptConfig("DAMA_VALOR_FINAL", config.Count, 85, 0, 3000, null, false, true);
      config["MEDIO_JUEGO"] = new cOptConfig("MEDIO_JUEGO", config.Count, 50, 0, cValoresJuego.MATE, null, false, true);
      config["FINAL"] = new cOptConfig("FINAL", config.Count, 12, 0, cValoresJuego.MATE, null, false, true);

      config["PROFUNDIDAD_DE_FRACTURA"] = new cOptConfig("PROFUNDIDAD_DE_FRACTURA", config.Count, 17, 0, 12, cOptConfig.OnThread, false, true); //16.6//2
      config["FACTOR_CONTENCION"] = new cOptConfig("FACTOR_CONTENCION", config.Count, 50, -50, 50, null, false, true);

      config["CONTROL_DE_HORIZONTE"] = new cOptConfig("CONTROL_DE_HORIZONTE", config.Count, 40, 0, 100, null, false, true);

      config["CONTROL_DE_EMERGENCIA"] = new cOptConfig("CONTROL_DE_EMERGENCIA", config.Count, 60, 0, 100, null, false, true);//60
      config["TIEMPO_EXTRA"] = new cOptConfig("TIEMPO_EXTRA", config.Count, 30, 0, 100, null, false, true);//30
      config["TIEMPO_MINIMO"] = new cOptConfig("TIEMPO_MINIMO", config.Count, 20, 0, 100, null, false, true);//20
      config["TIEMPO_LENTO"] = new cOptConfig("TIEMPO_LENTO", config.Count, 80, 10, 100, null, false, true);//80
    }

    //---------------------------------------------------------------------------------
    public static void SetPosicion(cPosicion posicion, Stack<string> lista)
    {
      mov m;
      string fen = string.Empty;
      string token = lista.Pop();

      if (token == "startpos")
      {
        fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        if (lista.Count > 0)
        {
          token = lista.Pop();
        }
      }
      else if (token == "fen")
        while ((lista.Count > 0) && (token = lista.Pop()) != "moves")
          fen += token + " ";
      else
        return;

      posicion.SetFEN(fen, cMotor.m_mapConfig["UCI_Chess960"].Get() != 0 ? true : false, cMotor.m_Threads.Principal());
      SetupStates = new Stack<cPosInfo>();

      while ((lista.Count > 0) && (m = cUci.GetFromUCI(posicion, token = lista.Pop())) != cMovType.MOV_NAN)
      {
        SetupStates.Push(new cPosInfo());
        posicion.DoMov(m, SetupStates.Peek());
      }
    }

    //---------------------------------------------------------------------------------
    public static void SetOption(Stack<string> lista)
    {
      string name = string.Empty, value = string.Empty;

      string token = lista.Pop();

      while ((lista.Count > 0) && ((token = lista.Pop()) != "value"))
        name += (name == null ? string.Empty : " ") + token;

      while ((lista.Count > 0) && ((token = lista.Pop()) != "value"))
        value += (value == null ? string.Empty : " ") + token;

      name = name.Trim();
      if (!String.IsNullOrEmpty(name) && cMotor.m_mapConfig.ContainsKey(name))
        cMotor.m_mapConfig[name].setCurrentValue(value);
      else
        cMotor.m_Consola.PrintLine("Opción no existente: ", AccionConsola.ATOMIC);
    }

    //---------------------------------------------------------------------------------
    public static void GoUCI(cPosicion pos, Stack<string> lista_valores)
    {
      cControlReloj control = new cControlReloj();

      string token = string.Empty;

      while (lista_valores.Count > 0)
      {
        token = lista_valores.Pop();
        if (token == "searchmoves")
          while ((token = lista_valores.Pop()) != null)
            control.searchmoves.Add(cUci.GetFromUCI(pos, token));

        else if (token == "wtime")
          control.time[cColor.BLANCO] = int.Parse(lista_valores.Pop());
        else if (token == "btime")
          control.time[cColor.NEGRO] = int.Parse(lista_valores.Pop());
        else if (token == "winc")
          control.inc[cColor.BLANCO] = int.Parse(lista_valores.Pop());
        else if (token == "binc")
          control.inc[cColor.NEGRO] = int.Parse(lista_valores.Pop());
        else if (token == "movestogo")
          control.movestogo = int.Parse(lista_valores.Pop());
        else if (token == "depth")
          control.depth = int.Parse(lista_valores.Pop());
        else if (token == "nodes")
          control.nodes = int.Parse(lista_valores.Pop());
        else if (token == "movetime")
          control.movetime = int.Parse(lista_valores.Pop());
        else if (token == "mate")
          control.mate = int.Parse(lista_valores.Pop());
        else if (token == "infinite")
          control.infinite = 1;
        else if (token == "ponder")
          control.ponder = 1;
      }

      cMotor.m_Threads.Analizando(pos, control, SetupStates);
    }

    //---------------------------------------------------------------------------------
    public static Stack<string> CreateStack(string input)
    {
      string[] lines = input.Trim().Split(' ');
      Stack<string> stack = new Stack<string>();
      for (int i = (lines.Length - 1); i >= 0; i--)
      {
        string line = lines[i];
        if (!String.IsNullOrEmpty(line))
        {
          line = line.Trim();
          stack.Push(line);
        }
      }
      return stack;
    }

    //---------------------------------------------------------------------------------
    public static string ShowOption(Dictionary<string, cOptConfig> o)
    {
      List<cOptConfig> list = new List<cOptConfig>();
      list.AddRange(o.Values);
      list.Sort(new cOptConfig.OptionComparer());
      StringBuilder sb = new StringBuilder();

      foreach (cOptConfig opt in list)
      {
        if (opt.m_bUCI == true)
        {
          sb.Append(cTypes.LF);
          sb.Append("option name ").Append(opt.m_nNombre).Append(" type ").Append(opt.m_strTipo);
          if (opt.m_strTipo != "button")
            sb.Append(" default ").Append(opt.m_strDefecto);

          if (opt.m_strTipo == "spin")
            sb.Append(" min ").Append(opt.m_nMin).Append(" max ").Append(opt.m_nMax);
        }
      }
      return sb.ToString();
    }

    //---------------------------------------------------------------------------------
    public static void Command(string cmd, cPosicion pos)
    {
      //cPosicion pos = new cPosicion("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", false, cMotor.m_Threads.Principal());
      //cPosicion pos = new cPosicion("8/8/3N1K2/8/1B6/4k3/8/8 w - - 1 78 ", false, cMotor.m_Threads.Principal());

      string token = ""/*, cmd = ""*/;
      
      do
      {
        //if(argv.Length == 0 && String.IsNullOrEmpty(cmd = cMotor.m_Consola.ReadLine(AccionConsola.NADA)))
        //  cmd = "quit";

        Stack<string> stack = CreateStack(cmd);
        token = stack.Pop();

        if(token == "quit" || token == "stop" || token == "ponderhit")
        {
          if(token != "ponderhit" || cSearch.Signals.STOP_ON_PONDER)
          {
            cSearch.Signals.STOP = true;
            cMotor.m_Threads.Principal().Signal();
          }
          else
            cSearch.Limits.ponder = 0;
        }
        else if(token == "uci")
        {
          cMotor.m_Consola.PrintLine("id name " + cMotor.Info(), AccionConsola.GET);
          cMotor.m_Consola.PrintLine(ShowOption(cMotor.m_mapConfig), AccionConsola.NADA);
          cMotor.m_Consola.PrintLine("uciok", AccionConsola.RELEASE);
        }
        else if(token == "go")
          GoUCI(pos, stack);
        else if(token == "position")
          SetPosicion(pos, stack);
        else if(token == "setoption")
          SetOption(stack);
        else if(token == "test")
          cMotor.Test();
        else if(token == "isready")
          cMotor.m_Consola.PrintLine("readyok", AccionConsola.ATOMIC);
        else if(token == "ucinewgame")
          cMotor.m_TablaHash.Clear();
        else
          cMotor.m_Consola.PrintLine("Comando desconocido: " + cmd, AccionConsola.ATOMIC);

      } while(token != "quit" && cmd.Length == 0);

      //cMotor.m_Threads.WaitAnalizando();
    }

    //---------------------------------------------------------------------------------
    public static void Recibir(String[] argv)
    {
      cPosicion pos = new cPosicion("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", false, cMotor.m_Threads.Principal());
      //cPosicion pos = new cPosicion("8/8/3N1K2/8/1B6/4k3/8/8 w - - 1 78 ", false, cMotor.m_Threads.Principal());
      
      string token = "", cmd = "";

      for (int i = 0; i < argv.Length; ++i)
        cmd += argv[i] + " ";

      do
      {
        if (argv.Length == 0 && String.IsNullOrEmpty(cmd = cMotor.m_Consola.ReadLine(AccionConsola.NADA)))
          cmd = "quit";

        Stack<string> stack = CreateStack(cmd);
        token = stack.Pop();

        if (token == "quit" || token == "stop" || token == "ponderhit")
        {
          if (token != "ponderhit" || cSearch.Signals.STOP_ON_PONDER)
          {
            cSearch.Signals.STOP = true;
            cMotor.m_Threads.Principal().Signal();
          }
          else
            cSearch.Limits.ponder = 0;
        }
        else if (token == "uci")
        {
          cMotor.m_Consola.PrintLine("id name " + cMotor.Info(), AccionConsola.GET);
          cMotor.m_Consola.PrintLine(ShowOption(cMotor.m_mapConfig), AccionConsola.NADA);
          cMotor.m_Consola.PrintLine("uciok", AccionConsola.RELEASE);
        }
        else if (token == "go")
          GoUCI(pos, stack);
        else if (token == "position")
          SetPosicion(pos, stack);
        else if (token == "setoption")
          SetOption(stack);
        else if (token == "test")
          cMotor.Test();
        else if (token == "isready")
          cMotor.m_Consola.PrintLine("readyok", AccionConsola.ATOMIC);
        else if (token == "ucinewgame")
          cMotor.m_TablaHash.Clear();
        else
          cMotor.m_Consola.PrintLine("Comando desconocido: " + cmd, AccionConsola.ATOMIC);

      } while (token != "quit" && argv.Length == 0);

      cMotor.m_Threads.WaitAnalizando();
    }
  }
}
