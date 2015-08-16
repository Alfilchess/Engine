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
        while (0==(cBitBoard.m_Distancia[ksq][m_nPuntosDistanciaPeonRey[Us]++] & pawns)) { }

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
      color Them = (Us == cColor.BLANCO ? cColor.NEGRO: cColor.BLANCO);

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

        if ((MiddleEdges & cBitBoard.m_nCasillas[cTypes.CreaCasilla(f, rkThem)])!=0
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

  //----------------------------------------------------------------------------------------
  //----------------------------------------------------------------------------------------
  public class cBonusPeones
  {
    //----------------------------------------------------------------------------------------
    public class cHashPeones : cHashTable<cEstructuraDePeones>
    {
      public cHashPeones() : base(16384)
      {
        for (int i = 0; i < this.Size; i++)
          m_TablaHash.Add(new cEstructuraDePeones());
      }
    }

    //----------------------------------------------------------------------------------------
    private static pnt Pnt(int nMedioJuego, int nFinalJuego)
    {
      return cTypes.Puntua(nMedioJuego, nFinalJuego);
    }

    private static pnt[] m_Doblados = new pnt[] {
          Pnt(13, 43), Pnt(20, 48), Pnt(23, 48), Pnt(23, 48), Pnt(23, 48), Pnt(23, 48), Pnt(20, 48), Pnt(13, 43) };
    private static pnt[][] m_Aislados = new pnt[][] {
      new pnt[] {   Pnt(37, 45), Pnt(54, 52), Pnt(60, 52), Pnt(60, 52), Pnt(60, 52), Pnt(60, 52), Pnt(54, 52), Pnt(37, 45) },
      new pnt[] {   Pnt(25, 30), Pnt(36, 35), Pnt(40, 35), Pnt(40, 35), Pnt(40, 35), Pnt(40, 35), Pnt(36, 35), Pnt(25, 30) }};
    private static pnt[][] m_Retrasados = new pnt[][] {
      new pnt[] {   Pnt(30, 42), Pnt(43, 46), Pnt(49, 46), Pnt(49, 46), Pnt(49, 46), Pnt(49, 46), Pnt(43, 46), Pnt(30, 42) },
      new pnt[] {   Pnt(20, 28), Pnt(29, 31), Pnt(33, 31), Pnt(33, 31), Pnt(33, 31), Pnt(33, 31), Pnt(29, 31), Pnt(20, 28) }};

    private static int[] m_ColumnaBonus = new int[] { 1, 2, 3, 4, 4, 3, 2, 1 };
    private static pnt[][] m_Conectados;
    private static pnt[] m_Pasados = new pnt[] {
          Pnt( 0, 0), Pnt( 6, 13), Pnt(6,13), Pnt(14,29), Pnt(34,68), Pnt(83,166), Pnt(0, 0), Pnt( 0, 0)
      };
    private static pnt m_Avanzados = Pnt(0, 15);
    private static pnt m_SinDefensa = Pnt(20, 10);

    //----------------------------------------------------------------------------------------
    public static void Init()
    {
      m_Conectados = new pnt[COLUMNA.OUT][];
      for (colum f = COLUMNA.A; f <= COLUMNA.H; ++f)
      {
        m_Conectados[f] = new pnt[FILA.OUT];
      }

      for (fila r = FILA.F1; r < FILA.F8; ++r)
        for (colum f = COLUMNA.A; f <= COLUMNA.H; ++f)
        {
          int nBonus = r * (r - 1) * (r - 2) + m_ColumnaBonus[f] * (r / 2 + 1);
          m_Conectados[f][r] = cTypes.Puntua(nBonus, nBonus + (nBonus / 2));
        }
    }

    //----------------------------------------------------------------------------------------
    private static pnt Evalua(cPosicion pos, cEstructuraDePeones estructPeon, color colr)
    {
      color colorOpuesto = (colr == cColor.BLANCO ? cColor.NEGRO : cColor.BLANCO);
      sq escaqueArriba = (colr == cColor.BLANCO ? cCasilla.NORTE : cCasilla.SUR);
      sq escaqueDerecha = (colr == cColor.BLANCO ? cCasilla.NORTE_ESTE : cCasilla.SUR_OESTE);
      sq escaqueIzquierda = (colr == cColor.BLANCO ? cCasilla.NORTE_OESTE : cCasilla.SUR_ESTE);

      bitbrd b, p, btDoblados;
      sq nCasilla;
      colum nColumn;
      bool bPasado, bAislado, bBloqueado, bConectado, backward, bCoronable, bNoSoportado;
      pnt value = 0;
      sq[] listPeones = pos.GetList(colr, cPieza.PEON);
      int listPeonesPos = 0;

      bitbrd btPeonesColor = pos.PiezasColor(colr, cPieza.PEON);
      bitbrd btPeonesColorOpuesto = pos.PiezasColor(colorOpuesto, cPieza.PEON);

      estructPeon.m_btPeonesPasados[colr] = estructPeon.m_btPeonesACoronar[colr] = 0;
      estructPeon.m_CasillaRey[colr] = cCasilla.NONE;
      estructPeon.m_nColumnasSemiAbiertas[colr] = 0xFF;
      estructPeon.m_btPeonesAtaques[colr] = cBitBoard.Desplazamiento(btPeonesColor, escaqueDerecha) | cBitBoard.Desplazamiento(btPeonesColor, escaqueIzquierda);
      estructPeon.m_nCasillasPeon[colr][cColor.NEGRO] = cBitBoard.CountMax15(btPeonesColor & cBitBoard.DarkSquares);
      estructPeon.m_nCasillasPeon[colr][cColor.BLANCO] = pos.GetNum(colr, cPieza.PEON) - estructPeon.m_nCasillasPeon[colr][cColor.NEGRO];

      while ((nCasilla = listPeones[listPeonesPos++]) != cCasilla.NONE)
      {
        nColumn = cTypes.Columna(nCasilla);
        estructPeon.m_nColumnasSemiAbiertas[colr] &= ~(1 << nColumn);
        p = cBitBoard.GetFilaS1(nCasilla - cTypes.AtaquePeon(colr));
        b = cBitBoard.GetFilaS1(nCasilla) | p;
        bConectado = (btPeonesColor & cBitBoard.ColumnaAdjacente(nColumn) & b) != 0;
        bNoSoportado = (0 == (btPeonesColor & cBitBoard.ColumnaAdjacente(nColumn) & p));
        bAislado = (0 == (btPeonesColor & cBitBoard.ColumnaAdjacente(nColumn)));
        btDoblados = btPeonesColor & cBitBoard.Atras(colr, nCasilla);
        bBloqueado = (btPeonesColorOpuesto & cBitBoard.Atras(colr, nCasilla)) != 0;
        bPasado = (0 == (btPeonesColorOpuesto & cBitBoard.PeonPasado(colr, nCasilla)));

        if ((bPasado | bAislado | bConectado) || (btPeonesColor & cBitBoard.AtaquePeon(colorOpuesto, nCasilla)) != 0 || (pos.AtaquesDePeon(nCasilla, colr) & btPeonesColorOpuesto) != 0)
          backward = false;
        else
        {
          b = cBitBoard.AtaquePeon(colr, nCasilla) & (btPeonesColor | btPeonesColorOpuesto);
          b = cBitBoard.AtaquePeon(colr, nCasilla) & cBitBoard.GetFilaS1(cBitBoard.backmost_sq(colr, b));
          backward = ((b | cBitBoard.Desplazamiento(b, escaqueArriba)) & btPeonesColorOpuesto) != 0;
        }

        bCoronable = !(bBloqueado | bPasado | backward | bAislado)
          && (b = cBitBoard.AtaquePeon(colorOpuesto, nCasilla + cTypes.AtaquePeon(colr)) & btPeonesColor) != 0
                 && cBitBoard.CountMax15(b) >= cBitBoard.CountMax15(cBitBoard.AtaquePeon(colr, nCasilla) & btPeonesColorOpuesto);

        if (bPasado && 0 == btDoblados)
          estructPeon.m_btPeonesPasados[colr] |= cBitBoard.m_nCasillas[nCasilla];

        if (bAislado)
          value -= m_Aislados[bBloqueado ? 1 : 0][nColumn];

        if (bNoSoportado && !bAislado)
          value -= m_SinDefensa;

        if (btDoblados != 0)
          value -= cTypes.divScore(m_Doblados[nColumn], cBitBoard.FilaDistancia(nCasilla, cBitBoard.LSB(btDoblados)));

        if (backward)
          value -= m_Retrasados[bBloqueado ? 1 : 0][nColumn];

        if (bConectado)
          value += m_Conectados[nColumn][cTypes.FilaProxima(colr, nCasilla)];

        if (bCoronable)
        {
          value += m_Pasados[cTypes.FilaProxima(colr, nCasilla)];

          if (0 == btDoblados)
            estructPeon.m_btPeonesACoronar[colr] |= cBitBoard.m_nCasillas[nCasilla];
        }
      }

      if (pos.GetNum(colr, cPieza.PEON) > 1)
      {
        b = (bitbrd)(estructPeon.m_nColumnasSemiAbiertas[colr] ^ 0xFF);
        value += m_Avanzados * (cBitBoard.MSB(b) - cBitBoard.LSB(b));
      }

      return value;
    }

    //----------------------------------------------------------------------------------------
    public static cEstructuraDePeones Buscar(cPosicion pos, cBonusPeones.cHashPeones hashPeones)
    {
      hash nClave = pos.ClaveHashPeon();
      cEstructuraDePeones estructPeones = hashPeones[nClave];

      if (estructPeones.m_nClave == nClave)
        return estructPeones;

      estructPeones.m_nClave = nClave;
      estructPeones.m_nValor = Evalua(pos, estructPeones, cColor.BLANCO) - Evalua(pos, estructPeones, cColor.NEGRO);

      return estructPeones;
    }   
  }
}
