using InOut;
using System;
using System.Collections.Generic;
using Types;

using mov = System.Int32;

namespace Motor
{
  //---------------------------------------------------------------------------------------------
  //---------------------------------------------------------------------------------------------
  public partial class cMotor
  {
    //-- LCT2
    //-- https://chessprogramming.wikispaces.com/LCT+II
    
    private static int[] Times = new int[] { 90, 290, 890, 2090, 3890, 6000 };

    private static string[,] Defaults = new string[,]
          {
            {"r3kb1r/3n1pp1/p6p/2pPp2q/Pp2N3/3B2PP/1PQ2P2/R3K2R w KQkq - ", "d5d6"},
            {"1k1r3r/pp2qpp1/3b1n1p/3pNQ2/2pP1P2/2N1P3/PP4PP/1K1RR3 b - - ", "d6b4"},
            {"r6k/pp4p1/2p1b3/3pP3/7q/P2B3r/1PP2Q1P/2K1R1R1 w - - ", "f2c5"},
            {"1nr5/2rbkppp/p3p3/Np6/2PRPP2/8/PKP1B1PP/3R4 b - - ", "e6e5"},
            {"2r2rk1/1p1bq3/p3p2p/3pPpp1/1P1Q4/P7/2P2PPP/2R1RBK1 b - - ", "d7b5"},
            {"3r1bk1/p4ppp/Qp2p3/8/1P1B4/Pq2P1P1/2r2P1P/R3R1K1 b - - ", "e6e5"},
            {"r1b2r1k/pp2q1pp/2p2p2/2p1n2N/4P3/1PNP2QP/1PP2RP1/5RK1 w - - ", "c3d1"},
            {"r2qrnk1/pp3ppb/3b1n1p/1Pp1p3/2P1P2N/P5P1/1B1NQPBP/R4RK1 w - - ", "g2h3"},
            {"5nk1/Q4bpp/5p2/8/P1n1PN2/q4P2/6PP/1R4K1 w - - ", "a7d4"},
            {"r3k2r/3bbp1p/p1nppp2/5P2/1p1NP3/5NP1/PPPK3P/3R1B1R b kq - ", "e7f8"},
            {"bn6/1q4n1/1p1p1kp1/2pPp1pp/1PP1P1P1/3N1P1P/4B1K1/2Q2N2 w - - ", "h3h4"},
            {"3r2k1/pp2npp1/2rqp2p/8/3PQ3/1BR3P1/PP3P1P/3R2K1 b - - ", "c6b6"},
            {"1r2r1k1/4ppbp/B5p1/3P4/pp1qPB2/2n2Q1P/P4PP1/4RRK1 b - - ", "c3a2"},
            {"r2qkb1r/1b3ppp/p3pn2/1p6/1n1P4/1BN2N2/PP2QPPP/R1BR2K1 w kq - ", "d4d5"},
            {"1r4k1/1q2bp2/3p2p1/2pP4/p1N4R/2P2QP1/1P3PK1/8 w - - ", "c4d6"},
            {"rn3rk1/pbppq1pp/1p2pb2/4N2Q/3PN3/3B4/PPP2PPP/R3K2R w KQ - ", "h5h7"},
            {"4r1k1/3b1p2/5qp1/1BPpn2p/7n/r3P1N1/2Q1RPPP/1R3NK1 b - - ", "f6f3"},
            {"2k2b1r/1pq3p1/2p1pp2/p1n1PnNp/2P2B2/2N4P/PP2QPP1/3R2K1 w - - ", "e5f6"},
            {"2r2r2/3qbpkp/p3n1p1/2ppP3/6Q1/1P1B3R/PBP3PP/5R1K w - - ", "h3h7"},
            {"2r1k2r/2pn1pp1/1p3n1p/p3PP2/4q2B/P1P5/2Q1N1PP/R4RK1 w q - ", "e5f6"},
            {"2rr2k1/1b3ppp/pb2p3/1p2P3/1P2BPnq/P1N3P1/1B2Q2P/R4R1K b - - ", "c8c3"},
            {"2b1r1k1/r4ppp/p7/2pNP3/4Q3/q6P/2P2PP1/3RR1K1 w - - ", "d5f6"},
            {"6k1/5p2/3P2p1/7n/3QPP2/7q/r2N3P/6RK b - - ", "a2d2"},
            {"rq2rbk1/6p1/p2p2Pp/1p1Rn3/4PB2/6Q1/PPP1B3/2K3R1 w - - ", "f4h6"},
            {"rnbq2k1/p1r2p1p/1p1p1Pp1/1BpPn1N1/P7/2P5/6PP/R1B1QRK1 w - - ", "g5h7"},
            {"r2qrb1k/1p1b2p1/p2ppn1p/8/3NP3/1BN5/PPP3QP/1K3RR1 w - - ", "e4e5"},
            {"8/1p3pp1/7p/5P1P/2k3P1/8/2K2P2/8 w - - ", "f5f6"},
            {"8/pp2r1k1/2p1p3/3pP2p/1P1P1P1P/P5KR/8/8 w - - ", "f4f5"},
            {"8/3p4/p1bk3p/Pp6/1Kp1PpPp/2P2P1P/2P5/5B2 b - - ", "c6e4"},
            {"5k2/7R/4P2p/5K2/p1r2P1p/8/8/8 b - - ", "h4h3"},
            {"6k1/6p1/7p/P1N5/1r3p2/7P/1b3PP1/3bR1K1 w - - ", "a5a6"},
            {"8/3b4/5k2/2pPnp2/1pP4N/pP1B2P1/P3K3/8 b - - ", "f5f4"},
            {"6k1/4pp1p/3p2p1/P1pPb3/R7/1r2P1PP/3B1P2/6K1 w - - ", "d2b4"},
            {"2k5/p7/Pp1p1b2/1P1P1p2/2P2P1p/3K3P/5B2/8 w - - ", "c4c5"},
            {"8/5Bp1/4P3/6pP/1b1k1P2/5K2/8/8 w - - ", "f3g4"},
        };

    //---------------------------------------------------------------------------------------------
    public static void Elo()
    {
      cControlReloj limits = new cControlReloj();
      //List<string[]> posicionesFEN = new List<string[]>();

      m_mapConfig["Hash"].setCurrentValue("128");
      m_TablaHash.Clear();

      m_mapConfig["Threads"].setCurrentValue((Environment.ProcessorCount).ToString());

      m_mapConfig["UCI_LimitStrength"].setCurrentValue("true");
      m_mapConfig["UCI_Elo"].setCurrentValue("3000");
      //m_mapConfig["Level"].setCurrentValue("16");
      m_mapConfig["Log"].setCurrentValue("false");

      int nPuntos = 0;
      Dictionary<int, bool> lstEncontrados = new Dictionary<int, bool>();

      m_Consola.m_bInTest = true;
      for (int t = 0; t < Times.Length; t++)
      {
        //-- Tiempo en milisegundos
        limits.movetime = Times[t];

        //posicionesFEN.AddRange(Defaults);
        long elapsed = cReloj.Now();
        Int64 nodes = 0;

        Stack<cPosInfo> st = new Stack<cPosInfo>();


        for (int i = 0; i < Defaults.Length / 2; ++i)
        {
          cMotor.m_TablaHash.Clear();
          if (lstEncontrados.ContainsKey(i))
          {
            lstEncontrados[i] = true;
            continue;
          }

          cPosicion posicion = new cPosicion(Defaults[i, 0], m_mapConfig["UCI_Chess960"].Get() != 0 ? true : false, m_Threads.Principal());

          m_Consola.Print(cTypes.LF + (i + 1).ToString() + "/" + (Defaults.Length / 2).ToString() + ": ", AccionConsola.NADA);
          m_Consola.Print(Defaults[i, 0] + " -> " + Defaults[i, 1], AccionConsola.NADA);
          m_Consola.Print(cTypes.LF, AccionConsola.NADA);

          //cUci.SetMission(posicion, cColor.BLANCO, "capture,qty_2[r,b,b,p],qty_1[b,r]");
          //cUci.SetMission(posicion, cColor.NEGRO, "capture,qty_2[R,B,P],qty_1[P,Q]");

          m_Threads.Analizando(posicion, limits, st);
          m_Threads.WaitAnalizando();
          nodes += (Int64)cSearch.RootPos.GetNodos();

          mov m = cSearch.RootMoves[0].m_PV[0];
          if (m != 0)
          {
            if (cUci.GetMovimiento(m, false) == Defaults[i, 1])
            {
              if (lstEncontrados.ContainsKey(i) == false)
                lstEncontrados.Add(i, false);
            }
          }
          //m_Consola.Print(posicion.DibujaTablero(), AccionConsola.NADA);
        }

        elapsed = cReloj.Now() - elapsed + 1;

        m_Consola.Print(cTypes.LF + "===========================", AccionConsola.NADA);
        m_Consola.Print(cTypes.LF + "Tiempo transcurrido: " + elapsed.ToString(), AccionConsola.NADA);
        m_Consola.Print(cTypes.LF + "Nodos analizados   : " + nodes.ToString(), AccionConsola.NADA);
        m_Consola.Print(cTypes.LF + "Encontrados        : " + lstEncontrados.Count.ToString(), AccionConsola.NADA);
        foreach (int n in lstEncontrados.Keys)
        {
          m_Consola.Print(cTypes.LF + Defaults[n, 0] + " -> " + Defaults[n, 1], AccionConsola.NADA);
          if (lstEncontrados[n] == false)
          {
            if (limits.movetime < Times[0])
              nPuntos += Times[0] * 3;
            else if (limits.movetime < Times[1])
              nPuntos += (Times[1] / 6) * 3;
            else if (limits.movetime < Times[2])
              nPuntos += (Times[2] / 15) * 2;
            else if (limits.movetime < Times[3])
              nPuntos += (Times[3] / (35));
            else if (limits.movetime < Times[4])
              nPuntos += (Times[4] / (65));
            else if (limits.movetime < Times[5])
              nPuntos += Times[5] / (105);
          }
        }
        m_Consola.Print(cTypes.LF + "ELO               : " + nPuntos.ToString(), AccionConsola.NADA);
      }
      m_Consola.m_bInTest = false;
    }
  }
}
