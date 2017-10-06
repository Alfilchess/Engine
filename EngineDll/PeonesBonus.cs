using Types;

using hash = System.UInt64;
using bitbrd = System.UInt64;
using pnt = System.Int32;
using sq = System.Int32;
using color = System.Int32;
using colum = System.Int32;
using fila = System.Int32;

namespace Motor
{
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

      while ((nCasilla = listPeones[listPeonesPos++]) != cCasilla.NONE && listPeonesPos < cPieza.MAX)
      {
        nColumn = cTypes.Columna(nCasilla);
        estructPeon.m_nColumnasSemiAbiertas[colr] &= ~(1 << nColumn);
        if ((nCasilla - cTypes.AvancePeon(colr)) >= cCasilla.A1 && (nCasilla - cTypes.AvancePeon(colr)) <= cCasilla.H8)
        {
          p = cBitBoard.GetFilaS1(nCasilla - cTypes.AvancePeon(colr));
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
            && (b = cBitBoard.AtaquePeon(colorOpuesto, nCasilla + cTypes.AvancePeon(colr)) & btPeonesColor) != 0
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
