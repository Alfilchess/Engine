using System;
using Types;

using hash = System.UInt64;
using Phase = System.Int32;
using pnt = System.Int32;
using val = System.Int32;
using factor = System.Int32;
using color = System.Int32;

//-- http://home.comcast.net/~danheisman/Articles/evaluation_of_material_imbalance.htm
namespace Motor
{
  //----------------------------------------------------------------------------------
  //----------------------------------------------------------------------------------
  public class cMaterial
  {
    //----------------------------------------------------------------------------------
    public class cTablaHashMaterial : cHashTable<cMaterial.cStructMaterial>
    {
      public cTablaHashMaterial() : base(8192)
      {
        for (int i = 0; i < this.Size; i++)
          m_TablaHash.Add(new cMaterial.cStructMaterial());
      }
    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------
    public class cStructMaterial
    {
      public hash m_Clave;
      public Int16 m_nValor;
      public byte[] factor = new byte[2];
      public cFinalesBase m_FuncionEvalucion;
      public cFinalesBase[] m_FuncionEscala = new cFinalesBase[2];
      public int m_nPesos;
      public Phase m_nFaseDeJuego;

      public pnt Valor()
      {
        return cTypes.Puntua(m_nValor, m_nValor);
      }
      public int Peso()
      {
        return m_nPesos;
      }
      public Phase Fase()
      {
        return m_nFaseDeJuego;
      }
      public bool FuncionEvaluacion()
      {
        return m_FuncionEvalucion != null;
      }
      public val Eval(cPosicion pos)
      {
        return m_FuncionEvalucion.m_Ejecucion(pos);
      }

      public factor FactorDeEscala(cPosicion pos, color c)
      {
        return m_FuncionEscala[c] == null || m_FuncionEscala[c].m_Ejecucion(pos) == cFactorEscala.NAN
        ? (factor[c]) : m_FuncionEscala[c].m_Ejecucion(pos);
      }
    }

    public static int[] m_lstCoeficientes = new int[] { 1852, -162, -1122, -183, 249, -154 };

    public static int[][] m_lstCoeficienteCuadraticos = new int[][] {
      new int[]{ 0,      0,      0,      0,      0,      0 },
      new int[]{ 39,     2,      0,      0,      0,      0 },
      new int[]{ 35,     271,   -4,      0,      0,      0 },
      new int[]{ 0,      105,    4,      0,      0,      0 },
      new int[]{ -27,   -2,      46,     100, -141,      0 },
      new int[]{ -177,   25,     129,    142, -137,      0 } };

    internal static int[][] m_lstCoeficienteCuadraticosVs = new int[][] {
      new int[]{ 0,     0,      0,     0,      0,      0 },
      new int[]{ 37,    0,      0,     0,      0,      0 },
      new int[]{ 10,   62,      0,     0,      0,      0 },
      new int[]{ 57,   64,     39,     0,      0,      0 },
      new int[]{ 50,   40,     23,   -22,      0,      0 },
      new int[]{ 98,  105,    -39,   141,    274,      0 } };

    public static cEndgame[] m_EvalKXK = new cEndgame[2] { new cEndgame(cColor.BLANCO, stFinalesTipo.KXK), new cEndgame(cColor.NEGRO, stFinalesTipo.KXK) };
    public static cEndgame[] m_EvalKBPsK = new cEndgame[2] { new cEndgame(cColor.BLANCO, stFinalesTipo.KBPsK), new cEndgame(cColor.NEGRO, stFinalesTipo.KBPsK) };
    public static cEndgame[] m_EvalKQKRPs = new cEndgame[2] { new cEndgame(cColor.BLANCO, stFinalesTipo.KQKRPs), new cEndgame(cColor.NEGRO, stFinalesTipo.KQKRPs) };
    public static cEndgame[] m_EvalKPsK = new cEndgame[2] { new cEndgame(cColor.BLANCO, stFinalesTipo.KPsK), new cEndgame(cColor.NEGRO, stFinalesTipo.KPsK) };
    public static cEndgame[] m_EvalKPKP = new cEndgame[2] { new cEndgame(cColor.BLANCO, stFinalesTipo.KPKP), new cEndgame(cColor.NEGRO, stFinalesTipo.KPKP) };

    //----------------------------------------------------------------------------------
    public static bool IsKXK(cPosicion pos, color colr)
    {
      color colorVS = (colr == cColor.BLANCO ? cColor.NEGRO : cColor.BLANCO);
      return pos.GetNum(colorVS, cPieza.PEON) == 0
      && pos.MaterialPieza(colorVS) == cValoresJuego.CERO
      && pos.MaterialPieza(colr) >= cValoresJuego.TORRE_MJ;
    }

    //----------------------------------------------------------------------------------
    public static bool IsKBPsKs(cPosicion pos, color colr)
    {
      return pos.MaterialPieza(colr) == cValoresJuego.ALFIL_MJ
      && pos.GetNum(colr, cPieza.ALFIL) == 1
      && pos.GetNum(colr, cPieza.PEON) >= 1;
    }

    //----------------------------------------------------------------------------------
    public static bool IsKQKRPs(cPosicion pos, color colr)
    {
      color colorVS = (colr == cColor.BLANCO ? cColor.NEGRO : cColor.BLANCO);
      return pos.GetNum(colr, cPieza.PEON) == 0
      && pos.MaterialPieza(colr) == cValoresJuego.DAMA_MJ
      && pos.GetNum(colr, cPieza.DAMA) == 1
      && pos.GetNum(colorVS, cPieza.TORRE) == 1
      && pos.GetNum(colorVS, cPieza.PEON) >= 1;
    }


    //----------------------------------------------------------------------------------
    public static int Balancear(int[][] pieceCount, color colr)
    {
      color colorVs = (colr == cColor.BLANCO ? cColor.NEGRO : cColor.BLANCO);

      int pt1, pt2, pc, v;
      int nRet = 0;

      for (pt1 = cPieza.NAN; pt1 <= cPieza.DAMA; ++pt1)
      {
        pc = pieceCount[colr][pt1];
        if (pc == 0)
          continue;

        v = m_lstCoeficientes[pt1];

        for (pt2 = cPieza.NAN; pt2 <= pt1; ++pt2)
          v += m_lstCoeficienteCuadraticos[pt1][pt2] * pieceCount[colr][pt2]
          + m_lstCoeficienteCuadraticosVs[pt1][pt2] * pieceCount[colorVs][pt2];

        nRet += pc * v;
      }
      return nRet;
    }

    //----------------------------------------------------------------------------------
    public static cMaterial.cStructMaterial Buscar(cPosicion pos, cMaterial.cTablaHashMaterial entradaMaterial, cFinales finales)
    {
      hash clave = pos.ClaveMaterial();
      cMaterial.cStructMaterial e = entradaMaterial[clave];


      if (e.m_Clave == clave)
        return e;

      e.m_nValor = 0;
      e.m_FuncionEvalucion = null;
      e.m_FuncionEscala[0] = e.m_FuncionEscala[1] = null;
      e.m_nPesos = 0;
      e.m_Clave = clave;
      e.factor[cColor.BLANCO] = e.factor[cColor.NEGRO] = (byte)cFactorEscala.NORMAL;
      e.m_nFaseDeJuego = FaseJuego(pos);


      if (finales.probeValueFunction(clave, out e.m_FuncionEvalucion) != null)
        return e;

      if (IsKXK(pos, cColor.BLANCO))
      {
        e.m_FuncionEvalucion = m_EvalKXK[cColor.BLANCO];
        return e;
      }

      if (IsKXK(pos, cColor.NEGRO))
      {
        e.m_FuncionEvalucion = m_EvalKXK[cColor.NEGRO];
        return e;
      }

      cFinalesBase sf;
      if (finales.probeScaleFunction(clave, out sf) != null)
      {
        e.m_FuncionEscala[sf.Ganador()] = sf;
        return e;
      }

      if (IsKBPsKs(pos, cColor.BLANCO))
        e.m_FuncionEscala[cColor.BLANCO] = m_EvalKBPsK[cColor.BLANCO];

      if (IsKBPsKs(pos, cColor.NEGRO))
        e.m_FuncionEscala[cColor.NEGRO] = m_EvalKBPsK[cColor.NEGRO];

      if (IsKQKRPs(pos, cColor.BLANCO))
        e.m_FuncionEscala[cColor.BLANCO] = m_EvalKQKRPs[cColor.BLANCO];

      else if (IsKQKRPs(pos, cColor.NEGRO))
        e.m_FuncionEscala[cColor.NEGRO] = m_EvalKQKRPs[cColor.NEGRO];

      val npm_w = pos.MaterialPieza(cColor.BLANCO);
      val npm_b = pos.MaterialPieza(cColor.NEGRO);

      if (npm_w + npm_b == cValoresJuego.CERO && pos.GetNumPiezas(cPieza.PEON) != 0)
      {
        if (0 == pos.GetNum(cColor.NEGRO, cPieza.PEON))
        {

          e.m_FuncionEscala[cColor.BLANCO] = m_EvalKPsK[cColor.BLANCO];
        }
        else if (0 == pos.GetNum(cColor.BLANCO, cPieza.PEON))
        {

          e.m_FuncionEscala[cColor.NEGRO] = m_EvalKPsK[cColor.NEGRO];
        }
        else if (pos.GetNum(cColor.BLANCO, cPieza.PEON) == 1 && pos.GetNum(cColor.NEGRO, cPieza.PEON) == 1)
        {

          e.m_FuncionEscala[cColor.BLANCO] = m_EvalKPKP[cColor.BLANCO];
          e.m_FuncionEscala[cColor.NEGRO] = m_EvalKPKP[cColor.NEGRO];
        }
      }

      if (0 == pos.GetNum(cColor.BLANCO, cPieza.PEON) && npm_w - npm_b <= cValoresJuego.ALFIL_MJ)
        e.factor[cColor.BLANCO] = (byte)(npm_w < cValoresJuego.TORRE_MJ ? cFactorEscala.TABLAS : npm_b <= cValoresJuego.ALFIL_MJ ? 4 : 12);

      if (0 == pos.GetNum(cColor.NEGRO, cPieza.PEON) && npm_b - npm_w <= cValoresJuego.ALFIL_MJ)
        e.factor[cColor.NEGRO] = (byte)(npm_b < cValoresJuego.TORRE_MJ ? cFactorEscala.TABLAS : npm_w <= cValoresJuego.ALFIL_MJ ? 4 : 12);

      if (pos.GetNum(cColor.BLANCO, cPieza.PEON) == 1 && npm_w - npm_b <= cValoresJuego.ALFIL_MJ)
        e.factor[cColor.BLANCO] = (byte)cFactorEscala.PEON;

      if (pos.GetNum(cColor.NEGRO, cPieza.PEON) == 1 && npm_b - npm_w <= cValoresJuego.ALFIL_MJ)
        e.factor[cColor.NEGRO] = (byte)cFactorEscala.PEON;

      if (npm_w + npm_b >= 2 * cValoresJuego.DAMA_MJ + 4 * cValoresJuego.TORRE_MJ + 2 * cValoresJuego.CABALLO_MJ)
      {
        int minorPieceCount = pos.GetNum(cColor.BLANCO, cPieza.CABALLO) + pos.GetNum(cColor.BLANCO, cPieza.ALFIL)
        + pos.GetNum(cColor.NEGRO, cPieza.CABALLO) + pos.GetNum(cColor.NEGRO, cPieza.ALFIL);

        e.m_nPesos = cTypes.Puntua(minorPieceCount * minorPieceCount, 0);
      }

      int[][] numPiezas = new int[cColor.SIZE][]
        {
          new int[]{ pos.GetNum(cColor.BLANCO, cPieza.ALFIL) > 1 ? 1 : 0, pos.GetNum(cColor.BLANCO, cPieza.PEON), pos.GetNum(cColor.BLANCO, cPieza.CABALLO), pos.GetNum(cColor.BLANCO, cPieza.ALFIL)            , pos.GetNum(cColor.BLANCO, cPieza.TORRE), pos.GetNum(cColor.BLANCO, cPieza.DAMA)},
          new int[]{ pos.GetNum(cColor.NEGRO, cPieza.ALFIL) > 1 ? 1 : 0, pos.GetNum(cColor.NEGRO, cPieza.PEON), pos.GetNum(cColor.NEGRO, cPieza.CABALLO), pos.GetNum(cColor.NEGRO, cPieza.ALFIL)            , pos.GetNum(cColor.NEGRO, cPieza.TORRE), pos.GetNum(cColor.NEGRO, cPieza.DAMA) } 
        };

      e.m_nValor = (Int16)((Balancear(numPiezas, cColor.BLANCO) - Balancear(numPiezas, cColor.NEGRO)) / 16);
      return e;
    }

    //----------------------------------------------------------------------------------
    public static Phase FaseJuego(cPosicion pos)
    {
      val materialPiezas = pos.MaterialPieza(cColor.BLANCO) + pos.MaterialPieza(cColor.NEGRO);

      return materialPiezas >= cValoresJuego.LIMITE_MJ ? cFaseJuego.MEDIO_JUEGO
      : materialPiezas <= cValoresJuego.LIMITE_FINAL ? cFaseJuego.FINAL
      : (Phase)(((materialPiezas - cValoresJuego.LIMITE_FINAL) * 128) / (cValoresJuego.LIMITE_MJ - cValoresJuego.LIMITE_FINAL));
    }
  }
}
