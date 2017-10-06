using Types;

using val = System.Int32;
using sq = System.Int32;
using mov = System.Int32;
using ply = System.Int32;
using type = System.Int32;
using static Motor.cReglas;

//-----------------------------------------------------------------------------------------------------------------
namespace Motor
{
  //-----------------------------------------------------------------------------------------------------------------
  //-----------------------------------------------------------------------------------------------------------------
  public class cListaOrdenadaMov
  {
    public cPosicion m_Pos;
    public cHistorial m_Historial;
    public cPilaMov m_Pila;
    public mov[] m_lstMovs;
    public mov[] m_lstFollows;
    public ply m_nPly;
    public mov m_TTMov;

    public sq recaptureSquare;
    public val captureThreshold;
    int m_nEstado;
    public int cur, end, endQuiets, endBadCaptures;
    public cMov[] m_lstMov = new cMov[cReglas.MAX_MOVES + 6];

    //-----------------------------------------------------------------------------------------------------------------
    public cListaOrdenadaMov(cPosicion p, mov ttm, ply d, cHistorial h, mov[] cm, mov[] fm, cPilaMov s)
    {
      m_Pos = p;
      m_Historial = h;
      m_nPly = d;

      cur = end = 0;
      endBadCaptures = cReglas.MAX_MOVES - 1;
      m_lstMovs = cm;
      m_lstFollows = fm;
      m_Pila = s;

      if (p.Jaques() != 0)
        m_nEstado = cEstado.EVASION;
      else
        m_nEstado = cEstado.MAIN_SEARCH;

      m_TTMov = (ttm != 0 && m_Pos.IsMovPseudoLegal(ttm) ? ttm : cMovType.MOV_NAN);
      end += ((m_TTMov != cMovType.MOV_NAN) ? 1 : 0);
    }

    //-----------------------------------------------------------------------------------------------------------------
    public cListaOrdenadaMov(cPosicion p, mov ttm, ply d, cHistorial h, sq sq)
    {
      m_Pos = p;
      m_Historial = h;
      cur = 0;
      end = 0;

      if (p.Jaques() != 0)
        m_nEstado = cEstado.EVASION;

      else if (d > cPly.DEPTH_QS_NO_CHECKS)
        m_nEstado = cEstado.QSEARCH_0;

      else if (d > cPly.DEPTH_QS_RECAPTURES)
      {
        m_nEstado = cEstado.QSEARCH_1;

        if (ttm != 0 && !m_Pos.IsCapturaOrPromocion(ttm))
          ttm = cMovType.MOV_NAN;
      }
      else
      {
        m_nEstado = cEstado.RECAPTURE;
        recaptureSquare = sq;
        ttm = cMovType.MOV_NAN;
      }

      m_TTMov = (ttm != 0 && m_Pos.IsMovPseudoLegal(ttm) ? ttm : cMovType.MOV_NAN);
      end += ((m_TTMov != cMovType.MOV_NAN) ? 1 : 0);
    }

    //-----------------------------------------------------------------------------------------------------------------
    public cListaOrdenadaMov(cPosicion p, mov ttm, cHistorial h, type pt)
    {
      m_Pos = p;
      m_Historial = h;
      cur = 0;
      end = 0;

      m_nEstado = cEstado.PROBCUT;

      captureThreshold = cPosicion.m_nValPieza[cFaseJuego.FASE_MEDIOJUEGO][pt];
      m_TTMov = (ttm != 0 && m_Pos.IsMovPseudoLegal(ttm) ? ttm : cMovType.MOV_NAN);

      if (m_TTMov != 0 && (!m_Pos.IsCaptura(m_TTMov) || m_Pos.SEE(m_TTMov) <= captureThreshold))
        m_TTMov = cMovType.MOV_NAN;

      end += ((m_TTMov != cMovType.MOV_NAN) ? 1 : 0);
    }

    //-----------------------------------------------------------------------------------------------------------------
    private static void Insert(cMov[] moves, int begin, int end)
    {
      cMov tmp;
      int p, q;

      for (p = begin + 1; p < end; ++p)
      {
        tmp = moves[p];
        for (q = p; q != begin && moves[q - 1].v < tmp.v; --q)
          moves[q] = moves[q - 1];
        moves[q] = tmp;
      }
    }

    //-----------------------------------------------------------------------------------------------------------------
    public bool IsValorPositivo(ref cMov ms) { return ms.v > 0; }

    //-----------------------------------------------------------------------------------------------------------------
    public static int GetBest(cMov[] moves, int begin, int end)
    {
      if (begin != end)
      {
        int cur = begin;
        int max = begin;
        while (++begin != end)
        {
          if (moves[max].v < moves[begin].v)
          {
            max = begin;
          }
        }

        cMov temp = moves[cur];
        moves[cur] = moves[max];
        moves[max] = temp;

        return cur;
      }

      return begin;
    }

    //-----------------------------------------------------------------------------------------------------------------
    public void Capturas()
    {
      mov m;

      for (int it = 0; it != end; ++it)
      {
        m = m_lstMov[it].m;
        m_lstMov[it].v = cPosicion.m_nValPieza[cFaseJuego.FASE_MEDIOJUEGO][m_Pos.GetPieza(cTypes.GetToCasilla(m))]
                 - cTypes.TipoPieza(m_Pos.GetPiezaMovida(m));

        if (cTypes.TipoMovimiento(m) == cMovType.ENPASO)
          m_lstMov[it].v += cPosicion.m_nValPieza[cFaseJuego.FASE_MEDIOJUEGO][cPieza.PEON];

        else if (cTypes.TipoMovimiento(m) == cMovType.PROMOCION)
          m_lstMov[it].v += cPosicion.m_nValPieza[cFaseJuego.FASE_MEDIOJUEGO][cTypes.GetPromocion(m)] - cPosicion.m_nValPieza[cFaseJuego.FASE_MEDIOJUEGO][cPieza.PEON];
      }
    }

    //-----------------------------------------------------------------------------------------------------------------
    public void score_quiets()
    {
      mov m;

      for (int it = 0; it != end; ++it)
      {
        m = m_lstMov[it].m;
        m_lstMov[it].v = m_Historial[m_Pos.GetPiezaMovida(m)][cTypes.GetToCasilla(m)];
      }
    }

    //-----------------------------------------------------------------------------------------------------------------
    public void Evasiones()
    {
      mov m;
      int see;

      for (int it = 0; it != end; ++it)
      {
        m = m_lstMov[it].m;
        if ((see = m_Pos.SEEReducido(m)) < cValoresJuego.CERO)
          m_lstMov[it].v = see - cHistorial.MAX;

        else if (m_Pos.IsCaptura(m))
          m_lstMov[it].v = cPosicion.m_nValPieza[cFaseJuego.FASE_MEDIOJUEGO][m_Pos.GetPieza(cTypes.GetToCasilla(m))]
                   - cTypes.TipoPieza(m_Pos.GetPiezaMovida(m)) + cHistorial.MAX;
        else
          m_lstMov[it].v = m_Historial[m_Pos.GetPiezaMovida(m)][cTypes.GetToCasilla(m)];
      }
    }

    //-----------------------------------------------------------------------------------------------------------------
    private static int partition(cMov[] moves, int first, int last)
    {
      for (; ; ++first)
      {
        for (; first != last && (moves[first].v > 0); ++first)
          ;
        if (first == last)
          break;

        for (; first != --last && (moves[last].v <= 0); )
          ;
        if (first == last)
          break;

        cMov temp = moves[last]; moves[last] = moves[first]; moves[first] = temp;
      }
      return first;
    }

    //-----------------------------------------------------------------------------------------------------------------
    public void SiguienteEstado()
    {
      cur = 0;

      switch (++m_nEstado)
      {
        case cEstado.CAPTURES_S1:
        case cEstado.CAPTURES_S3:
        case cEstado.CAPTURES_S4:
        case cEstado.CAPTURES_S5:
        case cEstado.CAPTURES_S6:
          end = cReglas.Generar(m_Pos, m_lstMov, 0, cMovType.CAPTURES);
          Capturas();
          return;

        case cEstado.KILLERS_S1:
          cur = cReglas.MAX_MOVES;
          end = cur + 2;

          m_lstMov[cReglas.MAX_MOVES].m = m_Pila.killers0;
          m_lstMov[cReglas.MAX_MOVES + 1].m = m_Pila.killers1;
          m_lstMov[cReglas.MAX_MOVES + 2].m = m_lstMov[cReglas.MAX_MOVES + 3].m = cMovType.MOV_NAN;
          m_lstMov[cReglas.MAX_MOVES + 4].m = m_lstMov[cReglas.MAX_MOVES + 5].m = cMovType.MOV_NAN;

          for (int i = 0; i < 2; ++i)
            if (m_lstMovs[i] != m_lstMov[cur].m
                && m_lstMovs[i] != m_lstMov[cur + 1].m)
              m_lstMov[end++].m = m_lstMovs[i];

          for (int i = 0; i < 2; ++i)
            if (m_lstFollows[i] != m_lstMov[cur].m
                && m_lstFollows[i] != m_lstMov[cur + 1].m
                && m_lstFollows[i] != m_lstMov[cur + 2].m
                && m_lstFollows[i] != m_lstMov[cur + 3].m)
              m_lstMov[end++].m = m_lstFollows[i];

          return;

        case cEstado.QUIETS_1_S1:
          endQuiets = end = cReglas.Generar(m_Pos, m_lstMov, 0, cMovType.QUIETS);
          score_quiets();
          end = partition(m_lstMov, cur, end);
          Insert(m_lstMov, cur, end);
          return;

        case cEstado.QUIETS_2_S1:
          cur = end;
          end = endQuiets;
          if (m_nPly >= 3 * cPly.ONE_PLY)
            Insert(m_lstMov, cur, end);
          return;

        case cEstado.BAD_CAPTURES_S1:
          cur = cReglas.MAX_MOVES - 1;
          end = endBadCaptures;
          return;

        case cEstado.EVASIONS_S2:
          end = cReglas.Generar(m_Pos, m_lstMov, 0, cMovType.EVASIONS);
          if (end > 1)
            Evasiones();
          return;

        case cEstado.QUIET_CHECKS_S3:
          end = cReglas.Generar(m_Pos, m_lstMov, 0, cMovType.QUIET_CHECKS);
          return;

        case cEstado.EVASION:
        case cEstado.QSEARCH_0:
        case cEstado.QSEARCH_1:
        case cEstado.PROBCUT:
        case cEstado.RECAPTURE:
          m_nEstado = cEstado.STOP;
          end = cur + 1;
          break;

        case cEstado.STOP:
          end = cur + 1;
          return;

        default:

          break;
      }
    }

    //-----------------------------------------------------------------------------------------------------------------
    public mov SiguienteMovimientoEnFalso()
    {
      mov move;

      while (true)
      {
        while (cur == end)
          SiguienteEstado();

        switch (m_nEstado)
        {
          case cEstado.MAIN_SEARCH:
          case cEstado.EVASION:
          case cEstado.QSEARCH_0:
          case cEstado.QSEARCH_1:
          case cEstado.PROBCUT:
            ++cur;
            return m_TTMov;

          case cEstado.CAPTURES_S1:
            move = m_lstMov[GetBest(m_lstMov, cur++, end)].m;
            if (move != m_TTMov)
            {
              if (m_Pos.SEEReducido(move) >= cValoresJuego.CERO)
                return move;
              m_lstMov[endBadCaptures--].m = move;
            }
            break;

          case cEstado.KILLERS_S1:
            move = m_lstMov[cur++].m;
            if (move != cMovType.MOV_NAN
                && move != m_TTMov
                && m_Pos.IsMovPseudoLegal(move)
                && !m_Pos.IsCaptura(move))
              return move;
            break;

          case cEstado.QUIETS_1_S1:
          case cEstado.QUIETS_2_S1:
            move = m_lstMov[cur++].m;
            if (move != m_TTMov
                && move != m_lstMov[cReglas.MAX_MOVES].m
                && move != m_lstMov[cReglas.MAX_MOVES + 1].m
                && move != m_lstMov[cReglas.MAX_MOVES + 2].m
                && move != m_lstMov[cReglas.MAX_MOVES + 3].m
                && move != m_lstMov[cReglas.MAX_MOVES + 4].m
                && move != m_lstMov[cReglas.MAX_MOVES + 5].m)
              return move;
            break;

          case cEstado.BAD_CAPTURES_S1:
            return m_lstMov[cur--].m;

          case cEstado.EVASIONS_S2:
          case cEstado.CAPTURES_S3:
          case cEstado.CAPTURES_S4:
            move = m_lstMov[GetBest(m_lstMov, cur++, end)].m;
            if (move != m_TTMov)
              return move;
            break;

          case cEstado.CAPTURES_S5:
            move = m_lstMov[GetBest(m_lstMov, cur++, end)].m;
            if (move != m_TTMov && m_Pos.SEE(move) > captureThreshold)
              return move;
            break;

          case cEstado.CAPTURES_S6:
            move = m_lstMov[GetBest(m_lstMov, cur++, end)].m;
            if (cTypes.GetToCasilla(move) == recaptureSquare)
              return move;
            break;

          case cEstado.QUIET_CHECKS_S3:
            move = m_lstMov[cur++].m;
            if (move != m_TTMov)
              return move;
            break;

          case cEstado.STOP:
            return cMovType.MOV_NAN;

          default:

            break;
        }
      }
    }

    //-----------------------------------------------------------------------------------------------------------------
    public mov SiguienteMovimiento()
    {
      return m_Pila.splitPoint.movePicker.SiguienteMovimientoEnFalso();
    }

  }

  //-----------------------------------------------------------------------------------------------------------------
  public class cPilaMov
  {
    public cSplitPoint splitPoint;
    public int ply;
    public mov currentMove;
    public mov ttMove;
    public mov excludedMove;
    public mov killers0;
    public mov killers1;
    public ply reduction;
    public val staticEval;
    public int skipNullMove;

    //-----------------------------------------------------------------------------------------------------------------
    public void Clear()
    {
      ply = 0;
      currentMove = 0;
      ttMove = 0;
      excludedMove = 0;
      killers0 = 0;
      killers1 = 0;
      reduction = 0;
      staticEval = 0;
      skipNullMove = 0;
    }

    //-----------------------------------------------------------------------------------------------------------------
    public void From(cPilaMov s)
    {

      ply = s.ply;
      currentMove = s.currentMove;
      ttMove = s.ttMove;
      excludedMove = s.excludedMove;
      killers0 = s.killers0;
      killers1 = s.killers1;
      reduction = s.reduction;
      staticEval = s.staticEval;
      skipNullMove = s.skipNullMove;
    }

    //-----------------------------------------------------------------------------------------------------------------
    public static void ActualizarPila(cPosicion pos, cPilaMov[] ss, int ssPos, mov move, ply depth, mov[] quiets, int quietsCnt)
    {
      if (ss[ssPos].killers0 != move)
      {
        ss[ssPos].killers1 = ss[ssPos].killers0;
        ss[ssPos].killers0 = move;
      }

      val bonus = ((depth) * (depth));
      cSearch.m_Historial.Fill(pos.GetPiezaMovida(move), cTypes.GetToCasilla(move), bonus);
      for (int i = 0; i < quietsCnt; ++i)
      {
        mov m = quiets[i];
        cSearch.m_Historial.Fill(pos.GetPiezaMovida(m), cTypes.GetToCasilla(m), -bonus);
      }
      if (cTypes.is_ok_move(ss[ssPos - 1].currentMove))
      {
        sq prevMoveSq = cTypes.GetToCasilla(ss[ssPos - 1].currentMove);
        cSearch.Countermoves.Fill(pos.GetPieza(prevMoveSq), prevMoveSq, move);
      }
      if (cTypes.is_ok_move(ss[ssPos - 2].currentMove) && ss[ssPos - 1].currentMove == ss[ssPos - 1].ttMove)
      {
        sq prevOwnMoveSq = cTypes.GetToCasilla(ss[ssPos - 2].currentMove);
        cSearch.Followupmoves.Fill(pos.GetPieza(prevOwnMoveSq), prevOwnMoveSq, move);
      }
    }
  }
}
