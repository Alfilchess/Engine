using Types;

using hash = System.UInt64;
using bitbrd = System.UInt64;
using pnt = System.Int32;
using sq = System.Int32;
using color = System.Int32;
using colum = System.Int32;
using fila = System.Int32;
using val = System.Int32;
using System;

namespace Motor
{
  //----------------------------------------------------------------------------------------
  //----------------------------------------------------------------------------------------
  public class cEstructuraDePeones
  {
    public hash m_nClave;
    public pnt m_nValor;
    public bitbrd[] m_btPeonesPasados = new bitbrd[cColor.SIZE];
    public bitbrd[] m_btPeonesACoronar = new bitbrd[cColor.SIZE];
    public bitbrd[] m_btPeonesAtaques = new bitbrd[cColor.SIZE];
    public sq[] m_CasillaRey = new sq[cColor.SIZE];
    public pnt[] m_nPuntosReySeguro = new sq[cColor.SIZE];
    public int[] m_nPuntosDistanciaPeonRey = new int[cColor.SIZE];
    public int[] m_nEnroques = new int[cColor.SIZE];
    public int[] m_nColumnasSemiAbiertas = new int[cColor.SIZE];
    public int[][] m_nCasillasPeon = new int[][] { new int[cColor.SIZE], new int[cColor.SIZE] };

    public static val[] ShelterWeakness = new val[FILA.OUT]
      {(100), (0), (27), (73), (92), (101), (101), 0 };

    public static pnt[][] StormDanger = new pnt[][] {
      new pnt[FILA.OUT] { ( 0),  (64), (128), (51), (26), 0, 0, 0 },
      new pnt[FILA.OUT] { (26),  (32), ( 96), (38), (20), 0, 0, 0 },
      new pnt[FILA.OUT] { ( 0),  ( 0), (160), (25), (13), 0, 0, 0 }};

    const pnt MaxSafetyBonus = (263);
    public static bitbrd MiddleEdges = (cBitBoard.A | cBitBoard.H) & (cBitBoard.F2 | cBitBoard.F3);

    public pnt GetValue() { return m_nValor; }
    public bitbrd GetPeonesAtaques(color c) { return m_btPeonesAtaques[c]; }
    public bitbrd GetPeonesPasados(color c) { return m_btPeonesPasados[c]; }
    public bitbrd GetPeonesACoronar(color c) { return m_btPeonesACoronar[c]; }

    //----------------------------------------------------------------------------------------
    public int SemiAbierta(color c, colum f)
    {
      return m_nColumnasSemiAbiertas[c] & (1 << (int)(f));
    }

    //----------------------------------------------------------------------------------------
    public int SemiAbierta(color c, colum f, bool leftSide)
    {
      return m_nColumnasSemiAbiertas[c] & (leftSide ? ((1 << f) - 1) : ~((1 << (f + 1)) - 1));
    }

    //----------------------------------------------------------------------------------------
    public int PeonesMismoColor(color c, sq s)
    {
      return m_nCasillasPeon[c][(cBitBoard.DarkSquares & cBitBoard.m_nCasillas[s]) != 0 ? 1 : 0];
    }
    
    //----------------------------------------------------------------------------------------
    public pnt ReyProtegido(cPosicion pos, sq ksq, color Us)
    {
     return m_CasillaRey[Us] == ksq && m_nEnroques[Us] == pos.CanEnroque(Us)
       ? m_nPuntosReySeguro[Us] : (m_nPuntosReySeguro[Us] = do_king_safety(pos, ksq, Us));
      //  return m_CasillaRey[Us] == ksq && m_nEnroques[Us] == pos.CanEnroque(Us)
      //    ? 400 : 0;
    }

    /// Entry::do_king_safety() calculates a bonus for king safety. It is called only
    /// when king square changes, which is about 20% of total king_safety() calls.
    public pnt do_king_safety(cPosicion pos, sq ksq, color Us)
    {
      m_CasillaRey[Us] = ksq;
      m_nEnroques[Us] = pos.CanEnroque(Us);
      m_nPuntosDistanciaPeonRey[Us] = 0;

      bitbrd pawns = pos.PiezasColor(Us, cPieza.PEON);
      if (pawns != 0)
        while (0 == (cBitBoard.m_Distancia[ksq][m_nPuntosDistanciaPeonRey[Us]++] & pawns)) { }

      if (cTypes.FilaProximaFromFila(Us, ksq) > FILA.F4)
        return cTypes.Puntua(0, -16 * m_nPuntosDistanciaPeonRey[Us]);

      val bonus = shelter_storm(pos, ksq, Us);

      if (pos.CanEnroque((new cEnroque(Us, cEnroque.LADO_REY)).m_Tipo) != 0)
        bonus = Math.Max(bonus, shelter_storm(pos, cTypes.CasillaProxima(Us, cCasilla.G1), Us));

      if (pos.CanEnroque((new cEnroque(Us, cEnroque.LADO_DAMA)).m_Tipo) != 0)
        bonus = Math.Max(bonus, shelter_storm(pos, cTypes.CasillaProxima(Us, cCasilla.C1), Us));

      return cTypes.Puntua(bonus, -16 * m_nPuntosDistanciaPeonRey[Us]);
    }

    /// Entry::shelter_storm() calculates shelter and storm penalties for the file
    /// the king is on, as well as the two adjacent files.     
    public val shelter_storm(cPosicion pos, sq ksq, color Us)
    {
      color Them = (Us == cColor.BLANCO ? cColor.NEGRO : cColor.BLANCO);

      pnt safety = cEstructuraDePeones.MaxSafetyBonus;
      bitbrd b = pos.PiezasColor(cPieza.PEON) & (cBitBoard.EnFrente(Us, cTypes.Fila(ksq)) | cBitBoard.GetFila(ksq));
      bitbrd ourPawns = b & pos.PiezasColor(Us);
      bitbrd theirPawns = b & pos.PiezasColor(Them);
      fila rkUs, rkThem;
      colum kf = Math.Max(COLUMNA.B, Math.Min(COLUMNA.G, cTypes.Fila(ksq)));

      for (colum f = kf - 1; f <= kf + 1; ++f)
      {
        b = ourPawns & cBitBoard.GetColumna(f);
        rkUs = b != 0 ? cTypes.FilaProxima(Us, cBitBoard.backmost_sq(Us, b)) : FILA.F1;

        b = theirPawns & cBitBoard.GetColumna(f);
        rkThem = b != 0 ? cTypes.FilaProxima(Us, cBitBoard.Adelantado(Them, b)) : FILA.F1;

        if ((MiddleEdges & cBitBoard.m_nCasillas[cTypes.CreaCasilla(f, rkThem)]) != 0
          && cTypes.Columna(ksq) == f
          && cTypes.FilaProxima(Us, ksq) == rkThem - 1)
          safety += 200;
        else
          safety -= ShelterWeakness[rkUs]
            + StormDanger[rkUs == FILA.F1 ? 0 : rkThem == rkUs + 1 ? 2 : 1][rkThem];
      }

      return safety;
    }
  }
}
