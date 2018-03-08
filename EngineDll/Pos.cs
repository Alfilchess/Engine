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
  public partial class cPosicion
  {
    private pieza[] m_Tablero = new pieza[cCasilla.ESCAQUES];
    private bitbrd[] byTypeBB = new bitbrd[cPieza.SIZE];
    private bitbrd[] byColorBB = new bitbrd[cColor.SIZE];
    private int[][] m_nNumPiezas = new int[cColor.SIZE][] { new int[cPieza.SIZE], new int[cPieza.SIZE] };
    public sq[][][] m_lstPiezas = new sq[cColor.SIZE][][] {
      new sq[cPieza.SIZE][] { new sq[cPieza.MAXNUM], new sq[cPieza.MAXNUM], new sq[cPieza.MAXNUM], new sq[cPieza.MAXNUM], new sq[cPieza.MAXNUM], new sq[cPieza.MAXNUM], new sq[cPieza.MAXNUM], new sq[cPieza.MAXNUM] },
      new sq[cPieza.SIZE][] { new sq[cPieza.MAXNUM], new sq[cPieza.MAXNUM], new sq[cPieza.MAXNUM], new sq[cPieza.MAXNUM], new sq[cPieza.MAXNUM], new sq[cPieza.MAXNUM], new sq[cPieza.MAXNUM], new sq[cPieza.MAXNUM] } };
    private int[] m_nIndex = new int[cCasilla.ESCAQUES];

    private bool[] m_Obstaculos = new bool[cCasilla.ESCAQUES];
    private bitbrd[] m_TesorosBB = new bitbrd[cCasilla.ESCAQUES];
    private bool[] m_Tesoros = new bool[cCasilla.ESCAQUES];
    public cMission[] m_Mission = new cMission[Types.cColor.SIZE];
    public bool[] m_bEnPasoAceptado = new bool[Types.cColor.SIZE];
    public int[][] m_n2SqPawn = new int[Types.cColor.SIZE][] {
      new int[8]{-1, -1, -1, -1, -1, -1, -1, -1}, new int[8]{ -1, -1, -1, -1, -1, -1, -1, -1}};
    public int[][] m_nPromotionLine = new int[Types.cColor.SIZE][] {
      new int[8]{-1, -1, -1, -1, -1, -1, -1, -1}, new int[8]{ -1, -1, -1, -1, -1, -1, -1, -1}};

    private int[] m_nMaskEnroque = new int[cCasilla.ESCAQUES];
    private sq[] m_nSqEnroqueTorre = new sq[cEnroque.ENROQUE_DERECHA];
    private bitbrd[] m_nEnroques = new bitbrd[cEnroque.ENROQUE_DERECHA];
    private cPosInfo m_PosInfoInicial = new cPosInfo();
    private UInt64 m_nNodos;
    private int m_nPly;
    private color m_clrActivo;
    private cThread m_ActiveThread;
    public cPosInfo m_PosInfo;
    
    private bool m_bIsChess960;
    public static val[][] m_nValPieza = new val[cFaseJuego.NAN][]{
            new val[cPieza.COLORES]{ cValoresJuego.CERO, cValoresJuego.PEON_MJ, cValoresJuego.CABALLO_MJ, cValoresJuego.ALFIL_MJ, cValoresJuego.TORRE_MJ, cValoresJuego.DAMA_MJ, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            new val[cPieza.COLORES]{ cValoresJuego.CERO, cValoresJuego.PEON_FINAL, cValoresJuego.CABALLO_FINAL, cValoresJuego.ALFIL_FINAL, cValoresJuego.TORRE_FINAL, cValoresJuego.DAMA_FINAL, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } };
    public static pnt[][][] m_nSqPeon = new pnt[cColor.SIZE][][];
    
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
    public cPosicion(string f, bool c960 = false, cThread t = null)
    {
      m_Mission[cColor.BLANCO] = new cMission(cColor.BLANCO);
      m_Mission[cColor.NEGRO] = new cMission(cColor.NEGRO);

      m_bEnPasoAceptado[cColor.BLANCO] = true;
      m_bEnPasoAceptado[cColor.NEGRO] = true;

      m_n2SqPawn[cColor.BLANCO][0] = 2;
      m_n2SqPawn[cColor.NEGRO][0] = 7;
      
      m_nPromotionLine[cColor.BLANCO][0] = 8;
      m_nPromotionLine[cColor.NEGRO][0] = 1;

      if (f.Length > 80)
        SetFENChessaria(f, c960, t);
      else
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
      if (cTypes.IsCasillaOcupable(s))
        return m_Tablero[s];
      else
        return cPieza.NAN;
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
    public bitbrd AtaquesDesdeTipoDePieza(sq s, type Pt)
    {
      return (Pt == cPieza.ALFIL || Pt == cPieza.TORRE) ? cBitBoard.AtaquesPieza(s, Piezas(), Pt)
           : (Pt == cPieza.DAMA) ? AtaquesDesdeTipoDePieza(s, cPieza.TORRE) | AtaquesDesdeTipoDePieza(s, cPieza.ALFIL)
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
      if (m_Mission[m_clrActivo].m_nMissionType == cMission.CHECKMATE || m_Mission[cTypes.Contrario(m_clrActivo)].m_nMissionType == cMission.CHECKMATE)
        return m_PosInfo.checkersBB;
      else
        return 0;
    }

    //---------------------------------------------------------------------------------------------------------
    public bitbrd CandidatosJaque()
    {
      return Jaques(m_clrActivo, cTypes.Contrario(m_clrActivo));
    }

    //---------------------------------------------------------------------------------------------------------
    public bitbrd PiezasClavadas(color c)
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

    public hash ClaveHashMaterial()
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
    public void SetPieza(sq s, color c, type pt, bool bSetPos = false, bool bSetMision = false)
    {
      if (m_Tesoros[s])
        m_Mission[ColorMueve()].SetPiezaDomination(s, c, cPieza.TESORO, bSetPos);
      else
        m_Mission[ColorMueve()].SetPiezaDomination(s, c, pt, bSetPos);
      
      if (m_Tesoros[s])
      {
        if (m_TesorosBB[s] == 0)
          m_Obstaculos[s] = true;
      }

      m_TesorosBB[s] |= cBitBoard.m_nCasillas[s];

      m_Tablero[s] = cTypes.CreaPieza(c, pt);

      byTypeBB[cPieza.NAN] |= cBitBoard.m_nCasillas[s];
      if (pt != cPieza.NAN)
        byTypeBB[pt] |= cBitBoard.m_nCasillas[s];
      byColorBB[c] |= cBitBoard.m_nCasillas[s];
      m_nIndex[s] = m_nNumPiezas[c][pt]++;
      m_lstPiezas[c][pt][m_nIndex[s]] = s;
    }

    //-------------------------------------------------------------------------------------------------------------
    public void MuevePieza(sq from, sq to, color c, type pt, bool bSetPos = false)
    {
      bitbrd from_to_bb = cBitBoard.m_nCasillas[from] ^ cBitBoard.m_nCasillas[to];

      if (m_Tesoros[from])
        m_Mission[ColorMueve()].BorraPiezaDomination(from, c, cPieza.TESORO, bSetPos);
      else
        m_Mission[ColorMueve()].BorraPiezaDomination(from, c, pt, bSetPos);

      byTypeBB[cPieza.NAN] ^= from_to_bb;
      byTypeBB[pt] ^= from_to_bb;
      byColorBB[c] ^= from_to_bb;
      m_Tablero[from] = cPieza.NAN;
      m_Tablero[to] = cTypes.CreaPieza(c, pt);
      m_nIndex[to] = m_nIndex[from];
      m_lstPiezas[c][pt][m_nIndex[to]] = to;

      if (m_Tesoros[to])
        m_Mission[ColorMueve()].SetPiezaDomination(to, c, cPieza.TESORO, bSetPos);
      else
        m_Mission[ColorMueve()].SetPiezaDomination(to, c, pt, bSetPos);
    }

    //-------------------------------------------------------------------------------------------------------------
    public void BorraPieza(sq s, color c, type pt, bool bSetPos = false, bool bSetMision = false)
    {
      m_TesorosBB[s] ^= cBitBoard.m_nCasillas[s];

      if (m_Tesoros[s])
        m_Mission[ColorMueve()].BorraPiezaDomination(s, c, cPieza.TESORO, bSetPos);
      else
        m_Mission[ColorMueve()].BorraPiezaDomination(s, c, pt, bSetPos);

      if (m_Tesoros[s] && m_TesorosBB[s] == 0)
      {
        if (m_TesorosBB[s] == 0)
        {
          m_Obstaculos[s] = false;
          if (bSetPos)
            m_Tesoros[s] = false;
        }
      }
      
      if (IsTesoro(s, c))
      {
        if (m_TesorosBB[s] == 0)
          m_Obstaculos[s] = false;
      }
      
      byTypeBB[cPieza.NAN] ^= cBitBoard.m_nCasillas[s];
      byTypeBB[pt] ^= cBitBoard.m_nCasillas[s];
      byColorBB[c] ^= cBitBoard.m_nCasillas[s];

      if (m_nNumPiezas[c][pt] > 0)
      {
        sq lastSquare = m_lstPiezas[c][pt][--m_nNumPiezas[c][pt]];
        m_nIndex[lastSquare] = m_nIndex[s];
        m_lstPiezas[c][pt][m_nIndex[lastSquare]] = lastSquare;
      }
      m_lstPiezas[c][pt][m_nNumPiezas[c][pt]] = cCasilla.NONE;
    }

    //-------------------------------------------------------------------------------------------------------------
    public hash GetClaveHashExclusion()
    {
      return m_PosInfo.key ^ cHashEstaticas.m_Exclusion;
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
      cAleatorio rk = new cAleatorio();
      for (color c = cColor.BLANCO; c <= cColor.NEGRO; ++c)
      {
        cHashEstaticas.m_sqPeon[c] = new hash[cPieza.SIZE][];
        for (type pt = cPieza.PEON; pt <= cPieza.MAXSIZE; ++pt)
        {
          cHashEstaticas.m_sqPeon[c][pt] = new hash[64];
          for (sq s = cCasilla.A1; s <= cCasilla.H8; ++s)
            cHashEstaticas.m_sqPeon[c][pt][s] = rk.GetRand();
        }
      }

      for (colum f = COLUMNA.A; f <= COLUMNA.H; ++f)
        cHashEstaticas.m_EnPaso[f] = rk.GetRand();

      for (int cf = cEnroque.NO_ENROQUE; cf <= cEnroque.CUALQUIER_ENROQUE; ++cf)
      {
        bitbrd b = (bitbrd)cf;
        while (b != 0)
        {
          hash k = cHashEstaticas.m_Enroque[1UL << cBitBoard.GetLSB(ref b)];
          cHashEstaticas.m_Enroque[cf] ^= k != 0 ? k : rk.GetRand();
        }
      }

      cHashEstaticas.m_Bando = rk.GetRand();
      cHashEstaticas.m_Exclusion = rk.GetRand();
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
      for (type pt = cPieza.PEON; pt <= cPieza.MAXSIZE; ++pt)
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
      Array.Copy(pos.m_Obstaculos, m_Obstaculos, cCasilla.ESCAQUES);
      Array.Copy(pos.m_Tesoros, m_Tesoros, cCasilla.ESCAQUES);
      Array.Copy(pos.m_TesorosBB, m_TesorosBB, cCasilla.ESCAQUES);
      Array.Copy(pos.byTypeBB, byTypeBB, cPieza.SIZE);
      Array.Copy(pos.byColorBB, byColorBB, cColor.SIZE);
      Array.Copy(pos.m_nIndex, m_nIndex, cCasilla.ESCAQUES);
      Array.Copy(pos.m_nMaskEnroque, m_nMaskEnroque, cCasilla.ESCAQUES);
      m_nNodos = 0;
      m_clrActivo = pos.m_clrActivo;
      m_nPly = pos.m_nPly;
      m_bIsChess960 = pos.m_bIsChess960;
      m_PosInfoInicial = pos.m_PosInfo.getCopy();
      m_Mission[cColor.BLANCO] = pos.m_Mission[cColor.BLANCO];
      m_Mission[cColor.NEGRO] = pos.m_Mission[cColor.NEGRO];
      m_bEnPasoAceptado[cColor.BLANCO] = pos.m_bEnPasoAceptado[cColor.BLANCO];
      m_bEnPasoAceptado[cColor.NEGRO] = pos.m_bEnPasoAceptado[cColor.NEGRO];
      m_n2SqPawn[cColor.BLANCO] = pos.m_n2SqPawn[cColor.BLANCO];
      m_n2SqPawn[cColor.NEGRO] = pos.m_n2SqPawn[cColor.NEGRO];
      m_nPromotionLine[cColor.BLANCO] = pos.m_nPromotionLine[cColor.BLANCO];
      m_nPromotionLine[cColor.NEGRO] = pos.m_nPromotionLine[cColor.NEGRO];

      m_PosInfo = m_PosInfoInicial;
      m_ActiveThread = pos.m_ActiveThread;
      Array.Copy(pos.m_nSqEnroqueTorre, m_nSqEnroqueTorre, cEnroque.ENROQUE_DERECHA);
      Array.Copy(pos.m_nEnroques, m_nEnroques, cEnroque.ENROQUE_DERECHA);
      Array.Copy(pos.m_nNumPiezas[cColor.BLANCO], m_nNumPiezas[cColor.BLANCO], cPieza.SIZE);
      Array.Copy(pos.m_nNumPiezas[cColor.NEGRO], m_nNumPiezas[cColor.NEGRO], cPieza.SIZE);
      Array.Copy(pos.m_lstPiezas[cColor.BLANCO][cPieza.NAN], m_lstPiezas[cColor.BLANCO][cPieza.NAN], cPieza.MAXNUM);
      Array.Copy(pos.m_lstPiezas[cColor.BLANCO][cPieza.PEON], m_lstPiezas[cColor.BLANCO][cPieza.PEON], cPieza.MAXNUM);
      Array.Copy(pos.m_lstPiezas[cColor.BLANCO][cPieza.CABALLO], m_lstPiezas[cColor.BLANCO][cPieza.CABALLO], cPieza.MAXNUM);
      Array.Copy(pos.m_lstPiezas[cColor.BLANCO][cPieza.ALFIL], m_lstPiezas[cColor.BLANCO][cPieza.ALFIL], cPieza.MAXNUM);
      Array.Copy(pos.m_lstPiezas[cColor.BLANCO][cPieza.TORRE], m_lstPiezas[cColor.BLANCO][cPieza.TORRE], cPieza.MAXNUM);
      Array.Copy(pos.m_lstPiezas[cColor.BLANCO][cPieza.DAMA], m_lstPiezas[cColor.BLANCO][cPieza.DAMA], cPieza.MAXNUM);
      Array.Copy(pos.m_lstPiezas[cColor.BLANCO][cPieza.REY], m_lstPiezas[cColor.BLANCO][cPieza.REY], cPieza.MAXNUM);
      Array.Copy(pos.m_lstPiezas[cColor.BLANCO][cPieza.TESORO], m_lstPiezas[cColor.BLANCO][cPieza.TESORO], cPieza.MAXNUM);
      Array.Copy(pos.m_lstPiezas[cColor.NEGRO][cPieza.NAN], m_lstPiezas[cColor.NEGRO][cPieza.NAN], cPieza.MAXNUM);
      Array.Copy(pos.m_lstPiezas[cColor.NEGRO][cPieza.PEON], m_lstPiezas[cColor.NEGRO][cPieza.PEON], cPieza.MAXNUM);
      Array.Copy(pos.m_lstPiezas[cColor.NEGRO][cPieza.CABALLO], m_lstPiezas[cColor.NEGRO][cPieza.CABALLO], cPieza.MAXNUM);
      Array.Copy(pos.m_lstPiezas[cColor.NEGRO][cPieza.ALFIL], m_lstPiezas[cColor.NEGRO][cPieza.ALFIL], cPieza.MAXNUM);
      Array.Copy(pos.m_lstPiezas[cColor.NEGRO][cPieza.TORRE], m_lstPiezas[cColor.NEGRO][cPieza.TORRE], cPieza.MAXNUM);
      Array.Copy(pos.m_lstPiezas[cColor.NEGRO][cPieza.DAMA], m_lstPiezas[cColor.NEGRO][cPieza.DAMA], cPieza.MAXNUM);
      Array.Copy(pos.m_lstPiezas[cColor.NEGRO][cPieza.REY], m_lstPiezas[cColor.NEGRO][cPieza.REY], cPieza.MAXNUM);
      Array.Copy(pos.m_lstPiezas[cColor.NEGRO][cPieza.TESORO], m_lstPiezas[cColor.NEGRO][cPieza.TESORO], cPieza.MAXNUM);
    }

    //--------------------------------------------------------------------------------------------------------------------------------
    public void Clear()
    {
      m_nNodos = 0;
      m_clrActivo = 0;
      m_nPly = 0;
      m_bIsChess960 = false;
      Array.Clear(m_Tablero, 0, cCasilla.ESCAQUES);
      Array.Clear(m_Obstaculos, 0, cCasilla.ESCAQUES);
      Array.Clear(m_Tesoros, 0, cCasilla.ESCAQUES);
      Array.Clear(m_TesorosBB, 0, cCasilla.ESCAQUES);
      Array.Clear(byTypeBB, 0, cPieza.SIZE);
      Array.Clear(byColorBB, 0, cColor.SIZE);
      Array.Clear(m_nIndex, 0, cCasilla.ESCAQUES);
      Array.Clear(m_nMaskEnroque, 0, cCasilla.ESCAQUES);
      Array.Clear(m_nNumPiezas[cColor.BLANCO], 0, cPieza.SIZE);
      Array.Clear(m_nNumPiezas[cColor.NEGRO], 0, cPieza.SIZE);
      Array.Clear(m_nSqEnroqueTorre, 0, cEnroque.ENROQUE_DERECHA);
      Array.Clear(m_nEnroques, 0, cEnroque.ENROQUE_DERECHA);
      m_PosInfoInicial.Clear();
      m_Mission[cColor.BLANCO].Clear();
      m_Mission[cColor.NEGRO].Clear();

      m_bEnPasoAceptado[cColor.BLANCO] = true;
      m_bEnPasoAceptado[cColor.NEGRO] = true;

      m_n2SqPawn[cColor.BLANCO][0] = 2;
      m_n2SqPawn[cColor.NEGRO][0] = 7;

      m_nPromotionLine[cColor.BLANCO][0] = 8;
      m_nPromotionLine[cColor.NEGRO][0] = 1;

      m_PosInfoInicial.enCasillaPeonuare = cCasilla.NONE;
      m_PosInfo = m_PosInfoInicial;
      for (int i = 0; i < cPieza.SIZE; i++)
        //for (int j = 0; j < cPieza.SIZE * 2; ++j)
        for (int j = 0; j < cPieza.MAXNUM; j++)
        {
          m_lstPiezas[cColor.BLANCO][i][j] = m_lstPiezas[cColor.NEGRO][i][j] = cCasilla.NONE;
        }
    }

    //----------------------------------------------------------------------------------------------   
    public string DibujaTablero()
    {
      StringBuilder ss = new StringBuilder();
      if (cSearch.RootMoves.Count > 0)
      {
        mov m = cSearch.RootMoves[0].m_PV[0];
        if (m != 0)
        {
          ss.Append(cTypes.LF + "Mov: " + (m_clrActivo == cColor.NEGRO ? ".." : ""));
          ss.Append(cUci.GetMovimiento(m, m_bIsChess960));
        }
      }
      ss.Append(cTypes.LF + " +---+---+---+---+---+---+---+---+" + cTypes.LF);

      const string PieceToChar = " PNBRQK pnbrqk";

      for (fila r = FILA.F8; r >= FILA.F1; --r)
      {
        for (colum f = COLUMNA.A; f <= COLUMNA.H; ++f)
        {
          sq casilla = cTypes.CreaCasilla(f, r);
          if (IsTesoro(casilla, cColor.NEGRO) == true && IsTesoro(casilla, cColor.BLANCO) == true)
            ss.Append(" | " + 'C');
          else if (IsTesoro(casilla, cColor.BLANCO) == true)
            ss.Append(" | " + 'D');
          else if (IsTesoro(casilla, cColor.NEGRO) == true)
            ss.Append(" | " + 'E');
          else if (IsWall(casilla) == true)
            ss.Append(" | " + 'W');
          else if (IsHole(casilla) == true)
            ss.Append(" | " + 'O');
          else
          {
            int nTipo = cTypes.TipoPieza(GetPieza(casilla));
            int nColor = cTypes.GetColor(m_Tablero[casilla]) == cColor.BLANCO ? 0 : 1;
            ss.Append(" | " + PieceToChar[nTipo + (7 * nColor)]);
          }
        }
        ss.Append(" |" + cTypes.LF + " +---+---+---+---+---+---+---+---+" + cTypes.LF);
      }

      //ss.Append(cTypes.LF + "Fen: " + fen() + cTypes.LF + "Key: " + st.key.ToString("X").ToUpper().PadLeft(16, '0') + cTypes.LF + "Checkers: ");

      //for(bitbrd b = Jaques(); b != 0;)
      //  ss.Append(cTypes.square_to_string(cBitBoard.GetLSB(ref b)) + " ");

      ss.Append(cTypes.LF + "Legales: ");
      for (cReglas listaMovimientos = new cReglas(this, cMovType.LEGAL); listaMovimientos.GetActualMov() != cMovType.MOV_NAN; ++listaMovimientos)
        ss.Append(cUci.GetMovimiento(listaMovimientos.GetActualMov(), m_bIsChess960) + " ");

      ss.Append(cTypes.LF + "Nivel: " + cMotor.m_ConfigFile.m_nNivelJuego);

      int i = 0;
      
      foreach (cRaizMov move in cSearch.RootMoves)
      {
        // bool updated = (i <= cSearch.nIndexPV);
        // val v = (updated ? cSearch.RootMoves[i].m_nVal : cSearch.RootMoves[i].m_LastVal);
        val v = (move.m_nVal == -cValoresJuego.INFINITO)? move.m_LastVal: move.m_nVal;
        i++;

        ss.Append(cTypes.LF + cUci.GetMovimiento(move.m_PV[0], IsChess960() != false));
        //ss.Append(" [" + move.m_percLevel.ToString() + "%]");
        if (Math.Abs(v) < cValoresJuego.MATE_MAXIMO)
          ss.Append(" val (" + (v * 100 / cValoresJuego.PEON_FINAL)); //-- En centipeones
        else if (Math.Abs(v) == cValoresJuego.INFINITO)
          ss.Append(" val (" + v.ToString());
        else
          ss.Append(" val (" + ((v > 0 ? cValoresJuego.MATE - v + 1 : -cValoresJuego.MATE - v) / 2));

        ss.Append(")");
      }
      ss.Append(cTypes.LF + "---");
      ss.Append(cTypes.LF + "Misión blancas: ");
      ss.Append(m_Mission[cColor.BLANCO].GetText());
      ss.Append(cTypes.LF + "Misión negras: ");
      ss.Append(m_Mission[cColor.NEGRO].GetText());
      return ss.ToString();

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
      
      while ((token = fen[ss++]) != ' ' && ss < fenStr.Length)
      {
        if (IsNum(token))
          sq += (token - '0');
        else if (token == '/')
          sq -= 16;
        else if (token == '-')
        {
          sq++;
        }

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
        sq nCasillaTorre = cCasilla.NONE;
        color c = IsLower(token) ? cColor.NEGRO : cColor.BLANCO;
        token = ToMayus(token);
        if (token == 'K')
        {
          for (nCasillaTorre = cTypes.CasillaProxima(c, cCasilla.H1); nCasillaTorre > 0 && cTypes.TipoPieza(GetPieza(nCasillaTorre)) != cPieza.TORRE; --nCasillaTorre) { }
        }
        else if (token == 'Q')
        {
          for (nCasillaTorre = cTypes.CasillaProxima(c, cCasilla.A1); nCasillaTorre > 0 && cTypes.TipoPieza(GetPieza(nCasillaTorre)) != cPieza.TORRE; --nCasillaTorre) { }
        }
        else if (token >= 'A' && token <= 'H')
        {
          nCasillaTorre = cTypes.CreaCasilla(token - 'A', cTypes.FilaProximaFromFila(c, FILA.F1));
        }
        else
        {
          continue;
        }

        if (cCasilla.E1 == GetRey(cColor.BLANCO) || cCasilla.E8 == GetRey(cColor.NEGRO))
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

      Stack<String> tokens = cUci.CreateStack(fenStr.Substring(ss));
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

      m_Mission[cColor.BLANCO].EvalActualMission(this);
      m_Mission[cColor.NEGRO].EvalActualMission(this);
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
      sq posRey = GetRey(m_clrActivo);
      if (cTypes.IsCasillaOcupable(posRey) && m_Mission[ColorMueve()].m_nMissionType == cMission.CHECKMATE)
        si.checkersBB = AtaquesA(GetRey(m_clrActivo)) & PiezasColor(cTypes.Contrario(m_clrActivo));
      for (bitbrd b = Piezas(); b != 0;)
      {
        sq s = cBitBoard.GetLSB(ref b);
        pieza pc = GetPieza(s);
        if (pc != cPieza.NAN)
        {
          color c = cTypes.GetColor(pc);
          pieza tipoPieza = cTypes.TipoPieza(pc);
          if (tipoPieza != cPieza.NAN)
          {
            si.key ^= cHashEstaticas.m_sqPeon[c][tipoPieza][s];
            si.nCasillaPeon += m_nSqPeon[cTypes.GetColor(pc)][cTypes.TipoPieza(pc)][s];
          }
        }
      }
      if (CasillaEnPaso() != cCasilla.NONE)
        si.key ^= cHashEstaticas.m_EnPaso[cTypes.Columna(CasillaEnPaso())];
      if (m_clrActivo == cColor.NEGRO)
        si.key ^= cHashEstaticas.m_Bando;
      si.key ^= cHashEstaticas.m_Enroque[m_PosInfo.castlingRights];
      for (bitbrd b = GetNumPiezas(cPieza.PEON); b != 0;)
      {
        sq s = cBitBoard.GetLSB(ref b);
        si.pawnKey ^= cHashEstaticas.m_sqPeon[cTypes.GetColor(GetPieza(s))][cPieza.PEON][s];
      }
      for (color c = cColor.BLANCO; c <= cColor.NEGRO; ++c)
        for (type pt = cPieza.PEON; pt <= cPieza.MAXSIZE; ++pt)
          for (int cnt = 0; cnt < m_nNumPiezas[c][pt]; ++cnt)
            si.materialKey ^= cHashEstaticas.m_sqPeon[c][pt][cnt];
      for (color c = cColor.BLANCO; c <= cColor.NEGRO; ++c)
        for (type pt = cPieza.CABALLO; pt <= cPieza.DAMA; ++pt)
          si.npMaterial[c] += m_nNumPiezas[c][pt] * m_nValPieza[cFaseJuego.FASE_MEDIOJUEGO][pt];
    }

    //--------------------------------------------------------------------------------------------------------------------------------
    public bitbrd Jaques(color c, color kingColor)
    {
      bitbrd b, pinners, result = 0;
      sq ksq = GetRey(kingColor);

      if (cTypes.IsCasillaOcupable(ksq) && m_Mission[c].m_nMissionType == cMission.CHECKMATE)
      {
        pinners = ((GetNumPiezas(cPieza.TORRE, cPieza.DAMA) & cBitBoard.m_PseudoAtaques[cPieza.TORRE][ksq])
               | (GetNumPiezas(cPieza.ALFIL, cPieza.DAMA) & cBitBoard.m_PseudoAtaques[cPieza.ALFIL][ksq])) & PiezasColor(cTypes.Contrario(kingColor));
        while (pinners != 0)
        {
          b = cBitBoard.Entre(ksq, cBitBoard.GetLSB(ref pinners)) & Piezas();
          if (!cBitBoard.MayorQue(b))
            result |= b & PiezasColor(c);
        }
      }
      return result;
    }

    //--------------------------------------------------------------------------------------------------------------------------------
    public bitbrd AtaquesA(sq s, UInt64 occ)
    {
      return (AtaquesDePeon(s, cColor.NEGRO) & PiezasColor(cColor.BLANCO, cPieza.PEON))
           | (AtaquesDePeon(s, cColor.BLANCO) & PiezasColor(cColor.NEGRO, cPieza.PEON))
           | (AtaquesDesdeTipoDePieza(s, cPieza.CABALLO) & GetNumPiezas(cPieza.CABALLO))
           | (cBitBoard.AtaquesPieza(s, occ, cPieza.TORRE) & GetNumPiezas(cPieza.TORRE, cPieza.DAMA))
           | (cBitBoard.AtaquesPieza(s, occ, cPieza.ALFIL) & GetNumPiezas(cPieza.ALFIL, cPieza.DAMA))
           | (AtaquesDesdeTipoDePieza(s, cPieza.REY) & GetNumPiezas(cPieza.REY));
    }

    //--------------------------------------------------------------------------------------------------------------------------------
    public bool IsLegalMov(mov m, bitbrd pinned)
    {
      color us = m_clrActivo;
      sq from = cTypes.GetFromCasilla(m);
      sq to = cTypes.GetToCasilla(m);

      if (IsObstaculo(from, false) == true || IsObstaculo(to, true) == true)
        return false;

      if (cTypes.TipoMovimiento(m) == cMovType.ENPASO)
      {
        sq ksq = GetRey(us);
        if (cTypes.IsCasillaOcupable(ksq))
        {
          sq canCasillaPeon = to - cTypes.AvancePeon(us);
          bitbrd occ = (Piezas() ^ cBitBoard.m_nCasillas[from] ^ cBitBoard.m_nCasillas[canCasillaPeon]) | cBitBoard.m_nCasillas[to];
          return 0 == (cBitBoard.AtaquesPieza(ksq, occ, cPieza.TORRE) & PiezasColor(cTypes.Contrario(us), cPieza.DAMA, cPieza.TORRE))
              && 0 == (cBitBoard.AtaquesPieza(ksq, occ, cPieza.ALFIL) & PiezasColor(cTypes.Contrario(us), cPieza.DAMA, cPieza.ALFIL));
        }
      }

      if (cTypes.TipoPieza(GetPieza(from)) == cPieza.REY && (m_Mission[us].m_nMissionType == cMission.CHECKMATE || (m_Mission[cTypes.Contrario(us)].m_nMissionType == cMission.CHECKMATE)))
        return cTypes.TipoMovimiento(m) == cMovType.ENROQUE || 0 == (AtaquesA(cTypes.GetToCasilla(m)) & PiezasColor(cTypes.Contrario(us)));

      return 0 == pinned
            || 0 == (pinned & cBitBoard.m_nCasillas[from])
            || cBitBoard.Alineado(from, cTypes.GetToCasilla(m), GetRey(us)) != 0;
    }


    //--------------------------------------------------------------------------------------------------------------------------------
    public bool IsMovPseudoLegal(mov m)
    {
      color us = m_clrActivo;
      sq from = cTypes.GetFromCasilla(m);
      sq to = cTypes.GetToCasilla(m);
      
      if (IsObstaculo(from, false) == true || IsObstaculo(to, true) == true)
        return false;

      if (cTypes.TipoMovimiento(m) != cMovType.NORMAL)
        return (new cReglas(this, cMovType.LEGAL)).Exist(m);

      if (cTypes.GetPromocion(m) - 2 != cPieza.NAN)
        return false;

      pieza pc = GetPiezaMovida(m);

      if (pc == cPieza.NAN || cTypes.GetColor(pc) != us)
        return false;

      if ((PiezasColor(us) & cBitBoard.m_nCasillas[to]) != 0)
        return false;

      if (cTypes.TipoPieza(pc) == cPieza.PEON)
      {
        if (cTypes.Fila(to) == cTypes.FilaProximaFromFila(us, FILA.F8))
          return false;
        if (0 == (AtaquesDePeon(from, us) & PiezasColor(cTypes.Contrario(us)) & cBitBoard.m_nCasillas[to]))
        {
          if (!((from + cTypes.AvancePeon(us) == to) && CasillaVacia(to)))
          {
            if (!((from + 2 * cTypes.AvancePeon(us) == to)
                && (m_Mission[us].m_bReglasCambiadas == true || cTypes.Fila(from) == cTypes.FilaProximaFromFila(us, FILA.F2))
                && CasillaVacia(to)
                && CasillaVacia(to - cTypes.AvancePeon(us))))
              return false;
          }
        }
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

          if (cTypes.IsCasillaOcupable(GetRey(us)))
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
      if (cTypes.IsCasillaOcupable(ci.m_SqRey) && m_Mission[m_clrActivo].m_nMissionType == cMission.CHECKMATE)
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
      else
        return false;
    }


    //--------------------------------------------------------------------------------------------------------------------------------
    public void DoMov(mov m, cPosInfo newSt, bool bSetPos = false)
    {
      cInfoJaque ci = new cInfoJaque(this);
      DoMov(m, newSt, ci, IsJaque(m, ci), bSetPos);
    }

    //--------------------------------------------------------------------------------------------------------------------------------
    public void DoMov(mov m, cPosInfo newSt, cInfoJaque ci, bool moveIsCheck, bool bSetPos = false)
    {
      if (m == cMovType.MOV_NULO)
      {
        DoNullMov(newSt);
        return;
      }

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
      
      k ^= cHashEstaticas.m_Bando;

      ++m_nPly;

#if DEBUG
      //string sPly = "";
      //for (int x = 0; x < m_nPly; x++)
      //  sPly += "  ";
      //cMotor.m_Consola.Print(sPly + cUci.GetMovimiento(m, false), AccionConsola.ATOMIC);
#endif
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
        k ^= cHashEstaticas.m_sqPeon[us][cPieza.TORRE][rfrom] ^ cHashEstaticas.m_sqPeon[us][cPieza.TORRE][rto];
      }
      if (capture != cPieza.NAN)
      {
        sq canCasillaPeon = to;

        if (capture == cPieza.PEON)
        {
          if (cTypes.TipoMovimiento(m) == cMovType.ENPASO)
          {
            canCasillaPeon += cTypes.AvancePeon(colorVS);
            m_Tablero[canCasillaPeon] = cPieza.NAN;
          }
          m_PosInfo.pawnKey ^= cHashEstaticas.m_sqPeon[colorVS][cPieza.PEON][canCasillaPeon];
        }
        else
          m_PosInfo.npMaterial[colorVS] -= cPosicion.m_nValPieza[cFaseJuego.FASE_MEDIOJUEGO][capture];

        if (m_Tesoros[canCasillaPeon])
          m_Mission[ColorMueve()].BorraPiezaCapture(canCasillaPeon, colorVS, cPieza.TESORO, bSetPos);
        else
          m_Mission[ColorMueve()].BorraPiezaCapture(canCasillaPeon, colorVS, m_Tablero[to], bSetPos);

        BorraPieza(canCasillaPeon, colorVS, capture, bSetPos, true);
        
        k ^= cHashEstaticas.m_sqPeon[colorVS][capture][canCasillaPeon];
        m_PosInfo.materialKey ^= cHashEstaticas.m_sqPeon[colorVS][capture][m_nNumPiezas[colorVS][capture]];

        m_PosInfo.nCasillaPeon -= m_nSqPeon[colorVS][capture][canCasillaPeon];

        m_PosInfo.rule50 = 0;
      }

      if (cHashEstaticas.m_sqPeon[us][pt] != null)
        k ^= cHashEstaticas.m_sqPeon[us][pt][from] ^ cHashEstaticas.m_sqPeon[us][pt][to];

      if (m_PosInfo.enCasillaPeonuare != cCasilla.NONE)
      {
        k ^= cHashEstaticas.m_EnPaso[cTypes.Columna(m_PosInfo.enCasillaPeonuare)];
        m_PosInfo.enCasillaPeonuare = cCasilla.NONE;
      }

      if (m_PosInfo.castlingRights != 0 && (m_nMaskEnroque[from] | m_nMaskEnroque[to]) != 0)
      {
        int cr = m_nMaskEnroque[from] | m_nMaskEnroque[to];
        k ^= cHashEstaticas.m_Enroque[m_PosInfo.castlingRights & cr];
        m_PosInfo.castlingRights &= ~cr;
      }
      
      if (cTypes.TipoMovimiento(m) != cMovType.ENROQUE)
        MuevePieza(from, to, us, pt, bSetPos);
      
      if (pt == cPieza.PEON)
      {
        if ((to ^ from) == 16
            && (AtaquesDePeon(from + cTypes.AvancePeon(us), us) & PiezasColor(colorVS, cPieza.PEON)) != 0)
        {
          m_PosInfo.enCasillaPeonuare = (from + to) / 2;
          k ^= cHashEstaticas.m_EnPaso[cTypes.Columna(m_PosInfo.enCasillaPeonuare)];
        }
        else if (cTypes.TipoMovimiento(m) == cMovType.PROMOCION)
        {
          type promotion = cTypes.GetPromocion(m);
          BorraPieza(to, us, cPieza.PEON, bSetPos);
          SetPieza(to, us, promotion, bSetPos);

          k ^= cHashEstaticas.m_sqPeon[us][cPieza.PEON][to] ^ cHashEstaticas.m_sqPeon[us][promotion][to];
          m_PosInfo.pawnKey ^= cHashEstaticas.m_sqPeon[us][cPieza.PEON][to];
          m_PosInfo.materialKey ^= cHashEstaticas.m_sqPeon[us][promotion][m_nNumPiezas[us][promotion] - 1]
                          ^ cHashEstaticas.m_sqPeon[us][cPieza.PEON][m_nNumPiezas[us][cPieza.PEON]];

          m_PosInfo.nCasillaPeon += cPosicion.m_nSqPeon[us][promotion][to] - cPosicion.m_nSqPeon[us][cPieza.PEON][to];

          m_PosInfo.npMaterial[us] += cPosicion.m_nValPieza[cFaseJuego.FASE_MEDIOJUEGO][promotion];
        }

        m_PosInfo.pawnKey ^= cHashEstaticas.m_sqPeon[us][cPieza.PEON][from] ^ cHashEstaticas.m_sqPeon[us][cPieza.PEON][to];

        m_PosInfo.rule50 = 0;
      }

      m_PosInfo.nCasillaPeon += m_nSqPeon[us][pt][to] - m_nSqPeon[us][pt][from];

      m_PosInfo.capturedType = capture;

      m_PosInfo.key = k;

      m_PosInfo.checkersBB = 0;
      if (moveIsCheck && m_Mission[m_clrActivo].m_nMissionType == cMission.CHECKMATE)
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
              m_PosInfo.checkersBB |= AtaquesDesdeTipoDePieza(GetRey(colorVS), cPieza.TORRE) & PiezasColor(us, cPieza.DAMA, cPieza.TORRE);
            if (pt != cPieza.ALFIL)
              m_PosInfo.checkersBB |= AtaquesDesdeTipoDePieza(GetRey(colorVS), cPieza.ALFIL) & PiezasColor(us, cPieza.DAMA, cPieza.ALFIL);
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
            canCasillaPeon -= cTypes.AvancePeon(us);
          }

          SetPieza(canCasillaPeon, cTypes.Contrario(us), m_PosInfo.capturedType, false, true);
          if (m_Tesoros[canCasillaPeon])
            m_Mission[ColorMueve()].SetPiezaCapture(canCasillaPeon, cTypes.Contrario(us), cPieza.TESORO);
          else
            m_Mission[ColorMueve()].SetPiezaCapture(canCasillaPeon, cTypes.Contrario(us), m_PosInfo.capturedType);
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
        m_PosInfo.key ^= cHashEstaticas.m_EnPaso[cTypes.Columna(m_PosInfo.enCasillaPeonuare)];
        m_PosInfo.enCasillaPeonuare = cCasilla.NONE;
      }
      m_PosInfo.key ^= cHashEstaticas.m_Bando;
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
      if (cPosicion.m_nValPieza[cFaseJuego.FASE_MEDIOJUEGO][GetPiezaMovida(m)] <= cPosicion.m_nValPieza[cFaseJuego.FASE_MEDIOJUEGO][GetPieza(cTypes.GetToCasilla(m))])
        return cValoresJuego.GANA;
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
        occupied ^= cBitBoard.m_nCasillas[to - cTypes.AvancePeon(stm)];
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
      if (m_Mission[ColorMueve()].m_nMissionType == cMission.CHECKMATE)
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
      }

      return false;
    }

    /*
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
        for (type p1 = cPieza.PEON; p1 <= cPieza.MAXSIZE; ++p1)
          for (type p2 = cPieza.PEON; p2 <= cPieza.MAXSIZE; ++p2)
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
          for (type pt = cPieza.PEON; pt <= cPieza.MAXSIZE; ++pt)
            if (m_nNumPiezas[c][pt] != cBitBoard.Count(PiezasColor(c, pt)))
              return false;
      }
      if (step > 0 && testPieceList)
      {
        ++step;
        for (color c = cColor.BLANCO; c <= cColor.NEGRO; ++c)
          for (type pt = cPieza.PEON; pt <= cPieza.MAXSIZE; ++pt)
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
    }*/
  }

  
}
