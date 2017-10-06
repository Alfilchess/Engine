using System;
using Types;

using val = System.Int32;
using pieza = System.Int32;
using sq = System.Int32;
using mov = System.Int32;

//-----------------------------------------------------------------------------------------------------------------
namespace Motor
{
  //-----------------------------------------------------------------------------------------------------------------
  //-----------------------------------------------------------------------------------------------------------------
  public abstract class cEstadisticas<T>
  {
    public const val MAX = 2000;

    public T[][] m_Tabla = new T[cPieza.COLORES][] {
      new T[cCasilla.ESCAQUES],
      new T[cCasilla.ESCAQUES],
      new T[cCasilla.ESCAQUES],
      new T[cCasilla.ESCAQUES],
      new T[cCasilla.ESCAQUES],
      new T[cCasilla.ESCAQUES],
      new T[cCasilla.ESCAQUES],
      new T[cCasilla.ESCAQUES],
      new T[cCasilla.ESCAQUES],
      new T[cCasilla.ESCAQUES],
      new T[cCasilla.ESCAQUES],
      new T[cCasilla.ESCAQUES],
      new T[cCasilla.ESCAQUES],
      new T[cCasilla.ESCAQUES],
      new T[cCasilla.ESCAQUES],
      new T[cCasilla.ESCAQUES] };

    //-----------------------------------------------------------------------------------------------------------------
    public T[] this[pieza pc]
    {
      get
      {
        return m_Tabla[pc];
      }

      set
      {
        m_Tabla[pc] = value;
      }
    }

    //-----------------------------------------------------------------------------------------------------------------
    public void Clear()
    {
      Array.Clear(m_Tabla[0], 0, cCasilla.ESCAQUES);
      Array.Clear(m_Tabla[1], 0, cCasilla.ESCAQUES);
      Array.Clear(m_Tabla[2], 0, cCasilla.ESCAQUES);
      Array.Clear(m_Tabla[3], 0, cCasilla.ESCAQUES);
      Array.Clear(m_Tabla[4], 0, cCasilla.ESCAQUES);
      Array.Clear(m_Tabla[5], 0, cCasilla.ESCAQUES);
      Array.Clear(m_Tabla[6], 0, cCasilla.ESCAQUES);
      Array.Clear(m_Tabla[7], 0, cCasilla.ESCAQUES);
      Array.Clear(m_Tabla[8], 0, cCasilla.ESCAQUES);
      Array.Clear(m_Tabla[9], 0, cCasilla.ESCAQUES);
      Array.Clear(m_Tabla[10], 0, cCasilla.ESCAQUES);
      Array.Clear(m_Tabla[11], 0, cCasilla.ESCAQUES);
      Array.Clear(m_Tabla[12], 0, cCasilla.ESCAQUES);
      Array.Clear(m_Tabla[13], 0, cCasilla.ESCAQUES);
      Array.Clear(m_Tabla[14], 0, cCasilla.ESCAQUES);
      Array.Clear(m_Tabla[15], 0, cCasilla.ESCAQUES);

    }

    //-----------------------------------------------------------------------------------------------------------------
    public abstract void Fill(pieza pc, sq to, val v);
  }

  //-----------------------------------------------------------------------------------------------------------------
  //-----------------------------------------------------------------------------------------------------------------
  public class cBeneficios : cEstadisticas<val>
  {
    public override void Fill(pieza pc, sq to, val v)
    {
      m_Tabla[pc][to] = Math.Max(v, m_Tabla[pc][to] - 1);
    }
  }

  //-----------------------------------------------------------------------------------------------------------------
  public class cHistorial : cEstadisticas<val>
  {
    public override void Fill(pieza pc, sq to, val v)
    {
      if (Math.Abs(m_Tabla[pc][to] + v) < MAX)
        m_Tabla[pc][to] += v;
    }
  }

  //-----------------------------------------------------------------------------------------------------------------
  public struct Pair
  {
    public mov m_Key1;
    public mov m_Key2;
  }

  //-----------------------------------------------------------------------------------------------------------------
  //-----------------------------------------------------------------------------------------------------------------
  public class cMovs : cEstadisticas<Pair>
  {
    public override void Fill(pieza pc, sq to, mov m)
    {
      if (m == m_Tabla[pc][to].m_Key1)
        return;

      m_Tabla[pc][to].m_Key2 = m_Tabla[pc][to].m_Key1;
      m_Tabla[pc][to].m_Key1 = m;
    }
  }

  //-----------------------------------------------------------------------------------------------------------------
  public struct cEstado
  {
    public const int MAIN_SEARCH = 0, CAPTURES_S1 = 1, KILLERS_S1 = 2, QUIETS_1_S1 = 3, QUIETS_2_S1 = 4, BAD_CAPTURES_S1 = 5;
    public const int EVASION = 6, EVASIONS_S2 = 7;
    public const int QSEARCH_0 = 8, CAPTURES_S3 = 9, QUIET_CHECKS_S3 = 10;
    public const int QSEARCH_1 = 11, CAPTURES_S4 = 12;
    public const int PROBCUT = 13, CAPTURES_S5 = 14;
    public const int RECAPTURE = 15, CAPTURES_S6 = 16;
    public const int STOP = 17;
  };
}
