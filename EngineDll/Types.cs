using System;
using System.Runtime.InteropServices;
using Motor;

using pnt = System.Int32;
using val = System.Int32;
using mov = System.Int32;
using color = System.Int32;
using pos = System.Int32;
using enroque = System.Int32;
using colum = System.Int32;
using fila = System.Int32;
using pieza = System.Int32;
using type = System.Int32;
using ply = System.Int32;

namespace Types
{
  //---------------------------------------------------------------------------------------
  public class cTypes
  {
    public static string LF = System.Environment.NewLine;

    //---------------------------------------------------------------------------------------
    public static pnt Puntua(int nValorMedioJuego, int nValorFinalJuego)
    {
      cPuntos v;
      v.m_nTotal = 0;
      v.m_nMedioJuego = (Int16)(nValorMedioJuego - ((UInt16)(nValorFinalJuego) >> 15));
      v.m_nFinal = (Int16)nValorFinalJuego;
      return (pnt)(v.m_nTotal);
    }

    //---------------------------------------------------------------------------------------
    public static val GetMedioJuego(pnt s)
    {
      cPuntos v;
      v.m_nFinal = v.m_nMedioJuego = 0;
      v.m_nTotal = (UInt32)s;
      return (val)(v.m_nMedioJuego + ((UInt16)(v.m_nFinal) >> 15));
    }

    //---------------------------------------------------------------------------------------
    public static val GetFinalJuego(pnt s)
    {
      cPuntos v;
      v.m_nFinal = 0;
      v.m_nTotal = (UInt32)s;
      return (val)(v.m_nFinal);
    }

    //---------------------------------------------------------------------------------------
    public static pnt divScore(pnt s, int i)
    {
      return Puntua(GetMedioJuego(s) / i, GetFinalJuego(s) / i);
    }

    public static bool minThan(ref cMov f, ref cMov s)
    {
      return f.v < s.v;
    }

    //---------------------------------------------------------------------------------------
    public static color Contrario(color c)
    {
      return c ^ cColor.NEGRO;
    }

    //---------------------------------------------------------------------------------------
    public static pos CasillaVS(pos s)
    {
      return s ^ cCasilla.A8;
    }

    //---------------------------------------------------------------------------------------
    public static pos GetCasillaEnroque(color c, enroque s)
    {
      return (cEnroque.OO_BLANCAS << (((s == cEnroque.LADO_DAMA) ? 1 : 0) + 2 * c));
    }

    //---------------------------------------------------------------------------------------
    public static val MateEn(int ply)
    {
      return cValoresJuego.MATE - ply;
    }

    //---------------------------------------------------------------------------------------
    public static val MateEnVs(int ply)
    {
      return (val)(-cValoresJuego.MATE + ply);
    }

    //---------------------------------------------------------------------------------------
    public static pos CreaCasilla(colum f, fila r)
    {
      return ((r << 3) | f);
    }

    //---------------------------------------------------------------------------------------
    public static pieza CreaPieza(color c, pieza pt)
    {
      return (pieza)((c << 3) | pt);
    }

    //---------------------------------------------------------------------------------------
    public static pieza TipoPieza(pieza pc)
    {
      return (pieza)(pc & 7);
    }

    //---------------------------------------------------------------------------------------
    public static color GetColor(pieza pc)
    {

      return (color)(pc >> 3);
    }

    //---------------------------------------------------------------------------------------
    public static bool IsCasillaOcupable(pos s)
    {
      return s >= cCasilla.A1 && s <= cCasilla.H8;
    }

    //---------------------------------------------------------------------------------------
    public static colum Columna(pos s)
    {
      return (colum)(s & 7);
    }

    //---------------------------------------------------------------------------------------
    public static fila Fila(pos s)
    {
      return (fila)(s >> 3);
    }

    //---------------------------------------------------------------------------------------
    public static pos CasillaProxima(color c, pos s)
    {
      return (pos)(s ^ (c * 56));
    }

    //---------------------------------------------------------------------------------------
    public static fila FilaProximaFromFila(color c, fila r)
    {
      return (fila)(r ^ (c * 7));
    }

    //---------------------------------------------------------------------------------------
    public static fila FilaProxima(color c, pos s)
    {
      return FilaProximaFromFila(c, Fila(s));
    }

    //---------------------------------------------------------------------------------------
    public static bool IsColorContrario(pos s1, pos s2)
    {
      

      int s = (int)s1 ^ (int)s2;
      return (((s >> 3) ^ s) & 1) != 0;
    }

    //---------------------------------------------------------------------------------------
    public static char GetCharColumna(colum f)
    {
      bool tolower = true;
      return (char)(f - COLUMNA.A + (int)(tolower ? ('a') : ('A')));
    }

    //---------------------------------------------------------------------------------------
    public static char GetCharFila(fila r)
    {
      return (char)(r - FILA.F1 + (int)('1'));
    }

    //---------------------------------------------------------------------------------------
    public static pos AtaquePeon(color c)
    {
      return c == cColor.BLANCO ? cCasilla.NORTE : cCasilla.SUR;
    }

    //---------------------------------------------------------------------------------------
    public static pos GetFromCasilla(mov m)
    {
      return ((m >> 6) & 0x3F);
    }

    //---------------------------------------------------------------------------------------
    public static pos GetToCasilla(mov m)
    {
      return (m & 0x3F);
    }

    //---------------------------------------------------------------------------------------
    public static type TipoMovimiento(mov m)
    {
      return (type)(m & (3 << 14));
    }

    public static pieza GetPromocion(mov m)
    {
      return (pieza)(((m >> 12) & 3) + 2);
    }

    //---------------------------------------------------------------------------------------
    public static mov CreaMov(pos from, pos to)
    {
        return (to | (from << 6));
    }

    //---------------------------------------------------------------------------------------
    public static mov CreaMov(pos from, pos to, type moveType, pieza pt)
    {
      return (to | (from << 6) | moveType | ((pt - cPieza.CABALLO) << 12));
    }

    //---------------------------------------------------------------------------------------
    public static bool is_ok_move(mov m)
    {
      return GetFromCasilla(m) != GetToCasilla(m);
    }
  }

  //---------------------------------------------------------------------------------------
  public struct cMovType
  {
    public const mov MOV_NAN = 0;
    public const mov MOV_NULO = 65;

    public const mov NORMAL = 0;
    public const mov PROMOCION = 1 << 14;
    public const mov ENPASO = 2 << 14;
    public const mov ENROQUE = 3 << 14;

    public const int CAPTURES = 0;
    public const int QUIETS = 1;
    public const int QUIET_CHECKS = 2;
    public const int EVASIONS = 3;
    public const int NON_EVASIONS = 4;
    public const int LEGAL = 5;
  };

  //---------------------------------------------------------------------------------------
  public struct cColor
  {
    public const color BLANCO = 0, NEGRO = 1, NAN = 2, SIZE = 2;
  };

  //---------------------------------------------------------------------------------------
  public struct cEnroque
  {
    public enroque m_Tipo;
    public cEnroque(color c, enroque l)
    {
      m_Tipo = c == cColor.BLANCO ? l == cEnroque.LADO_DAMA ? cEnroque.OOO_BLANCAS : cEnroque.OO_BLANCAS
        : l == cEnroque.LADO_DAMA ? cEnroque.OOO_NEGRAS : cEnroque.OO_NEGRAS;
    }
    public const int LADO_REY = 0;
    public const int LADO_DAMA = 1;
    public const int ENROQUE_IZQUIERDA = 2;

    public const int NO_ENROQUE = 0;
    public const int OO_BLANCAS = 1;
    public const int OOO_BLANCAS = OO_BLANCAS << 1;
    public const int OO_NEGRAS = OO_BLANCAS << 2;
    public const int OOO_NEGRAS = OO_BLANCAS << 3;
    public const int CUALQUIER_ENROQUE = OO_BLANCAS | OOO_BLANCAS | OO_NEGRAS | OOO_NEGRAS;
    public const int ENROQUE_DERECHA = 16;
  }

  //---------------------------------------------------------------------------------------
  public struct cFaseJuego
  {
    public const int FINAL = 0;
    public const int MEDIO_JUEGO = 128;
    public const int FASE_MEDIOJUEGO = 0, FASE_FINAL = 1, NAN = 2;
  };

  //---------------------------------------------------------------------------------------
  public struct cFactorEscala
  {
    public const int TABLAS = 0;
    public const int PEON = 48;
    public const int NORMAL = 64;
    public const int MAXIMO = 128;
    public const int NAN = 255;
  };

  //---------------------------------------------------------------------------------------
  public struct cBordes
  {
    public const int BOUND_NONE = 0;
    public const int BOUND_UPPER = 1;
    public const int BOUND_LOWER = 2;
    public const int BOUND_EXACT = BOUND_UPPER | BOUND_LOWER;
  };

  //---------------------------------------------------------------------------------------
  public struct cValoresJuego
  {
    public const val CERO = 0;
    public const val TABLAS = 0;
    public const val GANA = 10000;
    public const val MATE = 32000;
    public const val INFINITO = 32001;
    public const val NAN = 32002;

    public const val MATE_MAXIMO = MATE - cSearch.MAX_PLY;
    public const val MATE_MAXIMO_VS = -MATE + cSearch.MAX_PLY;

    public static val PEON_MJ = cMotor.m_mapConfig["PEON_VALOR_MEDIO_JUEGO"].Get();
    public static val PEON_FINAL = cMotor.m_mapConfig["PEON_VALOR_FINAL"].Get();
    public static val CABALLO_MJ = cMotor.m_mapConfig["CABALLO_VALOR_MEDIO_JUEGO"].Get();
    public static val CABALLO_FINAL = cMotor.m_mapConfig["CABALLO_VALOR_FINAL"].Get();
    public static val ALFIL_MJ = cMotor.m_mapConfig["ALFIL_VALOR_MEDIO_JUEGO"].Get();
    public static val ALFIL_FINAL = cMotor.m_mapConfig["ALFIL_VALOR_FINAL"].Get();
    public static val TORRE_MJ = cMotor.m_mapConfig["TORRE_VALOR_MEDIO_JUEGO"].Get();
    public static val TORRE_FINAL = cMotor.m_mapConfig["TORRE_VALOR_FINAL"].Get();
    public static val DAMA_MJ = cMotor.m_mapConfig["DAMA_VALOR_MEDIO_JUEGO"].Get();
    public static val DAMA_FINAL = cMotor.m_mapConfig["DAMA_VALOR_FINAL"].Get();
    public static val LIMITE_MJ = cMotor.m_mapConfig["MEDIO_JUEGO"].Get();
    public static val LIMITE_FINAL = cMotor.m_mapConfig["FINAL"].Get();
  };

  //---------------------------------------------------------------------------------------
  public class cPieza
  {
    public const pieza NAN = 0, PEON = 1, CABALLO = 2, ALFIL = 3, TORRE = 4, DAMA = 5, REY = 6, MAXSIZE = 6;
    public const pieza SIZE = 8;
    public const pieza SIZE2 = 16;
    public const pieza PEON_BLANCO = 1, CABALLO_BLANCO = 2, ALFIL_BLANCO = 3, TORRE_BLANCO = 4, DAMA_BLANCO = 5, REY_BLANCO = 6;
    public const pieza PEON_NEGRO = 9, CABALLO_NEGRO = 10, ALFIL_NEGRO = 11, TORRE_NEGRO = 12, DAMA_NEGRO = 13, REY_NEGRO = 14;
    public const pieza COLORES = 16;
  };

  //---------------------------------------------------------------------------------------
  public struct cPly
  {
    public const ply ONE_PLY = 2;

    public const ply DEPTH_ZERO = 0 * ONE_PLY;
    public const ply DEPTH_QS_CHECKS = 0 * ONE_PLY;
    public const ply DEPTH_QS_NO_CHECKS = -1 * ONE_PLY;
    public const ply DEPTH_QS_RECAPTURES = -5 * ONE_PLY;

    public const int DEPTH_NONE = -127 * ONE_PLY;
  };

  //---------------------------------------------------------------------------------------
  public struct cCasilla
  {
    public const int A1 = 0, B1 = 1, C1 = 2, D1 = 3, E1 = 4, F1 = 5, G1 = 6, H1 = 7;
    public const int A2 = 8, B2 = 9, C2 = 10, D2 = 11, E2 = 12, F2 = 13, G2 = 14, H2 = 15;
    public const int A3 = 16, B3 = 17, C3 = 18, D3 = 19, E3 = 20, F3 = 21, G3 = 22, H3 = 23;
    public const int A4 = 24, B4 = 25, C4 = 26, D4 = 27, E4 = 28, F4 = 29, G4 = 30, H4 = 31;
    public const int A5 = 32, B5 = 33, C5 = 34, D5 = 35, E5 = 36, F5 = 37, G5 = 38, H5 = 39;
    public const int A6 = 40, B6 = 41, C6 = 42, D6 = 43, E6 = 44, F6 = 45, G6 = 46, H6 = 47;
    public const int A7 = 48, B7 = 49, C7 = 50, D7 = 51, E7 = 52, F7 = 53, G7 = 54, H7 = 55;
    public const int A8 = 56, B8 = 57, C8 = 58, D8 = 59, E8 = 60, F8 = 61, G8 = 62, H8 = 63;
    public const int NONE = 64;

    public const int ESCAQUES = 64;

    public const int NORTE = 8;
    public const int ESTE = 1;
    public const int SUR = -8;
    public const int OESTE = -1;

    public const int NORTE_NORTE = NORTE + NORTE;
    public const int NORTE_ESTE = NORTE + ESTE;
    public const int SUR_ESTE = SUR + ESTE;
    public const int SUR_SUR = SUR + SUR;
    public const int SUR_OESTE = SUR + OESTE;
    public const int NORTE_OESTE = NORTE + OESTE;
  };

  //---------------------------------------------------------------------------------------
  public struct COLUMNA
  {
    public const int A = 0, B = 1, C = 2, D = 3, E = 4, F = 5, G = 6, H = 7, OUT = 8;
  };

  //---------------------------------------------------------------------------------------
  public struct FILA
  {
    public const int F1 = 0, F2 = 1, F3 = 2, F4 = 3, F5 = 4, F6 = 5, F7 = 6, F8 = 7, OUT = 8;
  };

  //---------------------------------------------------------------------------------------
  [StructLayout(LayoutKind.Explicit, Size = 4)]
  public struct cPuntos
  {
    [FieldOffset(0)]
    public UInt32 m_nTotal;
    [FieldOffset(0)]
    public Int16 m_nFinal;
    [FieldOffset(2)]
    public Int16 m_nMedioJuego;

    //public const int CERO = 0;
  }
}
