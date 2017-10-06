using System;
using System.Collections.Generic;
using InOut;
using Types;
using hash = System.UInt64;
using mov = System.Int32;
using ply = System.Int32;
using val = System.Int32;
using color = System.Int32;
using NodeType = System.Int32;
using sq = System.Int32;
using pnt = System.Int32;
using Book;
using System.Diagnostics;

namespace Motor
{
  public static class cSearch
  {
    public static float m_nFilterK = -.2f, m_nFilterC = .1f;
    public const int MAX_PLY = 60;//120
    public const int MAX_PLY_PLUS_6 = MAX_PLY + 6;
    public static cSignal m_Sennales;
    public static cControlReloj m_Limites = new cControlReloj();
    public static List<cRaizMov> RootMoves = new List<cRaizMov>();
    public static cPosicion RootPos;
    public static color RootColor;
    public static Int64 SearchTime;
    public static Stack<cPosInfo> m_PilaEstados = new Stack<cPosInfo>();

    public static int[][] FutilityMoveCounts = new int[2][] { new int[32], new int[32] };

    public static sbyte[][][][] m_Reducciones = new sbyte[2][][][];

    public static int MultiPV, nIndexPV;
    public static cReloj m_ControlReloj = new cReloj();
    public static double BestMoveChanges;
    public static val[] m_ValorTablas = new val[cColor.SIZE];
    public static cHistorial m_Historial = new cHistorial();
    public static cBeneficios m_Beneficios = new cBeneficios();
    public static cMovs Countermoves = new cMovs();
    public static cMovs Followupmoves = new cMovs();

#if BOOK
    public static cLibro book = new cLibro();
#endif

    //-----------------------------------------------------------------------------------------------------
    public static val Razor_margin(ply d)
    {
      return 512 + 16 * (int)d;
    }

    //-----------------------------------------------------------------------------------------------------
    public static val MargenFutilityPrunning(ply d)
    {
      return (cValoresJuego.PEON_MJ * d);
    }

    //-----------------------------------------------------------------------------------------------------
    public static ply Reduccion(int i, ply d, int mn, int PvNode)
    {
      return (ply)m_Reducciones[PvNode][i][Math.Min((int)d / cPly.ONE_PLY, 63)][Math.Min(mn, 63)];
    }

    //-----------------------------------------------------------------------------------------------------
    public static void Init()
    {
      int d;
      int hd;
      int mc;
      for (int i = 0; i < 2; i++)
      {
        m_Reducciones[i] = new sbyte[2][][];
        for (int k = 0; k < 2; k++)
        {
          m_Reducciones[i][k] = new sbyte[64][];
          for (int m = 0; m < 64; m++)
          {
            m_Reducciones[i][k][m] = new sbyte[64];
          }
        }
      }

      for (hd = 1; hd < 64; ++hd)
        for (mc = 1; mc < 64; ++mc)
        {
          double pvRed = 0.00 + Math.Log((double)hd) * Math.Log((double)mc) / 3.0;
          double nonPVRed = 0.33 + Math.Log((double)hd) * Math.Log((double)mc) / 2.25;
          m_Reducciones[1][1][hd][mc] = (sbyte)(pvRed >= 1.0 ? Math.Floor(pvRed * (int)cPly.ONE_PLY) : 0);
          m_Reducciones[0][1][hd][mc] = (sbyte)(nonPVRed >= 1.0 ? Math.Floor(nonPVRed * (int)cPly.ONE_PLY) : 0);
          m_Reducciones[1][0][hd][mc] = m_Reducciones[1][1][hd][mc];
          m_Reducciones[0][0][hd][mc] = m_Reducciones[0][1][hd][mc];
          if (m_Reducciones[0][0][hd][mc] > 2 * cPly.ONE_PLY)
            m_Reducciones[0][0][hd][mc] += cPly.ONE_PLY;
          else if (m_Reducciones[0][0][hd][mc] > 1 * cPly.ONE_PLY)
            m_Reducciones[0][0][hd][mc] += cPly.ONE_PLY / 2;
        }

      for (d = 0; d < 32; ++d)
      {
        FutilityMoveCounts[0][d] = (int)(2.4 + 0.222 * Math.Pow(d + 0.00, 1.8));
        FutilityMoveCounts[1][d] = (int)(3.0 + 0.300 * Math.Pow(d + 0.98, 1.8));
      }
    }

    //-----------------------------------------------------------------------------------------------------
    public static void Busqueda()
    {
      RootColor = RootPos.ColorMueve();
      m_ControlReloj.Init(m_Limites, RootPos.GetPly(), RootColor);
      int cf = cMotor.m_mapConfig["FACTOR_CONTENCION"].Get() * cValoresJuego.PEON_FINAL / 100;

      m_ValorTablas[RootColor] = cValoresJuego.TABLAS - (cf);
      m_ValorTablas[cTypes.Contrario(RootColor)] = cValoresJuego.TABLAS + (cf);

      if (RootPos.m_Mission[RootColor].m_nMissionType != cMission.CHECKMATE)
        m_ValorTablas[RootColor] = -cValoresJuego.GANA - (cf);
      if (RootPos.m_Mission[cTypes.Contrario(RootColor)].m_nMissionType != cMission.CHECKMATE)
        m_ValorTablas[cTypes.Contrario(RootColor)] = -cValoresJuego.GANA + (cf);

      if (RootMoves.Count == 0)
      {
        RootMoves.Add(new cRaizMov(cMovType.MOV_NAN));
        //cMotor.m_Consola.Print("info depth 0 score ", AccionConsola.GET);
        //cMotor.m_Consola.Print(cUci.Puntos(RootPos.Jaques() != 0 ? -cValoresJuego.MATE : cValoresJuego.TABLAS, -cValoresJuego.INFINITO, cValoresJuego.INFINITO), AccionConsola.RELEASE);
      }
      else
      {
        bool bBookFound = false;
#if BOOK

        if (RootPos.m_Mission[RootColor].m_nMissionType == cMission.CHECKMATE && RootPos.HasObstacles() == false)
        {
          if (cMotor.m_mapConfig.ContainsKey("OwnBook") && cMotor.m_mapConfig["OwnBook"].Get() != 0 && 0 == m_Limites.infinite && 0 == m_Limites.mate)
          {
            mov bookMove = book.Buscar(RootPos);
            if (bookMove != 0 && existRootMove(RootMoves, bookMove))
            {
              int bestpos = BuscarMov(RootMoves, 0, RootMoves.Count, bookMove);
              cRaizMov temp = RootMoves[0];
              RootMoves[0] = RootMoves[bestpos];
              RootMoves[bestpos] = temp;
              bBookFound = true;
            }
          }
        }
#endif
        if (bBookFound == false)
        {
          cMotor.m_TablaHash.m_nHashFull = 0;

          for (int i = 0; i < cMotor.m_Threads.Count; ++i)
            cMotor.m_Threads[i].m_nMaxPly = 0;
          cMotor.m_Threads.m_RelojThread.m_bRunning = true;
          cMotor.m_Threads.m_RelojThread.Signal();
          BusquedaRecursiva(RootPos);
          cMotor.m_Threads.m_RelojThread.m_bRunning = false;
        }
      }

      if (!m_Sennales.STOP && (m_Limites.ponder != 0 || m_Limites.infinite != 0))
      {
        m_Sennales.STOP_ON_PONDER = true;
#pragma warning disable 0420
        RootPos.ThreadActive().Wait(ref m_Sennales.STOP);
#pragma warning restore 0420
      }

      cMotor.m_Consola.Print("bestmove ", AccionConsola.GET);
      cMotor.m_Consola.Print(cUci.GetMovimiento(RootMoves[0].m_PV[0], RootPos.IsChess960() != false), AccionConsola.NADA);
      cMotor.m_Consola.Print(" ponder ", AccionConsola.NADA);
      cMotor.m_Consola.Print(cUci.GetMovimiento(RootMoves[0].m_PV[1], RootPos.IsChess960() != false), AccionConsola.NADA);
      cMotor.m_Consola.Print(cTypes.LF, AccionConsola.RELEASE);
    }

    //-----------------------------------------------------------------------------------------------------
    public static mov GetMovFromLevel(int nLevel, bool bChessaria, ref pnt score)
    {
      mov bestMove = cSearch.RootMoves[0].m_PV[0];
      int nLevelRoot = 0;

      {
        double percent = ((nLevel + 1) * 100) / cConfigFile.MAX_LEVEL;
        int nMinus = 0;
        int i = 0;
        foreach (cRaizMov mov in cSearch.RootMoves)
        {

          bool updated = (i <= cSearch.nIndexPV);
          val v = (updated ? cSearch.RootMoves[i].m_nVal : cSearch.RootMoves[i].m_LastVal);
          i++;
          if (v < (-cValoresJuego.PEON_MJ * (cConfigFile.MAX_LEVEL - nLevel)))
            break;
          else
            nMinus++;
        }

        Random r = new Random((int)(cReloj.Now() % 1000));
        int rnext = r.Next(1, cConfigFile.MAX_LEVEL);

        if (rnext > nLevel)
        {
          if (nMinus > 0)
            nLevelRoot = Math.Min(nMinus - (int)Math.Round(((nMinus * (percent / 100.0))), 0), nMinus - 1);

          bestMove = cSearch.RootMoves[nLevelRoot].m_PV[0];
          //bool updated = (nLevelRoot <= cSearch.nIndexPV);
          //val v = (updated ? cSearch.RootMoves[nLevelRoot].m_nVal : cSearch.RootMoves[nLevelRoot].m_LastVal);
          val v = (cSearch.RootMoves[i].m_nVal == -cValoresJuego.INFINITO) ? cSearch.RootMoves[i].m_LastVal : cSearch.RootMoves[i].m_nVal;
          score = v;
        }

      }

      int bestpos = BuscarMov(cSearch.RootMoves, 0, cSearch.RootMoves.Count, bestMove);
      if (bestpos >= 0)
      {
        cRaizMov temp = cSearch.RootMoves[0];
        cSearch.RootMoves[0] = cSearch.RootMoves[bestpos];
        cSearch.RootMoves[bestpos] = temp;
      }

      return bestMove;
    }
    
    //-----------------------------------------------------------------------------------------------------
    public static void BusquedaRecursiva(cPosicion pos)
    {
      cPilaMov[] stack = new cPilaMov[cSearch.MAX_PLY_PLUS_6];
      int ss = 2;
      ply depth;
      val bestValue, alpha, beta, delta;

      for (int i = 0; i < cSearch.MAX_PLY_PLUS_6; i++)
        stack[i] = new cPilaMov();

      stack[ss - 1].currentMove = cMovType.MOV_NULO;
      depth = 0;
      BestMoveChanges = 0;
      bestValue = delta = alpha = -cValoresJuego.INFINITO;
      beta = cValoresJuego.INFINITO;
      cMotor.m_TablaHash.NewIteraccion();
      m_Historial.Clear();
      m_Beneficios.Clear();
      Countermoves.Clear();
      Followupmoves.Clear();

      MultiPV = Math.Min(cMotor.m_mapConfig["MultiPV"].Get(), RootMoves.Count);

      if (cMotor.m_ConfigFile.m_nNivelJuego < cConfigFile.MAX_LEVEL || (cMotor.m_mapConfig.ContainsKey("UCI_LimitStrength") && cMotor.m_mapConfig["UCI_Elo"].Get() < cConfigFile.MAX_LEVEL_ELO))
        MultiPV = RootMoves.Count;//Math.Min(cMotor.m_ConfigFile.m_nNivelJuego, RootMoves.Count);

      while (++depth <= cSearch.MAX_PLY && !m_Sennales.STOP && (0 == m_Limites.depth || depth <= m_Limites.depth))
      {

        BestMoveChanges *= 0.5;


        for (int i = 0; i < RootMoves.Count; ++i)
          RootMoves[i].m_LastVal = RootMoves[i].m_nVal;

        for (nIndexPV = 0; nIndexPV < MultiPV && !m_Sennales.STOP; ++nIndexPV)
        {

          if (depth >= 5)
          {
            delta = 16;
            alpha = Math.Max(RootMoves[nIndexPV].m_LastVal - delta, -cValoresJuego.INFINITO);
            beta = Math.Min(RootMoves[nIndexPV].m_LastVal + delta, cValoresJuego.INFINITO);
          }

          while (true)
          {
            try
            {
              bestValue = Buscar(pos, stack, ss, alpha, beta, depth * cPly.ONE_PLY, false, cTipoNodo.RAIZ, false);

              //cRaizMov.Ordenar(RootMoves, PVIdx, RootMoves.Count);
              //cRaizMov.Ordenar(RootMoves, 0, RootMoves.Count);
              cRaizMov.Ordenar(RootMoves, 0, RootMoves.Count);


              for (int i = 0; i <= nIndexPV; ++i)
                RootMoves[i].InsertarPVToHash(pos);

            }
            catch (Exception ex)
            {
              Debug.Write(ex.Message);
              cMotor.m_Consola.Print("Exception: " + ex.Message, AccionConsola.NADA);
              m_Sennales.FAILED = true;
              break;
            }

            if (m_Sennales.STOP)
              break;

            //if ((bestValue <= alpha || bestValue >= beta))
            //cMotor.m_Consola.PrintLine(cRaizMov.PrintPV(pos, depth, alpha, beta), AccionConsola.ATOMIC, cMotor.m_Consola.m_bInTest);

            if (bestValue <= alpha)
            {
              alpha = Math.Max(bestValue - delta, -cValoresJuego.INFINITO);
              m_Sennales.FAILED = true;
              m_Sennales.STOP_ON_PONDER = false;
            }
            else if (bestValue >= beta)
              beta = Math.Min(bestValue + delta, cValoresJuego.INFINITO);
            else
              break;

            delta += delta / 2;

          }

          //cRaizMov.Ordenar(RootMoves, 0, PVIdx + 1);
          //cRaizMov.Ordenar(RootMoves, 0, RootMoves.Count);

          //if ((nIndexPV + 1 == MultiPV && MultiPV > 1))
            cMotor.m_Consola.PrintLine(cRaizMov.PrintPV(pos, depth, alpha, beta), AccionConsola.ATOMIC, cMotor.m_Consola.m_bInTest);
        }

        //if (cMotor.m_ConfigFile.m_nNivelJuego < cConfigFile.MAX_LEVEL || (cMotor.m_mapConfig.ContainsKey("UCI_LimitStrength") && cMotor.m_mapConfig["UCI_Elo"].Get() < cConfigFile.MAX_LEVEL_ELO))
        //  GetMovFromLevel(cMotor.m_ConfigFile.m_nNivelJuego, cMotor.m_mapConfig.ContainsKey("LevelELO") == false, ref bestValue);

        if (m_Limites.mate != 0
          && bestValue >= cValoresJuego.MATE_MAXIMO
          && cValoresJuego.MATE - bestValue <= 2 * m_Limites.mate)
          m_Sennales.STOP = true;

        if (m_Limites.ControlDeTiempoDefinido() && !m_Sennales.STOP && !m_Sennales.STOP_ON_PONDER)
        {

          if (depth > 4 && depth < 50 && MultiPV == 1)
            m_ControlReloj.Inestabilidad(BestMoveChanges);

          if (RootMoves.Count == 1 || cReloj.Now() - SearchTime > m_ControlReloj.Disponible())
          {
            if (m_Limites.ponder != 0)
              m_Sennales.STOP_ON_PONDER = true;
            else
              m_Sennales.STOP = true;
          }
        }
      }

      //-- Get level move
      if (cMotor.m_ConfigFile.m_nNivelJuego < cConfigFile.MAX_LEVEL || (cMotor.m_mapConfig.ContainsKey("UCI_LimitStrength") && cMotor.m_mapConfig["UCI_Elo"].Get() < cConfigFile.MAX_LEVEL_ELO))
      {
        GetMovFromLevel(cMotor.m_ConfigFile.m_nNivelJuego, cMotor.m_mapConfig["LevelELO"].Get() == 0, ref bestValue);
      }

      //if (cMotor.m_mapConfig["Log"].Get() != 0)
      //{
      //	cMotor.m_Consola.Print(cTypes.LF + "Legal Moves: " + cTypes.LF, AccionConsola.NADA);
      //	foreach (cRaizMov move in cSearch.RootMoves)
      //	{
      //		string strLog = cUci.GetMovimiento(move.m_PV[0], pos.IsChess960() != false);

      //		if (Math.Abs(move.m_LastVal) != cValoresJuego.INFINITO)
      //			strLog += " (" + (move.m_LastVal * 100 / cValoresJuego.PEON_FINAL) + ")";
      //		else
      //			strLog += " (nul)";

      //		cMotor.m_Consola.Print(strLog + cTypes.LF, AccionConsola.NADA);
      //	}
      //}

    }

    //-----------------------------------------------------------------------------------------------------
    public static val Buscar(cPosicion pos, cPilaMov[] ss, int ssPos, val alpha, val beta, ply depth, bool cutNode, NodeType NT, bool SpNode)
    {
      bool RootNode = (NT == cTipoNodo.RAIZ);
      bool PvNode = (NT == cTipoNodo.PV || NT == cTipoNodo.RAIZ);
      mov[] quietsSearched = new mov[64];
      cPosInfo st = new cPosInfo();
      cTablaHashItem tte;
      cSplitPoint splitPoint = null;
      hash posKey = 0;
      mov ttMove, move, excludedMove, bestMove;
      ply ext, newDepth, predictedDepth;
      val bestValue, value, ttValue, eval, nullValue, futilityValue;
      bool inCheck, givesCheck, pvMove, singularExtensionNode, improving;
      bool bCapturaOPromocion, dangerous, doFullDepthSearch;
      int moveCount = 0, quietCount = 0;

      cThread thisThread = pos.ThreadActive();

      inCheck = pos.Jaques() != 0;

      //inCheck &= pos.m_Mission[pos.ColorMueve()].m_nMissionType == cMission.CHECKMATE;


      if (pos.EvalMission(pos.ColorMueve()) != 0)
      {
        return cTypes.MateEn(ss[ssPos].ply);
      }

      if (SpNode)
      {
        splitPoint = ss[ssPos].splitPoint;
        bestMove = splitPoint.bestMove;
        bestValue = splitPoint.bestValue;
        tte = null;
        ttMove = excludedMove = cMovType.MOV_NAN;
        ttValue = cValoresJuego.NAN;
        goto moves_loop;
      }
      moveCount = quietCount = 0;
      bestValue = -cValoresJuego.INFINITO;

      ss[ssPos].currentMove = ss[ssPos].ttMove = ss[ssPos + 1].excludedMove = bestMove = cMovType.MOV_NAN;
      ss[ssPos].ply = ss[ssPos - 1].ply + 1;
      ss[ssPos + 1].skipNullMove = 0;
      ss[ssPos + 1].reduction = cPly.DEPTH_ZERO;
      ss[ssPos + 2].killers0 = ss[ssPos + 2].killers1 = cMovType.MOV_NAN;

      if (PvNode && thisThread.m_nMaxPly < ss[ssPos].ply)
        thisThread.m_nMaxPly = ss[ssPos].ply;

      if (!RootNode)
      {

        if (m_Sennales.STOP || pos.IsTablas() || ss[ssPos].ply > cSearch.MAX_PLY)
          return ss[ssPos].ply > cSearch.MAX_PLY && !inCheck ? cEval.Eval(pos) : m_ValorTablas[pos.ColorMueve()];

        alpha = Math.Max(cTypes.MateEnVs(ss[ssPos].ply), alpha);
        beta = Math.Min(cTypes.MateEn(ss[ssPos].ply + 1), beta);

        if (alpha >= beta)
          return alpha;
      }

      excludedMove = ss[ssPos].excludedMove;
      posKey = excludedMove != 0 ? pos.GetClaveHashExclusion() : pos.ClaveHash();
      tte = cMotor.m_TablaHash.Buscar(posKey);
      ss[ssPos].ttMove = ttMove = RootNode ? RootMoves[nIndexPV].m_PV[0] : tte != null ? tte.GetMove() : cMovType.MOV_NAN;
      ttValue = (tte != null) ? ValorFromHash(tte.GetValue(), ss[ssPos].ply) : cValoresJuego.NAN;

      if (!RootNode
        && tte != null
        && tte.GetDepth() >= depth
        && ttValue != cValoresJuego.NAN
        && (PvNode ? tte.GetBound() == cBordes.BOUND_EXACT
          : ttValue >= beta ? (tte.GetBound() & cBordes.BOUND_LOWER) != 0
                    : (tte.GetBound() & cBordes.BOUND_UPPER) != 0))
      {
        ss[ssPos].currentMove = ttMove;

        if (ttValue >= beta && ttMove != 0 && !pos.IsCapturaOrPromocion(ttMove) && !inCheck)
          cPilaMov.ActualizarPila(pos, ss, ssPos, ttMove, depth, null, 0);
        return ttValue;
      }

      if (inCheck || pos.EvalMission(pos.ColorMueve()) != 0)
      {
        ss[ssPos].staticEval = eval = cValoresJuego.NAN;
        goto moves_loop;
      }
      else if (tte != null)
      {

        if ((ss[ssPos].staticEval = eval = tte.GetValueEval()) == cValoresJuego.NAN)
          eval = ss[ssPos].staticEval = cEval.Eval(pos);

        if (ttValue != cValoresJuego.NAN)
          if ((tte.GetBound() & (ttValue > eval ? cBordes.BOUND_LOWER : cBordes.BOUND_UPPER)) != 0)
            eval = ttValue;
      }
      else
      {
        eval = ss[ssPos].staticEval = cEval.Eval(pos);
        cMotor.m_TablaHash.Save(posKey, cValoresJuego.NAN, cBordes.BOUND_NONE, cPly.DEPTH_NONE, cMovType.MOV_NAN, ss[ssPos].staticEval);
      }
      if (0 == pos.GetTipoPiezaCapturada()
        && ss[ssPos].staticEval != cValoresJuego.NAN
        && ss[ssPos - 1].staticEval != cValoresJuego.NAN
        && (move = ss[ssPos - 1].currentMove) != cMovType.MOV_NULO
        && cTypes.TipoMovimiento(move) == cMovType.NORMAL)
      {
        sq to = cTypes.GetToCasilla(move);
        m_Beneficios.Fill(pos.GetPieza(to), to, -ss[ssPos - 1].staticEval - ss[ssPos].staticEval);
      }

      if (!PvNode
        && depth < 4 * cPly.ONE_PLY
        && eval + Razor_margin(depth) <= beta
        && ttMove == cMovType.MOV_NAN
        && Math.Abs(beta) < cValoresJuego.MATE_MAXIMO
        && !pos.IsPeonOn7(pos.ColorMueve())
        )
      {
        if (depth <= cPly.ONE_PLY
          && eval + Razor_margin(3 * cPly.ONE_PLY) <= alpha)
          return BuscarQ(pos, ss, ssPos, alpha, beta, cPly.DEPTH_ZERO, cTipoNodo.NO_PV, false);

        val ralpha = alpha - Razor_margin(depth);
        val v = BuscarQ(pos, ss, ssPos, ralpha, ralpha + 1, cPly.DEPTH_ZERO, cTipoNodo.NO_PV, false);
        if (v <= ralpha)
          return v;
      }

      if (!PvNode
        && 0 == ss[ssPos].skipNullMove
        && depth < 7 * cPly.ONE_PLY
        && eval - MargenFutilityPrunning(depth) >= beta
        && Math.Abs(beta) < cValoresJuego.MATE_MAXIMO
        && Math.Abs(eval) < cValoresJuego.GANA
        && pos.MaterialPieza(pos.ColorMueve()) != 0)
        return eval - MargenFutilityPrunning(depth);

      if (!PvNode
        && 0 == ss[ssPos].skipNullMove
        && depth >= 2 * cPly.ONE_PLY
        && eval >= beta
        && Math.Abs(beta) < cValoresJuego.MATE_MAXIMO
        && pos.MaterialPieza(pos.ColorMueve()) != 0)
      {
        ss[ssPos].currentMove = cMovType.MOV_NULO;

        ply R = 3 * cPly.ONE_PLY
         + depth / 4
         + (eval - beta) / cValoresJuego.PEON_MJ * cPly.ONE_PLY;
        pos.DoNullMov(st);

        ss[ssPos + 1].skipNullMove = 1;
        nullValue = depth - R < cPly.ONE_PLY ? -BuscarQ(pos, ss, ssPos + 1, -beta, -beta + 1, cPly.DEPTH_ZERO, cTipoNodo.NO_PV, false)
                    : -Buscar(pos, ss, ssPos + 1, -beta, -beta + 1, depth - R, !cutNode, cTipoNodo.NO_PV, false);
        ss[ssPos + 1].skipNullMove = 0;
        pos.undo_null_move();
        if (nullValue >= beta)
        {

          if (nullValue >= cValoresJuego.MATE_MAXIMO)
            nullValue = beta;
          if (depth < 12 * cPly.ONE_PLY)
            return nullValue;

          ss[ssPos].skipNullMove = 1;
          val v = depth - R < cPly.ONE_PLY ? BuscarQ(pos, ss, ssPos, beta - 1, beta, cPly.DEPTH_ZERO, cTipoNodo.NO_PV, false)
                    : Buscar(pos, ss, ssPos, beta - 1, beta, depth - R, false, cTipoNodo.NO_PV, false);
          ss[ssPos].skipNullMove = 0;
          if (v >= beta)
            return nullValue;
        }
      }




      if (!PvNode
        && depth >= 5 * cPly.ONE_PLY
        && 0 == ss[ssPos].skipNullMove
        && Math.Abs(beta) < cValoresJuego.MATE_MAXIMO)
      {
        val rbeta = Math.Min(beta + 200, cValoresJuego.INFINITO);
        ply rdepth = depth - 4 * cPly.ONE_PLY;
        cListaOrdenadaMov mp2 = new cListaOrdenadaMov(pos, ttMove, m_Historial, pos.GetTipoPiezaCapturada());
        cInfoJaque ci2 = new cInfoJaque(pos);
        while ((move = mp2.SiguienteMovimientoEnFalso()) != cMovType.MOV_NAN)
          if (pos.IsLegalMov(move, ci2.m_Clavadas))
          {
            ss[ssPos].currentMove = move;
            pos.DoMov(move, st, ci2, pos.IsJaque(move, ci2));
            value = -Buscar(pos, ss, ssPos + 1, -rbeta, -rbeta + 1, rdepth, !cutNode, cTipoNodo.NO_PV, false);
            pos.DesMov(move);
            if (value >= rbeta)
              return value;
          }
      }

      if (depth >= (PvNode ? 5 * cPly.ONE_PLY : 8 * cPly.ONE_PLY)
        && 0 == ttMove
        && (PvNode || (ss[ssPos].staticEval + 256 >= beta)))
      {
        ply d = depth - 2 * cPly.ONE_PLY - (PvNode ? cPly.DEPTH_ZERO : depth / 4);
        ss[ssPos].skipNullMove = 1;
        Buscar(pos, ss, ssPos, alpha, beta, d, true, PvNode ? cTipoNodo.PV : cTipoNodo.NO_PV, false);
        ss[ssPos].skipNullMove = 0;
        tte = cMotor.m_TablaHash.Buscar(posKey);
        ttMove = (tte != null) ? tte.GetMove() : cMovType.MOV_NAN;
      }
      moves_loop:
      sq prevMoveSq = cTypes.GetToCasilla(ss[ssPos - 1].currentMove);
      mov[] countermoves = { Countermoves[pos.GetPieza(prevMoveSq)][prevMoveSq].m_Key1,
                  Countermoves[pos.GetPieza(prevMoveSq)][prevMoveSq].m_Key2 };
      sq prevOwnMoveSq = cTypes.GetToCasilla(ss[ssPos - 2].currentMove);
      mov[] followupmoves = {Followupmoves[pos.GetPieza(prevOwnMoveSq)][prevOwnMoveSq].m_Key1,
                  Followupmoves[pos.GetPieza(prevOwnMoveSq)][prevOwnMoveSq].m_Key2 };
      cListaOrdenadaMov mp = new cListaOrdenadaMov(pos, ttMove, depth, m_Historial, countermoves, followupmoves, ss[ssPos]);
      cInfoJaque ci = new cInfoJaque(pos);
      value = bestValue;
      improving = ss[ssPos].staticEval >= ss[ssPos - 2].staticEval
         || ss[ssPos].staticEval == cValoresJuego.NAN
         || ss[ssPos - 2].staticEval == cValoresJuego.NAN;
      singularExtensionNode = !RootNode
                   && !SpNode
                   && depth >= 8 * cPly.ONE_PLY
                   && ttMove != cMovType.MOV_NAN
                   && 0 == excludedMove
                   && (tte.GetBound() & cBordes.BOUND_LOWER) != 0
                   && tte.GetDepth() >= depth - 3 * cPly.ONE_PLY;


      while ((move = (SpNode ? mp.SiguienteMovimiento() : mp.SiguienteMovimientoEnFalso())) != cMovType.MOV_NAN)
      {
        if (move == excludedMove)
          continue;



        if (RootNode && (BuscarMov(RootMoves, nIndexPV, RootMoves.Count, move) == -1))
          continue;
        if (SpNode)
        {

          if (!pos.IsLegalMov(move, ci.m_Clavadas))
            continue;
          moveCount = ++splitPoint.moveCount;
          splitPoint.mutex.Liberar();
        }
        else
          ++moveCount;
        if (RootNode)
        {
          m_Sennales.FIRST_MOVE = (moveCount == 1);

        }
        ext = cPly.DEPTH_ZERO;
        bCapturaOPromocion = pos.IsCapturaOrPromocion(move);
        givesCheck = (cTypes.TipoMovimiento(move) == cMovType.NORMAL && 0 == ci.m_Candidatas
          ? (ci.m_Jaque[cTypes.TipoPieza(pos.GetPieza(cTypes.GetFromCasilla(move)))] & cBitBoard.m_nCasillas[cTypes.GetToCasilla(move)]) != 0
          : pos.IsJaque(move, ci));

        //givesCheck &= pos.m_Mission[pos.ColorMueve()].m_nMissionType == cMission.CHECKMATE;

        dangerous = givesCheck
         || cTypes.TipoMovimiento(move) != cMovType.NORMAL
         || pos.IsPeonAvanzado(move);

        if (givesCheck && pos.SEEReducido(move) >= cValoresJuego.CERO)
          ext = cPly.ONE_PLY;





        if (singularExtensionNode
          && move == ttMove
          && 0 == ext
          && pos.IsLegalMov(move, ci.m_Clavadas)
          && Math.Abs(ttValue) < cValoresJuego.GANA)
        {
          val rBeta = ttValue - depth;
          ss[ssPos].excludedMove = move;
          ss[ssPos].skipNullMove = 1;
          value = Buscar(pos, ss, ssPos, rBeta - 1, rBeta, depth / 2, cutNode, cTipoNodo.NO_PV, false);
          ss[ssPos].skipNullMove = 0;
          ss[ssPos].excludedMove = cMovType.MOV_NAN;
          if (value < rBeta)
            ext = cPly.ONE_PLY;
        }

        newDepth = depth - cPly.ONE_PLY + ext;

        if (!PvNode
          && !bCapturaOPromocion
          && !inCheck
          && !dangerous
          && bestValue > cValoresJuego.MATE_MAXIMO_VS)
        {

          if (depth < 16 * cPly.ONE_PLY && moveCount >= FutilityMoveCounts[improving ? 1 : 0][depth])
          {
            if (SpNode)
              splitPoint.mutex.Bloquear();
            continue;
          }
          predictedDepth = newDepth - Reduccion(improving ? 1 : 0, depth, moveCount, PvNode ? 1 : 0);

          if (predictedDepth < 7 * cPly.ONE_PLY)
          {
            futilityValue = ss[ssPos].staticEval + MargenFutilityPrunning(predictedDepth)
                  + 128 + m_Beneficios[pos.GetPiezaMovida(move)][cTypes.GetToCasilla(move)];
            if (futilityValue <= alpha)
            {
              bestValue = Math.Max(bestValue, futilityValue);
              if (SpNode)
              {
                splitPoint.mutex.Bloquear();
                if (bestValue > splitPoint.bestValue)
                  splitPoint.bestValue = bestValue;
              }
              continue;
            }
          }

          if (predictedDepth < 4 * cPly.ONE_PLY && pos.SEEReducido(move) < cValoresJuego.CERO)
          {
            if (SpNode)
              splitPoint.mutex.Bloquear();
            continue;
          }
        }

        if (!RootNode && !SpNode && !pos.IsLegalMov(move, ci.m_Clavadas))
        {
          moveCount--;
          continue;
        }
        pvMove = PvNode && moveCount == 1;
        ss[ssPos].currentMove = move;
        if (!SpNode && !bCapturaOPromocion && quietCount < 64)
          quietsSearched[quietCount++] = move;

        pos.DoMov(move, st, ci, givesCheck);

        if (pos.EvalMission(cTypes.Contrario(pos.ColorMueve())) != 0)
        {
          //pos.DesMov(move);
          value = cTypes.MateEn(ss[ssPos].ply);
        }
        else
        {
          if (depth >= 3 * cPly.ONE_PLY
            && !pvMove
            && !bCapturaOPromocion
            && move != ttMove
            && move != ss[ssPos].killers0
            && move != ss[ssPos].killers1)
          {
            ss[ssPos].reduction = Reduccion(improving ? 1 : 0, depth, moveCount, PvNode ? 1 : 0);
            if (!PvNode && cutNode)
              ss[ssPos].reduction += cPly.ONE_PLY;
            else if (m_Historial[pos.GetPieza(cTypes.GetToCasilla(move))][cTypes.GetToCasilla(move)] < 0)
              ss[ssPos].reduction += cPly.ONE_PLY / 2;
            if (move == countermoves[0] || move == countermoves[1])
              ss[ssPos].reduction = Math.Max(cPly.DEPTH_ZERO, ss[ssPos].reduction - cPly.ONE_PLY);
            ply d = Math.Max(newDepth - ss[ssPos].reduction, cPly.ONE_PLY);
            if (SpNode)
              alpha = splitPoint.alpha;
            value = -Buscar(pos, ss, ssPos + 1, -(alpha + 1), -alpha, d, true, cTipoNodo.NO_PV, false);

            if (value > alpha && ss[ssPos].reduction >= 4 * cPly.ONE_PLY)
            {
              ply d2 = Math.Max(newDepth - 2 * cPly.ONE_PLY, cPly.ONE_PLY);
              value = -Buscar(pos, ss, ssPos + 1, -(alpha + 1), -alpha, d2, true, cTipoNodo.NO_PV, false);
            }
            doFullDepthSearch = (value > alpha && ss[ssPos].reduction != cPly.DEPTH_ZERO);
            ss[ssPos].reduction = cPly.DEPTH_ZERO;
          }
          else
            doFullDepthSearch = !pvMove;

          if (doFullDepthSearch)
          {
            if (SpNode)
              alpha = splitPoint.alpha;

            value = newDepth < cPly.ONE_PLY ?
                -BuscarQ(pos, ss, ssPos + 1, -(alpha + 1), -alpha, cPly.DEPTH_ZERO, cTipoNodo.NO_PV, givesCheck)
                : -Buscar(pos, ss, ssPos + 1, -(alpha + 1), -alpha, newDepth, !cutNode, cTipoNodo.NO_PV, false);

          }



          if (PvNode && (pvMove || (value > alpha && (RootNode || value < beta))))
            value = newDepth < cPly.ONE_PLY ?
            -BuscarQ(pos, ss, ssPos + 1, -beta, -alpha, cPly.DEPTH_ZERO, cTipoNodo.PV, givesCheck)
            : -Buscar(pos, ss, ssPos + 1, -beta, -alpha, newDepth, false, cTipoNodo.PV, false);
        }
        pos.DesMov(move);

        if (SpNode)
        {
          splitPoint.mutex.Bloquear();
          bestValue = splitPoint.bestValue;
          alpha = splitPoint.alpha;
        }



        if (m_Sennales.STOP || thisThread.IsCorte())
          return cValoresJuego.CERO;

        if (RootNode)
        {
          int rmPos = BuscarMov(RootMoves, 0, RootMoves.Count, move);
          cRaizMov rm = RootMoves[rmPos];

          if (pvMove || value > alpha)
          {
            rm.m_nVal = value;
            rm.ExtraerPVFromHash(pos);



            if (!pvMove)
              ++BestMoveChanges;
          }
          else



            rm.m_nVal = -cValoresJuego.INFINITO;
        }

        if (value > bestValue)
        {
          bestValue = SpNode ? splitPoint.bestValue = value : value;
          if (value > alpha)
          {
            bestMove = SpNode ? splitPoint.bestMove = move : move;
            if (PvNode && value < beta)
              alpha = SpNode ? splitPoint.alpha = value : value;
            else
            {
              if (SpNode)
                splitPoint.m_bCorte = true;
              break;
            }
          }
        }

        if (!SpNode
          && cMotor.m_Threads.Count >= 2
          && depth >= cMotor.m_Threads.minimumSplitDepth
          && (null == thisThread.m_SplitPointActivo
          || !thisThread.m_SplitPointActivo.allSlavesSearching)
          && thisThread.m_nSplitPointSize < cThreadBase.MAX_SPLITPOINTS_PER_THREAD)
        {
          thisThread.Split(pos, ss, ssPos, alpha, beta, ref bestValue, ref bestMove,
                         depth, moveCount, mp, NT, cutNode);
          if (m_Sennales.STOP || thisThread.IsCorte())
            return cValoresJuego.CERO;
          if (bestValue >= beta)
            break;
        }
      }

      if (SpNode)
        return bestValue;

      if (0 == moveCount)
      {
        if (excludedMove != 0)
          bestValue = alpha;
        else
        {
          if (inCheck || pos.EvalMission(cTypes.Contrario(pos.ColorMueve())) != 0)
            bestValue = cTypes.MateEnVs(ss[ssPos].ply);
          else
            bestValue = m_ValorTablas[pos.ColorMueve()];
        }
      }
      else if (bestValue >= beta && !pos.IsCapturaOrPromocion(bestMove) && !inCheck)
        cPilaMov.ActualizarPila(pos, ss, ssPos, bestMove, depth, quietsSearched, quietCount - 1);
      cMotor.m_TablaHash.Save(posKey, value_to_tt(bestValue, ss[ssPos].ply),
         bestValue >= beta ? cBordes.BOUND_LOWER :
         PvNode && bestMove != 0 ? cBordes.BOUND_EXACT : cBordes.BOUND_UPPER,
         depth, bestMove, ss[ssPos].staticEval);
      return bestValue;
    }


    public static val BuscarQ(cPosicion pos, cPilaMov[] ss, int ssPos, val alpha, val beta, ply depth, NodeType NT, bool InCheck)
    {
      bool PvNode = (NT == cTipoNodo.PV);
      cPosInfo st = null;
      cTablaHashItem tte;
      hash posKey;
      mov ttMove, move, bestMove;
      val bestValue, value, ttValue, futilityValue, futilityBase = -cValoresJuego.INFINITO, oldAlpha = 0;
      bool givesCheck, evasionPrunable;
      ply ttDepth;



      if (PvNode)
        oldAlpha = alpha;

      ss[ssPos].currentMove = bestMove = cMovType.MOV_NAN;
      ss[ssPos].ply = ss[ssPos - 1].ply + 1;

      if (pos.IsTablas() || ss[ssPos].ply > cSearch.MAX_PLY)
        return ss[ssPos].ply > cSearch.MAX_PLY && !InCheck ? cEval.Eval(pos) : m_ValorTablas[pos.ColorMueve()];

      ttDepth = ((InCheck || depth >= cPly.DEPTH_QS_CHECKS) ? cPly.DEPTH_QS_CHECKS
                                  : cPly.DEPTH_QS_NO_CHECKS);


      posKey = pos.ClaveHash();
      tte = cMotor.m_TablaHash.Buscar(posKey);
      ttMove = (tte != null ? tte.GetMove() : cMovType.MOV_NAN);
      ttValue = tte != null ? ValorFromHash(tte.GetValue(), ss[ssPos].ply) : cValoresJuego.NAN;
      if (tte != null
        && tte.GetDepth() >= ttDepth
        && ttValue != cValoresJuego.NAN
        && (PvNode ? tte.GetBound() == cBordes.BOUND_EXACT
          : (ttValue >= beta) ? (tte.GetBound() & cBordes.BOUND_LOWER) != 0
                    : (tte.GetBound() & cBordes.BOUND_UPPER) != 0))
      {
        ss[ssPos].currentMove = ttMove;
        return ttValue;
      }


      if (InCheck)
      {
        ss[ssPos].staticEval = cValoresJuego.NAN;
        bestValue = futilityBase = -cValoresJuego.INFINITO;
      }
      else
      {
        if (tte != null)
        {

          if ((ss[ssPos].staticEval = bestValue = tte.GetValueEval()) == cValoresJuego.NAN)
            ss[ssPos].staticEval = bestValue = cEval.Eval(pos);

          if (ttValue != cValoresJuego.NAN)
            if ((tte.GetBound() & (ttValue > bestValue ? cBordes.BOUND_LOWER : cBordes.BOUND_UPPER)) != 0)
              bestValue = ttValue;
        }
        else
          ss[ssPos].staticEval = bestValue = cEval.Eval(pos);

        if (bestValue >= beta)
        {
          if (tte == null)
            cMotor.m_TablaHash.Save(pos.ClaveHash(), value_to_tt(bestValue, ss[ssPos].ply), cBordes.BOUND_LOWER,
              cPly.DEPTH_NONE, cMovType.MOV_NAN, ss[ssPos].staticEval);
          return bestValue;
        }
        if (PvNode && bestValue > alpha)
          alpha = bestValue;
        futilityBase = bestValue + 128;
      }



      cListaOrdenadaMov mp = new cListaOrdenadaMov(pos, ttMove, depth, m_Historial, cTypes.GetToCasilla(ss[ssPos - 1].currentMove));
      cInfoJaque ci = new cInfoJaque(pos);
      st = new cPosInfo();

      while ((move = mp.SiguienteMovimientoEnFalso()) != cMovType.MOV_NAN)
      {
        givesCheck = cTypes.TipoMovimiento(move) == cMovType.NORMAL && 0 == ci.m_Candidatas
          ? (ci.m_Jaque[cTypes.TipoPieza(pos.GetPieza(cTypes.GetFromCasilla(move)))] & cBitBoard.m_nCasillas[cTypes.GetToCasilla(move)]) != 0
          : pos.IsJaque(move, ci);

        //givesCheck &= pos.m_Mission[pos.ColorMueve()].m_nMissionType == cMission.CHECKMATE;

        if (!PvNode
          && !InCheck
          && !givesCheck
          && move != ttMove
          && futilityBase > -cValoresJuego.GANA
          && !pos.IsPeonAvanzado(move))
        {
          futilityValue = futilityBase + cPosicion.m_nValPieza[cFaseJuego.FASE_FINAL][pos.GetPieza(cTypes.GetToCasilla(move))];
          if (futilityValue < beta)
          {
            bestValue = Math.Max(bestValue, futilityValue);
            continue;
          }
          if (futilityBase < beta && pos.SEE(move) <= cValoresJuego.CERO)
          {
            bestValue = Math.Max(bestValue, futilityBase);
            continue;
          }
        }

        evasionPrunable = InCheck
                 && bestValue > cValoresJuego.MATE_MAXIMO_VS
                 && !pos.IsCaptura(move)
                 && 0 == pos.CanEnroque(pos.ColorMueve());

        if (!PvNode
          && (!InCheck || evasionPrunable)
          && move != ttMove
          && cTypes.TipoMovimiento(move) != cMovType.PROMOCION
          && pos.SEEReducido(move) < cValoresJuego.CERO)
          continue;

        if (!pos.IsLegalMov(move, ci.m_Clavadas))
          continue;
        ss[ssPos].currentMove = move;

        pos.DoMov(move, st, ci, givesCheck);

        value = -BuscarQ(pos, ss, ssPos + 1, -beta, -alpha, depth - cPly.ONE_PLY, NT, givesCheck)
                  ;
        pos.DesMov(move);

        if (value > bestValue)
        {
          bestValue = value;
          if (value > alpha)
          {
            if (PvNode && value < beta)
            {
              alpha = value;
              bestMove = move;
            }
            else
            {
              cMotor.m_TablaHash.Save(posKey, value_to_tt(value, ss[ssPos].ply), cBordes.BOUND_LOWER,
                   ttDepth, move, ss[ssPos].staticEval);
              return value;
            }
          }
        }
      }

      if (InCheck && bestValue == -cValoresJuego.INFINITO)
        return cTypes.MateEnVs(ss[ssPos].ply);

      cMotor.m_TablaHash.Save(posKey, value_to_tt(bestValue, ss[ssPos].ply),
        PvNode && bestValue > oldAlpha ? cBordes.BOUND_EXACT : cBordes.BOUND_UPPER,
        ttDepth, bestMove, ss[ssPos].staticEval);
      return bestValue;
    }



    public static val value_to_tt(val v, int ply)
    {
      return v >= cValoresJuego.MATE_MAXIMO ? v + ply
          : v <= cValoresJuego.MATE_MAXIMO_VS ? v - ply : v;
    }



    public static val ValorFromHash(val v, int ply)
    {
      return v == cValoresJuego.NAN ? cValoresJuego.NAN
          : v >= cValoresJuego.MATE_MAXIMO ? v - ply
          : v <= cValoresJuego.MATE_MAXIMO_VS ? v + ply : v;
    }


    public static int BuscarMov(List<cRaizMov> RootMoves, int firstPos, int lastPos, mov moveToFind)
    {
      for (int i = firstPos; i < lastPos; i++)
      {
        if (RootMoves[i].m_PV[0] == moveToFind)
          return i;
      }
      return -1;
    }

#if BOOK
    public static bool existRootMove(List<cRaizMov> moves, mov m)
    {
      int moveLength = moves.Count;
      if (moveLength == 0)
        return false;
      for (int i = 0; i < moveLength; i++)
      {
        if (moves[i].m_PV[0] == m)
          return true;
      }
      return false;
    }
#endif
  }
}
