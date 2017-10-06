using System;
using Types;
using Finales;

using bitbrd = System.UInt64;
using pnt = System.Int32;
using val = System.Int32;
using color = System.Int32;
using sq = System.Int32;
using type = System.Int32;
using factor = System.Int32;
using InOut;

namespace Motor
{
  //---------------------------------------------------------------------------------------------------------------------------
  //---------------------------------------------------------------------------------------------------------------------------
  public class cEval
  {
    //---------------------------------------------------------------------------------------------------------------------------
    public class cEvalPesos
    {
      public int m_nMedioJ, m_nFinal;
      public cEvalPesos(int nMg, int nEg)
      {
        m_nMedioJ = nMg;
        m_nFinal = nEg;
      }
      public const int MOVILIDAD = 0, ESTRUCTURA_PEONES = 1, PEONES_PASADOS = 2, ESPACIO = 3, REY_AMENAZADO = 4, REY_AMENAZADO_VS = 5;
    }

    //---------------------------------------------------------------------------------------------------------------------------
    public class cEvalInfo
    {
      public cMaterial.cStructMaterial m_Material = null;
      public cEstructuraDePeones m_Peones = null;
      public bitbrd[][] m_Ataques = new bitbrd[cColor.SIZE][] { new bitbrd[cPieza.SIZE], new bitbrd[cPieza.SIZE] };
      public bitbrd[] m_ReyAcorralado = new bitbrd[cColor.SIZE];
      public int[] m_AtaquesAlRey = new int[cColor.SIZE];
      public int[] m_PesoAtaquesAlRey = new int[cColor.SIZE];
      public int[] m_ZonaDelRey = new int[cColor.SIZE];
      public bitbrd[] m_Clavadas = new bitbrd[cColor.SIZE];
    }
    //---------------------------------------------------------------------------------------------------------------------------
    public static pnt Pnt(int mg, int eg)
    {
      return cTypes.Puntua(mg, eg);
    }

    //---------------------------------------------------------------------------------------------------------------------------
    public static cEvalPesos[] m_Pesos = new cEvalPesos[6];
    public static pnt[] m_PesosInternos = { Pnt(289, 344), Pnt(233, 201), Pnt(221, 273), Pnt(46, 0), Pnt(271, 0), Pnt(307, 0) };

    public static pnt[][] m_Movilidad = new pnt[][] {
             new pnt[]{}, new pnt[]{},
             new pnt[]{     Pnt(-65,-50), Pnt(-42,-30), Pnt(-9,-10), Pnt( 3,  0), Pnt(15, 10), Pnt(27, 20), //-- Caballo
                            Pnt( 37, 28), Pnt( 42, 31), Pnt(44, 33) },
             new pnt[]{     Pnt(-52,-47), Pnt(-28,-23), Pnt( 6,  1), Pnt(20, 15), Pnt(34, 29), Pnt(48, 43), //-- Alfil
                            Pnt( 60, 55), Pnt( 68, 63), Pnt(74, 68), Pnt(77, 72), Pnt(80, 75), Pnt(82, 77),
                            Pnt( 84, 79), Pnt( 86, 81) },
             new pnt[]{     Pnt(-47,-53), Pnt(-31,-26), Pnt(-5,  0), Pnt( 1, 16), Pnt( 7, 32), Pnt(13, 48), //-- Torre
                            Pnt( 18, 64), Pnt( 22, 80), Pnt(26, 96), Pnt(29,109), Pnt(31,115), Pnt(33,119),
                            Pnt( 35,122), Pnt( 36,123), Pnt(37,124) },
             new pnt[]{     Pnt(-42,-40), Pnt(-28,-23), Pnt(-5, -7), Pnt( 0,  0), Pnt( 6, 10), Pnt(11, 19), //-- Dama
                            Pnt( 13, 29), Pnt( 18, 38), Pnt(20, 40), Pnt(21, 41), Pnt(22, 41), Pnt(22, 41),
                            Pnt( 22, 41), Pnt( 23, 41), Pnt(24, 41), Pnt(25, 41), Pnt(25, 41), Pnt(25, 41),
                            Pnt( 25, 41), Pnt( 25, 41), Pnt(25, 41), Pnt(25, 41), Pnt(25, 41), Pnt(25, 41),
                            Pnt( 25, 41), Pnt( 25, 41), Pnt(25, 41), Pnt(25, 41) }
        };

    public static val[][] m_Avanzado = new val[][] {
            new val[]{          
                (0), (0), (0), (0), (0), (0), (0), (0), //-- Caballo
                (0), (0), (0), (0), (0), (0), (0), (0),
                (0), (0), (4), (8), (8), (4), (0), (0),
                (0), (4),(17),(26),(26),(17), (4), (0),
                (0), (8),(26),(35),(35),(26), (8), (0),
                (0), (4),(17),(17),(17),(17), (4), (0),
                (0), (0), (0), (0), (0), (0), (0), (0),
                (0), (0), (0), (0), (0), (0), (0), (0)},
            new val[]{
                (0), (0), (0), (0), (0), (0), (0), (0), //-- Alfil
                (0), (0), (0), (0), (0), (0), (0), (0),
                (0), (0), (5), (5), (5), (5), (0), (0),
                (0), (5),(10),(10),(10),(10), (5), (0),
                (0), (10),(21),(21),(21),(21),(10), (0),
                (0), (5), (8), (8), (8), (8), (5), (0),
                (0), (0), (0), (0), (0), (0), (0), (0),
                (0), (0), (0), (0), (0), (0), (0), (0)}
        };

    public static pnt[][] m_Amenazado = new pnt[][] {
            new pnt[]{ Pnt(0, 0), Pnt( 7, 39), Pnt(24, 49), Pnt(24, 49), Pnt(41,100), Pnt(41,100), Pnt(0, 0)},
            new pnt[]{ Pnt(0, 0), Pnt(15, 39), Pnt(15, 45), Pnt(15, 45), Pnt(15, 45), Pnt(24, 49), Pnt(0, 0)}
        };

    public static pnt[] ThreatenedByPawn = new pnt[] {
            Pnt(0, 0), Pnt(0, 0), Pnt(56, 70), Pnt(56, 70), Pnt(76, 99), Pnt(86, 118)
        };

    public static pnt[] Hanging = new pnt[] {
            Pnt(23, 20) , Pnt(35, 45)
        };

    public static readonly pnt m_Tempo = cTypes.Puntua(24, 11);
    public static readonly pnt m_TorreEnPeon = cTypes.Puntua(10, 28);
    public static readonly pnt m_TorreColAbierta = cTypes.Puntua(43, 21);
    public static readonly pnt m_TorreColumnaSemiAbierta = cTypes.Puntua(19, 10);
    public static readonly pnt m_AlfilEnPeon = cTypes.Puntua(8, 12);
    public static readonly pnt m_CercaDePeon = cTypes.Puntua(16, 0);
    public static readonly pnt m_TorreAtrapada = cTypes.Puntua(90, 0);
    public static readonly pnt m_Imparable = cTypes.Puntua(0, 20);
    public static readonly pnt m_AlfilAtrapado = cTypes.Puntua(50, 50);
    public static UInt64[] m_EspacioMasc = new UInt64[] {
            (cBitBoard.C | cBitBoard.D | cBitBoard.E | cBitBoard.F) & (cBitBoard.F2 | cBitBoard.F3 | cBitBoard.F4),
            (cBitBoard.C | cBitBoard.D | cBitBoard.E | cBitBoard.F) & (cBitBoard.F7 | cBitBoard.F6 | cBitBoard.F5)};

    public static int[] KingAttackWeights = new int[] { 0, 0, 2, 2, 3, 5 };
    public static readonly int QueenContactCheck = 24;
    public static readonly int RookContactCheck = 16;
    public static readonly int QueenCheck = 12;
    public static readonly int RookCheck = 8;
    public static readonly int BishopCheck = 2;
    public static readonly int KnightCheck = 3;
    public static pnt[][] m_ReyAmenazado = new pnt[cColor.SIZE][] { new pnt[128], new pnt[128] };
    public static pnt SetPesos(pnt v, cEvalPesos w)
    {
      return cTypes.Puntua(cTypes.GetMedioJuego(v) * w.m_nMedioJ / 256, cTypes.GetFinalJuego(v) * w.m_nFinal / 256);
    }

    //----------------------------------------------------------------------------------------------------------
    public static cEvalPesos Weight_option(string mgOpt, string egOpt, pnt internalWeight)
    {
      return new cEvalPesos(cMotor.m_mapConfig[mgOpt].Get() * cTypes.GetMedioJuego(internalWeight) / 100,
        cMotor.m_mapConfig[egOpt].Get() * cTypes.GetFinalJuego(internalWeight) / 100);
    }

    //----------------------------------------------------------------------------------------------------------
    public static void Init(cPosicion pos, cEvalInfo evalInfo, color colr)
    {
      color colorVS = (colr == cColor.BLANCO ? cColor.NEGRO : cColor.BLANCO);
      sq Down = (colr == cColor.BLANCO ? cCasilla.SUR : cCasilla.NORTE);

      evalInfo.m_Clavadas[colr] = pos.PiezasClavadas(colr);
      if(cTypes.IsCasillaOcupable(pos.GetRey(colorVS)) && pos.m_Mission[colorVS].m_nMissionType == cMission.CHECKMATE)
      {
        bitbrd b = evalInfo.m_Ataques[colorVS][cPieza.REY] = pos.AtaquesDesdeTipoDePieza(pos.GetRey(colorVS), cPieza.REY);

        evalInfo.m_Ataques[colr][cPieza.NAN] = evalInfo.m_Ataques[colr][cPieza.PEON] = evalInfo.m_Peones.GetPeonesAtaques(colr);

        if(pos.GetNum(colr, cPieza.DAMA) != 0 && pos.MaterialPieza(colr) > cValoresJuego.DAMA_MJ + cValoresJuego.PEON_MJ)
        {
          evalInfo.m_ReyAcorralado[colorVS] = b | cBitBoard.Desplazamiento(b, Down);
          b &= evalInfo.m_Ataques[colr][cPieza.PEON];
          evalInfo.m_AtaquesAlRey[colr] = (b != 0) ? cBitBoard.CountMax15(b) : 0;
          evalInfo.m_ZonaDelRey[colr] = evalInfo.m_PesoAtaquesAlRey[colr] = 0;
        }
        else
        {
          evalInfo.m_ReyAcorralado[colorVS] = 0;
          evalInfo.m_AtaquesAlRey[colr] = 0;
        }
      }
    }

    //----------------------------------------------------------------------------------------------------------
    public static pnt Evaluate_outposts(cPosicion pos, cEvalInfo ei, sq s, type Pt, color colr)
    {
      color colorVS = (colr == cColor.BLANCO ? cColor.NEGRO : cColor.BLANCO);

      val bonus = m_Avanzado[Pt == cPieza.ALFIL ? 1 : 0][cTypes.CasillaProxima(colr, s)];

      if (bonus != 0 && (ei.m_Ataques[colr][cPieza.PEON] & cBitBoard.m_nCasillas[s]) != 0)
      {
        if (0 == pos.PiezasColor(colorVS, cPieza.CABALLO) && 0 == (cBitBoard.GetColor(s) & pos.PiezasColor(colorVS, cPieza.ALFIL)))
          bonus += bonus + bonus / 2;
        else
          bonus += bonus / 2;
      }
      return cTypes.Puntua(bonus, bonus);
    }

    //----------------------------------------------------------------------------------------------------------
    public static pnt EvalPiezas(cPosicion pos, cEvalInfo ei, pnt[] mobility, bitbrd[] mobilityArea, type Pt, color colr)
    {
      if (colr == cColor.BLANCO && Pt == cPieza.REY)
      {
        return 0;
      }

      bitbrd b;
      sq s;
      pnt nPuntos = 0;

      type NextPt = (colr == cColor.BLANCO ? Pt : (Pt + 1));
      color colorVS = (colr == cColor.BLANCO ? cColor.NEGRO : cColor.BLANCO);
      sq[] pl = pos.GetList(colr, Pt);
      int plPos = 0;

      ei.m_Ataques[colr][Pt] = 0;

      while ((s = pl[plPos++]) != cCasilla.NONE)
      {
        b = Pt == cPieza.ALFIL ? cBitBoard.AtaquesPieza(s, pos.Piezas() ^ pos.PiezasColor(colr, cPieza.DAMA), cPieza.ALFIL)
          : Pt == cPieza.TORRE ? cBitBoard.AtaquesPieza(s, pos.Piezas() ^ pos.PiezasColor(colr, cPieza.TORRE, cPieza.DAMA), cPieza.TORRE)
                            : pos.AtaquesDesdeTipoDePieza(s, Pt);

        if ((ei.m_Clavadas[colr] & cBitBoard.m_nCasillas[s]) != 0)
          b &= cBitBoard.m_EnLinea[pos.GetRey(colr)][s];

        ei.m_Ataques[colr][cPieza.NAN] |= ei.m_Ataques[colr][Pt] |= b;

        if ((b & ei.m_ReyAcorralado[colorVS]) != 0)
        {
          ei.m_AtaquesAlRey[colr]++;
          ei.m_PesoAtaquesAlRey[colr] += KingAttackWeights[Pt];
          bitbrd bb = (b & ei.m_Ataques[colorVS][cPieza.REY]);
          if (bb != 0)
            ei.m_ZonaDelRey[colr] += cBitBoard.CountMax15(bb);
        }

        if (Pt == cPieza.DAMA)
          b &= ~(ei.m_Ataques[colorVS][cPieza.CABALLO]
                 | ei.m_Ataques[colorVS][cPieza.ALFIL]
                 | ei.m_Ataques[colorVS][cPieza.TORRE]);

        int mob = (Pt != cPieza.DAMA ? cBitBoard.CountMax15(b & mobilityArea[colr])
                                          : cBitBoard.Count(b & mobilityArea[colr]));

        mobility[colr] += m_Movilidad[Pt][mob];

        if ((ei.m_Ataques[colorVS][cPieza.PEON] & cBitBoard.m_nCasillas[s]) != 0)
          nPuntos -= ThreatenedByPawn[Pt];

        if (Pt == cPieza.ALFIL || Pt == cPieza.CABALLO)
        {
          if (Pt == cPieza.ALFIL)
            nPuntos -= m_AlfilEnPeon * ei.m_Peones.PeonesMismoColor(colr, s);
          if (0 == (pos.PiezasColor(colorVS, cPieza.PEON) & cBitBoard.AtaquePeon(colr, s)))
            nPuntos += Evaluate_outposts(pos, ei, s, Pt, colr);

          if (cTypes.FilaProxima(colr, s) < FILA.F5 && (pos.GetNumPiezas(cPieza.PEON) & cBitBoard.m_nCasillas[(s + cTypes.AvancePeon(colr))]) != 0)
            nPuntos += m_CercaDePeon;
        }

        if (Pt == cPieza.TORRE)
        {
          if (cTypes.FilaProxima(colr, s) >= FILA.F5)
          {
            bitbrd pawns = pos.PiezasColor(colorVS, cPieza.PEON) & cBitBoard.m_PseudoAtaques[cPieza.TORRE][s];
            if (pawns != 0)
              nPuntos += cBitBoard.CountMax15(pawns) * m_TorreEnPeon;
          }

          if (ei.m_Peones.SemiAbierta(colr, cTypes.Columna(s)) != 0)
            nPuntos += ei.m_Peones.SemiAbierta(colorVS, cTypes.Columna(s)) != 0 ? m_TorreColAbierta : m_TorreColumnaSemiAbierta;

          if (mob > 3 || ei.m_Peones.SemiAbierta(colr, cTypes.Columna(s)) != 0)
            continue;

          sq ksq = pos.GetRey(colr);

          if (((cTypes.Columna(ksq) < COLUMNA.E) == (cTypes.Columna(s) < cTypes.Columna(ksq)))
              && (cTypes.Fila(ksq) == cTypes.Fila(s) || cTypes.FilaProxima(colr, ksq) == FILA.F1)
              && 0 == ei.m_Peones.SemiAbierta(colr, cTypes.Columna(ksq), cTypes.Columna(s) < cTypes.Columna(ksq)))
            nPuntos -= (m_TorreAtrapada - cTypes.Puntua(mob * 8, 0)) * (1 + (pos.CanEnroque(colr) == 0 ? 1 : 0));
        }

        if (Pt == cPieza.ALFIL
            && pos.IsChess960() != false
            && (s == cTypes.CasillaProxima(colr, cCasilla.A1) || s == cTypes.CasillaProxima(colr, cCasilla.H1)))
        {
          sq d = cTypes.AvancePeon(colr) + (cTypes.Columna(s) == COLUMNA.A ? cCasilla.ESTE : cCasilla.OESTE);
          if (pos.GetPieza(s + d) == cTypes.CreaPieza(colr, cPieza.PEON))
            nPuntos -= !pos.CasillaVacia(s + d + cTypes.AvancePeon(colr)) ? m_AlfilAtrapado * 4
                : pos.GetPieza(s + d + d) == cTypes.CreaPieza(colr, cPieza.PEON) ? m_AlfilAtrapado * 2
                                                                : m_AlfilAtrapado;
        }
      }

      return nPuntos - EvalPiezas(pos, ei, mobility, mobilityArea, NextPt, colorVS);
      /*
      type NextPt = (colr == cColor.BLANCO ? Pt : (Pt + 1));
      bitbrd b;
      sq s;
      pnt nPuntos = 0;
      while(Pt != cPieza.REY)
      {
        if (colr != cColor.BLANCO || Pt != cPieza.REY)
        {
          color colorVS = (colr == cColor.BLANCO ? cColor.NEGRO : cColor.BLANCO);
          sq[] pl = pos.GetList(colr, Pt);
          int plPos = 0;

          ei.m_Ataques[colr][Pt] = 0;

          while (plPos < cCasilla.ESCAQUES && (s = pl[plPos++]) != cCasilla.NONE)
          {
            b = Pt == cPieza.ALFIL ? cBitBoard.AtaquesPieza(s, pos.Piezas() ^ pos.PiezasColor(colr, cPieza.DAMA), cPieza.ALFIL)
              : Pt == cPieza.TORRE ? cBitBoard.AtaquesPieza(s, pos.Piezas() ^ pos.PiezasColor(colr, cPieza.TORRE, cPieza.DAMA), cPieza.TORRE)
                                : pos.AtaquesDesdeTipoDePieza(s, Pt);

            if ((ei.m_Clavadas[colr] & cBitBoard.m_nCasillas[s]) != 0)
              b &= cBitBoard.m_EnLinea[pos.GetRey(colr)][s];

            ei.m_Ataques[colr][cPieza.NAN] |= ei.m_Ataques[colr][Pt] |= b;

            if ((b & ei.m_ReyAcorralado[colorVS]) != 0)
            {
              ei.m_AtaquesAlRey[colr]++;
              ei.m_PesoAtaquesAlRey[colr] += KingAttackWeights[Pt];
              bitbrd bb = (b & ei.m_Ataques[colorVS][cPieza.REY]);
              if (bb != 0)
                ei.m_ZonaDelRey[colr] += cBitBoard.CountMax15(bb);
            }

            if (Pt == cPieza.DAMA)
              b &= ~(ei.m_Ataques[colorVS][cPieza.CABALLO]
                     | ei.m_Ataques[colorVS][cPieza.ALFIL]
                     | ei.m_Ataques[colorVS][cPieza.TORRE]);

            int mob = (Pt != cPieza.DAMA ? cBitBoard.CountMax15(b & mobilityArea[colr])
                                              : cBitBoard.Count(b & mobilityArea[colr]));

            mobility[colr] += m_Movilidad[Pt][mob];

            if ((ei.m_Ataques[colorVS][cPieza.PEON] & cBitBoard.m_nCasillas[s]) != 0)
              nPuntos -= ThreatenedByPawn[Pt];

            if (Pt == cPieza.ALFIL || Pt == cPieza.CABALLO)
            {
              if (Pt == cPieza.ALFIL)
                nPuntos -= m_AlfilEnPeon * ei.m_Peones.PeonesMismoColor(colr, s);
              if (0 == (pos.PiezasColor(colorVS, cPieza.PEON) & cBitBoard.AtaquePeon(colr, s)))
                nPuntos += Evaluate_outposts(pos, ei, s, Pt, colr);

              if (cTypes.FilaProxima(colr, s) < FILA.F5 && (pos.GetNumPiezas(cPieza.PEON) & cBitBoard.m_nCasillas[(s + cTypes.AvancePeon(colr))]) != 0)
                nPuntos += m_CercaDePeon;
            }

            if (Pt == cPieza.TORRE)
            {
              if (cTypes.FilaProxima(colr, s) >= FILA.F5)
              {
                bitbrd pawns = pos.PiezasColor(colorVS, cPieza.PEON) & cBitBoard.m_PseudoAtaques[cPieza.TORRE][s];
                if (pawns != 0)
                  nPuntos += cBitBoard.CountMax15(pawns) * m_TorreEnPeon;
              }

              if (ei.m_Peones.SemiAbierta(colr, cTypes.Columna(s)) != 0)
                nPuntos += ei.m_Peones.SemiAbierta(colorVS, cTypes.Columna(s)) != 0 ? m_TorreColAbierta : m_TorreColumnaSemiAbierta;

              if (mob > 3 || ei.m_Peones.SemiAbierta(colr, cTypes.Columna(s)) != 0)
                continue;

              sq ksq = pos.GetRey(colr);

              if (((cTypes.Columna(ksq) < COLUMNA.E) == (cTypes.Columna(s) < cTypes.Columna(ksq)))
                  && (cTypes.Fila(ksq) == cTypes.Fila(s) || cTypes.FilaProxima(colr, ksq) == FILA.F1)
                  && 0 == ei.m_Peones.SemiAbierta(colr, cTypes.Columna(ksq), cTypes.Columna(s) < cTypes.Columna(ksq)))
                nPuntos -= (m_TorreAtrapada - cTypes.Puntua(mob * 8, 0)) * (1 + (pos.CanEnroque(colr) == 0 ? 1 : 0));
            }

            if (Pt == cPieza.ALFIL
                && pos.IsChess960() != false
                && (s == cTypes.CasillaProxima(colr, cCasilla.A1) || s == cTypes.CasillaProxima(colr, cCasilla.H1)))
            {
              sq d = cTypes.AvancePeon(colr) + (cTypes.Columna(s) == COLUMNA.A ? cCasilla.ESTE : cCasilla.OESTE);
              if (pos.GetPieza(s + d) == cTypes.CreaPieza(colr, cPieza.PEON))
                nPuntos -= !pos.CasillaVacia(s + d + cTypes.AvancePeon(colr)) ? m_AlfilAtrapado * 4
                    : pos.GetPieza(s + d + d) == cTypes.CreaPieza(colr, cPieza.PEON) ? m_AlfilAtrapado * 2
                                                                    : m_AlfilAtrapado;
            }
          }

          Pt = (colr == cColor.BLANCO ? Pt : (Pt + 1));
          colr = colorVS;
        }
      }

      
      return nPuntos;*/
    }

    //----------------------------------------------------------------------------------------------------------
    public static pnt EvalRey(cPosicion pos, cEvalInfo ei, color colr)
    {
      color colorVS = (colr == cColor.BLANCO ? cColor.NEGRO : cColor.BLANCO);

      bitbrd indefenso, b, b1, b2, safe;
      int attackUnits;
      sq nCasillaRey = pos.GetRey(colr);
      pnt score = 0;

      if (cTypes.IsCasillaOcupable(nCasillaRey))
      {
        //score = ei.m_Peones.ReyProtegido(pos, nCasillaRey, colr);

        if (ei.m_AtaquesAlRey[colorVS] != 0)
        {
          indefenso = ei.m_Ataques[colorVS][cPieza.NAN] & ei.m_Ataques[colr][cPieza.REY] & ~(ei.m_Ataques[colr][cPieza.PEON] | ei.m_Ataques[colr][cPieza.CABALLO]
                  | ei.m_Ataques[colr][cPieza.ALFIL] | ei.m_Ataques[colr][cPieza.TORRE] | ei.m_Ataques[colr][cPieza.DAMA]);

          attackUnits = Math.Min(20, (ei.m_AtaquesAlRey[colorVS] * ei.m_PesoAtaquesAlRey[colorVS]) / 2)
                       + 3 * (ei.m_ZonaDelRey[colorVS] + cBitBoard.CountMax15(indefenso))
                       + 2 * (ei.m_Clavadas[colr] != 0 ? 1 : 0)
                       - cTypes.GetMedioJuego(score) / 32;

          b = indefenso & ei.m_Ataques[colorVS][cPieza.DAMA] & ~pos.PiezasColor(colorVS);
          if (b != 0)
          {
            b &= (ei.m_Ataques[colorVS][cPieza.PEON] | ei.m_Ataques[colorVS][cPieza.CABALLO]
                  | ei.m_Ataques[colorVS][cPieza.ALFIL] | ei.m_Ataques[colorVS][cPieza.TORRE]);
            if (b != 0)
              attackUnits += QueenContactCheck
                            * cBitBoard.CountMax15(b)
                            * (colorVS == pos.ColorMueve() ? 2 : 1);
          }

          b = indefenso & ei.m_Ataques[colorVS][cPieza.TORRE] & ~pos.PiezasColor(colorVS);


          b &= cBitBoard.m_PseudoAtaques[cPieza.TORRE][nCasillaRey];

          if (b != 0)
          {

            b &= (ei.m_Ataques[colorVS][cPieza.PEON] | ei.m_Ataques[colorVS][cPieza.CABALLO]
                  | ei.m_Ataques[colorVS][cPieza.ALFIL] | ei.m_Ataques[colorVS][cPieza.DAMA]);

            if (b != 0)
              attackUnits += RookContactCheck
                            * cBitBoard.CountMax15(b)
                            * (colorVS == pos.ColorMueve() ? 2 : 1);
          }

          safe = ~(pos.PiezasColor(colorVS) | ei.m_Ataques[colr][cPieza.NAN]);

          b1 = pos.AtaquesDesdeTipoDePieza(nCasillaRey, cPieza.TORRE) & safe;
          b2 = pos.AtaquesDesdeTipoDePieza(nCasillaRey, cPieza.ALFIL) & safe;


          b = (b1 | b2) & ei.m_Ataques[colorVS][cPieza.DAMA];
          if (b != 0)
            attackUnits += QueenCheck * cBitBoard.CountMax15(b);

          b = b1 & ei.m_Ataques[colorVS][cPieza.TORRE];
          if (b != 0)
            attackUnits += RookCheck * cBitBoard.CountMax15(b);

          b = b2 & ei.m_Ataques[colorVS][cPieza.ALFIL];
          if (b != 0)
            attackUnits += BishopCheck * cBitBoard.CountMax15(b);

          b = pos.AtaquesDesdeTipoDePieza(nCasillaRey, cPieza.CABALLO) & ei.m_Ataques[colorVS][cPieza.CABALLO] & safe;
          if (b != 0)
            attackUnits += KnightCheck * cBitBoard.CountMax15(b);

          attackUnits = Math.Min(99, Math.Max(0, attackUnits));

          score -= m_ReyAmenazado[colr == cSearch.RootColor ? 1 : 0][attackUnits];
        }
      }

      return score;
    }

    //-----------------------------------------------------------------------------------------------------------------------------------
    public static pnt EvalAmenazas(cPosicion pos, cEvalInfo evalInfo, color colr)
    {
      color colorVS = (colr == cColor.BLANCO ? cColor.NEGRO : cColor.BLANCO);

      bitbrd b, weakEnemies;
      pnt nPuntos = 0;

      if (pos.m_Mission[colr].m_nMissionType == cMission.CHECKMATE)
      {
        weakEnemies = pos.PiezasColor(colorVS) & ~evalInfo.m_Ataques[colorVS][cPieza.PEON] & evalInfo.m_Ataques[colr][cPieza.NAN];

        if (weakEnemies != 0)
        {
          b = weakEnemies & (evalInfo.m_Ataques[colr][cPieza.PEON] | evalInfo.m_Ataques[colr][cPieza.CABALLO] | evalInfo.m_Ataques[colr][cPieza.ALFIL]);
          if (b != 0)
          {
            int nIndexAmenazado = cTypes.TipoPieza(pos.GetPieza(cBitBoard.LSB(b)));
            nPuntos += m_Amenazado[0][nIndexAmenazado];
          }

          b = weakEnemies & (evalInfo.m_Ataques[colr][cPieza.TORRE] | evalInfo.m_Ataques[colr][cPieza.DAMA]);
          if (b != 0)
          {
            int nIndexAmenazado = cTypes.TipoPieza(pos.GetPieza(cBitBoard.LSB(b)));
            nPuntos += m_Amenazado[1][nIndexAmenazado];
          }

          b = weakEnemies & ~evalInfo.m_Ataques[colorVS][cPieza.NAN];

          if (b != 0)
            nPuntos += cBitBoard.MayorQue(b) ? Hanging[colr != pos.ColorMueve() ? 1 : 0] * cBitBoard.CountMax15(b) : Hanging[colr == pos.ColorMueve() ? 1 : 0];
        }
      }

      return nPuntos;
    }

    //-----------------------------------------------------------------------------------------------------------------------------------
    public static pnt EvalPeonesPasados(cPosicion pos, cEvalInfo ei, color colr)
    {
      color colorVS = (colr == cColor.BLANCO ? cColor.NEGRO : cColor.BLANCO);

      bitbrd b, squaresToQueen, defendedSquares, unsafeSquares;
      pnt nPuntos = 0;

      b = ei.m_Peones.GetPeonesPasados(colr);

      while (b != 0)
      {
        sq casilla = cBitBoard.GetLSB(ref b);

        int r = (int)(cTypes.FilaProxima(colr, casilla) - FILA.F2);
        int rr = r * (r - 1);

        val mbonus = (17 * rr), ebonus = (7 * (rr + r + 1));

        if (rr != 0)
        {
          sq blockSq = casilla + cTypes.AvancePeon(colr);
          if(cTypes.IsCasillaOcupable(pos.GetRey(colorVS)) && cTypes.IsCasillaOcupable(pos.GetRey(colr)) && cTypes.IsCasillaOcupable(blockSq))
          {
            ebonus += (cBitBoard.Distancia(pos.GetRey(colorVS), blockSq) * 5 * rr)
                 - (cBitBoard.Distancia(pos.GetRey(colr), blockSq) * 2 * rr);
            
            if(cTypes.FilaProxima(colr, blockSq) != FILA.F8)
              ebonus -= (cBitBoard.Distancia(pos.GetRey(colr), blockSq + cTypes.AvancePeon(colr)) * rr);
          }
          if (cTypes.IsCasillaOcupable(blockSq) && pos.CasillaVacia(blockSq))
          {
            squaresToQueen = cBitBoard.Atras(colr, casilla);
            if ((cBitBoard.Atras(colorVS, casilla) & pos.PiezasColor(colorVS, cPieza.TORRE, cPieza.DAMA)) != 0
                && (cBitBoard.Atras(colorVS, casilla) & pos.PiezasColor(colorVS, cPieza.TORRE, cPieza.DAMA) & pos.AtaquesDesdeTipoDePieza(casilla, cPieza.TORRE)) != 0)
              unsafeSquares = squaresToQueen;
            else
              unsafeSquares = squaresToQueen & (ei.m_Ataques[colorVS][cPieza.NAN] | pos.PiezasColor(colorVS));

            if ((cBitBoard.Atras(colorVS, casilla) & pos.PiezasColor(colr, cPieza.TORRE, cPieza.DAMA)) != 0
                && (cBitBoard.Atras(colorVS, casilla) & pos.PiezasColor(colr, cPieza.TORRE, cPieza.DAMA) & pos.AtaquesDesdeTipoDePieza(casilla, cPieza.TORRE)) != 0)
              defendedSquares = squaresToQueen;
            else
              defendedSquares = squaresToQueen & ei.m_Ataques[colr][cPieza.NAN];

            int k = 0 == unsafeSquares ? 15 : 0 == (unsafeSquares & cBitBoard.m_nCasillas[blockSq]) ? 9 : 0;

            if (defendedSquares == squaresToQueen)
              k += 6;

            else if ((defendedSquares & cBitBoard.m_nCasillas[blockSq]) != 0)
              k += 4;

            mbonus += (k * rr); ebonus += (k * rr);
          }
        } //-- rr != 0

        if (pos.GetNum(colr, cPieza.PEON) < pos.GetNum(colorVS, cPieza.PEON))
          ebonus += ebonus / 4;

        nPuntos += cTypes.Puntua(mbonus, ebonus);

      }

      return cEval.SetPesos(nPuntos, m_Pesos[cEvalPesos.PEONES_PASADOS]);
    }

    //-----------------------------------------------------------------------------------------------------------------------------------
    public static pnt EvalPeonImparable(cPosicion pos, color colr, cEvalInfo ei)
    {
      bitbrd b = ei.m_Peones.GetPeonesPasados(colr) | ei.m_Peones.GetPeonesACoronar(colr);

      if (0 == b || pos.MaterialPieza(cTypes.Contrario(colr)) != 0)
        return 0;

      return m_Imparable * (cTypes.FilaProxima(colr, cBitBoard.Adelantado(colr, b)));
    }

    //-----------------------------------------------------------------------------------------------------------------------------------
    public static int EvalEspacio(cPosicion pos, cEvalInfo ei, color colr)
    {
      color colorVS = (colr == cColor.BLANCO ? cColor.NEGRO : cColor.BLANCO);

      bitbrd safe = m_EspacioMasc[colr]
                 & ~pos.PiezasColor(colr, cPieza.PEON)
                 & ~ei.m_Ataques[colorVS][cPieza.PEON]
                 & (ei.m_Ataques[colr][cPieza.NAN] | ~ei.m_Ataques[colorVS][cPieza.NAN]);

      bitbrd behind = pos.PiezasColor(colr, cPieza.PEON);
      behind |= (colr == cColor.BLANCO ? behind >> 8 : behind << 8);
      behind |= (colr == cColor.BLANCO ? behind >> 16 : behind << 16);

      return cBitBoard.Count((colr == cColor.BLANCO ? safe << 32 : safe >> 32) | (behind & safe));
    }

    //-----------------------------------------------------------------------------------------------------------------------------------
    public static val Eval(cPosicion pos/*, int nDepth*/)
    {
      cEvalInfo eval = new cEvalInfo();
      pnt nPuntos = 0;
      pnt[] movilidad = new pnt[] { 0, 0 };
      cThread thread = pos.ThreadActive();
      nPuntos = pos.GetPuntosPeon() + (pos.ColorMueve() == cColor.BLANCO ? m_Tempo : -m_Tempo);

      eval.m_Material = thread.m_Material[pos.ClaveHashMaterial()];

      if (pos.m_Mission[pos.ColorMueve()].m_nMissionType == cMission.CHECKMATE && pos.HasObstacles() == false)
      {
        if (thread.m_Finales == null)
          thread.m_Finales = new cListaDeFinales();

        eval.m_Material = cMaterial.BuscarFinal(pos, thread.m_Material, thread.m_Finales);
        nPuntos += eval.m_Material.Valor();
        
        if (cTypes.IsCasillaOcupable(pos.GetRey(cColor.BLANCO)) && cTypes.IsCasillaOcupable(pos.GetRey(cColor.NEGRO)))
        {
          if (eval.m_Material.FuncionEvaluacion())
          {
            
            pnt ret = eval.m_Material.EvalFinal(pos);
            
            return ret;
          }
        }
      }
      
      eval.m_Peones = cBonusPeones.Buscar(pos, thread.m_TablaPeones);
      
      Init(pos, eval, cColor.BLANCO);
      Init(pos, eval, cColor.NEGRO);

      nPuntos += pos.EvalMissionPuntosEval(cColor.BLANCO) - pos.EvalMissionPuntosEval(cColor.NEGRO);

      nPuntos += SetPesos(eval.m_Peones.GetValue(), m_Pesos[cEvalPesos.ESTRUCTURA_PEONES]);

      eval.m_Ataques[cColor.BLANCO][cPieza.NAN] |= eval.m_Ataques[cColor.BLANCO][cPieza.REY];
      eval.m_Ataques[cColor.NEGRO][cPieza.NAN] |= eval.m_Ataques[cColor.NEGRO][cPieza.REY];

      bitbrd[] areaMovilidad = new bitbrd[]{   ~(eval.m_Ataques[cColor.NEGRO][cPieza.PEON] | pos.PiezasColor(cColor.BLANCO, cPieza.PEON, cPieza.REY)),
        ~(eval.m_Ataques[cColor.BLANCO][cPieza.PEON] | pos.PiezasColor(cColor.NEGRO, cPieza.PEON, cPieza.REY)) };
      nPuntos += EvalPiezas(pos, eval, movilidad, areaMovilidad, cPieza.CABALLO, cColor.BLANCO);
      nPuntos += cEval.SetPesos(movilidad[cColor.BLANCO] - movilidad[cColor.NEGRO], m_Pesos[cEvalPesos.MOVILIDAD]);

      if (pos.m_Mission[pos.ColorMueve()].m_nMissionType == cMission.CHECKMATE)
      {
        nPuntos += EvalRey(pos, eval, cColor.BLANCO) - EvalRey(pos, eval, cColor.NEGRO);
      }
      nPuntos += EvalAmenazas(pos, eval, cColor.BLANCO) - EvalAmenazas(pos, eval, cColor.NEGRO);
      nPuntos += EvalPeonesPasados(pos, eval, cColor.BLANCO) - EvalPeonesPasados(pos, eval, cColor.NEGRO);

      if (0 == pos.MaterialPieza(cColor.BLANCO) || 0 == pos.MaterialPieza(cColor.NEGRO))
        nPuntos += EvalPeonImparable(pos, cColor.BLANCO, eval) - EvalPeonImparable(pos, cColor.NEGRO, eval);

      if (eval.m_Material.Peso() != 0)
      {
        int s = EvalEspacio(pos, eval, cColor.BLANCO) - EvalEspacio(pos, eval, cColor.NEGRO);
        nPuntos += cEval.SetPesos(s * eval.m_Material.Peso(), m_Pesos[cEvalPesos.ESPACIO]);
      }

      factor sf = cFactorEscala.NORMAL;
      if (pos.m_Mission[pos.ColorMueve()].m_nMissionType == cMission.CHECKMATE && pos.HasObstacles() == false)
      {
        if (cTypes.IsCasillaOcupable(pos.GetRey(cColor.BLANCO)) && cTypes.IsCasillaOcupable(pos.GetRey(cColor.NEGRO)))
        {
          //cMotor.m_Threads.mutex.Bloquear();
          
          
          sf = cTypes.GetFinalJuego(nPuntos) > cValoresJuego.TABLAS ? eval.m_Material.FactorDeEscalaFinal(pos, cColor.BLANCO)
                                                                   : eval.m_Material.FactorDeEscalaFinal(pos, cColor.NEGRO);
          //cMotor.m_Threads.mutex.Liberar();
          if (eval.m_Material.Fase() < cFaseJuego.MEDIO_JUEGO
            && pos.AlfilesOpuestos()
            && (sf == cFactorEscala.NORMAL || sf == cFactorEscala.PEON))
          {
            if (pos.MaterialPieza(cColor.BLANCO) == cValoresJuego.ALFIL_MJ
                && pos.MaterialPieza(cColor.NEGRO) == cValoresJuego.ALFIL_MJ)
            {
              bool one_pawn = (pos.GetNum(cColor.BLANCO, cPieza.PEON) + pos.GetNum(cColor.NEGRO, cPieza.PEON) == 1);
              sf = one_pawn ? (8) : (32);
            }
            else
              sf = (50 * sf / cFactorEscala.NORMAL);
          }
        }
      }
      
      val v = cTypes.GetMedioJuego(nPuntos) * (eval.m_Material.Fase())
               + cTypes.GetFinalJuego(nPuntos) * (cFaseJuego.MEDIO_JUEGO - eval.m_Material.Fase()) * sf / cFactorEscala.NORMAL;

      v /= (cFaseJuego.MEDIO_JUEGO);
      
      return pos.ColorMueve() == cColor.BLANCO ? v : -v;
    }

    //---------------------------------------------------------------------------------------------------------------------------
    public static void Init()
    {
      m_Pesos[cEvalPesos.MOVILIDAD] = Weight_option("MOVILIDAD_MEDIO_JUEGO", "MOVILIDAD_FINAL", m_PesosInternos[cEvalPesos.MOVILIDAD]);
      m_Pesos[cEvalPesos.ESTRUCTURA_PEONES] = Weight_option("PEON_DEFENDIDO_MEDIO_JUEGO", "PEON_DEFENDIDO_FINAL", m_PesosInternos[cEvalPesos.ESTRUCTURA_PEONES]);
      m_Pesos[cEvalPesos.PEONES_PASADOS] = Weight_option("PEON_PASADO_MEDIO_JUEGO", "PEON_PASADO_FINAL", m_PesosInternos[cEvalPesos.PEONES_PASADOS]);
      m_Pesos[cEvalPesos.ESPACIO] = Weight_option("ESPACIOS", "ESPACIOS", m_PesosInternos[cEvalPesos.ESPACIO]);
      m_Pesos[cEvalPesos.REY_AMENAZADO] = Weight_option("JUEGO_DEFENSIVO", "JUEGO_DEFENSIVO", m_PesosInternos[cEvalPesos.REY_AMENAZADO]);
      m_Pesos[cEvalPesos.REY_AMENAZADO_VS] = Weight_option("JUEGO_AGRESIVO", "JUEGO_AGRESIVO", m_PesosInternos[cEvalPesos.REY_AMENAZADO_VS]);

      const int MaxSlope = 30;
      const int Peak = 1280;

      for (int t = 0, i = 1; i < 100; ++i)
      {
        t = Math.Min(Peak, Math.Min((int)(0.4 * i * i), t + MaxSlope));

        m_ReyAmenazado[1][i] = cEval.SetPesos(cTypes.Puntua(t, 0), m_Pesos[cEvalPesos.REY_AMENAZADO]);
        m_ReyAmenazado[0][i] = cEval.SetPesos(cTypes.Puntua(t, 0), m_Pesos[cEvalPesos.REY_AMENAZADO_VS]);
      }
    }
  }

  
}
