using System;
using System.Collections.Generic;
using System.Text;
using InOut;
using Types;
using hash = System.UInt64;
using mov = System.Int32;
using ply = System.Int32;
using val = System.Int32;
using color = System.Int32;
using NodeType = System.Int32;
using sq = System.Int32;
using System.IO;
namespace Motor
{

  public struct NodeTypeS
  {
    public const int Root = 0, PV = 1, NonPV = 2;
  };
  public class RootMove
  {
    public val score;
    public val prevScore;
    public List<mov> pv = new List<mov>();
    public RootMove(mov m)
    {
      score = prevScore = -cValoresJuego.INFINITO;
      pv.Add(m);
      pv.Add(cMovType.MOV_NAN);
    }
    public void extract_pv_from_tt(cPosicion pos)
    {
      cPosInfo[] estate = new cPosInfo[cSearch.MAX_PLY_PLUS_6];
      for (int i = 0; i < cSearch.MAX_PLY_PLUS_6; i++)
        estate[i] = new cPosInfo();
      cTablaHashStruct tte;
      int st = 0;
      int ply = 1;
      mov m = pv[0];
      val expectedScore = score;
      pv.Clear();
      do
      {
        pv.Add(m);
        pos.DoMov(pv[ply++ - 1], estate[st++]);
        tte = cMotor.m_TablaHash.Buscar(pos.ClaveHash());
        expectedScore = -expectedScore;
      } while (tte != null
          && expectedScore == cSearch.value_from_tt(tte.GetValue(), ply)
          && pos.IsPseudoLegalMov(m = tte.GetMove())
          && pos.IsLegalMov(m, pos.pinned_pieces(pos.ColorMueve()))
          && ply < cSearch.MAX_PLY
          && (!pos.IsTablas() || ply <= 2));
      pv.Add(cMovType.MOV_NAN);
      while (--ply != 0)
        pos.DesMov(pv[ply - 1]);
    }



    public void insert_pv_in_tt(cPosicion pos)
    {
      cPosInfo[] state = new cPosInfo[cSearch.MAX_PLY_PLUS_6];
      int st = 0;
      for (int i = 0; i < cSearch.MAX_PLY_PLUS_6; i++)
        state[i] = new cPosInfo();
      cTablaHashStruct tte;
      int idx = 0;
      do
      {
        tte = cMotor.m_TablaHash.Buscar(pos.ClaveHash());
        if (tte == null || tte.GetMove() != pv[idx])
          cMotor.m_TablaHash.Save(pos.ClaveHash(), cValoresJuego.NAN, cBordes.BOUND_NONE, cPly.DEPTH_NONE, pv[idx], cValoresJuego.NAN);
        pos.DoMov(pv[idx++], state[st++]);
      } while (pv[idx] != cMovType.MOV_NAN);
      while (idx != 0)
        pos.DesMov(pv[--idx]);
    }
  }
  public class cSearch
  {
    public const int MAX_PLY = 120;
    public const int MAX_PLY_PLUS_6 = MAX_PLY + 6;
    public static cSignal Signals;
    public static cControlReloj Limits = new cControlReloj();
    public static List<RootMove> RootMoves = new List<RootMove>();
    public static cPosicion RootPos;
    public static color RootColor;
    public static Int64 SearchTime;
    public static Stack<cPosInfo> SetupStates = new Stack<cPosInfo>();

    public static int[][] FutilityMoveCounts = new int[2][] { new int[32], new int[32] };

    public static sbyte[][][][] Reductions = new sbyte[2][][][];

    public static int MultiPV, PVIdx;
    public static cReloj TimeMgr = new cReloj();
    public static double BestMoveChanges;
    public static val[] DrawValue = new val[cColor.SIZE];
    public static cHistorial History = new cHistorial();
    public static cBeneficios Gains = new cBeneficios();
    public static cMovs Countermoves = new cMovs();
    public static cMovs Followupmoves = new cMovs();
    public static cLibro book = new cLibro();

    public static val razor_margin(ply d)
    {
      return 512 + 16 * (int)d;
    }

    public static val futility_margin(ply d)
    {
      return (100 * d);
    }

    public static ply reduction(int i, ply d, int mn, int PvNode)
    {
      return (ply)Reductions[PvNode][i][Math.Min((int)d / cPly.ONE_PLY, 63)][Math.Min(mn, 63)];
    }

    public static void Init()
    {
      int d;
      int hd;
      int mc;
      for (int i = 0; i < 2; i++)
      {
        Reductions[i] = new sbyte[2][][];
        for (int k = 0; k < 2; k++)
        {
          Reductions[i][k] = new sbyte[64][];
          for (int m = 0; m < 64; m++)
          {
            Reductions[i][k][m] = new sbyte[64];
          }
        }
      }

      for (hd = 1; hd < 64; ++hd)
        for (mc = 1; mc < 64; ++mc)
        {
          double pvRed = 0.00 + Math.Log((double)hd) * Math.Log((double)mc) / 3.0;
          double nonPVRed = 0.33 + Math.Log((double)hd) * Math.Log((double)mc) / 2.25;
          Reductions[1][1][hd][mc] = (sbyte)(pvRed >= 1.0 ? Math.Floor(pvRed * (int)cPly.ONE_PLY) : 0);
          Reductions[0][1][hd][mc] = (sbyte)(nonPVRed >= 1.0 ? Math.Floor(nonPVRed * (int)cPly.ONE_PLY) : 0);
          Reductions[1][0][hd][mc] = Reductions[1][1][hd][mc];
          Reductions[0][0][hd][mc] = Reductions[0][1][hd][mc];
          if (Reductions[0][0][hd][mc] > 2 * cPly.ONE_PLY)
            Reductions[0][0][hd][mc] += cPly.ONE_PLY;
          else if (Reductions[0][0][hd][mc] > 1 * cPly.ONE_PLY)
            Reductions[0][0][hd][mc] += cPly.ONE_PLY / 2;
        }

      for (d = 0; d < 32; ++d)
      {
        FutilityMoveCounts[0][d] = (int)(2.4 + 0.222 * Math.Pow(d + 0.00, 1.8));
        FutilityMoveCounts[1][d] = (int)(3.0 + 0.300 * Math.Pow(d + 0.98, 1.8));
      }
    }


    public static UInt64 static_perft(cPosicion pos, ply depth)
    {
      cPosInfo st = new cPosInfo();
      UInt64 cnt = 0;
      cInfoJaque ci = new cInfoJaque(pos);
      bool leaf = depth == 2 * cPly.ONE_PLY;
      for (cReglas it = new cReglas(pos, cMovType.LEGAL); it.GetActualMov() != cMovType.MOV_NAN; ++it)
      {
        pos.DoMov(it.GetActualMov(), st, ci, pos.IsJaque(it.GetActualMov(), ci));
        cnt += leaf ? (UInt64)(new cReglas(pos, cMovType.LEGAL)).Size() : cSearch.static_perft(pos, depth - cPly.ONE_PLY);
        pos.DesMov(it.GetActualMov());
      }
      return cnt;
    }
    public static UInt64 perft(cPosicion pos, ply depth)
    {
      return depth > cPly.ONE_PLY ? cSearch.static_perft(pos, depth) : (UInt64)(new cReglas(pos, cMovType.LEGAL)).Size();
    }





    public static void think()
    {
      RootColor = RootPos.ColorMueve();
      TimeMgr.Init(Limits, RootPos.GetPly(), RootColor);
      int cf = cMotor.m_mapConfig["FACTOR_CONTENCION"].Get() * cValoresJuego.PEON_FINAL / 100;
      DrawValue[RootColor] = cValoresJuego.TABLAS - (cf);
      DrawValue[cTypes.Contrario(RootColor)] = cValoresJuego.TABLAS + (cf);
      if (RootMoves.Count == 0)
      {
        RootMoves.Add(new RootMove(cMovType.MOV_NAN));
        cMotor.m_Consola.Print("info depth 0 score ", AccionConsola.GET);
        cMotor.m_Consola.Print(cUci.Puntos(RootPos.Jaques() != 0 ? -cValoresJuego.MATE : cValoresJuego.TABLAS, -cValoresJuego.INFINITO, cValoresJuego.INFINITO), AccionConsola.RELEASE);
        goto finalize;
      }
      if (cMotor.m_mapConfig.ContainsKey("OwnBook") && cMotor.m_mapConfig["OwnBook"].Get() != 0 && 0 == Limits.infinite && 0 == Limits.mate)
      {
        mov bookMove = book.Buscar(RootPos);
        if (bookMove != 0 && existRootMove(RootMoves, bookMove))
        {
          int bestpos = Buscar(RootMoves, 0, RootMoves.Count, bookMove);
          RootMove temp = RootMoves[0];
          RootMoves[0] = RootMoves[bestpos];
          RootMoves[bestpos] = temp;
          goto finalize;
        }
      }
      cMotor.m_TablaHash.m_nHashFull = 0;

      for (int i = 0; i < cMotor.m_Threads.Count; ++i)
        cMotor.m_Threads[i].m_nMaxPly = 0;
      cMotor.m_Threads.m_RelojThread.m_bRunning = true;
      cMotor.m_Threads.m_RelojThread.Signal();
      id_loop(RootPos);
      cMotor.m_Threads.m_RelojThread.m_bRunning = false;
    finalize:










      if (!Signals.STOP && (Limits.ponder != 0 || Limits.infinite != 0))
      {
        Signals.STOP_ON_PONDER = true;
#pragma warning disable 0420
        RootPos.ThreadActive().Wait(ref Signals.STOP);
#pragma warning restore 0420
      }

      cMotor.m_Consola.Print("bestmove ", AccionConsola.GET);
      cMotor.m_Consola.Print(cUci.GetMovimiento(RootMoves[0].pv[0], RootPos.IsChess960() != false), AccionConsola.NADA);
      cMotor.m_Consola.Print(" ponder ", AccionConsola.NADA);
      cMotor.m_Consola.Print(cUci.GetMovimiento(RootMoves[0].pv[1], RootPos.IsChess960() != false), AccionConsola.NADA);
      cMotor.m_Consola.Print(cTypes.LF, AccionConsola.RELEASE);
    }



    public static void id_loop(cPosicion pos)
    {
      cStackMov[] stack = new cStackMov[cSearch.MAX_PLY_PLUS_6];
      int ss = 2;
      ply depth;
      val bestValue, alpha, beta, delta;
      for (int i = 0; i < cSearch.MAX_PLY_PLUS_6; i++)
        stack[i] = new cStackMov();
      stack[ss - 1].currentMove = cMovType.MOV_NULO;
      depth = 0;
      BestMoveChanges = 0;
      bestValue = delta = alpha = -cValoresJuego.INFINITO;
      beta = cValoresJuego.INFINITO;
      cMotor.m_TablaHash.NewIteraccion();
      History.Clear();
      Gains.Clear();
      Countermoves.Clear();
      Followupmoves.Clear();
      MultiPV = cMotor.m_mapConfig["MultiPV"].Get();
      MultiPV = Math.Min(MultiPV, RootMoves.Count);

      while (++depth <= cSearch.MAX_PLY && !Signals.STOP && (0 == Limits.depth || depth <= Limits.depth))
      {

        BestMoveChanges *= 0.5;


        for (int i = 0; i < RootMoves.Count; ++i)
          RootMoves[i].prevScore = RootMoves[i].score;

        for (PVIdx = 0; PVIdx < MultiPV && !Signals.STOP; ++PVIdx)
        {

          if (depth >= 5)
          {
            delta = 16;
            alpha = Math.Max(RootMoves[PVIdx].prevScore - delta, -cValoresJuego.INFINITO);
            beta = Math.Min(RootMoves[PVIdx].prevScore + delta, cValoresJuego.INFINITO);
          }


          while (true)
          {
            bestValue = search(pos, stack, ss, alpha, beta, depth * cPly.ONE_PLY, false, NodeTypeS.Root, false);

            sort(RootMoves, PVIdx, RootMoves.Count);


            for (int i = 0; i <= PVIdx; ++i)
              RootMoves[i].insert_pv_in_tt(pos);



            if (Signals.STOP)
              break;


            if ((bestValue <= alpha || bestValue >= beta)
                && cReloj.Now() - SearchTime > 3000)
              cMotor.m_Consola.PrintLine(uci_pv(pos, depth, alpha, beta), AccionConsola.ATOMIC);


            if (bestValue <= alpha)
            {
              alpha = Math.Max(bestValue - delta, -cValoresJuego.INFINITO);
              Signals.FAILED = true;
              Signals.STOP_ON_PONDER = false;
            }
            else if (bestValue >= beta)
              beta = Math.Min(bestValue + delta, cValoresJuego.INFINITO);
            else
              break;
            delta += delta / 2;
          }

          sort(RootMoves, 0, PVIdx + 1);
          if (PVIdx + 1 == MultiPV || cReloj.Now() - SearchTime > 3000)
            cMotor.m_Consola.PrintLine(uci_pv(pos, depth, alpha, beta), AccionConsola.ATOMIC);
        }




        if (Limits.mate != 0
            && bestValue >= cValoresJuego.MATE_MAXIMO
            && cValoresJuego.MATE - bestValue <= 2 * Limits.mate)
          Signals.STOP = true;

        if (Limits.ControlDeTiempoDefinido() && !Signals.STOP && !Signals.STOP_ON_PONDER)
        {

          if (depth > 10 && depth < 50 && MultiPV == 1)
            TimeMgr.Inestabilidad(BestMoveChanges);


          if (RootMoves.Count == 1 || cReloj.Now() - SearchTime > TimeMgr.Disponible())
          {


            if (Limits.ponder != 0)
              Signals.STOP_ON_PONDER = true;
            else
              Signals.STOP = true;
          }
        }
      }
    }

    public static val search(cPosicion pos, cStackMov[] ss, int ssPos, val alpha, val beta, ply depth, bool cutNode, NodeType NT, bool SpNode)
    {
      bool RootNode = (NT == NodeTypeS.Root);
      bool PvNode = (NT == NodeTypeS.PV || NT == NodeTypeS.Root);
      mov[] quietsSearched = new mov[64];
      cPosInfo st = new cPosInfo();
      cTablaHashStruct tte;
      cSplitPoint splitPoint = null;
      hash posKey = 0;
      mov ttMove, move, excludedMove, bestMove;
      ply ext, newDepth, predictedDepth;
      val bestValue, value, ttValue, eval, nullValue, futilityValue;
      bool inCheck, givesCheck, pvMove, singularExtensionNode, improving;
      bool captureOrPromotion, dangerous, doFullDepthSearch;
      int moveCount = 0, quietCount = 0;

      cThread thisThread = pos.ThreadActive();
      inCheck = pos.Jaques() != 0;
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

        if (Signals.STOP || pos.IsTablas() || ss[ssPos].ply > cSearch.MAX_PLY)
          return ss[ssPos].ply > cSearch.MAX_PLY && !inCheck ? cEval.Eval(pos) : DrawValue[pos.ColorMueve()];






        alpha = Math.Max(cTypes.MateEnVs(ss[ssPos].ply), alpha);
        beta = Math.Min(cTypes.MateEn(ss[ssPos].ply + 1), beta);
        if (alpha >= beta)
          return alpha;
      }



      excludedMove = ss[ssPos].excludedMove;
      posKey = excludedMove != 0 ? pos.GetClaveHashExclusion() : pos.ClaveHash();
      tte = cMotor.m_TablaHash.Buscar(posKey);
      ss[ssPos].ttMove = ttMove = RootNode ? RootMoves[PVIdx].pv[0] : tte != null ? tte.GetMove() : cMovType.MOV_NAN;
      ttValue = (tte != null) ? value_from_tt(tte.GetValue(), ss[ssPos].ply) : cValoresJuego.NAN;




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
          update_stats(pos, ss, ssPos, ttMove, depth, null, 0);
        return ttValue;
      }

      if (inCheck)
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
        Gains.Fill(pos.GetPieza(to), to, -ss[ssPos - 1].staticEval - ss[ssPos].staticEval);
      }

      if (!PvNode
          && depth < 4 * cPly.ONE_PLY
          && eval + razor_margin(depth) <= beta
          && ttMove == cMovType.MOV_NAN
          && Math.Abs(beta) < cValoresJuego.MATE_MAXIMO
          && !pos.IsPeonOn7(pos.ColorMueve())
          )
      {
        if (depth <= cPly.ONE_PLY
            && eval + razor_margin(3 * cPly.ONE_PLY) <= alpha)
          return qsearch(pos, ss, ssPos, alpha, beta, cPly.DEPTH_ZERO, NodeTypeS.NonPV, false);
        val ralpha = alpha - razor_margin(depth);
        val v = qsearch(pos, ss, ssPos, ralpha, ralpha + 1, cPly.DEPTH_ZERO, NodeTypeS.NonPV, false);
        if (v <= ralpha)
          return v;
      }

      if (!PvNode
          && 0 == ss[ssPos].skipNullMove
          && depth < 7 * cPly.ONE_PLY
          && eval - futility_margin(depth) >= beta
          && Math.Abs(beta) < cValoresJuego.MATE_MAXIMO
          && Math.Abs(eval) < cValoresJuego.GANA
          && pos.MaterialPieza(pos.ColorMueve()) != 0)
        return eval - futility_margin(depth);

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
        nullValue = depth - R < cPly.ONE_PLY ? -qsearch(pos, ss, ssPos + 1, -beta, -beta + 1, cPly.DEPTH_ZERO, NodeTypeS.NonPV, false)
                              : -search(pos, ss, ssPos + 1, -beta, -beta + 1, depth - R, !cutNode, NodeTypeS.NonPV, false);
        ss[ssPos + 1].skipNullMove = 0;
        pos.undo_null_move();
        if (nullValue >= beta)
        {

          if (nullValue >= cValoresJuego.MATE_MAXIMO)
            nullValue = beta;
          if (depth < 12 * cPly.ONE_PLY)
            return nullValue;

          ss[ssPos].skipNullMove = 1;
          val v = depth - R < cPly.ONE_PLY ? qsearch(pos, ss, ssPos, beta - 1, beta, cPly.DEPTH_ZERO, NodeTypeS.NonPV, false)
                              : search(pos, ss, ssPos, beta - 1, beta, depth - R, false, NodeTypeS.NonPV, false);
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
        cMovOrder mp2 = new cMovOrder(pos, ttMove, History, pos.GetTipoPiezaCapturada());
        cInfoJaque ci2 = new cInfoJaque(pos);
        while ((move = mp2.SiguienteMovimientoEnFalso()) != cMovType.MOV_NAN)
          if (pos.IsLegalMov(move, ci2.m_Clavadas))
          {
            ss[ssPos].currentMove = move;
            pos.DoMov(move, st, ci2, pos.IsJaque(move, ci2));
            value = -search(pos, ss, ssPos + 1, -rbeta, -rbeta + 1, rdepth, !cutNode, NodeTypeS.NonPV, false);
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
        search(pos, ss, ssPos, alpha, beta, d, true, PvNode ? NodeTypeS.PV : NodeTypeS.NonPV, false);
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
      cMovOrder mp = new cMovOrder(pos, ttMove, depth, History, countermoves, followupmoves, ss[ssPos]);
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



        if (RootNode && (Buscar(RootMoves, PVIdx, RootMoves.Count, move) == -1))
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
          Signals.FIRST_MOVE = (moveCount == 1);

        }
        ext = cPly.DEPTH_ZERO;
        captureOrPromotion = pos.IsCapturaOrPromocion(move);
        givesCheck = cTypes.TipoMovimiento(move) == cMovType.NORMAL && 0 == ci.m_Candidatas
          ? (ci.m_Jaque[cTypes.TipoPieza(pos.GetPieza(cTypes.GetFromCasilla(move)))] & cBitBoard.m_nCasillas[cTypes.GetToCasilla(move)]) != 0
          : pos.IsJaque(move, ci);
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
          value = search(pos, ss, ssPos, rBeta - 1, rBeta, depth / 2, cutNode, NodeTypeS.NonPV, false);
          ss[ssPos].skipNullMove = 0;
          ss[ssPos].excludedMove = cMovType.MOV_NAN;
          if (value < rBeta)
            ext = cPly.ONE_PLY;
        }

        newDepth = depth - cPly.ONE_PLY + ext;

        if (!PvNode
            && !captureOrPromotion
            && !inCheck
            && !dangerous
          /* &&  move != ttMove Already implicit in the next condition */
            && bestValue > cValoresJuego.MATE_MAXIMO_VS)
        {

          if (depth < 16 * cPly.ONE_PLY
              && moveCount >= FutilityMoveCounts[improving ? 1 : 0][depth])
          {
            if (SpNode)
              splitPoint.mutex.Bloquear();
            continue;
          }
          predictedDepth = newDepth - reduction(improving ? 1 : 0, depth, moveCount, PvNode ? 1 : 0);

          if (predictedDepth < 7 * cPly.ONE_PLY)
          {
            futilityValue = ss[ssPos].staticEval + futility_margin(predictedDepth)
                        + 128 + Gains[pos.GetPiezaMovida(move)][cTypes.GetToCasilla(move)];
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
        if (!SpNode && !captureOrPromotion && quietCount < 64)
          quietsSearched[quietCount++] = move;

        pos.DoMov(move, st, ci, givesCheck);


        if (depth >= 3 * cPly.ONE_PLY
            && !pvMove
            && !captureOrPromotion
            && move != ttMove
            && move != ss[ssPos].killers0
            && move != ss[ssPos].killers1)
        {
          ss[ssPos].reduction = reduction(improving ? 1 : 0, depth, moveCount, PvNode ? 1 : 0);
          if (!PvNode && cutNode)
            ss[ssPos].reduction += cPly.ONE_PLY;
          else if (History[pos.GetPieza(cTypes.GetToCasilla(move))][cTypes.GetToCasilla(move)] < 0)
            ss[ssPos].reduction += cPly.ONE_PLY / 2;
          if (move == countermoves[0] || move == countermoves[1])
            ss[ssPos].reduction = Math.Max(cPly.DEPTH_ZERO, ss[ssPos].reduction - cPly.ONE_PLY);
          ply d = Math.Max(newDepth - ss[ssPos].reduction, cPly.ONE_PLY);
          if (SpNode)
            alpha = splitPoint.alpha;
          value = -search(pos, ss, ssPos + 1, -(alpha + 1), -alpha, d, true, NodeTypeS.NonPV, false);

          if (value > alpha && ss[ssPos].reduction >= 4 * cPly.ONE_PLY)
          {
            ply d2 = Math.Max(newDepth - 2 * cPly.ONE_PLY, cPly.ONE_PLY);
            value = -search(pos, ss, ssPos + 1, -(alpha + 1), -alpha, d2, true, NodeTypeS.NonPV, false);
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
                givesCheck ? -qsearch(pos, ss, ssPos + 1, -(alpha + 1), -alpha, cPly.DEPTH_ZERO, NodeTypeS.NonPV, true)
                           : -qsearch(pos, ss, ssPos + 1, -(alpha + 1), -alpha, cPly.DEPTH_ZERO, NodeTypeS.NonPV, false)
                           : -search(pos, ss, ssPos + 1, -(alpha + 1), -alpha, newDepth, !cutNode, NodeTypeS.NonPV, false);
        }



        if (PvNode && (pvMove || (value > alpha && (RootNode || value < beta))))
          value = newDepth < cPly.ONE_PLY ?
                        givesCheck ? -qsearch(pos, ss, ssPos + 1, -beta, -alpha, cPly.DEPTH_ZERO, NodeTypeS.PV, true)
                           : -qsearch(pos, ss, ssPos + 1, -beta, -alpha, cPly.DEPTH_ZERO, NodeTypeS.PV, false)
                           : -search(pos, ss, ssPos + 1, -beta, -alpha, newDepth, false, NodeTypeS.PV, false);

        pos.DesMov(move);

        if (SpNode)
        {
          splitPoint.mutex.Bloquear();
          bestValue = splitPoint.bestValue;
          alpha = splitPoint.alpha;
        }



        if (Signals.STOP || thisThread.IsCorte())
          return cValoresJuego.CERO;
        if (RootNode)
        {
          int rmPos = Buscar(RootMoves, 0, RootMoves.Count, move);
          RootMove rm = RootMoves[rmPos];

          if (pvMove || value > alpha)
          {
            rm.score = value;
            rm.extract_pv_from_tt(pos);



            if (!pvMove)
              ++BestMoveChanges;
          }
          else



            rm.score = -cValoresJuego.INFINITO;
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
          if (Signals.STOP || thisThread.IsCorte())
            return cValoresJuego.CERO;
          if (bestValue >= beta)
            break;
        }
      }
      if (SpNode)
        return bestValue;





      if (0 == moveCount)
        bestValue = excludedMove != 0 ? alpha
           : inCheck ? cTypes.MateEnVs(ss[ssPos].ply) : DrawValue[pos.ColorMueve()];

      else if (bestValue >= beta && !pos.IsCapturaOrPromocion(bestMove) && !inCheck)
        update_stats(pos, ss, ssPos, bestMove, depth, quietsSearched, quietCount - 1);
      cMotor.m_TablaHash.Save(posKey, value_to_tt(bestValue, ss[ssPos].ply),
           bestValue >= beta ? cBordes.BOUND_LOWER :
           PvNode && bestMove != 0 ? cBordes.BOUND_EXACT : cBordes.BOUND_UPPER,
           depth, bestMove, ss[ssPos].staticEval);
      return bestValue;
    }



    public static val qsearch(cPosicion pos, cStackMov[] ss, int ssPos, val alpha, val beta, ply depth, NodeType NT, bool InCheck)
    {
      bool PvNode = (NT == NodeTypeS.PV);
      cPosInfo st = null;
      cTablaHashStruct tte;
      hash posKey;
      mov ttMove, move, bestMove;
      val bestValue, value, ttValue, futilityValue, futilityBase, oldAlpha = 0;
      bool givesCheck, evasionPrunable;
      ply ttDepth;

      if (PvNode)
        oldAlpha = alpha;
      ss[ssPos].currentMove = bestMove = cMovType.MOV_NAN;
      ss[ssPos].ply = ss[ssPos - 1].ply + 1;

      if (pos.IsTablas() || ss[ssPos].ply > cSearch.MAX_PLY)
        return ss[ssPos].ply > cSearch.MAX_PLY && !InCheck ? cEval.Eval(pos) : DrawValue[pos.ColorMueve()];



      ttDepth = ((InCheck || depth >= cPly.DEPTH_QS_CHECKS) ? cPly.DEPTH_QS_CHECKS
                                                              : cPly.DEPTH_QS_NO_CHECKS);

      posKey = pos.ClaveHash();
      tte = cMotor.m_TablaHash.Buscar(posKey);
      ttMove = (tte != null ? tte.GetMove() : cMovType.MOV_NAN);
      ttValue = tte != null ? value_from_tt(tte.GetValue(), ss[ssPos].ply) : cValoresJuego.NAN;
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




      cMovOrder mp = new cMovOrder(pos, ttMove, depth, History, cTypes.GetToCasilla(ss[ssPos - 1].currentMove));
      cInfoJaque ci = new cInfoJaque(pos);
      st = new cPosInfo();

      while ((move = mp.SiguienteMovimientoEnFalso()) != cMovType.MOV_NAN)
      {
        givesCheck = cTypes.TipoMovimiento(move) == cMovType.NORMAL && 0 == ci.m_Candidatas
          ? (ci.m_Jaque[cTypes.TipoPieza(pos.GetPieza(cTypes.GetFromCasilla(move)))] & cBitBoard.m_nCasillas[cTypes.GetToCasilla(move)]) != 0
          : pos.IsJaque(move, ci);

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
        value = givesCheck ? -qsearch(pos, ss, ssPos + 1, -beta, -alpha, depth - cPly.ONE_PLY, NT, true)
                            : -qsearch(pos, ss, ssPos + 1, -beta, -alpha, depth - cPly.ONE_PLY, NT, false);
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



    public static val value_from_tt(val v, int ply)
    {
      return v == cValoresJuego.NAN ? cValoresJuego.NAN
            : v >= cValoresJuego.MATE_MAXIMO ? v - ply
            : v <= cValoresJuego.MATE_MAXIMO_VS ? v + ply : v;
    }


    public static void update_stats(cPosicion pos, cStackMov[] ss, int ssPos, mov move, ply depth, mov[] quiets, int quietsCnt)
    {
      if (ss[ssPos].killers0 != move)
      {
        ss[ssPos].killers1 = ss[ssPos].killers0;
        ss[ssPos].killers0 = move;
      }


      val bonus = ((depth) * (depth));
      History.Fill(pos.GetPiezaMovida(move), cTypes.GetToCasilla(move), bonus);
      for (int i = 0; i < quietsCnt; ++i)
      {
        mov m = quiets[i];
        History.Fill(pos.GetPiezaMovida(m), cTypes.GetToCasilla(m), -bonus);
      }
      if (cTypes.is_ok_move(ss[ssPos - 1].currentMove))
      {
        sq prevMoveSq = cTypes.GetToCasilla(ss[ssPos - 1].currentMove);
        Countermoves.Fill(pos.GetPieza(prevMoveSq), prevMoveSq, move);
      }
      if (cTypes.is_ok_move(ss[ssPos - 2].currentMove) && ss[ssPos - 1].currentMove == ss[ssPos - 1].ttMove)
      {
        sq prevOwnMoveSq = cTypes.GetToCasilla(ss[ssPos - 2].currentMove);
        Followupmoves.Fill(pos.GetPieza(prevOwnMoveSq), prevOwnMoveSq, move);
      }
    }



    public static string uci_pv(cPosicion pos, int depth, val alpha, val beta)
    {
      StringBuilder s = new StringBuilder();
      long elaspsed = cReloj.Now() - SearchTime + 1;
      int uciPVSize = Math.Min(cMotor.m_mapConfig["MultiPV"].Get(), RootMoves.Count);
      int selDepth = 0;
      for (int i = 0; i < cMotor.m_Threads.Count; ++i)
        if (cMotor.m_Threads[i].m_nMaxPly > selDepth)
          selDepth = cMotor.m_Threads[i].m_nMaxPly;
      for (int i = 0; i < uciPVSize; ++i)
      {
        bool updated = (i <= PVIdx);
        if (depth == 1 && !updated)
          continue;
        int d = (updated ? depth : depth - 1);
        if (RootMoves[i].pv.Count > 0 && RootMoves[i].pv[0] != cMovType.MOV_NAN && 0 < d)
        {
          val v = (updated ? RootMoves[i].score : RootMoves[i].prevScore);
          if (s.Length != 0)
            s.Append(cTypes.LF);
          bool bShow = true;
          for (int j = 0; j < d; ++j)
          {
            if (RootMoves[i].pv.Count > j && RootMoves[i].pv[j] == cMovType.MOV_NAN)
              bShow = false;
          }
          if (RootMoves[i].pv[0] != cMovType.MOV_NAN)
          {
            s.Append("info depth ");
            s.Append(d);


            s.Append(" score ");
            s.Append((i == PVIdx ? cUci.Puntos(v, alpha, beta) : cUci.Puntos(v, -cValoresJuego.INFINITO, cValoresJuego.INFINITO)));
            s.Append(" nodes ");
            s.Append(pos.GetNodos());
            s.Append(" nps ");
            s.Append((pos.GetNodos() * 1000 / (UInt64)elaspsed).ToString());
            s.Append(" time ");
            s.Append(elaspsed);
            s.Append(" cpuload ");
            s.Append(Program.m_CPU.Get() * 10);
            s.Append(" hashfull ");
            s.Append(cMotor.m_TablaHash.HashFullPercent());
            if (bShow)
            {
              if (i > 0)
              {
                s.Append(" multipv ");
                s.Append(i + 1);
              }
              s.Append(" pv");

              for (int j = 0; RootMoves[i].pv.Count > j && RootMoves[i].pv[j] != cMovType.MOV_NAN && j < d; ++j)
              {
                s.Append(" ");
                s.Append(cUci.GetMovimiento(RootMoves[i].pv[j], pos.IsChess960() != false));
              }
            }
          }
        }
      }
      return s.ToString();
    }

    public static int Buscar(List<RootMove> RootMoves, int firstPos, int lastPos, mov moveToFind)
    {
      for (int i = firstPos; i < lastPos; i++)
      {
        if (RootMoves[i].pv[0] == moveToFind)
          return i;
      }
      return -1;
    }
    public static bool existRootMove(List<RootMove> moves, mov m)
    {
      int moveLength = moves.Count;
      if (moveLength == 0)
        return false;
      for (int i = 0; i < moveLength; i++)
      {
        if (moves[i].pv[0] == m)
          return true;
      }
      return false;
    }
    public static void sort(List<RootMove> data, int firstMove, int lastMove)
    {
      RootMove tmp;
      int p, q;
      for (p = firstMove + 1; p < lastMove; p++)
      {
        tmp = data[p];
        for (q = p; q != firstMove && data[q - 1].score < tmp.score; --q)
          data[q] = data[q - 1];
        data[q] = tmp;
      }
    }
  }
  public partial class cThread
  {

    public override void OnIdle()
    {


      cSplitPoint this_sp = m_nSplitPointSize != 0 ? m_SplitPointActivo : null;
      while (true)
      {


        while (!m_bBuscando || exit)
        {
          if (exit)
          {
            return;
          }

          mutex.Bloquear();

          if (this_sp != null && this_sp.slavesMask.none())
          {
            mutex.Liberar();
            break;
          }




          if (!m_bBuscando && !exit)
            sleepCondition.Wait(mutex);
          mutex.Liberar();
        }

        if (m_bBuscando)
        {
          cMotor.m_Threads.mutex.Bloquear();
          cSplitPoint sp = m_SplitPointActivo;
          cMotor.m_Threads.mutex.Liberar();
          cStackMov[] stack = new cStackMov[cSearch.MAX_PLY_PLUS_6];
          int ss = 2;
          for (int i = 0; i < cSearch.MAX_PLY_PLUS_6; i++)
            stack[i] = new cStackMov();
          cPosicion pos = new cPosicion(sp.pos, this);
          for (int i = sp.ssPos - 2, n = 0; n < 5; n++, i++)
            stack[ss - 2 + n].From(sp.ss[i]);
          stack[ss].splitPoint = sp;
          sp.mutex.Bloquear();
          m_posActiva = pos;
          if (sp.nodeType == NodeTypeS.NonPV)
            cSearch.search(pos, stack, ss, sp.alpha, sp.beta, sp.depth, sp.cutNode, NodeTypeS.NonPV, true);
          else if (sp.nodeType == NodeTypeS.PV)
            cSearch.search(pos, stack, ss, sp.alpha, sp.beta, sp.depth, sp.cutNode, NodeTypeS.PV, true);
          else if (sp.nodeType == NodeTypeS.Root)
            cSearch.search(pos, stack, ss, sp.alpha, sp.beta, sp.depth, sp.cutNode, NodeTypeS.Root, true);
          m_bBuscando = false;
          m_posActiva = null;
          sp.slavesMask[idx] = false;
          sp.allSlavesSearching = false;
          sp.nodes += (UInt32)pos.GetNodos();


          if (this != sp.masterThread
              && sp.slavesMask.none())
          {
            sp.masterThread.Signal();
          }



          sp.mutex.Liberar();
          if (cMotor.m_Threads.Count > 2)
            for (int i = 0; i < cMotor.m_Threads.Count; ++i)
            {
              int size = cMotor.m_Threads[i].m_nSplitPointSize;
              sp = size != 0 ? cMotor.m_Threads[i].m_SplitPoints[size - 1] : null;
              if (sp != null
                  && sp.allSlavesSearching
                  && Disponible(cMotor.m_Threads[i]))
              {

                cMotor.m_Threads.mutex.Bloquear();
                sp.mutex.Bloquear();
                if (sp.allSlavesSearching
                    && Disponible(cMotor.m_Threads[i]))
                {
                  sp.slavesMask[idx] = true;
                  m_SplitPointActivo = sp;
                  m_bBuscando = true;
                }
                sp.mutex.Liberar();
                cMotor.m_Threads.mutex.Liberar();
                break;
              }
            }
        }


        if (this_sp != null && this_sp.slavesMask.none())
        {
          this_sp.mutex.Bloquear();
          bool finished = this_sp.slavesMask.none();
          this_sp.mutex.Liberar();
          if (finished)
            return;
        }
      }
    }
    public virtual void Bucle()
    {
      OnIdle();
    }
  }
}
