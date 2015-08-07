using System;
using System.Collections.Generic;
using System.Text;
using InOut;
using Types;

using bitbrd = System.UInt64;
using hash = System.UInt64;
using sq = System.Int32;
using color = System.Int32;
using type = System.Int32;
using val = System.Int32;
using pnt = System.Int32;
using pieza = System.Int32;
using mov = System.Int32;
using enroque = System.Int32;
using colum = System.Int32;
using fila = System.Int32;

namespace Motor
{
  //---------------------------------------------------------------------------------------------------------
  //---------------------------------------------------------------------------------------------------------
  public class cPosicion
  {
    private pieza[] m_Tablero = new pieza[cCasilla.ESCAQUES];
    private bitbrd[] byTypeBB = new bitbrd[cPieza.SIZE];
    private bitbrd[] byColorBB = new bitbrd[cColor.SIZE];
    private int[][] m_nNumPiezas = new int[cColor.SIZE][] { new int[cPieza.SIZE], new int[cPieza.SIZE] };
    private sq[][][] m_lstPiezas = new sq[cColor.SIZE][][] { new sq[cPieza.SIZE][] { new sq[16], new sq[16], new sq[16], new sq[16], new sq[16], new sq[16], new sq[16], new sq[16] }, new sq[cPieza.SIZE][] { new sq[16], new sq[16], new sq[16], new sq[16], new sq[16], new sq[16], new sq[16], new sq[16] } };
    private int[] m_nIndex = new int[cCasilla.ESCAQUES];

    private int[] m_nMaskEnroque = new int[cCasilla.ESCAQUES];
    private sq[] m_nSqEnroqueTorre = new sq[cEnroque.ENROQUE_DERECHA];
    private bitbrd[] m_nEnroques = new bitbrd[cEnroque.ENROQUE_DERECHA];
    private cPosInfo m_estadoInicial = new cPosInfo();
    private UInt64 m_nNodos;
    private int m_nPly;
    private color m_clrActivo;
    private cThread m_ActiveThread;
    private cPosInfo m_PosInfo;
    private bool m_bIsChess960;
    public static readonly val[][] m_nValPieza = new val[cFaseJuego.NAN][]{
            new val[cPieza.COLORES]{ cValoresJuego.CERO, cValoresJuego.PEON_MJ, cValoresJuego.CABALLO_MJ, cValoresJuego.ALFIL_MJ, cValoresJuego.TORRE_MJ, cValoresJuego.DAMA_MJ, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            new val[cPieza.COLORES]{ cValoresJuego.CERO, cValoresJuego.PEON_FINAL, cValoresJuego.CABALLO_FINAL, cValoresJuego.ALFIL_FINAL, cValoresJuego.TORRE_FINAL, cValoresJuego.DAMA_FINAL, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } };
    public static readonly pnt[][][] m_nSqPeon = new pnt[cColor.SIZE][][];

    //---------------------------------------------------------------------------------------------------------
    public cPosicion(cPosicion pos, cThread t)
    {
      CopyFrom(pos);
      m_ActiveThread = t;
    }

    //---------------------------------------------------------------------------------------------------------
    public cPosicion(cPosicion pos)
    {
      CopyFrom(pos);
    }

    //---------------------------------------------------------------------------------------------------------
    public cPosicion(string f, bool c960, cThread t)
    {
      SetFEN(f, c960, t);
    }

    //---------------------------------------------------------------------------------------------------------
    public static bool IsNum(char c)
    {
      return c >= '0' && c <= '9';
    }

    //---------------------------------------------------------------------------------------------------------
    public static bool IsLower(char token)
    {
      return token.ToString().ToLowerInvariant() == token.ToString();
    }

    //---------------------------------------------------------------------------------------------------------
    public static char ToMayus(char token)
    {
      return token.ToString().ToUpperInvariant()[0];
    }

    //---------------------------------------------------------------------------------------------------------
    public UInt64 GetNodos()
    {
      return m_nNodos;
    }

    //---------------------------------------------------------------------------------------------------------
    public void SetNodos(UInt64 n)
    {
      m_nNodos = n;
    }

    //---------------------------------------------------------------------------------------------------------
    public pieza GetPieza(sq s)
    {
      return m_Tablero[s];
    }

    //---------------------------------------------------------------------------------------------------------
    public pieza GetPiezaMovida(mov m)
    {
      return m_Tablero[cTypes.GetFromCasilla(m)];
    }

    //---------------------------------------------------------------------------------------------------------
    public bool CasillaVacia(sq s)
    {
      return m_Tablero[s] == cPieza.NAN;
    }

    //---------------------------------------------------------------------------------------------------------
    public color ColorMueve()
    {
      return m_clrActivo;
    }

    //---------------------------------------------------------------------------------------------------------
    public bitbrd Piezas()
    {
      return byTypeBB[cPieza.NAN];
    }

    //---------------------------------------------------------------------------------------------------------
    public bitbrd GetNumPiezas(type pt)
    {
      return byTypeBB[pt];
    }

    //---------------------------------------------------------------------------------------------------------
    public bitbrd GetNumPiezas(type pt1, type pt2)
    {
      return byTypeBB[pt1] | byTypeBB[pt2];
    }

    //---------------------------------------------------------------------------------------------------------
    public bitbrd PiezasColor(color c)
    {
      return byColorBB[c];
    }

    public bitbrd PiezasColor(color c, type pt)
    {
      return byColorBB[c] & byTypeBB[pt];
    }

    public bitbrd PiezasColor(color c, type pt1, type pt2)
    {
      return byColorBB[c] & (byTypeBB[pt1] | byTypeBB[pt2]);
    }

    //---------------------------------------------------------------------------------------------------------
    public int GetNum(color c, type pt)
    {
      return m_nNumPiezas[c][pt];
    }

    //---------------------------------------------------------------------------------------------------------
    public sq[] GetList(color c, type pt)
    {
      return m_lstPiezas[c][pt];
    }

    //---------------------------------------------------------------------------------------------------------
    public sq CasillaEnPaso()
    {
      return m_PosInfo.enCasillaPeonuare;
    }

    //---------------------------------------------------------------------------------------------------------
    public sq GetRey(color c)
    {
      return m_lstPiezas[c][cPieza.REY][0];
    }

    //---------------------------------------------------------------------------------------------------------
    public int PosibleEnrocar(enroque cr)
    {
      return m_PosInfo.castlingRights & cr;
    }

    //---------------------------------------------------------------------------------------------------------
    public int CanEnroque(color c)
    {
      return m_PosInfo.castlingRights & ((cEnroque.OO_BLANCAS | cEnroque.OOO_BLANCAS) << (2 * c));
    }

    //---------------------------------------------------------------------------------------------------------
    public bool CanNotEnroque(enroque cr)
    {
      return (byTypeBB[cPieza.NAN] & m_nEnroques[cr]) != 0;
    }

    //---------------------------------------------------------------------------------------------------------
    public sq CasillaTorreEnroque(enroque cr)
    {
      return m_nSqEnroqueTorre[cr];
    }

    //---------------------------------------------------------------------------------------------------------
    public bitbrd attacks_from_square_piecetype(sq s, type Pt)
    {
      return (Pt == cPieza.ALFIL || Pt == cPieza.TORRE) ? cBitBoard.AtaquesPieza(s, Piezas(), Pt)
           : (Pt == cPieza.DAMA) ? attacks_from_square_piecetype(s, cPieza.TORRE) | attacks_from_square_piecetype(s, cPieza.ALFIL)
           : cBitBoard.m_Ataques[Pt][s];
    }

    //---------------------------------------------------------------------------------------------------------
    public bitbrd AtaquesDePeon(sq s, color c)
    {
      return cBitBoard.m_Ataques[cTypes.CreaPieza(c, cPieza.PEON)][s];
    }

    //---------------------------------------------------------------------------------------------------------
    public bitbrd AtaquesDePieza(pieza pc, sq s)
    {
      return cBitBoard.Ataques(pc, s, byTypeBB[cPieza.NAN]);
    }

    //---------------------------------------------------------------------------------------------------------
    public bitbrd AtaquesA(sq s)
    {
      return AtaquesA(s, byTypeBB[cPieza.NAN]);
    }

    //---------------------------------------------------------------------------------------------------------
    public bitbrd Jaques()
    {
      return m_PosInfo.checkersBB;
    }

    //---------------------------------------------------------------------------------------------------------
    public bitbrd CandidatosJaque()
    {
      return Jaques(m_clrActivo, cTypes.Contrario(m_clrActivo));
    }

    //---------------------------------------------------------------------------------------------------------
    public bitbrd pinned_pieces(color c)
    {
      return Jaques(c, c);
    }

    //---------------------------------------------------------------------------------------------------------
    public bool IsPeonPasado(color c, sq s)
    {
      return 0 == (PiezasColor(cTypes.Contrario(c), cPieza.PEON) & cBitBoard.PeonPasado(c, s));
    }

    //---------------------------------------------------------------------------------------------------------
    public bool IsPeonAvanzado(mov m)
    {
      return cTypes.TipoPieza(GetPiezaMovida(m)) == cPieza.PEON
          && cTypes.FilaProxima(m_clrActivo, cTypes.GetFromCasilla(m)) > FILA.F4;
    }

    public hash ClaveHash()
    {
      return m_PosInfo.key;
    }

    public hash ClaveHashPeon()
    {
      return m_PosInfo.pawnKey;
    }

    public hash ClaveMaterial()
    {
      return m_PosInfo.materialKey;
    }

    public pnt GetPuntosPeon()
    {
      return m_PosInfo.nCasillaPeon;
    }

    public val MaterialPieza(color c)
    {
      return m_PosInfo.npMaterial[c];
    }

    public int GetPly()
    {
      return m_nPly;
    }

    public bool AlfilesOpuestos()
    {
      return m_nNumPiezas[cColor.BLANCO][cPieza.ALFIL] == 1
          && m_nNumPiezas[cColor.NEGRO][cPieza.ALFIL] == 1
          && cTypes.IsColorContrario(m_lstPiezas[cColor.BLANCO][cPieza.ALFIL][0], m_lstPiezas[cColor.NEGRO][cPieza.ALFIL][0]);
    }

    //-------------------------------------------------------------------------------------------------------------
    public bool IsAlfilPar(color c)
    {
      return m_nNumPiezas[c][cPieza.ALFIL] >= 2
          && cTypes.IsColorContrario(m_lstPiezas[c][cPieza.ALFIL][0], m_lstPiezas[c][cPieza.ALFIL][1]);
    }

    //-------------------------------------------------------------------------------------------------------------
    public bool IsPeonOn7(color c)
    {
      return (PiezasColor(c, cPieza.PEON) & cBitBoard.GetFila(cTypes.FilaProximaFromFila(c, FILA.F7))) != 0;
    }

    //-------------------------------------------------------------------------------------------------------------
    public bool IsChess960()
    {
      return m_bIsChess960;
    }

    //-------------------------------------------------------------------------------------------------------------
    public bool IsCapturaOrPromocion(mov m)
    {
      return cTypes.TipoMovimiento(m) != cMovType.NORMAL ? cTypes.TipoMovimiento(m) != cMovType.ENROQUE : !CasillaVacia(cTypes.GetToCasilla(m));
    }

    //-------------------------------------------------------------------------------------------------------------
    public bool IsCaptura(mov m)
    {

      return (!CasillaVacia(cTypes.GetToCasilla(m)) && cTypes.TipoMovimiento(m) != cMovType.ENROQUE) || cTypes.TipoMovimiento(m) == cMovType.ENPASO;
    }

    //-------------------------------------------------------------------------------------------------------------
    public type GetTipoPiezaCapturada()
    {
      return m_PosInfo.capturedType;
    }

    //-------------------------------------------------------------------------------------------------------------
    public cThread ThreadActive()
    {
      return m_ActiveThread;
    }

    //-------------------------------------------------------------------------------------------------------------
    public void SetPieza(sq s, color c, type pt)
    {
      m_Tablero[s] = cTypes.CreaPieza(c, pt);
      byTypeBB[cPieza.NAN] |= cBitBoard.m_nCasillas[s];
      byTypeBB[pt] |= cBitBoard.m_nCasillas[s];
      byColorBB[c] |= cBitBoard.m_nCasillas[s];
      m_nIndex[s] = m_nNumPiezas[c][pt]++;
      m_lstPiezas[c][pt][m_nIndex[s]] = s;
    }

    //-------------------------------------------------------------------------------------------------------------
    public void MuevePieza(sq from, sq to, color c, type pt)
    {
      bitbrd from_to_bb = cBitBoard.m_nCasillas[from] ^ cBitBoard.m_nCasillas[to];
      byTypeBB[cPieza.NAN] ^= from_to_bb;
      byTypeBB[pt] ^= from_to_bb;
      byColorBB[c] ^= from_to_bb;
      m_Tablero[from] = cPieza.NAN;
      m_Tablero[to] = cTypes.CreaPieza(c, pt);
      m_nIndex[to] = m_nIndex[from];
      m_lstPiezas[c][pt][m_nIndex[to]] = to;
    }

    //-------------------------------------------------------------------------------------------------------------
    public void BorraPieza(sq s, color c, type pt)
    {
      byTypeBB[cPieza.NAN] ^= cBitBoard.m_nCasillas[s];
      byTypeBB[pt] ^= cBitBoard.m_nCasillas[s];
      byColorBB[c] ^= cBitBoard.m_nCasillas[s];

      sq lastSquare = m_lstPiezas[c][pt][--m_nNumPiezas[c][pt]];
      m_nIndex[lastSquare] = m_nIndex[s];
      m_lstPiezas[c][pt][m_nIndex[lastSquare]] = lastSquare;
      m_lstPiezas[c][pt][m_nNumPiezas[c][pt]] = cCasilla.NONE;
    }

    //-------------------------------------------------------------------------------------------------------------
    public hash GetClaveHashExclusion()
    {
      return m_PosInfo.key ^ cHashZobrist.m_Exclusion;
    }

    //-------------------------------------------------------------------------------------------------------------
    public type GetTipoAtaqueMinimo(bitbrd[] bb, sq to, bitbrd stmAttackers, ref bitbrd occupied, ref bitbrd attackers, int Pt)
    {
      bitbrd b = stmAttackers & bb[Pt];
      if (0 == b)
      {
        if ((Pt + 1) == cPieza.REY)
          return cPieza.REY;
        else
          return GetTipoAtaqueMinimo(bb, to, stmAttackers, ref occupied, ref attackers, Pt + 1);
      }
      occupied ^= b & ~(b - 1);
      if (Pt == cPieza.PEON || Pt == cPieza.ALFIL || Pt == cPieza.DAMA)
        attackers |= cBitBoard.AtaquesPieza(to, occupied, cPieza.ALFIL) & (bb[cPieza.ALFIL] | bb[cPieza.DAMA]);
      if (Pt == cPieza.TORRE || Pt == cPieza.DAMA)
        attackers |= cBitBoard.AtaquesPieza(to, occupied, cPieza.TORRE) & (bb[cPieza.TORRE] | bb[cPieza.DAMA]);
      attackers &= occupied;
      return (type)Pt;
    }

    //--------------------------------------------------------------------------------------------------------------------------------
    public static void Init()
    {
      cAleatorio rk = new cAleatorio(4474);
      for (color c = cColor.BLANCO; c <= cColor.NEGRO; ++c)
      {
        cHashZobrist.m_sqPeon[c] = new hash[8][];
        for (type pt = cPieza.PEON; pt <= cPieza.REY; ++pt)
        {
          cHashZobrist.m_sqPeon[c][pt] = new hash[64];
          for (sq s = cCasilla.A1; s <= cCasilla.H8; ++s)
            cHashZobrist.m_sqPeon[c][pt][s] = rk.GetRand();
        }
      }
      for (colum f = COLUMNA.A; f <= COLUMNA.H; ++f)
        cHashZobrist.m_EnPaso[f] = rk.GetRand();
      for (int cf = cEnroque.NO_ENROQUE; cf <= cEnroque.CUALQUIER_ENROQUE; ++cf)
      {
        bitbrd b = (bitbrd)cf;
        while (b != 0)
        {
          hash k = cHashZobrist.m_Enroque[1UL << cBitBoard.GetLSB(ref b)];
          cHashZobrist.m_Enroque[cf] ^= k != 0 ? k : rk.GetRand();
        }
      }
      cHashZobrist.m_Bando = rk.GetRand();
      cHashZobrist.m_Exclusion = rk.GetRand();
      for (int i = 0; i < cColor.SIZE; i++)
      {
        m_nSqPeon[i] = new pnt[cPieza.SIZE][];
        for (int k = 0; k < cPieza.SIZE; k++)
        {
          m_nSqPeon[i][k] = new pnt[cCasilla.ESCAQUES];
        }
      }

      for (int i = 0; i < cColor.SIZE; i++)
      {
        m_nSqPeon[i] = new pnt[cPieza.SIZE][];
        for (int k = 0; k < cPieza.SIZE; k++)
        {
          m_nSqPeon[i][k] = new pnt[cCasilla.ESCAQUES];
        }
      }
      for (type pt = cPieza.PEON; pt <= cPieza.REY; ++pt)
      {
        m_nValPieza[cFaseJuego.FASE_MEDIOJUEGO][cTypes.CreaPieza(cColor.NEGRO, pt)] = m_nValPieza[cFaseJuego.FASE_MEDIOJUEGO][pt];
        m_nValPieza[cFaseJuego.FASE_FINAL][cTypes.CreaPieza(cColor.NEGRO, pt)] = m_nValPieza[cFaseJuego.FASE_FINAL][pt];
        pnt v = cTypes.Puntua(m_nValPieza[cFaseJuego.FASE_MEDIOJUEGO][pt], m_nValPieza[cFaseJuego.FASE_FINAL][pt]);
        for (sq s = cCasilla.A1; s <= cCasilla.H8; ++s)
        {
          m_nSqPeon[cColor.BLANCO][pt][s] = (v + cTablero.TABLERO[pt][s]);
          m_nSqPeon[cColor.NEGRO][pt][cTypes.CasillaVS(s)] = -(v + cTablero.TABLERO[pt][s]);
        }
      }
    }

    //--------------------------------------------------------------------------------------------------------------------------------
    public void CopyFrom(cPosicion pos)
    {
      Array.Copy(pos.m_Tablero, m_Tablero, cCasilla.ESCAQUES);
      Array.Copy(pos.byTypeBB, byTypeBB, cPieza.SIZE);
      Array.Copy(pos.byColorBB, byColorBB, cColor.SIZE);
      Array.Copy(pos.m_nIndex, m_nIndex, cCasilla.ESCAQUES);
      Array.Copy(pos.m_nMaskEnroque, m_nMaskEnroque, cCasilla.ESCAQUES);
      m_nNodos = 0;
      m_clrActivo = pos.m_clrActivo;
      m_nPly = pos.m_nPly;
      m_bIsChess960 = pos.m_bIsChess960;
      m_estadoInicial = pos.m_PosInfo.getCopy();
      m_PosInfo = m_estadoInicial;
      m_ActiveThread = pos.m_ActiveThread;
      Array.Copy(pos.m_nSqEnroqueTorre, m_nSqEnroqueTorre, cEnroque.ENROQUE_DERECHA);
      Array.Copy(pos.m_nEnroques, m_nEnroques, cEnroque.ENROQUE_DERECHA);
      Array.Copy(pos.m_nNumPiezas[cColor.BLANCO], m_nNumPiezas[cColor.BLANCO], 8);
      Array.Copy(pos.m_nNumPiezas[cColor.NEGRO], m_nNumPiezas[cColor.NEGRO], 8);
      Array.Copy(pos.m_lstPiezas[cColor.BLANCO][cPieza.NAN], m_lstPiezas[cColor.BLANCO][cPieza.NAN], 16);
      Array.Copy(pos.m_lstPiezas[cColor.BLANCO][cPieza.PEON], m_lstPiezas[cColor.BLANCO][cPieza.PEON], 16);
      Array.Copy(pos.m_lstPiezas[cColor.BLANCO][cPieza.CABALLO], m_lstPiezas[cColor.BLANCO][cPieza.CABALLO], 16);
      Array.Copy(pos.m_lstPiezas[cColor.BLANCO][cPieza.ALFIL], m_lstPiezas[cColor.BLANCO][cPieza.ALFIL], 16);
      Array.Copy(pos.m_lstPiezas[cColor.BLANCO][cPieza.TORRE], m_lstPiezas[cColor.BLANCO][cPieza.TORRE], 16);
      Array.Copy(pos.m_lstPiezas[cColor.BLANCO][cPieza.DAMA], m_lstPiezas[cColor.BLANCO][cPieza.DAMA], 16);
      Array.Copy(pos.m_lstPiezas[cColor.BLANCO][cPieza.REY], m_lstPiezas[cColor.BLANCO][cPieza.REY], 16);
      Array.Copy(pos.m_lstPiezas[cColor.BLANCO][7], m_lstPiezas[cColor.BLANCO][7], 16);
      Array.Copy(pos.m_lstPiezas[cColor.NEGRO][cPieza.NAN], m_lstPiezas[cColor.NEGRO][cPieza.NAN], 16);
      Array.Copy(pos.m_lstPiezas[cColor.NEGRO][cPieza.PEON], m_lstPiezas[cColor.NEGRO][cPieza.PEON], 16);
      Array.Copy(pos.m_lstPiezas[cColor.NEGRO][cPieza.CABALLO], m_lstPiezas[cColor.NEGRO][cPieza.CABALLO], 16);
      Array.Copy(pos.m_lstPiezas[cColor.NEGRO][cPieza.ALFIL], m_lstPiezas[cColor.NEGRO][cPieza.ALFIL], 16);
      Array.Copy(pos.m_lstPiezas[cColor.NEGRO][cPieza.TORRE], m_lstPiezas[cColor.NEGRO][cPieza.TORRE], 16);
      Array.Copy(pos.m_lstPiezas[cColor.NEGRO][cPieza.DAMA], m_lstPiezas[cColor.NEGRO][cPieza.DAMA], 16);
      Array.Copy(pos.m_lstPiezas[cColor.NEGRO][cPieza.REY], m_lstPiezas[cColor.NEGRO][cPieza.REY], 16);
      Array.Copy(pos.m_lstPiezas[cColor.NEGRO][7], m_lstPiezas[cColor.NEGRO][7], 16);
    }

    //--------------------------------------------------------------------------------------------------------------------------------
    public void Clear()
    {
      m_nNodos = 0;
      m_clrActivo = 0;
      m_nPly = 0;
      m_bIsChess960 = false;
      Array.Clear(m_Tablero, 0, cCasilla.ESCAQUES);
      Array.Clear(byTypeBB, 0, cPieza.SIZE);
      Array.Clear(byColorBB, 0, cColor.SIZE);
      Array.Clear(m_nIndex, 0, cCasilla.ESCAQUES);
      Array.Clear(m_nMaskEnroque, 0, cCasilla.ESCAQUES);
      Array.Clear(m_nNumPiezas[cColor.BLANCO], 0, 8);
      Array.Clear(m_nNumPiezas[cColor.NEGRO], 0, 8);
      Array.Clear(m_nSqEnroqueTorre, 0, cEnroque.ENROQUE_DERECHA);
      Array.Clear(m_nEnroques, 0, cEnroque.ENROQUE_DERECHA);
      m_estadoInicial.Clear();
      m_estadoInicial.enCasillaPeonuare = cCasilla.NONE;
      m_PosInfo = m_estadoInicial;
      for (int i = 0; i < cPieza.SIZE; ++i)
        for (int j = 0; j < 16; ++j)
          m_lstPiezas[cColor.BLANCO][i][j] = m_lstPiezas[cColor.NEGRO][i][j] = cCasilla.NONE;
    }


    //--------------------------------------------------------------------------------------------------------------------------------
    public void SetFEN(string fenStr, bool bChess960, cThread th)
    {
      const string m_strPieza = " PNBRQK  pnbrqk";
      char col, row, token;
      SByte idx;
      sq sq = cCasilla.A8;
      char[] fen = fenStr.ToCharArray();
      int ss = 0;
      Clear();

      while ((token = fen[ss++]) != ' ')
      {
        if (IsNum(token))
          sq += (token - '0');
        else if (token == '/')
          sq -= 16;
        else
        {
          if ((idx = (SByte)m_strPieza.IndexOf(token)) > -1)
          {
            SetPieza(sq, cTypes.GetColor(idx), cTypes.TipoPieza(idx));
            ++sq;
          }
        }
      }

      token = fen[ss++];
      m_clrActivo = (token == 'w' ? cColor.BLANCO : cColor.NEGRO);
      token = fen[ss++];

      while ((token = fen[ss++]) != ' ')
      {
        sq nCasillaTorre;
        color c = IsLower(token) ? cColor.NEGRO : cColor.BLANCO;
        token = ToMayus(token);
        if (token == 'K')
        {
          for (nCasillaTorre = cTypes.CasillaProxima(c, cCasilla.H1); cTypes.TipoPieza(GetPieza(nCasillaTorre)) != cPieza.TORRE; --nCasillaTorre) { }
        }
        else if (token == 'Q')
        {
          for (nCasillaTorre = cTypes.CasillaProxima(c, cCasilla.A1); cTypes.TipoPieza(GetPieza(nCasillaTorre)) != cPieza.TORRE; --nCasillaTorre) { }
        }
        else if (token >= 'A' && token <= 'H')
        {
          nCasillaTorre = cTypes.CreaCasilla(token - 'A', cTypes.FilaProximaFromFila(c, FILA.F1));
        }
        else
        {
          continue;
        }
        SetEnroque(c, nCasillaTorre);
      }
      if (ss < fenStr.Length)
      {
        col = fen[ss++];
        if (ss < fenStr.Length)
        {
          row = fen[ss++];

          if (((col >= 'a' && col <= 'h')) && ((row == '3' || row == '6')))
          {
            m_PosInfo.enCasillaPeonuare = cTypes.CreaCasilla(col - 'a', row - '1');
            if ((AtaquesA(m_PosInfo.enCasillaPeonuare) & PiezasColor(m_clrActivo, cPieza.PEON)) == 0)
              m_PosInfo.enCasillaPeonuare = cCasilla.NONE;
          }
        }
      }

      Stack<string> tokens = cUci.CreateStack(fenStr.Substring(ss));
      if (tokens.Count > 0)
      {
        m_PosInfo.rule50 = int.Parse(tokens.Pop());
      }

      if (tokens.Count > 0)
      {
        m_nPly = int.Parse(tokens.Pop());
      }
      
      m_nPly = Math.Max(2 * (m_nPly - 1), 0) + ((m_clrActivo == cColor.NEGRO) ? 1 : 0);
      m_bIsChess960 = bChess960;
      m_ActiveThread = th;
      SetEstado(m_PosInfo);
    }

    //--------------------------------------------------------------------------------------------------------------------------------
    public void SetEnroque(color c, sq rfrom)
    {
      sq kfrom = GetRey(c);
      enroque cs = kfrom < rfrom ? cEnroque.LADO_REY : cEnroque.LADO_DAMA;
      enroque cr = cTypes.GetCasillaEnroque(c, cs);
      m_PosInfo.castlingRights |= cr;
      m_nMaskEnroque[kfrom] |= cr;
      m_nMaskEnroque[rfrom] |= cr;
      m_nSqEnroqueTorre[cr] = rfrom;
      sq kto = cTypes.CasillaProxima(c, cs == cEnroque.LADO_REY ? cCasilla.G1 : cCasilla.C1);
      sq rto = cTypes.CasillaProxima(c, cs == cEnroque.LADO_REY ? cCasilla.F1 : cCasilla.D1);
      for (sq s = Math.Min(rfrom, rto); s <= Math.Max(rfrom, rto); ++s)
        if (s != kfrom && s != rfrom)
          m_nEnroques[cr] |= cBitBoard.m_nCasillas[s];
      for (sq s = Math.Min(kfrom, kto); s <= Math.Max(kfrom, kto); ++s)
        if (s != kfrom && s != rfrom)
          m_nEnroques[cr] |= cBitBoard.m_nCasillas[s];
    }

    //--------------------------------------------------------------------------------------------------------------------------------
    public void SetEstado(cPosInfo si)
    {
      si.key = si.pawnKey = si.materialKey = 0;
      si.npMaterial[cColor.BLANCO] = si.npMaterial[cColor.NEGRO] = cValoresJuego.CERO;
      si.nCasillaPeon = cValoresJuego.CERO;
      si.checkersBB = AtaquesA(GetRey(m_clrActivo)) & PiezasColor(cTypes.Contrario(m_clrActivo));
      for (bitbrd b = Piezas(); b != 0; )
      {
        sq s = cBitBoard.GetLSB(ref b);
        pieza pc = GetPieza(s);
        si.key ^= cHashZobrist.m_sqPeon[cTypes.GetColor(pc)][cTypes.TipoPieza(pc)][s];
        si.nCasillaPeon += m_nSqPeon[cTypes.GetColor(pc)][cTypes.TipoPieza(pc)][s];
      }
      if (CasillaEnPaso() != cCasilla.NONE)
        si.key ^= cHashZobrist.m_EnPaso[cTypes.Columna(CasillaEnPaso())];
      if (m_clrActivo == cColor.NEGRO)
        si.key ^= cHashZobrist.m_Bando;
      si.key ^= cHashZobrist.m_Enroque[m_PosInfo.castlingRights];
      for (bitbrd b = GetNumPiezas(cPieza.PEON); b != 0; )
      {
        sq s = cBitBoard.GetLSB(ref b);
        si.pawnKey ^= cHashZobrist.m_sqPeon[cTypes.GetColor(GetPieza(s))][cPieza.PEON][s];
      }
      for (color c = cColor.BLANCO; c <= cColor.NEGRO; ++c)
        for (type pt = cPieza.PEON; pt <= cPieza.REY; ++pt)
          for (int cnt = 0; cnt < m_nNumPiezas[c][pt]; ++cnt)
            si.materialKey ^= cHashZobrist.m_sqPeon[c][pt][cnt];
      for (color c = cColor.BLANCO; c <= cColor.NEGRO; ++c)
        for (type pt = cPieza.CABALLO; pt <= cPieza.DAMA; ++pt)
          si.npMaterial[c] += m_nNumPiezas[c][pt] * m_nValPieza[cFaseJuego.FASE_MEDIOJUEGO][pt];
    }

    //--------------------------------------------------------------------------------------------------------------------------------
    public bitbrd Jaques(color c, color kingColor)
    {
      bitbrd b, pinners, result = 0;
      sq ksq = GetRey(kingColor);

      pinners = ((GetNumPiezas(cPieza.TORRE, cPieza.DAMA) & cBitBoard.m_PseudoAtaques[cPieza.TORRE][ksq])
               | (GetNumPiezas(cPieza.ALFIL, cPieza.DAMA) & cBitBoard.m_PseudoAtaques[cPieza.ALFIL][ksq])) & PiezasColor(cTypes.Contrario(kingColor));
      while (pinners != 0)
      {
        b = cBitBoard.Entre(ksq, cBitBoard.GetLSB(ref pinners)) & Piezas();
        if (!cBitBoard.MayorQue(b))
          result |= b & PiezasColor(c);
      }
      return result;
    }

    //--------------------------------------------------------------------------------------------------------------------------------
    public bitbrd AtaquesA(sq s, UInt64 occ)
    {
      return (AtaquesDePeon(s, cColor.NEGRO) & PiezasColor(cColor.BLANCO, cPieza.PEON))
           | (AtaquesDePeon(s, cColor.BLANCO) & PiezasColor(cColor.NEGRO, cPieza.PEON))
           | (attacks_from_square_piecetype(s, cPieza.CABALLO) & GetNumPiezas(cPieza.CABALLO))
           | (cBitBoard.AtaquesPieza(s, occ, cPieza.TORRE) & GetNumPiezas(cPieza.TORRE, cPieza.DAMA))
           | (cBitBoard.AtaquesPieza(s, occ, cPieza.ALFIL) & GetNumPiezas(cPieza.ALFIL, cPieza.DAMA))
           | (attacks_from_square_piecetype(s, cPieza.REY) & GetNumPiezas(cPieza.REY));
    }

    //--------------------------------------------------------------------------------------------------------------------------------
    public bool IsLegalMov(mov m, bitbrd pinned)
    {
      color us = m_clrActivo;
      sq from = cTypes.GetFromCasilla(m);



      if (cTypes.TipoMovimiento(m) == cMovType.ENPASO)
      {
        sq ksq = GetRey(us);
        sq to = cTypes.GetToCasilla(m);
        sq canCasillaPeon = to - cTypes.AtaquePeon(us);
        bitbrd occ = (Piezas() ^ cBitBoard.m_nCasillas[from] ^ cBitBoard.m_nCasillas[canCasillaPeon]) | cBitBoard.m_nCasillas[to];
        return 0 == (cBitBoard.AtaquesPieza(ksq, occ, cPieza.TORRE) & PiezasColor(cTypes.Contrario(us), cPieza.DAMA, cPieza.TORRE))
            && 0 == (cBitBoard.AtaquesPieza(ksq, occ, cPieza.ALFIL) & PiezasColor(cTypes.Contrario(us), cPieza.DAMA, cPieza.ALFIL));
      }



      if (cTypes.TipoPieza(GetPieza(from)) == cPieza.REY)
        return cTypes.TipoMovimiento(m) == cMovType.ENROQUE || 0 == (AtaquesA(cTypes.GetToCasilla(m)) & PiezasColor(cTypes.Contrario(us)));


      return 0 == pinned
            || 0 == (pinned & cBitBoard.m_nCasillas[from])
            || cBitBoard.Alineado(from, cTypes.GetToCasilla(m), GetRey(us)) != 0;
    }


    //--------------------------------------------------------------------------------------------------------------------------------
    public bool IsPseudoLegalMov(mov m)
    {
      color us = m_clrActivo;
      sq from = cTypes.GetFromCasilla(m);
      sq to = cTypes.GetToCasilla(m);
      pieza pc = GetPiezaMovida(m);

      if (cTypes.TipoMovimiento(m) != cMovType.NORMAL)
        return (new cReglas(this, cMovType.LEGAL)).Exist(m);

      if (cTypes.GetPromocion(m) - 2 != cPieza.NAN)
        return false;


      if (pc == cPieza.NAN || cTypes.GetColor(pc) != us)
        return false;

      if ((PiezasColor(us) & cBitBoard.m_nCasillas[to]) != 0)
        return false;

      if (cTypes.TipoPieza(pc) == cPieza.PEON)
      {


        if (cTypes.Fila(to) == cTypes.FilaProximaFromFila(us, FILA.F8))
          return false;
        if (0 == (AtaquesDePeon(from, us) & PiezasColor(cTypes.Contrario(us)) & cBitBoard.m_nCasillas[to])
            && !((from + cTypes.AtaquePeon(us) == to) && CasillaVacia(to))
            && !((from + 2 * cTypes.AtaquePeon(us) == to)
                 && (cTypes.Fila(from) == cTypes.FilaProximaFromFila(us, FILA.F2))
                 && CasillaVacia(to)
                 && CasillaVacia(to - cTypes.AtaquePeon(us))))
          return false;
      }
      else
        if (0 == (AtaquesDePieza(pc, from) & cBitBoard.m_nCasillas[to]))
          return false;

      if (Jaques() != 0)
      {
        if (cTypes.TipoPieza(pc) != cPieza.REY)
        {

          if (cBitBoard.MayorQue(Jaques()))
            return false;

          if (0 == ((cBitBoard.Entre(cBitBoard.LSB(Jaques()), GetRey(us)) | Jaques()) & cBitBoard.m_nCasillas[to]))
            return false;
        }


        else if ((AtaquesA(to, Piezas() ^ cBitBoard.m_nCasillas[from]) & PiezasColor(cTypes.Contrario(us))) != 0)
          return false;
      }
      return true;
    }

    //--------------------------------------------------------------------------------------------------------------------------------
    public bool IsJaque(mov m, cInfoJaque ci)
    {
      sq from = cTypes.GetFromCasilla(m);
      sq to = cTypes.GetToCasilla(m);
      type pt = cTypes.TipoPieza(GetPieza(from));

      if ((ci.m_Jaque[pt] & cBitBoard.m_nCasillas[to]) != 0)
        return true;

      if (ci.m_Candidatas != 0
          && (ci.m_Candidatas & cBitBoard.m_nCasillas[from]) != 0
          && 0 == cBitBoard.Alineado(from, to, ci.m_SqRey))
        return true;
      switch (cTypes.TipoMovimiento(m))
      {
        case cMovType.NORMAL:
          return false;
        case cMovType.PROMOCION:
          return (cBitBoard.Ataques(cTypes.GetPromocion(m), to, Piezas() ^ cBitBoard.m_nCasillas[from]) & cBitBoard.m_nCasillas[ci.m_SqRey]) != 0;
        case cMovType.ENPASO:
          {
            sq canCasillaPeon = cTypes.CreaCasilla(cTypes.Columna(to), cTypes.Fila(from));
            bitbrd b = (Piezas() ^ cBitBoard.m_nCasillas[from] ^ cBitBoard.m_nCasillas[canCasillaPeon]) | cBitBoard.m_nCasillas[to];
            return ((cBitBoard.AtaquesPieza(ci.m_SqRey, b, cPieza.TORRE) & PiezasColor(m_clrActivo, cPieza.DAMA, cPieza.TORRE))
                | (cBitBoard.AtaquesPieza(ci.m_SqRey, b, cPieza.ALFIL) & PiezasColor(m_clrActivo, cPieza.DAMA, cPieza.ALFIL))) != 0;
          }
        case cMovType.ENROQUE:
          {
            sq kfrom = from;
            sq rfrom = to;
            sq kto = cTypes.CasillaProxima(m_clrActivo, rfrom > kfrom ? cCasilla.G1 : cCasilla.C1);
            sq rto = cTypes.CasillaProxima(m_clrActivo, rfrom > kfrom ? cCasilla.F1 : cCasilla.D1);
            return (cBitBoard.m_PseudoAtaques[cPieza.TORRE][rto] & cBitBoard.m_nCasillas[ci.m_SqRey]) != 0
                    && (cBitBoard.AtaquesPieza(rto, (Piezas() ^ cBitBoard.m_nCasillas[kfrom] ^ cBitBoard.m_nCasillas[rfrom]) | cBitBoard.m_nCasillas[rto] | cBitBoard.m_nCasillas[kto], cPieza.TORRE) & cBitBoard.m_nCasillas[ci.m_SqRey]) != 0;
          }
        default:
          {
            return false;
          }
      }
    }


    //--------------------------------------------------------------------------------------------------------------------------------
    public void DoMov(mov m, cPosInfo newSt)
    {
      cInfoJaque ci = new cInfoJaque(this);
      DoMov(m, newSt, ci, IsJaque(m, ci));
    }

    //--------------------------------------------------------------------------------------------------------------------------------
    public void DoMov(mov m, cPosInfo newSt, cInfoJaque ci, bool moveIsCheck)
    {
      ++m_nNodos;
      hash k = m_PosInfo.key;



      newSt.pawnKey = m_PosInfo.pawnKey;
      newSt.materialKey = m_PosInfo.materialKey;
      newSt.npMaterial[0] = m_PosInfo.npMaterial[0];
      newSt.npMaterial[1] = m_PosInfo.npMaterial[1];
      newSt.castlingRights = m_PosInfo.castlingRights;
      newSt.rule50 = m_PosInfo.rule50;
      newSt.pliesFromNull = m_PosInfo.pliesFromNull;
      newSt.nCasillaPeon = m_PosInfo.nCasillaPeon;
      newSt.enCasillaPeonuare = m_PosInfo.enCasillaPeonuare;
      newSt.previous = m_PosInfo;
      m_PosInfo = newSt;

      k ^= cHashZobrist.m_Bando;


      ++m_nPly;
      ++m_PosInfo.rule50;
      ++m_PosInfo.pliesFromNull;
      color us = m_clrActivo;
      color colorVS = cTypes.Contrario(us);
      sq from = cTypes.GetFromCasilla(m);
      sq to = cTypes.GetToCasilla(m);
      pieza pc = GetPieza(from);
      type pt = cTypes.TipoPieza(pc);
      type capture = cTypes.TipoMovimiento(m) == cMovType.ENPASO ? cPieza.PEON : cTypes.TipoPieza(GetPieza(to));
      if (cTypes.TipoMovimiento(m) == cMovType.ENROQUE)
      {
        sq rfrom, rto;
        DoEnroque(from, ref to, out rfrom, out rto, true);
        capture = cPieza.NAN;
        m_PosInfo.nCasillaPeon += m_nSqPeon[us][cPieza.TORRE][rto] - m_nSqPeon[us][cPieza.TORRE][rfrom];
        k ^= cHashZobrist.m_sqPeon[us][cPieza.TORRE][rfrom] ^ cHashZobrist.m_sqPeon[us][cPieza.TORRE][rto];
      }
      if (capture != 0)
      {
        sq canCasillaPeon = to;


        if (capture == cPieza.PEON)
        {
          if (cTypes.TipoMovimiento(m) == cMovType.ENPASO)
          {
            canCasillaPeon += cTypes.AtaquePeon(colorVS);
            m_Tablero[canCasillaPeon] = cPieza.NAN;
          }
          m_PosInfo.pawnKey ^= cHashZobrist.m_sqPeon[colorVS][cPieza.PEON][canCasillaPeon];
        }
        else
          m_PosInfo.npMaterial[colorVS] -= cPosicion.m_nValPieza[cFaseJuego.FASE_MEDIOJUEGO][capture];

        BorraPieza(canCasillaPeon, colorVS, capture);

        k ^= cHashZobrist.m_sqPeon[colorVS][capture][canCasillaPeon];
        m_PosInfo.materialKey ^= cHashZobrist.m_sqPeon[colorVS][capture][m_nNumPiezas[colorVS][capture]];

        m_PosInfo.nCasillaPeon -= m_nSqPeon[colorVS][capture][canCasillaPeon];

        m_PosInfo.rule50 = 0;
      }

      k ^= cHashZobrist.m_sqPeon[us][pt][from] ^ cHashZobrist.m_sqPeon[us][pt][to];

      if (m_PosInfo.enCasillaPeonuare != cCasilla.NONE)
      {
        k ^= cHashZobrist.m_EnPaso[cTypes.Columna(m_PosInfo.enCasillaPeonuare)];
        m_PosInfo.enCasillaPeonuare = cCasilla.NONE;
      }

      if (m_PosInfo.castlingRights != 0 && (m_nMaskEnroque[from] | m_nMaskEnroque[to]) != 0)
      {
        int cr = m_nMaskEnroque[from] | m_nMaskEnroque[to];
        k ^= cHashZobrist.m_Enroque[m_PosInfo.castlingRights & cr];
        m_PosInfo.castlingRights &= ~cr;
      }

      if (cTypes.TipoMovimiento(m) != cMovType.ENROQUE)
        MuevePieza(from, to, us, pt);

      if (pt == cPieza.PEON)
      {

        if ((to ^ from) == 16
            && (AtaquesDePeon(from + cTypes.AtaquePeon(us), us) & PiezasColor(colorVS, cPieza.PEON)) != 0)
        {
          m_PosInfo.enCasillaPeonuare = (from + to) / 2;
          k ^= cHashZobrist.m_EnPaso[cTypes.Columna(m_PosInfo.enCasillaPeonuare)];
        }
        else if (cTypes.TipoMovimiento(m) == cMovType.PROMOCION)
        {
          type promotion = cTypes.GetPromocion(m);
          BorraPieza(to, us, cPieza.PEON);
          SetPieza(to, us, promotion);

          k ^= cHashZobrist.m_sqPeon[us][cPieza.PEON][to] ^ cHashZobrist.m_sqPeon[us][promotion][to];
          m_PosInfo.pawnKey ^= cHashZobrist.m_sqPeon[us][cPieza.PEON][to];
          m_PosInfo.materialKey ^= cHashZobrist.m_sqPeon[us][promotion][m_nNumPiezas[us][promotion] - 1]
                          ^ cHashZobrist.m_sqPeon[us][cPieza.PEON][m_nNumPiezas[us][cPieza.PEON]];

          m_PosInfo.nCasillaPeon += cPosicion.m_nSqPeon[us][promotion][to] - cPosicion.m_nSqPeon[us][cPieza.PEON][to];

          m_PosInfo.npMaterial[us] += cPosicion.m_nValPieza[cFaseJuego.FASE_MEDIOJUEGO][promotion];
        }

        m_PosInfo.pawnKey ^= cHashZobrist.m_sqPeon[us][cPieza.PEON][from] ^ cHashZobrist.m_sqPeon[us][cPieza.PEON][to];

        m_PosInfo.rule50 = 0;
      }

      m_PosInfo.nCasillaPeon += m_nSqPeon[us][pt][to] - m_nSqPeon[us][pt][from];

      m_PosInfo.capturedType = capture;

      m_PosInfo.key = k;

      m_PosInfo.checkersBB = 0;
      if (moveIsCheck)
      {
        if (cTypes.TipoMovimiento(m) != cMovType.NORMAL)
          m_PosInfo.checkersBB = AtaquesA(GetRey(colorVS)) & PiezasColor(us);
        else
        {

          if ((ci.m_Jaque[pt] & cBitBoard.m_nCasillas[to]) != 0)
            m_PosInfo.checkersBB |= cBitBoard.m_nCasillas[to];

          if (ci.m_Candidatas != 0 && (ci.m_Candidatas & cBitBoard.m_nCasillas[from]) != 0)
          {
            if (pt != cPieza.TORRE)
              m_PosInfo.checkersBB |= attacks_from_square_piecetype(GetRey(colorVS), cPieza.TORRE) & PiezasColor(us, cPieza.DAMA, cPieza.TORRE);
            if (pt != cPieza.ALFIL)
              m_PosInfo.checkersBB |= attacks_from_square_piecetype(GetRey(colorVS), cPieza.ALFIL) & PiezasColor(us, cPieza.DAMA, cPieza.ALFIL);
          }
        }
      }
      m_clrActivo = cTypes.Contrario(m_clrActivo);
    }

    //--------------------------------------------------------------------------------------------------------------------------------
    public void DesMov(mov m)
    {
      m_clrActivo = cTypes.Contrario(m_clrActivo);
      color us = m_clrActivo;
      sq from = cTypes.GetFromCasilla(m);
      sq to = cTypes.GetToCasilla(m);
      type pt = cTypes.TipoPieza(GetPieza(to));
      if (cTypes.TipoMovimiento(m) == cMovType.PROMOCION)
      {
        BorraPieza(to, us, cTypes.GetPromocion(m));
        SetPieza(to, us, cPieza.PEON);
        pt = cPieza.PEON;
      }
      if (cTypes.TipoMovimiento(m) == cMovType.ENROQUE)
      {
        sq rfrom, rto;
        DoEnroque(from, ref to, out rfrom, out rto, false);
      }
      else
      {
        MuevePieza(to, from, us, pt);
        if (m_PosInfo.capturedType != 0)
        {
          sq canCasillaPeon = to;
          if (cTypes.TipoMovimiento(m) == cMovType.ENPASO)
          {
            canCasillaPeon -= cTypes.AtaquePeon(us);
          }
          SetPieza(canCasillaPeon, cTypes.Contrario(us), m_PosInfo.capturedType);
        }
      }

      m_PosInfo = m_PosInfo.previous;
      --m_nPly;
    }

    //--------------------------------------------------------------------------------------------------------------------------------
    public void DoEnroque(sq from, ref sq to, out sq rfrom, out sq rto, bool Do)
    {
      bool kingSide = to > from;
      rfrom = to;
      rto = cTypes.CasillaProxima(m_clrActivo, kingSide ? cCasilla.F1 : cCasilla.D1);
      to = cTypes.CasillaProxima(m_clrActivo, kingSide ? cCasilla.G1 : cCasilla.C1);

      BorraPieza(Do ? from : to, m_clrActivo, cPieza.REY);
      BorraPieza(Do ? rfrom : rto, m_clrActivo, cPieza.TORRE);
      m_Tablero[Do ? from : to] = m_Tablero[Do ? rfrom : rto] = cPieza.NAN;
      SetPieza(Do ? to : from, m_clrActivo, cPieza.REY);
      SetPieza(Do ? rto : rfrom, m_clrActivo, cPieza.TORRE);
    }

    //--------------------------------------------------------------------------------------------------------------------------------
    public void DoNullMov(cPosInfo newSt)
    {
      newSt = m_PosInfo.getCopy();
      newSt.previous = m_PosInfo;
      m_PosInfo = newSt;
      if (m_PosInfo.enCasillaPeonuare != cCasilla.NONE)
      {
        m_PosInfo.key ^= cHashZobrist.m_EnPaso[cTypes.Columna(m_PosInfo.enCasillaPeonuare)];
        m_PosInfo.enCasillaPeonuare = cCasilla.NONE;
      }
      m_PosInfo.key ^= cHashZobrist.m_Bando;
      ++m_PosInfo.rule50;
      m_PosInfo.pliesFromNull = 0;
      m_clrActivo = cTypes.Contrario(m_clrActivo);
    }
    public void undo_null_move()
    {
      m_PosInfo = m_PosInfo.previous;
      m_clrActivo = cTypes.Contrario(m_clrActivo);
    }

    //--------------------------------------------------------------------------------------------------------------------------------
    //-- https://chessprogramming.wikispaces.com/SEE+-+The+Swap+Algorithm
    public int SEEReducido(mov m)
    {
      //if (cPosicion.PieceValue[cFaseJuego.FASE_MEDIOJUEGO][GetPiezaMovida(m)] <= cPosicion.PieceValue[cFaseJuego.FASE_MEDIOJUEGO][GetPieza(cTypes.GetToCasilla(m))])
      // return cValoresJuego.GANA;
      return SEE(m);
    }

    //--------------------------------------------------------------------------------------------------------------------------------
    public int SEE(mov m)
    {
      sq from, to;
      bitbrd occupied, attackers, stmAttackers;
      int[] swapList = new int[32];
      int slIndex = 1;
      type captured;
      color stm;
      from = cTypes.GetFromCasilla(m);
      to = cTypes.GetToCasilla(m);
      swapList[0] = m_nValPieza[cFaseJuego.FASE_MEDIOJUEGO][GetPieza(to)];
      stm = cTypes.GetColor(GetPieza(from));
      occupied = Piezas() ^ cBitBoard.m_nCasillas[from];

      if (cTypes.TipoMovimiento(m) == cMovType.ENROQUE)
        return cValoresJuego.CERO;
      if (cTypes.TipoMovimiento(m) == cMovType.ENPASO)
      {
        occupied ^= cBitBoard.m_nCasillas[to - cTypes.AtaquePeon(stm)];
        swapList[0] = m_nValPieza[cFaseJuego.FASE_MEDIOJUEGO][cPieza.PEON];
      }

      attackers = AtaquesA(to, occupied) & occupied;

      stm = cTypes.Contrario(stm);
      stmAttackers = attackers & PiezasColor(stm);
      if (0 == stmAttackers)
        return swapList[0];

      captured = cTypes.TipoPieza(GetPieza(from));
      do
      {

        swapList[slIndex] = -swapList[slIndex - 1] + cPosicion.m_nValPieza[cFaseJuego.FASE_MEDIOJUEGO][captured];

        captured = GetTipoAtaqueMinimo(byTypeBB, to, stmAttackers, ref occupied, ref attackers, cPieza.PEON);

        if (captured == cPieza.REY)
        {
          if (stmAttackers == attackers)
            ++slIndex;
          break;
        }
        stm = cTypes.Contrario(stm);
        stmAttackers = attackers & PiezasColor(stm);
        ++slIndex;
      } while (stmAttackers != 0);


      while (--slIndex != 0)
        swapList[slIndex - 1] = Math.Min(-swapList[slIndex], swapList[slIndex - 1]);
      return swapList[0];
    }

    //--------------------------------------------------------------------------------------------------------------------------------
    public bool IsTablas()
    {
      if (0 == GetNumPiezas(cPieza.PEON)
            && (MaterialPieza(cColor.BLANCO) + MaterialPieza(cColor.NEGRO) <= cValoresJuego.ALFIL_MJ))
        return true;
      if (m_PosInfo.rule50 > 99 && (0 == Jaques() || (new cReglas(this, cMovType.LEGAL)).Size() != 0))
        return true;
      cPosInfo stp = m_PosInfo;
      for (int i = 2, e = Math.Min(m_PosInfo.rule50, m_PosInfo.pliesFromNull); i <= e; i += 2)
      {
        stp = stp.previous.previous;
        if (stp.key == m_PosInfo.key)
          return true;
      }
      return false;
    }

    //--------------------------------------------------------------------------------------------------------------------------------
    public bool IsOkPos(ref int step)
    {
      const bool all = false;
      const bool testBitboards = all || false;
      const bool testState = all || false;
      const bool testKingCount = all || false;
      const bool testKingCapture = all || false;
      const bool testPieceCounts = all || false;
      const bool testPieceList = all || false;
      const bool testCastlingSquares = all || false;
      if (step >= 0)
        step = 1;
      if ((m_clrActivo != cColor.BLANCO && m_clrActivo != cColor.NEGRO)
        || GetPieza(GetRey(cColor.BLANCO)) != cPieza.REY_BLANCO
        || GetPieza(GetRey(cColor.NEGRO)) != cPieza.REY_NEGRO
        || (CasillaEnPaso() != cCasilla.NONE
            && cTypes.FilaProxima(m_clrActivo, CasillaEnPaso()) != FILA.F6))
        return false;
      if (step > 0 && testBitboards)
      {
        ++step;

        if ((PiezasColor(cColor.BLANCO) & PiezasColor(cColor.NEGRO)) != 0)
          return false;


        if ((PiezasColor(cColor.BLANCO) | PiezasColor(cColor.NEGRO)) != Piezas())
          return false;
        for (type p1 = cPieza.PEON; p1 <= cPieza.REY; ++p1)
          for (type p2 = cPieza.PEON; p2 <= cPieza.REY; ++p2)
            if (p1 != p2 && (GetNumPiezas(p1) & GetNumPiezas(p2)) != 0)
              return false;
      }
      if (step > 0 && testState)
      {
        ++step;
        cPosInfo si = new cPosInfo();
        SetEstado(si);
        if (m_PosInfo.key != si.key
            || m_PosInfo.pawnKey != si.pawnKey
            || m_PosInfo.materialKey != si.materialKey
            || m_PosInfo.npMaterial[cColor.BLANCO] != si.npMaterial[cColor.BLANCO]
            || m_PosInfo.npMaterial[cColor.NEGRO] != si.npMaterial[cColor.NEGRO]
            || m_PosInfo.nCasillaPeon != si.nCasillaPeon
            || m_PosInfo.checkersBB != si.checkersBB)
          return false;
      }
      if (step > 0 && testKingCount)
      {
        ++step;
        int[] kingCount = new int[2];
        for (sq s = cCasilla.A1; s <= cCasilla.H8; s++)
          if (cTypes.TipoPieza(GetPieza(s)) == cPieza.REY)
            kingCount[cTypes.GetColor(GetPieza(s))]++;
        if (kingCount[0] != 1 || kingCount[1] != 1)
          return false;
      }
      if (step > 0 && testKingCapture)
      {
        ++step;
        if ((AtaquesA(GetRey(cTypes.Contrario(m_clrActivo))) & PiezasColor(m_clrActivo)) != 0)
          return false;
      }
      if (step > 0 && testPieceCounts)
      {
        ++step;
        for (color c = cColor.BLANCO; c <= cColor.NEGRO; ++c)
          for (type pt = cPieza.PEON; pt <= cPieza.REY; ++pt)
            if (m_nNumPiezas[c][pt] != cBitBoard.Count(PiezasColor(c, pt)))
              return false;
      }
      if (step > 0 && testPieceList)
      {
        ++step;
        for (color c = cColor.BLANCO; c <= cColor.NEGRO; ++c)
          for (type pt = cPieza.PEON; pt <= cPieza.REY; ++pt)
            for (int i = 0; i < m_nNumPiezas[c][pt]; ++i)
              if (m_Tablero[m_lstPiezas[c][pt][i]] != cTypes.CreaPieza(c, pt)
                  || m_nIndex[m_lstPiezas[c][pt][i]] != i)
                return false;
      }
      if (step > 0 && testCastlingSquares)
      {
        ++step;
        for (color c = cColor.BLANCO; c <= cColor.NEGRO; ++c)
          for (enroque s = cEnroque.LADO_REY; s <= cEnroque.LADO_DAMA; s = (enroque)(s + 1))
          {
            if (0 == PosibleEnrocar(cTypes.GetCasillaEnroque(c, s)))
              continue;
            if ((m_nMaskEnroque[GetRey(c)] & cTypes.GetCasillaEnroque(c, s)) != cTypes.GetCasillaEnroque(c, s)
                || GetPieza(m_nSqEnroqueTorre[cTypes.GetCasillaEnroque(c, s)]) != cTypes.CreaPieza(c, cPieza.TORRE)
                || m_nMaskEnroque[m_nSqEnroqueTorre[cTypes.GetCasillaEnroque(c, s)]] != (cTypes.GetCasillaEnroque(c, s)))
              return false;
          }
      }
      return true;
    }

    public bool IsOkPos()
    {
      int junk = 0;
      return IsOkPos(ref junk);
    }
  }

  //---------------------------------------------------------------------------------------------------------
  //---------------------------------------------------------------------------------------------------------
  public class cInfoJaque
  {
    public bitbrd m_Candidatas;
    public bitbrd m_Clavadas;
    public bitbrd[] m_Jaque = new bitbrd[cPieza.SIZE];
    public sq m_SqRey;

    //---------------------------------------------------------------------------------------------------------
    public cInfoJaque(cPosicion pos)
    {
      color colorVS = cTypes.Contrario(pos.ColorMueve());
      m_SqRey = pos.GetRey(colorVS);
      m_Clavadas = pos.pinned_pieces(pos.ColorMueve());
      m_Candidatas = pos.CandidatosJaque();
      m_Jaque[cPieza.PEON] = pos.AtaquesDePeon(m_SqRey, colorVS);
      m_Jaque[cPieza.CABALLO] = pos.attacks_from_square_piecetype(m_SqRey, cPieza.CABALLO);
      m_Jaque[cPieza.ALFIL] = pos.attacks_from_square_piecetype(m_SqRey, cPieza.ALFIL);
      m_Jaque[cPieza.TORRE] = pos.attacks_from_square_piecetype(m_SqRey, cPieza.TORRE);
      m_Jaque[cPieza.DAMA] = m_Jaque[cPieza.ALFIL] | m_Jaque[cPieza.TORRE];
      m_Jaque[cPieza.REY] = 0;
    }
  }

  //---------------------------------------------------------------------------------------------------------
  //---------------------------------------------------------------------------------------------------------
  public class cPosInfo
  {
    public hash pawnKey, materialKey;
    public val[] npMaterial = new val[cColor.SIZE];
    public int castlingRights, rule50, pliesFromNull;
    public pnt nCasillaPeon;
    public sq enCasillaPeonuare;
    public hash key;
    public bitbrd checkersBB;
    public type capturedType;
    public cPosInfo previous;

    //---------------------------------------------------------------------------------------------------------
    public cPosInfo getCopy()
    {
      cPosInfo si = new cPosInfo();
      si.pawnKey = pawnKey;
      si.materialKey = materialKey;
      si.npMaterial[0] = npMaterial[0];
      si.npMaterial[1] = npMaterial[1];
      si.castlingRights = castlingRights;
      si.rule50 = rule50;
      si.pliesFromNull = pliesFromNull;
      si.nCasillaPeon = nCasillaPeon;
      si.enCasillaPeonuare = enCasillaPeonuare;
      si.key = key;
      si.checkersBB = checkersBB;
      si.capturedType = capturedType;
      si.previous = previous;
      return si;
    }

    //---------------------------------------------------------------------------------------------------------
    public void Clear()
    {
      pawnKey = 0;
      materialKey = 0;
      npMaterial[0] = 0;
      npMaterial[1] = 0;
      castlingRights = 0;
      rule50 = 0;
      pliesFromNull = 0;
      nCasillaPeon = 0;
      enCasillaPeonuare = 0;
      key = 0;
      checkersBB = 0;
      capturedType = 0;
      previous = null;
    }
  }
}
