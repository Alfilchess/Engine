using System;
using System.Collections.Generic;
using System.Text;
using InOut;
using Types;
using Finales;

using mov = System.Int32;
using val = System.Int32;

namespace Motor
{
  //-----------------------------------------------------------------------------------
  public struct cTipoNodo
  {
    public const int RAIZ = 0, PV = 1, NO_PV = 2;
  };

  //-----------------------------------------------------------------------------------
  //-----------------------------------------------------------------------------------
  public class cRaizMov
  {
    public val m_nVal;
    public val m_LastVal;
    //public double m_percLevel;

    public List<mov> m_PV = new List<mov>();
    public cRaizMov(mov m)
    {
      m_nVal = m_LastVal = -cValoresJuego.INFINITO;
      m_PV.Add(m);
      m_PV.Add(cMovType.MOV_NAN);
      //m_percLevel = 0.0f;
    }

    //-----------------------------------------------------------------------------------
    public static void Ordenar(List<cRaizMov> data, int firstMove, int lastMove)
    {
      data.Sort(
          delegate (cRaizMov p1, cRaizMov p2)
          {
            return p2.m_nVal.CompareTo(p1.m_nVal);
          }
      );
    }

    //-----------------------------------------------------------------------------------
    /*public static void OrdenarPerc(List<cRaizMov> data, int firstMove, int lastMove)
    {
      data.Sort(
          delegate (cRaizMov p1, cRaizMov p2)
          {
            return p2.m_percLevel.CompareTo(p1.m_percLevel);
          }
      );
    }*/

    /*
        public static void Ordenar(List<cRaizMov> data, int firstMove, int lastMove)
        {
          cRaizMov tmp;
          int p, q;

          for (p = firstMove + 1; p < lastMove; p++)
          {
            tmp = data[p];
            for (q = p; q != firstMove && data[q - 1].m_nVal < tmp.m_nVal; --q)
              data[q] = data[q - 1];
            data[q] = tmp;
          }
        }

            //------------------------------------------------------------------------------------------------
            public static void Ordenar(List<cRaizMov> data, int left, int right)
            {
              int i = left - 1,
              j = right;

              while (true)
              {
                int d = data[left].m_nVal;
                do i++; while (data[i].m_nVal > d);
                do j--; while (data[j].m_nVal < d);

                if (i < j) 
                {
                  cRaizMov tmp = data[i];
                  data[i] = data[j];
                  data[j] = tmp;
                }
                else
                {
                  if (left < j)    Ordenar(data, left, j);
                  if (++j < right) Ordenar(data, j, right);
                  return;
                }
              }
            }

        */


    //-----------------------------------------------------------------------------------
    public void ExtraerPVFromHash(cPosicion pos)
    {
      List<cPosInfo> lstEstados = new List<cPosInfo>();

      //for (int i = 0; i < cSearch.MAX_PLY_PLUS_6; i++)
      //  lstEstados[i] = new cPosInfo();

      cTablaHashItem itemHash;
      int nEstado = 0;
      int nPly = 1;
      mov m = m_PV[0];
      val nValorEsperado = m_nVal;
      m_PV.Clear();

      bool bContinue = false;
      do
      {
        lstEstados.Add(new cPosInfo());
        m_PV.Add(m);
        pos.DoMov(m_PV[nPly++ - 1], lstEstados[nEstado++]);

        itemHash = cMotor.m_TablaHash.Buscar(pos.ClaveHash());
        nValorEsperado = -nValorEsperado;

        bContinue = itemHash != null && nValorEsperado == cSearch.ValorFromHash(itemHash.GetValue(), nPly)
          && pos.IsMovPseudoLegal(m = itemHash.GetMove())
          && pos.IsLegalMov(m, pos.PiezasClavadas(pos.ColorMueve()))
          && nPly < cSearch.MAX_PLY
          && (!pos.IsTablas() || nPly <= 2);
      } while (bContinue);

      m_PV.Add(cMovType.MOV_NAN);

      while (--nPly != 0)
        pos.DesMov(m_PV[nPly - 1]);
    }

    //-----------------------------------------------------------------------------------
    public void InsertarPVToHash(cPosicion pos)
    {
      List<cPosInfo> lstEstados = new List<cPosInfo>();
      int nEstado = 0;
      int idx = 0;

      do
      {
        cTablaHashItem tte;
        lstEstados.Add(new cPosInfo());
        tte = cMotor.m_TablaHash.Buscar(pos.ClaveHash());
        if (tte == null || tte.GetMove() != m_PV[idx])
          cMotor.m_TablaHash.Save(pos.ClaveHash(), cValoresJuego.NAN, cBordes.BOUND_NONE, cPly.DEPTH_NONE, m_PV[idx], cValoresJuego.NAN);
        pos.DoMov(m_PV[idx++], lstEstados[nEstado++]);
      } while (m_PV[idx] != cMovType.MOV_NAN);

      while (idx != 0)
        pos.DesMov(m_PV[--idx]);
    }

    //--------------------------------------------------------------------------------------------------
    public static string PrintPV(cPosicion pos, int depth, val alpha, val beta)
    {
      StringBuilder s = new StringBuilder();
      long elaspsed = cReloj.Now() - cSearch.SearchTime + 1;
      //int uciPVSize = Math.Min(cMotor.m_mapConfig["MultiPV"].Get(), cSearch.RootMoves.Count);
      int uciPVSize = Math.Min(cSearch.MultiPV, cSearch.RootMoves.Count);
      int selDepth = 0;
      for (int i = 0; i < cMotor.m_Threads.Count; ++i)
        if (cMotor.m_Threads[i].m_nMaxPly > selDepth)
          selDepth = cMotor.m_Threads[i].m_nMaxPly;

      for (int i = 0; i < uciPVSize; ++i)
      {
        bool updated = (i <= cSearch.nIndexPV);

        if (depth == 1 && !updated)
          continue;
        int d = (updated ? depth : depth - 1);

        if (cSearch.RootMoves[i].m_PV.Count > 0 && cSearch.RootMoves[i].m_PV[0] != cMovType.MOV_NAN && 0 < d)
        {
          val v = (updated ? cSearch.RootMoves[i].m_nVal : cSearch.RootMoves[i].m_LastVal);

          if (s.Length != 0)
            s.Append(cTypes.LF);

          /*bool bShow = true;
          for (int j = 0; j < d; ++j)
          {
            if (cSearch.RootMoves[i].m_PV.Count > j && cSearch.RootMoves[i].m_PV[j] == cMovType.MOV_NAN)
              bShow = false;
          }*/
          if (/*bShow == true &&*/ cSearch.RootMoves[i].m_PV[0] != cMovType.MOV_NAN)
          {
            s.Append("info depth ");
            s.Append(d);
            s.Append(" score ");
            s.Append((i == cSearch.nIndexPV ? cUci.Puntos(v, alpha, beta) : cUci.Puntos(v, -cValoresJuego.INFINITO, cValoresJuego.INFINITO)));
            s.Append(" nodes ");
            s.Append(pos.GetNodos());
            s.Append(" nps ");
            s.Append((pos.GetNodos() * 1000 / (UInt64)elaspsed).ToString());
            s.Append(" time ");
            s.Append(elaspsed);
#if CHESSARIA
#else
            s.Append(" cpuload ");
            s.Append(cCpu.m_CPU.Get() * 10);
#endif
            s.Append(" hashfull ");
            s.Append(cMotor.m_TablaHash.HashFullPercent());
            s.Append(" tbhits ");
            s.Append(cFinalesBase.m_nTBHits);

            if (i > 0 && uciPVSize > 1)
            {
              s.Append(" multipv ");
              s.Append(i + 1);
            }
            //else
              s.Append(" pv");

            for (int j = 0; cSearch.RootMoves[i].m_PV.Count > j && cSearch.RootMoves[i].m_PV[j] != cMovType.MOV_NAN && j < d; ++j)
            {
              s.Append(" ");
              s.Append(cUci.GetMovimiento(cSearch.RootMoves[i].m_PV[j], pos.IsChess960() != false));
            }
          }
        }
      }
      return s.ToString();
    }
  }
}