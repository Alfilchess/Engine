using System;
using System.Collections.Generic;
using Types;
using Finales;

using hash = System.UInt64;
using type = System.Int32;
using color = System.Int32;
using val = System.Int32;
using sq = System.Int32;
using bitbrd = System.UInt64;
using colum = System.Int32;
using fila = System.Int32;
using factor = System.Int32;

//-- Sustituir por tabla de finales nalimov en cuanto haya tiempo
namespace Motor
{
  //-----------------------------------------------------------------------------------------------
  public delegate Int32 Ejecucion(cPosicion pos);

  //-----------------------------------------------------------------------------------------------
  //-----------------------------------------------------------------------------------------------
  public struct stFinalesTipo
  {
    public const int KNNK = 0;
    public const int KXK = 1;
    public const int KBNK = 2;
    public const int KPK = 3;
    public const int KRKP = 4;
    public const int KRKB = 5;
    public const int KRKN = 6;
    public const int KQKP = 7;
    public const int KQKR = 8;

    public const int FUNCIONES_ESCALA = 9;
    public const int KBPsK = 10;
    public const int KQKRPs = 11;
    public const int KRPKR = 12;
    public const int KRPKB = 13;
    public const int KRPPKRP = 14;
    public const int KPsK = 15;
    public const int KBPKB = 16;
    public const int KBPPKB = 17;
    public const int KBPKN = 18;
    public const int KNPK = 19;
    public const int KNPKB = 20;
    public const int KPKP = 21;
  };

  //-----------------------------------------------------------------------------------------------
  //-----------------------------------------------------------------------------------------------
  public class cFinalesBase
  {
    public type endgameType;
    public Ejecucion m_Ejecucion;

    public static int[] m_lstValorBorde = new int[cCasilla.ESCAQUES] {
            100, 90, 80, 70, 70, 80, 90, 100,
            90, 70, 60, 50, 50, 60, 70,  90,
            80, 60, 40, 30, 30, 40, 60,  80,
            70, 50, 30, 20, 20, 30, 50,  70,
            70, 50, 30, 20, 20, 30, 50,  70,
            80, 60, 40, 30, 30, 40, 60,  80,
            90, 70, 60, 50, 50, 60, 70,  90,
            100, 90, 80, 70, 70, 80, 90, 100,};

    public static int[] m_lstValorEsquinas = new int[cCasilla.ESCAQUES] {
            200, 190, 180, 170, 160, 150, 140, 130,
            190, 180, 170, 160, 150, 140, 130, 140,
            180, 170, 155, 140, 140, 125, 140, 150,
            170, 160, 140, 120, 110, 140, 150, 160,
            160, 150, 140, 110, 120, 140, 160, 170,
            150, 140, 125, 140, 140, 155, 170, 180,
            140, 130, 140, 150, 160, 170, 180, 190,
            130, 140, 150, 160, 170, 180, 190, 200};

    public static int[] m_lstValorCerrado = new int[8] { 0, 0, 100, 80, 60, 40, 20, 10 };
    public static int[] m_lstValorLejania = new int[] { 0, 5, 20, 40, 60, 80, 90, 100 };

    protected color m_cFuerte, m_cDebil;

    //-----------------------------------------------------------------------------------------------
    public cFinalesBase(color colr, type tipoFinal)
    {
      endgameType = tipoFinal;
      m_cFuerte = colr;
      m_cDebil = cTypes.Contrario(colr);
    }

    //-----------------------------------------------------------------------------------------------
    public color Ganador()
    {
      return m_cFuerte;
    }

    //-----------------------------------------------------------------------------------------------
    public bool CheckMaterial(cPosicion posicion, color colr, val nMaterialPeones, int nPeones)
    {
      return posicion.MaterialPieza(colr) == nMaterialPeones && posicion.GetNum(colr, cPieza.PEON) == nPeones;
    }

    //-----------------------------------------------------------------------------------------------
    public sq Normalizar(cPosicion posicion, color fuerte, sq escaque)
    {
      if (cTypes.Columna(posicion.GetList(fuerte, cPieza.PEON)[0]) >= COLUMNA.E)
        escaque = (sq)(escaque ^ 7);

      if (fuerte == cColor.NEGRO)
        escaque = cTypes.CasillaVS(escaque);

      return escaque;
    }

    //-----------------------------------------------------------------------------------------------
    public static hash GetClave(string codigo, color c)
    {
      int kpos = codigo.IndexOf('K', 1);
      string[] sides = new string[] { codigo.Substring(kpos), codigo.Substring(0, kpos) };
      sides[c] = sides[c].ToLowerInvariant();

      string fen = sides[0] + (char)(8 - sides[0].Length + '0') + "/8/8/8/8/8/8/"
                 + sides[1] + (char)(8 - sides[1].Length + '0') + " w - - 0 10";

      return new cPosicion(fen, false, null).ClaveMaterial();
    }
  }

  //-----------------------------------------------------------------------------------------------
  //-----------------------------------------------------------------------------------------------
  public class cFinales
  {
    private Dictionary<hash, cFinalesBase> m_mapValores = new Dictionary<hash, cFinalesBase>();
    private Dictionary<hash, cFinalesBase> m_mapEscala = new Dictionary<hash, cFinalesBase>();

    //-----------------------------------------------------------------------------------------------
    public cFinales()
    {
      Insert("KPK", stFinalesTipo.KPK);
      Insert("KNNK", stFinalesTipo.KNNK);
      Insert("KBNK", stFinalesTipo.KBNK);
      Insert("KRKP", stFinalesTipo.KRKP);
      Insert("KRKB", stFinalesTipo.KRKB);
      Insert("KRKN", stFinalesTipo.KRKN);
      Insert("KQKP", stFinalesTipo.KQKP);
      Insert("KQKR", stFinalesTipo.KQKR);

      Insert("KNPK", stFinalesTipo.KNPK);
      Insert("KNPKB", stFinalesTipo.KNPKB);
      Insert("KRPKR", stFinalesTipo.KRPKR);
      Insert("KRPKB", stFinalesTipo.KRPKB);
      Insert("KBPKB", stFinalesTipo.KBPKB);
      Insert("KBPKN", stFinalesTipo.KBPKN);
      Insert("KBPPKB", stFinalesTipo.KBPPKB);
      Insert("KRPPKRP", stFinalesTipo.KRPPKRP);
    }

    //-----------------------------------------------------------------------------------------------
    public void Insert(string code, type E)
    {
      GetMapa(E)[cEndgame.GetClave(code, cColor.BLANCO)] = new cEndgame(cColor.BLANCO, E);
      GetMapa(E)[cEndgame.GetClave(code, cColor.NEGRO)] = new cEndgame(cColor.NEGRO, E);
    }

    //-----------------------------------------------------------------------------------------------
    public cFinalesBase probeScaleFunction(hash key, out cFinalesBase eg)
    {
      return eg = m_mapEscala.ContainsKey(key) ? m_mapEscala[key] : null;
    }

    //-----------------------------------------------------------------------------------------------
    public cFinalesBase probeValueFunction(hash key, out cFinalesBase eg)
    {
      return eg = m_mapValores.ContainsKey(key) ? m_mapValores[key] : null;
    }

    //-----------------------------------------------------------------------------------------------
    public Dictionary<hash, cFinalesBase> GetMapa(cFinalesBase eg)
    {
      if (eg.endgameType > stFinalesTipo.FUNCIONES_ESCALA)
        return m_mapEscala;

      return m_mapValores;
    }

    //-----------------------------------------------------------------------------------------------
    public Dictionary<hash, cFinalesBase> GetMapa(type E)
    {
      if (E > stFinalesTipo.FUNCIONES_ESCALA)
        return m_mapEscala;

      return m_mapValores;
    }
  }

  //-----------------------------------------------------------------------------------------------
  //-----------------------------------------------------------------------------------------------
  //-----------------------------------------------------------------------------------------------
  public class cEndgame : cFinalesBase
  {
    //-----------------------------------------------------------------------------------------------
    public cEndgame(color c, type E)
      : base(c, E)
    {
      switch (E)
      {
        case stFinalesTipo.KNNK: m_Ejecucion = KNNK; break;
        case stFinalesTipo.KXK: m_Ejecucion = KXK; break;
        case stFinalesTipo.KBNK: m_Ejecucion = KBNK; break;
        case stFinalesTipo.KPK: m_Ejecucion = KPK; break;
        case stFinalesTipo.KRKP: m_Ejecucion = KRKP; break;
        case stFinalesTipo.KRKB: m_Ejecucion = KRKB; break;
        case stFinalesTipo.KRKN: m_Ejecucion = KRKN; break;
        case stFinalesTipo.KQKP: m_Ejecucion = KQKP; break;
        case stFinalesTipo.KQKR: m_Ejecucion = KQKR; break;

        case stFinalesTipo.KBPsK: m_Ejecucion = KBPsK; break;
        case stFinalesTipo.KQKRPs: m_Ejecucion = KQKRPs; break;
        case stFinalesTipo.KRPKR: m_Ejecucion = KRPKR; break;
        case stFinalesTipo.KRPKB: m_Ejecucion = KRPKB; break;
        case stFinalesTipo.KRPPKRP: m_Ejecucion = KRPPKRP; break;
        case stFinalesTipo.KPsK: m_Ejecucion = KPsK; break;
        case stFinalesTipo.KBPKB: m_Ejecucion = KBPKB; break;
        case stFinalesTipo.KBPPKB: m_Ejecucion = KBPPKB; break;
        case stFinalesTipo.KBPKN: m_Ejecucion = KBPKN; break;
        case stFinalesTipo.KNPK: m_Ejecucion = KNPK; break;
        case stFinalesTipo.KNPKB: m_Ejecucion = KNPKB; break;
        case stFinalesTipo.KPKP: m_Ejecucion = KPKP; break;

        default: break;
      }
    }

    //-----------------------------------------------------------------------------------------------
    public val KXK(cPosicion pos)
    {
      if (pos.ColorMueve() == m_cDebil && 0 == (new cReglas(pos, cMovType.LEGAL)).Size())
        return cValoresJuego.TABLAS;

      sq posReyGanador = pos.GetRey(m_cFuerte);
      sq posReyPerdedor = pos.GetRey(m_cDebil);

      val result = pos.MaterialPieza(m_cFuerte)
                   + pos.GetNum(m_cFuerte, cPieza.PEON) * cValoresJuego.PEON_FINAL
                   + m_lstValorBorde[posReyPerdedor]
                   + m_lstValorCerrado[cBitBoard.Distancia(posReyGanador, posReyPerdedor)];

      if (pos.GetNum(m_cFuerte, cPieza.DAMA) != 0
          || pos.GetNum(m_cFuerte, cPieza.TORRE) != 0
          || (pos.GetNum(m_cFuerte, cPieza.ALFIL) != 0 && pos.GetNum(m_cFuerte, cPieza.CABALLO) != 0)
          || pos.IsAlfilPar(m_cFuerte))
      {
        result += cValoresJuego.GANA;
      }

      return m_cFuerte == pos.ColorMueve() ? result : -result;
    }

    //-----------------------------------------------------------------------------------------------
    public val KBNK(cPosicion pos)
    {
      color colr = m_cFuerte==pos.ColorMueve() ? cColor.BLANCO : cColor.NEGRO;
      val result = cValoresJuego.TABLAS;
      if(SetConjuntoPiezasPosicion(pos))
      {
        ProbeResultType pr = cTablaFinales.Probe(whitePieceSquares, blackPieceSquares, whiteTypesSquares, blackTypesSquares, colr, enPassantSquare);
        if(pr.found)
        {
          if(pr.stm==MateResult.MATE_NEGRAS||pr.stm==MateResult.MATE_BLANCAS)
          {
            if(m_cFuerte==pos.ColorMueve())
              result=(cValoresJuego.MATE-pr.ply);
            else
              result=-cValoresJuego.MATE+pr.ply;
          }

        }
      }
      return result;
      /*sq posReyGanador = pos.GetRey(m_cFuerte);
      sq posReyPerdedor = pos.GetRey(m_cDebil);
      sq posAlfil = pos.GetList(m_cFuerte, cPieza.ALFIL)[0];

      if (cTypes.IsColorContrario(posAlfil, cCasilla.A1))
      {
        posReyGanador = cTypes.CasillaVS(posReyGanador);
        posReyPerdedor = cTypes.CasillaVS(posReyPerdedor);
      }

      val result = cValoresJuego.GANA + m_lstValorCerrado[cBitBoard.Distancia(posReyGanador, posReyPerdedor)]
                  + m_lstValorEsquinas[posReyPerdedor];

      return m_cFuerte == pos.ColorMueve() ? result : -result;*/
    }

    //-----------------------------------------------------------------------------------------------
    public val KPK(cPosicion pos)
    {
      //sq posReyBlanco = Normalizar(posicion, m_cFuerte, posicion.GetRey(m_cFuerte));
      //sq posReyNegro = Normalizar(posicion, m_cFuerte, posicion.GetRey(m_cDebil));*/
      //sq casillaFromPos = Normalizar(posicion, m_cFuerte, posicion.GetList(m_cFuerte, cPieza.PEON)[0]);

      //color colr = m_cFuerte == posicion.ColorMueve() ? cColor.BLANCO : cColor.NEGRO;

      //if (!cBitbaseKPK.Buscar(posReyBlanco, casillaFromPos, posReyNegro, colr))
      //  return cValoresJuego.TABLAS;

      //val result = cValoresJuego.GANA + cValoresJuego.PEON_FINAL + cTypes.Fila(casillaFromPos);

      //return m_cFuerte == posicion.ColorMueve() ? result : -result;*/


      color colr = m_cFuerte==pos.ColorMueve() ? cColor.BLANCO : cColor.NEGRO;
      val result = cValoresJuego.TABLAS;
      if(SetConjuntoPiezasPosicion(pos))
      {
        ProbeResultType pr = cTablaFinales.Probe(whitePieceSquares, blackPieceSquares, whiteTypesSquares, blackTypesSquares, colr, enPassantSquare);
        if(pr.found)
        {
          if(pr.stm==MateResult.MATE_NEGRAS||pr.stm==MateResult.MATE_BLANCAS)
          {
            if(m_cFuerte==pos.ColorMueve())
              result=(cValoresJuego.MATE-pr.ply);
            else
              result=-cValoresJuego.MATE+pr.ply;
          }

        }
      }
      return result;
    }

    //-----------------------------------------------------------------------------------------------
    public val KRKP(cPosicion pos)
    {
      val result = 0;
      sq nCasillaReyBlanco = cTypes.CasillaProxima(m_cFuerte, pos.GetRey(m_cFuerte));
      sq nCasillaReyNegro = cTypes.CasillaProxima(m_cFuerte, pos.GetRey(m_cDebil));
      sq nCasillaTorre = cTypes.CasillaProxima(m_cFuerte, pos.GetList(m_cFuerte, cPieza.TORRE)[0]);
      sq nCasillaPeon = cTypes.CasillaProxima(m_cFuerte, pos.GetList(m_cDebil, cPieza.PEON)[0]);
      sq nCasillaCoronacion = cTypes.CreaCasilla(cTypes.Columna(nCasillaPeon), FILA.F1);

      if (nCasillaReyBlanco < nCasillaPeon && cTypes.Columna(nCasillaReyBlanco) == cTypes.Columna(nCasillaPeon))
        result = cValoresJuego.TORRE_FINAL - (cBitBoard.Distancia(nCasillaReyBlanco, nCasillaPeon));
      else if (cBitBoard.Distancia(nCasillaReyNegro, nCasillaPeon) >= 3 + ((pos.ColorMueve() == m_cDebil) ? 1 : 0)
              && cBitBoard.Distancia(nCasillaReyNegro, nCasillaTorre) >= 3)
        result = cValoresJuego.TORRE_FINAL - (cBitBoard.Distancia(nCasillaReyBlanco, nCasillaPeon));
      else if (cTypes.Fila(nCasillaReyNegro) <= FILA.F3
              && cBitBoard.Distancia(nCasillaReyNegro, nCasillaPeon) == 1
              && cTypes.Fila(nCasillaReyBlanco) >= FILA.F4
              && cBitBoard.Distancia(nCasillaReyBlanco, nCasillaPeon) > 2 + ((pos.ColorMueve() == m_cFuerte) ? 1 : 0))
        result = 80 - 8 * cBitBoard.Distancia(nCasillaReyBlanco, nCasillaPeon);
      else
        result = (val)(200) - 8 * (cBitBoard.Distancia(nCasillaReyBlanco, nCasillaPeon + cCasilla.SUR)
                          - cBitBoard.Distancia(nCasillaReyNegro, nCasillaPeon + cCasilla.SUR)
                          - cBitBoard.Distancia(nCasillaPeon, nCasillaCoronacion));

      return m_cFuerte == pos.ColorMueve() ? result : -result;
    }

    //-----------------------------------------------------------------------------------------------
    public val KRKB(cPosicion pos)
    {
      color colr = m_cFuerte==pos.ColorMueve() ? cColor.BLANCO : cColor.NEGRO;
      val result = cValoresJuego.TABLAS;
      if(SetConjuntoPiezasPosicion(pos))
      {
        ProbeResultType pr = cTablaFinales.Probe(whitePieceSquares, blackPieceSquares, whiteTypesSquares, blackTypesSquares, colr, enPassantSquare);
        if(pr.found)
        {
          if(pr.stm==MateResult.MATE_NEGRAS||pr.stm==MateResult.MATE_BLANCAS)
          {
            if(m_cFuerte==pos.ColorMueve())
              result=(cValoresJuego.MATE-pr.ply);
            else
              result=-cValoresJuego.MATE+pr.ply;
          }

        }
      }
      return result;
      //val result = (m_lstValorBorde[pos.GetRey(m_cDebil)]);
      //return m_cFuerte == pos.ColorMueve() ? result : -result;
    }

    //-----------------------------------------------------------------------------------------------
    public val KRKN(cPosicion pos)
    {
      sq nCasillaReyNegro = pos.GetRey(m_cDebil);
      sq nCasillaCaballo = pos.GetList(m_cDebil, cPieza.CABALLO)[0];
      val result = (m_lstValorBorde[nCasillaReyNegro] + m_lstValorLejania[cBitBoard.Distancia(nCasillaReyNegro, nCasillaCaballo)]);
      return m_cFuerte == pos.ColorMueve() ? result : -result;
    }

    //-----------------------------------------------------------------------------------------------
    public val KQKP(cPosicion pos)
    {
      sq nCasillaReyGanador = pos.GetRey(m_cFuerte);
      sq nCasillaReyPerdedor = pos.GetRey(m_cDebil);
      sq nCasillaPeon = pos.GetList(m_cDebil, cPieza.PEON)[0];

      val result = (m_lstValorCerrado[cBitBoard.Distancia(nCasillaReyGanador, nCasillaReyPerdedor)]);

      if (cTypes.FilaProxima(m_cDebil, nCasillaPeon) != FILA.F7 || cBitBoard.Distancia(nCasillaReyPerdedor, nCasillaPeon) != 1
        || 0 == ((cBitBoard.A | cBitBoard.C | cBitBoard.F | cBitBoard.H) & cBitBoard.m_nCasillas[nCasillaPeon]))
        result += cValoresJuego.DAMA_FINAL - cValoresJuego.PEON_FINAL;

      return m_cFuerte == pos.ColorMueve() ? result : -result;
    }

    //-----------------------------------------------------------------------------------------------
    public val KQKR(cPosicion pos)
    {
      color colr = m_cFuerte==pos.ColorMueve() ? cColor.BLANCO : cColor.NEGRO;
      val result = cValoresJuego.TABLAS;
      if(SetConjuntoPiezasPosicion(pos))
      {
        ProbeResultType pr = cTablaFinales.Probe(whitePieceSquares, blackPieceSquares, whiteTypesSquares, blackTypesSquares, colr, enPassantSquare);
        if(pr.found)
        {
          if(pr.stm==MateResult.MATE_NEGRAS||pr.stm==MateResult.MATE_BLANCAS)
          {
            if (m_cFuerte==pos.ColorMueve())
              result=(cValoresJuego.MATE-pr.ply);
            else
              result=-cValoresJuego.MATE+pr.ply;
          }
            
        }
      }
      return result;
      /*sq winnerKSq = pos.GetRey(m_cFuerte);
      sq loserKSq = pos.GetRey(m_cDebil);

      val result = cValoresJuego.DAMA_FINAL
                  - cValoresJuego.TORRE_FINAL
                  + m_lstValorBorde[loserKSq]
                  + m_lstValorCerrado[cBitBoard.Distancia(winnerKSq, loserKSq)];

      return m_cFuerte == pos.ColorMueve() ? result : -result;*/
    }

    //-----------------------------------------------------------------------------------------------
    public val KNNK(cPosicion pos)
    {
      return cValoresJuego.TABLAS;
    }

    //-----------------------------------------------------------------------------------------------
    public factor KBPsK(cPosicion pos)
    {
      bitbrd pawns = pos.PiezasColor(m_cFuerte, cPieza.PEON);
      colum pawnFile = cTypes.Columna(pos.GetList(m_cFuerte, cPieza.PEON)[0]);

      if ((pawnFile == COLUMNA.A || pawnFile == COLUMNA.H) && 0 == (pawns & ~cBitBoard.GetColumna(pawnFile)))
      {
        sq bishonCasillaPeon = pos.GetList(m_cFuerte, cPieza.ALFIL)[0];
        sq queeningSq = cTypes.CasillaProxima(m_cFuerte, cTypes.CreaCasilla(pawnFile, FILA.F8));
        sq kingSq = pos.GetRey(m_cDebil);

        if (cTypes.IsColorContrario(queeningSq, bishonCasillaPeon)
            && cBitBoard.Distancia(queeningSq, kingSq) <= 1)
          return cFactorEscala.TABLAS;
      }

      if ((pawnFile == COLUMNA.B || pawnFile == COLUMNA.G)
          && 0 == (pos.GetNumPiezas(cPieza.PEON) & ~cBitBoard.GetColumna(pawnFile))
          && pos.MaterialPieza(m_cDebil) == 0
          && pos.GetNum(m_cDebil, cPieza.PEON) >= 1)
      {
        sq weaksqPeon = cBitBoard.backmost_sq(m_cDebil, pos.PiezasColor(m_cDebil, cPieza.PEON));
        sq strongKingSq = pos.GetRey(m_cFuerte);
        sq weakKingSq = pos.GetRey(m_cDebil);
        sq bishonCasillaPeon = pos.GetList(m_cFuerte, cPieza.ALFIL)[0];

        if (cTypes.FilaProxima(m_cFuerte, weaksqPeon) == FILA.F7
            && (pos.PiezasColor(m_cFuerte, cPieza.PEON) & cBitBoard.m_nCasillas[(weaksqPeon + cTypes.AtaquePeon(m_cDebil))]) != 0
            && (cTypes.IsColorContrario(bishonCasillaPeon, weaksqPeon) || pos.GetNum(m_cFuerte, cPieza.PEON) == 1))
        {
          int strongKingDist = cBitBoard.Distancia(weaksqPeon, strongKingSq);
          int weakKingDist = cBitBoard.Distancia(weaksqPeon, weakKingSq);

          if (cTypes.FilaProxima(m_cFuerte, weakKingSq) >= FILA.F7
              && weakKingDist <= 2
              && weakKingDist <= strongKingDist)
            return cFactorEscala.TABLAS;
        }
      }

      return cFactorEscala.NAN;
    }

    //-----------------------------------------------------------------------------------------------
    public factor KQKRPs(cPosicion pos)
    {
      sq kingSq = pos.GetRey(m_cDebil);
      sq nCasillaTorre = pos.GetList(m_cDebil, cPieza.TORRE)[0];

      if (cTypes.FilaProxima(m_cDebil, kingSq) <= FILA.F2
      && cTypes.FilaProxima(m_cDebil, pos.GetRey(m_cFuerte)) >= FILA.F4
      && cTypes.FilaProxima(m_cDebil, nCasillaTorre) == FILA.F3
      && (pos.PiezasColor(m_cDebil, cPieza.PEON)
          & pos.attacks_from_square_piecetype(kingSq, cPieza.REY)
          & pos.AtaquesDePeon(nCasillaTorre, m_cFuerte)) != 0)
        return cFactorEscala.TABLAS;

      return cFactorEscala.NAN;
    }

    //-----------------------------------------------------------------------------------------------
    public factor KRPKR(cPosicion pos)
    {
      sq nCasillaReyBlanco = Normalizar(pos, m_cFuerte, pos.GetRey(m_cFuerte));
      sq nCasillaReyNegro = Normalizar(pos, m_cFuerte, pos.GetRey(m_cDebil));
      sq wnCasillaTorre = Normalizar(pos, m_cFuerte, pos.GetList(m_cFuerte, cPieza.TORRE)[0]);
      sq wnCasillaPeon = Normalizar(pos, m_cFuerte, pos.GetList(m_cFuerte, cPieza.PEON)[0]);
      sq bnCasillaTorre = Normalizar(pos, m_cFuerte, pos.GetList(m_cDebil, cPieza.TORRE)[0]);

      colum f = cTypes.Columna(wnCasillaPeon);
      fila r = cTypes.Fila(wnCasillaPeon);
      sq queeningSq = cTypes.CreaCasilla(f, FILA.F8);
      int tempo = (pos.ColorMueve() == m_cFuerte ? 1 : 0);

      if (r <= FILA.F5 && cBitBoard.Distancia(nCasillaReyNegro, queeningSq) <= 1
        && nCasillaReyBlanco <= cCasilla.H5 && (cTypes.Fila(bnCasillaTorre) == FILA.F6 || (r <= FILA.F3 && cTypes.Fila(wnCasillaTorre) != FILA.F6)))
        return cFactorEscala.TABLAS;

      if (r == FILA.F6 && cBitBoard.Distancia(nCasillaReyNegro, queeningSq) <= 1
        && cTypes.Fila(nCasillaReyBlanco) + tempo <= FILA.F6 && (cTypes.Fila(bnCasillaTorre) == FILA.F1 || (0 == tempo && Math.Abs(cTypes.Columna(bnCasillaTorre) - f) >= 3)))
        return cFactorEscala.TABLAS;

      if (r >= FILA.F6 && nCasillaReyNegro == queeningSq
        && cTypes.Fila(bnCasillaTorre) == FILA.F1
        && (0 == tempo || cBitBoard.Distancia(nCasillaReyBlanco, wnCasillaPeon) >= 2))
        return cFactorEscala.TABLAS;

      if (wnCasillaPeon == cCasilla.A7 && wnCasillaTorre == cCasilla.A8
        && (nCasillaReyNegro == cCasilla.H7 || nCasillaReyNegro == cCasilla.G7)
        && cTypes.Columna(bnCasillaTorre) == COLUMNA.A
        && (cTypes.Fila(bnCasillaTorre) <= FILA.F3 || cTypes.Columna(nCasillaReyBlanco) >= COLUMNA.D || cTypes.Fila(nCasillaReyBlanco) <= FILA.F5))
        return cFactorEscala.TABLAS;

      if (r <= FILA.F5 && nCasillaReyNegro == wnCasillaPeon + cCasilla.NORTE
        && cBitBoard.Distancia(nCasillaReyBlanco, wnCasillaPeon) - tempo >= 2
        && cBitBoard.Distancia(nCasillaReyBlanco, bnCasillaTorre) - tempo >= 2)
        return cFactorEscala.TABLAS;

      if (r == FILA.F7 && f != COLUMNA.A
        && cTypes.Columna(wnCasillaTorre) == f
        && wnCasillaTorre != queeningSq
        && (cBitBoard.Distancia(nCasillaReyBlanco, queeningSq) < cBitBoard.Distancia(nCasillaReyNegro, queeningSq) - 2 + tempo)
        && (cBitBoard.Distancia(nCasillaReyBlanco, queeningSq) < cBitBoard.Distancia(nCasillaReyNegro, wnCasillaTorre) + tempo))
        return (factor)(cFactorEscala.MAXIMO - 2 * cBitBoard.Distancia(nCasillaReyBlanco, queeningSq));

      if (f != COLUMNA.A && cTypes.Columna(wnCasillaTorre) == f
        && wnCasillaTorre < wnCasillaPeon
        && (cBitBoard.Distancia(nCasillaReyBlanco, queeningSq) < cBitBoard.Distancia(nCasillaReyNegro, queeningSq) - 2 + tempo)
        && (cBitBoard.Distancia(nCasillaReyBlanco, wnCasillaPeon + cCasilla.NORTE) < cBitBoard.Distancia(nCasillaReyNegro, wnCasillaPeon + cCasilla.NORTE) - 2 + tempo)
        && (cBitBoard.Distancia(nCasillaReyNegro, wnCasillaTorre) + tempo >= 3
            || (cBitBoard.Distancia(nCasillaReyBlanco, queeningSq) < cBitBoard.Distancia(nCasillaReyNegro, wnCasillaTorre) + tempo
                && (cBitBoard.Distancia(nCasillaReyBlanco, wnCasillaPeon + cCasilla.NORTE) < cBitBoard.Distancia(nCasillaReyNegro, wnCasillaTorre) + tempo))))
        return (factor)(cFactorEscala.MAXIMO
                           - 8 * cBitBoard.Distancia(wnCasillaPeon, queeningSq)
                           - 2 * cBitBoard.Distancia(nCasillaReyBlanco, queeningSq));

      if (r <= FILA.F4 && nCasillaReyNegro > wnCasillaPeon)
      {
        if (cTypes.Columna(nCasillaReyNegro) == cTypes.Columna(wnCasillaPeon))
          return 10;
        if (Math.Abs(cTypes.Columna(nCasillaReyNegro) - cTypes.Columna(wnCasillaPeon)) == 1
          && cBitBoard.Distancia(nCasillaReyBlanco, nCasillaReyNegro) > 2)
          return (24 - 2 * cBitBoard.Distancia(nCasillaReyBlanco, nCasillaReyNegro));
      }
      return cFactorEscala.NAN;
    }

    //-----------------------------------------------------------------------------------------------
    public factor KRPKB(cPosicion pos)
    {
      if ((pos.GetNumPiezas(cPieza.PEON) & (cBitBoard.A | cBitBoard.H)) != 0)
      {
        sq ksq = pos.GetRey(m_cDebil);
        sq bsq = pos.GetList(m_cDebil, cPieza.ALFIL)[0];
        sq nCasillaPeon = pos.GetList(m_cFuerte, cPieza.PEON)[0];
        fila rk = cTypes.FilaProxima(m_cFuerte, nCasillaPeon);
        sq push = cTypes.AtaquePeon(m_cFuerte);

        if (rk == FILA.F5 && !cTypes.IsColorContrario(bsq, nCasillaPeon))
        {
          int d = cBitBoard.Distancia(nCasillaPeon + 3 * push, ksq);

          if (d <= 2 && !(d == 0 && ksq == pos.GetRey(m_cFuerte) + 2 * push))
            return (24);
          else
            return (48);
        }

        if (rk == FILA.F6
            && cBitBoard.Distancia(nCasillaPeon + 2 * push, ksq) <= 1
            && (cBitBoard.m_PseudoAtaques[cPieza.ALFIL][bsq] & cBitBoard.m_nCasillas[(nCasillaPeon + push)]) != 0
            && cBitBoard.ColumDistancia(bsq, nCasillaPeon) >= 2)
          return (8);
      }

      return cFactorEscala.NAN;
    }

    //-----------------------------------------------------------------------------------------------
    public factor KRPPKRP(cPosicion pos)
    {
      sq wnCasillaPeon1 = pos.GetList(m_cFuerte, cPieza.PEON)[0];
      sq wnCasillaPeon2 = pos.GetList(m_cFuerte, cPieza.PEON)[1];
      sq nCasillaReyNegro = pos.GetRey(m_cDebil);

      if (pos.IsPeonPasado(m_cFuerte, wnCasillaPeon1) || pos.IsPeonPasado(m_cFuerte, wnCasillaPeon2))
        return cFactorEscala.NAN;

      fila r = Math.Max(cTypes.FilaProxima(m_cFuerte, wnCasillaPeon1), cTypes.FilaProxima(m_cFuerte, wnCasillaPeon2));

      if (cBitBoard.ColumDistancia(nCasillaReyNegro, wnCasillaPeon1) <= 1
        && cBitBoard.ColumDistancia(nCasillaReyNegro, wnCasillaPeon2) <= 1
        && cTypes.FilaProxima(m_cFuerte, nCasillaReyNegro) > r)
      {
        switch (r)
        {
          case FILA.F2: return (10);
          case FILA.F3: return (10);
          case FILA.F4: return (15);
          case FILA.F5: return (20);
          case FILA.F6: return (40);
          default: break;
        }
      }
      return cFactorEscala.NAN;
    }

    //-----------------------------------------------------------------------------------------------
    public factor KPsK(cPosicion pos)
    {
      sq ksq = pos.GetRey(m_cDebil);
      bitbrd pawns = pos.PiezasColor(m_cFuerte, cPieza.PEON);
      sq nCasillaPeon = pos.GetList(m_cFuerte, cPieza.PEON)[0];

      if (0 == (pawns & ~cBitBoard.EnFrente(m_cDebil, cTypes.Fila(ksq)))
          && !((pawns & ~cBitBoard.A) != 0 && (pawns & ~cBitBoard.H) != 0)
          && cBitBoard.ColumDistancia(ksq, nCasillaPeon) <= 1)
        return cFactorEscala.TABLAS;

      return cFactorEscala.NAN;
    }

    //-----------------------------------------------------------------------------------------------
    public factor KBPKB(cPosicion pos)
    {
      sq sqPeon = pos.GetList(m_cFuerte, cPieza.PEON)[0];
      sq strongerBishonCasillaPeon = pos.GetList(m_cFuerte, cPieza.ALFIL)[0];
      sq weakerBishonCasillaPeon = pos.GetList(m_cDebil, cPieza.ALFIL)[0];
      sq weakerKingSq = pos.GetRey(m_cDebil);

      if (cTypes.Columna(weakerKingSq) == cTypes.Columna(sqPeon)
          && cTypes.FilaProxima(m_cFuerte, sqPeon) < cTypes.FilaProxima(m_cFuerte, weakerKingSq)
          && (cTypes.IsColorContrario(weakerKingSq, strongerBishonCasillaPeon)
              || cTypes.FilaProxima(m_cFuerte, weakerKingSq) <= FILA.F6))
        return cFactorEscala.TABLAS;

      if (cTypes.IsColorContrario(strongerBishonCasillaPeon, weakerBishonCasillaPeon))
      {
        if (cTypes.FilaProxima(m_cFuerte, sqPeon) <= FILA.F5)
          return cFactorEscala.TABLAS;
        else
        {
          bitbrd path = cBitBoard.Atras(m_cFuerte, sqPeon);

          if ((path & pos.PiezasColor(m_cDebil, cPieza.REY)) != 0)
            return cFactorEscala.TABLAS;

          if (((pos.attacks_from_square_piecetype(weakerBishonCasillaPeon, cPieza.ALFIL) & path) != 0)
              && cBitBoard.Distancia(weakerBishonCasillaPeon, sqPeon) >= 3)
            return cFactorEscala.TABLAS;
        }
      }

      return cFactorEscala.NAN;
    }

    //-----------------------------------------------------------------------------------------------
    public factor KBPPKB(cPosicion pos)
    {
      sq wbsq = pos.GetList(m_cFuerte, cPieza.ALFIL)[0];
      sq bbsq = pos.GetList(m_cDebil, cPieza.ALFIL)[0];

      if (!cTypes.IsColorContrario(wbsq, bbsq))
        return cFactorEscala.NAN;

      sq ksq = pos.GetRey(m_cDebil);
      sq nCasillaPeon1 = pos.GetList(m_cFuerte, cPieza.PEON)[0];
      sq nCasillaPeon2 = pos.GetList(m_cFuerte, cPieza.PEON)[1];
      fila r1 = cTypes.Fila(nCasillaPeon1);
      fila r2 = cTypes.Fila(nCasillaPeon2);
      sq blockSq1, blockSq2;

      if (cTypes.FilaProxima(m_cFuerte, nCasillaPeon1) > cTypes.FilaProxima(m_cFuerte, nCasillaPeon2))
      {
        blockSq1 = nCasillaPeon1 + cTypes.AtaquePeon(m_cFuerte);
        blockSq2 = cTypes.CreaCasilla(cTypes.Columna(nCasillaPeon2), cTypes.Fila(nCasillaPeon1));
      }
      else
      {
        blockSq1 = nCasillaPeon2 + cTypes.AtaquePeon(m_cFuerte);
        blockSq2 = cTypes.CreaCasilla(cTypes.Columna(nCasillaPeon1), cTypes.Fila(nCasillaPeon2));
      }

      switch (cBitBoard.ColumDistancia(nCasillaPeon1, nCasillaPeon2))
      {
        case 0:
          if (cTypes.Columna(ksq) == cTypes.Columna(blockSq1)
              && cTypes.FilaProxima(m_cFuerte, ksq) >= cTypes.FilaProxima(m_cFuerte, blockSq1)
              && cTypes.IsColorContrario(ksq, wbsq))
            return cFactorEscala.TABLAS;
          else
            return cFactorEscala.NAN;

        case 1:
          if (ksq == blockSq1
              && cTypes.IsColorContrario(ksq, wbsq)
              && (bbsq == blockSq2
                  || (pos.attacks_from_square_piecetype(blockSq2, cPieza.ALFIL) & pos.PiezasColor(m_cDebil, cPieza.ALFIL)) != 0
                  || Math.Abs(r1 - r2) >= 2))
            return cFactorEscala.TABLAS;

          else if (ksq == blockSq2
              && cTypes.IsColorContrario(ksq, wbsq)
              && (bbsq == blockSq1
                  || (pos.attacks_from_square_piecetype(blockSq1, cPieza.ALFIL) & pos.PiezasColor(m_cDebil, cPieza.ALFIL)) != 0))
            return cFactorEscala.TABLAS;
          else
            return cFactorEscala.NAN;
        default:
          return cFactorEscala.NAN;
      }
    }

    //-----------------------------------------------------------------------------------------------
    public factor KBPKN(cPosicion pos)
    {
      sq sqPeon = pos.GetList(m_cFuerte, cPieza.PEON)[0];
      sq strongerBishonCasillaPeon = pos.GetList(m_cFuerte, cPieza.ALFIL)[0];
      sq weakerKingSq = pos.GetRey(m_cDebil);

      if (cTypes.Columna(weakerKingSq) == cTypes.Columna(sqPeon)
          && cTypes.FilaProxima(m_cFuerte, sqPeon) < cTypes.FilaProxima(m_cFuerte, weakerKingSq)
          && (cTypes.IsColorContrario(weakerKingSq, strongerBishonCasillaPeon)
              || cTypes.FilaProxima(m_cFuerte, weakerKingSq) <= FILA.F6))
        return cFactorEscala.TABLAS;

      return cFactorEscala.NAN;
    }

    //-----------------------------------------------------------------------------------------------
    public factor KNPK(cPosicion pos)
    {
      sq sqPeon = Normalizar(pos, m_cFuerte, pos.GetList(m_cFuerte, cPieza.PEON)[0]);
      sq weakKingSq = Normalizar(pos, m_cFuerte, pos.GetRey(m_cDebil));

      if (sqPeon == cCasilla.A7 && cBitBoard.Distancia(cCasilla.A8, weakKingSq) <= 1)
        return cFactorEscala.TABLAS;

      return cFactorEscala.NAN;
    }

    //-----------------------------------------------------------------------------------------------
    public factor KNPKB(cPosicion pos)
    {
      sq sqPeon = pos.GetList(m_cFuerte, cPieza.PEON)[0];
      sq alfilCasillaPeon = pos.GetList(m_cDebil, cPieza.ALFIL)[0];
      sq weakerKingSq = pos.GetRey(m_cDebil);

      if ((cBitBoard.Atras(m_cFuerte, sqPeon) & pos.attacks_from_square_piecetype(alfilCasillaPeon, cPieza.ALFIL)) != 0)
        return cBitBoard.Distancia(weakerKingSq, sqPeon);

      return cFactorEscala.NAN;
    }

    //-----------------------------------------------------------------------------------------------
    public factor KPKP(cPosicion pos)
    {
      color colr = m_cFuerte==pos.ColorMueve() ? cColor.BLANCO : cColor.NEGRO;
      val result = cFactorEscala.TABLAS;
      if(SetConjuntoPiezasPosicion(pos))
      {
        ProbeResultType pr = cTablaFinales.Probe(whitePieceSquares, blackPieceSquares, whiteTypesSquares, blackTypesSquares, colr, enPassantSquare);
        if(pr.found)
        {
          if(pr.stm==MateResult.MATE_NEGRAS||pr.stm==MateResult.MATE_BLANCAS)
            result=cFactorEscala.NAN;
        }
      }
      return result;
      /*sq nCasillaReyBlanco = Normalizar(pos, m_cFuerte, pos.GetRey(m_cFuerte));
      sq nCasillaReyNegro = Normalizar(pos, m_cFuerte, pos.GetRey(m_cDebil));
      sq nCasillaPeon = Normalizar(pos, m_cFuerte, pos.GetList(m_cFuerte, cPieza.PEON)[0]);

      color us = m_cFuerte == pos.ColorMueve() ? cColor.BLANCO : cColor.NEGRO;

      if (cTypes.Fila(nCasillaPeon) >= FILA.F5 && cTypes.Columna(nCasillaPeon) != COLUMNA.A)
        return cFactorEscala.NAN;

      return cBitbaseKPK.Buscar(nCasillaReyBlanco, nCasillaPeon, nCasillaReyNegro, us) ? cFactorEscala.NAN : cFactorEscala.TABLAS;*/
    }

    //-----------------------------------------------------------------------------------------------
    public static bool SetConjuntoPiezasPosicion(cPosicion pos)
    {
      try
      {
        whitePieceSquares.Clear();
        whiteTypesSquares.Clear();
        blackPieceSquares.Clear();
        blackTypesSquares.Clear();

        for(type pt = cPieza.PEON; pt <= cPieza.REY; ++pt)
        {
          for(int i = 0; i < pos.GetNum(cColor.BLANCO, pt); i++)
          {
            whitePieceSquares.Add(pos.GetList(cColor.BLANCO, pt)[i]);
            whiteTypesSquares.Add(pt);
          }

          for(int i = 0; i < pos.GetNum(cColor.NEGRO, pt); i++)
          {
            blackPieceSquares.Add(pos.GetList(cColor.NEGRO, pt)[i]);
            blackTypesSquares.Add(pt);
          }
        }

        enPassantSquare = pos.CasillaEnPaso();
        whosTurn = pos.ColorMueve();
      }
      catch (Exception /*ex*/)
      {
      }

      return true;
    }

    static bool SetConjuntoPiezasFEN(string fen)
    {
      byte index = 0;
      byte spc = 0;
      byte spacers = 0;

      whosTurn=0;
      enPassantSquare=0;
      whitePieceSquares.Clear();
      whiteTypesSquares.Clear();
      blackPieceSquares.Clear();
      blackTypesSquares.Clear();

      if(fen.Contains("a3"))
        enPassantSquare=cTablaFinales.a8toa1[40];
      else if(fen.Contains("b3"))
        enPassantSquare=cTablaFinales.a8toa1[41];
      else if(fen.Contains("c3"))
        enPassantSquare=cTablaFinales.a8toa1[42];
      else if(fen.Contains("d3"))
        enPassantSquare=cTablaFinales.a8toa1[43];
      else if(fen.Contains("e3"))
        enPassantSquare=cTablaFinales.a8toa1[44];
      else if(fen.Contains("f3"))
        enPassantSquare=cTablaFinales.a8toa1[45];
      else if(fen.Contains("g3"))
        enPassantSquare=cTablaFinales.a8toa1[46];
      else if(fen.Contains("h3"))
        enPassantSquare=cTablaFinales.a8toa1[47];

      if(fen.Contains("a6"))
        enPassantSquare=cTablaFinales.a8toa1[16];
      else if(fen.Contains("b6"))
        enPassantSquare=cTablaFinales.a8toa1[17];
      else if(fen.Contains("c6"))
        enPassantSquare=cTablaFinales.a8toa1[18];
      else if(fen.Contains("d6"))
        enPassantSquare=cTablaFinales.a8toa1[19];
      else if(fen.Contains("e6"))
        enPassantSquare=cTablaFinales.a8toa1[20];
      else if(fen.Contains("f6"))
        enPassantSquare=cTablaFinales.a8toa1[21];
      else if(fen.Contains("g6"))
        enPassantSquare=cTablaFinales.a8toa1[22];
      else if(fen.Contains("h6"))
        enPassantSquare=cTablaFinales.a8toa1[23];

      foreach(char c in fen)
      {
        if(index<64&&spc==0)
        {
          if(c=='1'&&index<63)
          {
            index++;
          }
          else if(c=='2'&&index<62)
          {
            index+=2;
          }
          else if(c=='3'&&index<61)
          {
            index+=3;
          }
          else if(c=='4'&&index<60)
          {
            index+=4;
          }
          else if(c=='5'&&index<59)
          {
            index+=5;
          }
          else if(c=='6'&&index<58)
          {
            index+=6;
          }
          else if(c=='7'&&index<57)
          {
            index+=7;
          }
          else if(c=='8'&&index<56)
          {
            index+=8;
          }
          else if(c=='P')
          {
            whitePieceSquares.Add(cTablaFinales.a8toa1[index]);
            whiteTypesSquares.Add(cTablaFinales.PAWN);
            index++;
          }
          else if(c=='N')
          {
            whitePieceSquares.Add(cTablaFinales.a8toa1[index]);
            whiteTypesSquares.Add(cTablaFinales.KNIGHT);
            index++;
          }
          else if(c=='B')
          {
            whitePieceSquares.Add(cTablaFinales.a8toa1[index]);
            whiteTypesSquares.Add(cTablaFinales.BISHOP);
            index++;
          }
          else if(c=='R')
          {
            whitePieceSquares.Add(cTablaFinales.a8toa1[index]);
            whiteTypesSquares.Add(cTablaFinales.ROOK);
            index++;
          }
          else if(c=='Q')
          {
            whitePieceSquares.Add(cTablaFinales.a8toa1[index]);
            whiteTypesSquares.Add(cTablaFinales.QUEEN);
            index++;
          }
          else if(c=='K')
          {
            whitePieceSquares.Add(cTablaFinales.a8toa1[index]);
            whiteTypesSquares.Add(cTablaFinales.KING);
            index++;
          }
          else if(c=='p')
          {
            blackPieceSquares.Add(cTablaFinales.a8toa1[index]);
            blackTypesSquares.Add(cTablaFinales.PAWN);
            index++;
          }
          else if(c=='n')
          {
            blackPieceSquares.Add(cTablaFinales.a8toa1[index]);
            blackTypesSquares.Add(cTablaFinales.KNIGHT);
            index++;
          }
          else if(c=='b')
          {
            blackPieceSquares.Add(cTablaFinales.a8toa1[index]);
            blackTypesSquares.Add(cTablaFinales.BISHOP);
            index++;
          }
          else if(c=='r')
          {
            blackPieceSquares.Add(cTablaFinales.a8toa1[index]);
            blackTypesSquares.Add(cTablaFinales.ROOK);
            index++;
          }
          else if(c=='q')
          {
            blackPieceSquares.Add(cTablaFinales.a8toa1[index]);
            blackTypesSquares.Add(cTablaFinales.QUEEN);
            index++;
          }
          else if(c=='k')
          {
            blackPieceSquares.Add(cTablaFinales.a8toa1[index]);
            blackTypesSquares.Add(cTablaFinales.KING);
            index++;
          }
          else if(c=='/')
          {
            continue;
          }
          else if(c==' ')
          {
            spc++;
          }
        }
        else
        {
          if((c=='w')&&(spacers<2))
          {
            whosTurn=(int)cColor.BLANCO;//;PlayerColor.White;
          }
          else if((c=='b')&&(spacers<2))
          {
            whosTurn=(int)cColor.NEGRO;//PlayerColor.Black;
          }
          else if(c==' ')
          {
            spacers++;
          }
        }
      }
      return true;
    }

    static List<int> whitePieceSquares = new List<int>();
    static List<int> blackPieceSquares = new List<int>();
    static List<int> whiteTypesSquares = new List<int>();
    static List<int> blackTypesSquares = new List<int>();

    static int whosTurn = 0;
    static int enPassantSquare = 0;
  }
}
