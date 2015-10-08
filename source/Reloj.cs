using System;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Motor;
using Types;

using TipoTiempo = System.Int32;
using color = System.Int32;
using mov = System.Int32;

namespace InOut
{
  //----------------------------------------------------------------------------------------
  public class cReloj
  {
    //----------------------------------------------------------------------------------------
    public struct TipoReloj
    {
      public const int OPTIMO = 0;
      public const int MAXIMO = 1;
    };

    private int m_nTiempoDisponible;
    private int m_nTiempoMaximo;
    private double m_nFactorInestabilidad;
 
    //----------------------------------------------------------------------------------------
    public static Int64 Now()
    {
      return DateTime.Now.Ticks/TimeSpan.TicksPerMillisecond;
    }
    
    //----------------------------------------------------------------------------------------
    private static int TiempoRestante(int nTiempo, int nMoves, int nPly, int nLento, TipoTiempo nTipo)
    {
      double TMaxRatio = (nTipo == TipoReloj.OPTIMO ? 1 : 7.0);
      double TStealRatio = (nTipo == TipoReloj.OPTIMO ? 0 : 0.33);
      
      double thisMoveImportance = (Math.Pow((1 + Math.Exp((nPly - 60) / 9)), -0.2) + Double.Epsilon) * nLento / 100;
      double otherMovesImportance = 0;

      for (int i = 1; i < nMoves; ++i)
        otherMovesImportance += (Math.Pow((1 + Math.Exp(((nPly + 2 * i) - 60) / 9)), -0.2) + Double.Epsilon);

      double ratio1 = (TMaxRatio * thisMoveImportance) / (TMaxRatio * thisMoveImportance + otherMovesImportance);
      double ratio2 = (thisMoveImportance + TStealRatio * otherMovesImportance) / (thisMoveImportance + otherMovesImportance);

      return (int)(nTiempo * Math.Min(ratio1, ratio2));
    }

    //----------------------------------------------------------------------------------------
    public void Inestabilidad(double nCambios)
    {
      m_nFactorInestabilidad = 1 + nCambios;
    }

    //----------------------------------------------------------------------------------------
    public int Disponible() { return (int)(m_nTiempoDisponible * m_nFactorInestabilidad * 0.2); }

    //----------------------------------------------------------------------------------------
    public int Maximo() { return m_nTiempoMaximo; }

    //----------------------------------------------------------------------------------------
    public void Init(cControlReloj control, int nPly, color nColor)
    {
      int nCalculoDeHorizonte  = cMotor.m_mapConfig["CONTROL_DE_HORIZONTE"].Get();
      int nEmergencia          = cMotor.m_mapConfig["CONTROL_DE_EMERGENCIA"].Get();
      int nTiempoExtra         = cMotor.m_mapConfig["TIEMPO_EXTRA"].Get();
      int nTiempoMinimo        = cMotor.m_mapConfig["TIEMPO_MINIMO"].Get();
      int nJuegoLento          = cMotor.m_mapConfig["TIEMPO_LENTO"].Get();

      m_nFactorInestabilidad = 1;
      m_nTiempoDisponible = m_nTiempoMaximo = Math.Max(control.time[nColor], nTiempoMinimo);

      for (int i = 1; i <= (control.movestogo != 0 ? Math.Min(control.movestogo, 50) : 50); ++i)
      {
        int nTiempo = control.time[nColor] + control.inc[nColor] * (i - 1) - nEmergencia - nTiempoExtra * Math.Min(i, nCalculoDeHorizonte);
        nTiempo = Math.Max(nTiempo, 0);

        int t1 = nTiempoMinimo + TiempoRestante(nTiempo, i, nPly, nJuegoLento, TipoReloj.OPTIMO);
        int t2 = nTiempoMinimo + TiempoRestante(nTiempo, i, nPly, nJuegoLento, TipoReloj.MAXIMO);

        m_nTiempoDisponible = Math.Min(m_nTiempoDisponible, t1);
        m_nTiempoMaximo = Math.Min(m_nTiempoMaximo, t2);
      }

      if (cMotor.m_mapConfig["Ponder"].Get() != 0)
        m_nTiempoDisponible += m_nTiempoDisponible / 4;

      m_nTiempoDisponible = Math.Min(m_nTiempoDisponible, m_nTiempoMaximo);
    }
  }

  //----------------------------------------------------------------------------------------
  //----------------------------------------------------------------------------------------
  public class cRelojThread : cThreadBase
  {
    public bool m_bRunning = false;

    private const int TIME_WAIT_RELOJ = 100;

    private static long m_nLastTime = -1;

    //----------------------------------------------------------------------------------------
    public cRelojThread() : base() { }
    //----------------------------------------------------------------------------------------
    public void Check()
    {
      if (m_nLastTime < 0)
        m_nLastTime = cReloj.Now();

      Int64 nNodos = 0;
      if (cReloj.Now() - m_nLastTime >= 100)
        m_nLastTime = cReloj.Now();

      if (cSearch.Limits.ponder == 0)
      {
        if (cSearch.Limits.nodes != 0)
        {
          cMotor.m_Threads.mutex.Bloquear();

          nNodos = (Int64)cSearch.RootPos.GetNodos();

          for (int i = 0; i < cMotor.m_Threads.Count; ++i)
          {
            for (int j = 0; j < cMotor.m_Threads[i].m_nSplitPointSize; ++j)
            {
              cSplitPoint sp = cMotor.m_Threads[i].m_SplitPoints[j];

              sp.mutex.Bloquear();

              nNodos += sp.nodes;

              for (int i2 = 0; i2 < cMotor.m_Threads.Count; ++i2)
                if (sp.slavesMask[i2] && cMotor.m_Threads[i2].m_posActiva != null)
                  nNodos += (Int64)cMotor.m_Threads[i2].m_posActiva.GetNodos();

              sp.mutex.Liberar();
            }
          }

          cMotor.m_Threads.mutex.Liberar();
        }
        
        Int64 nTiempoTranscurrido = cReloj.Now() - cSearch.SearchTime;
        bool stillAtFirstMove = cSearch.Signals.FIRST_MOVE && !cSearch.Signals.FAILED && nTiempoTranscurrido > cSearch.TimeMgr.Disponible() * 75 / 100;

        bool noMoreTime = (nTiempoTranscurrido > cSearch.TimeMgr.Maximo() - (2 * cRelojThread.TIME_WAIT_RELOJ)) || stillAtFirstMove;

        if ((cSearch.Limits.ControlDeTiempoDefinido() && noMoreTime) || (cSearch.Limits.movetime != 0 && nTiempoTranscurrido >= cSearch.Limits.movetime)
          || (cSearch.Limits.nodes != 0 && nNodos >= cSearch.Limits.nodes))
          cSearch.Signals.STOP = true;
      }
    }

    //----------------------------------------------------------------------------------------
    public override void OnIdle()
    {
      while (!exit)
      {
        mutex.Bloquear();

        if(!exit)
        {
          sleepCondition.Wait(mutex, m_bRunning ? TIME_WAIT_RELOJ : Int32.MaxValue);
        }

        mutex.Liberar();

        if (m_bRunning)
          Check();
      }
    }
  }

  //-------------------------------------------------------------------------------------------------
  //-------------------------------------------------------------------------------------------------
  public class cControlReloj
  {
    public List<mov> searchmoves = new List<mov>();
    public int[] time = new int[cColor.SIZE], inc = new int[cColor.SIZE];
    public int movestogo, depth, nodes, movetime, mate, infinite, ponder;

    //-------------------------------------------------------------------------------------------------
    public bool ControlDeTiempoDefinido()
    {
      return 0 == (mate | movetime | depth | nodes | infinite);
    }
  };

}
