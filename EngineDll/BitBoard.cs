using System;
using Types;

using bitbrd = System.UInt64;
using sq = System.Int32;
using fila = System.Int32;
using colum = System.Int32;
using color = System.Int32;
using type = System.Int32;
using pieza = System.Int32;

//-----------------------------------------------------------------------------------------------
namespace Motor
{
  //-----------------------------------------------------------------------------------------------
  //-----------------------------------------------------------------------------------------------
  public class cBitBoard
  {
    public const bitbrd A = 0x0101010101010101UL;
    public const bitbrd B = A << 1;
    public const bitbrd C = A << 2;
    public const bitbrd D = A << 3;
    public const bitbrd E = A << 4;
    public const bitbrd F = A << 5;
    public const bitbrd G = A << 6;
    public const bitbrd H = A << 7;

    public const bitbrd F1 = 0xFF;
    public const bitbrd F2 = F1 << (8 * 1);
    public const bitbrd F3 = F1 << (8 * 2);
    public const bitbrd F4 = F1 << (8 * 3);
    public const bitbrd F5 = F1 << (8 * 4);
    public const bitbrd F6 = F1 << (8 * 5);
    public const bitbrd F7 = F1 << (8 * 6);
    public const bitbrd F8 = F1 << (8 * 7);

    public static bitbrd[] m_MascTorre = new bitbrd[cCasilla.ESCAQUES];
    public static bitbrd[] m_MagicTorre = new bitbrd[cCasilla.ESCAQUES];
    public static bitbrd[][] m_AtaquesTorre = new bitbrd[cCasilla.ESCAQUES][];
    public static uint[] m_DesplTorre = new uint[cCasilla.ESCAQUES];

    public static bitbrd[] m_MascAlfil = new bitbrd[cCasilla.ESCAQUES];
    public static bitbrd[] m_MagicAlfil = new bitbrd[cCasilla.ESCAQUES];
    public static bitbrd[][] m_AtaquesAlfil = new bitbrd[cCasilla.ESCAQUES][];
    public static uint[] m_DesplAlfil = new uint[cCasilla.ESCAQUES];

    public static bitbrd[] m_nCasillas = new bitbrd[cCasilla.ESCAQUES];
    public static bitbrd[] m_Colum = new bitbrd[COLUMNA.OUT];
    public static bitbrd[] m_Fila = new bitbrd[FILA.OUT];
    public static bitbrd[] m_ColAdyacentes = new bitbrd[COLUMNA.OUT];
    public static bitbrd[][] m_Frente = new bitbrd[cColor.SIZE][];
    public static bitbrd[][] m_Ataques = new bitbrd[cPieza.COLORES][];
    public static bitbrd[][] m_Entre = new bitbrd[cCasilla.ESCAQUES][];
    public static bitbrd[][] m_EnLinea = new bitbrd[cCasilla.ESCAQUES][/*SQUARE_NB*/];
    public static bitbrd[][] m_Distancia = new bitbrd[cCasilla.ESCAQUES][];
    public static bitbrd[][] m_Atras = new bitbrd[cColor.SIZE][];
    public static bitbrd[][] m_MascPeonesPasados = new bitbrd[cColor.SIZE][];
    public static bitbrd[][] m_MascPeones = new bitbrd[cColor.SIZE][];
    public static bitbrd[][] m_PseudoAtaques = new bitbrd[cPieza.SIZE][];

    public static int[][] SquareDistance = new int[cCasilla.ESCAQUES][];

    public const bitbrd DarkSquares = 0xAA55AA55AA55AA55UL;
    public const UInt64 DeBruijn_64 = 0x3F79D71B4CB0A89UL;
    public const UInt32 DeBruijn_32 = 0x783A9B23;

    public static int[] MS1BTable = new int[256];
    public static sq[] BSFTable = new sq[cCasilla.ESCAQUES];
    public static UInt64[] RTable = new UInt64[0x19000];
    public static UInt64[] BTable = new UInt64[0x1480];

    //-----------------------------------------------------------------------------------------------
    public static int Count(bitbrd b)
    {
      UInt32 w = (UInt32)(b >> 32), v = (UInt32)(b);
      v -= (v >> 1) & 0x55555555;
      w -= (w >> 1) & 0x55555555;
      v = ((v >> 2) & 0x33333333) + (v & 0x33333333);
      w = ((w >> 2) & 0x33333333) + (w & 0x33333333);
      v = ((v >> 4) + v + (w >> 4) + w) & 0x0F0F0F0F;
      return (int)(v * 0x01010101) >> 24;
    }

    //-----------------------------------------------------------------------------------------------
    public static int CountMax15(bitbrd b)
    {
      UInt32 w = (UInt32)(b >> 32), v = (UInt32)(b);
      v -= (v >> 1) & 0x55555555;
      w -= (w >> 1) & 0x55555555;
      v = ((v >> 2) & 0x33333333) + (v & 0x33333333);
      w = ((w >> 2) & 0x33333333) + (w & 0x33333333);
      return (int)(((v + w) * 0x11111111) >> 28);
    }


    public delegate uint Funcion(sq s, bitbrd occ, type Pt);

    //-----------------------------------------------------------------------------------------------
    public static bitbrd And(bitbrd b, sq s)
    {
      return b & m_nCasillas[s];
    }

    //-----------------------------------------------------------------------------------------------
    public static bitbrd OrEq(ref bitbrd b, sq s)
    {
      return b |= m_nCasillas[s];
    }

    //-----------------------------------------------------------------------------------------------
    public static bitbrd XorEq(ref bitbrd b, sq s)
    {
      return b ^= m_nCasillas[s];
    }

    //-----------------------------------------------------------------------------------------------
    public static bitbrd Or(bitbrd b, sq s)
    {
      return b | m_nCasillas[s];
    }

    //-----------------------------------------------------------------------------------------------
    public static bitbrd Xon(bitbrd b, sq s)
    {
      return b ^ m_nCasillas[s];
    }

    //-----------------------------------------------------------------------------------------------
    public static bool MayorQue(UInt64 b)
    {
      return (b & (b - 1)) != 0;
    }

    //-----------------------------------------------------------------------------------------------
    public static int Distancia(sq s1, sq s2)
    {
      if (s2 >= cCasilla.ESCAQUES || s1 >= cCasilla.ESCAQUES)
        return 0;
      return cBitBoard.SquareDistance[s1][s2];
    }

    //-----------------------------------------------------------------------------------------------
    public static int ColumDistancia(sq s1, sq s2)
    {
      return Math.Abs(cTypes.Columna(s1) - cTypes.Columna(s2));
    }

    //-----------------------------------------------------------------------------------------------
    public static int FilaDistancia(sq s1, sq s2)
    {
      return Math.Abs(cTypes.Fila(s1) - cTypes.Fila(s2));
    }

    //-----------------------------------------------------------------------------------------------
    public static bitbrd Desplazamiento(bitbrd b, sq Delta)
    {
      return Delta == cCasilla.NORTE ? b << 8 : Delta == cCasilla.SUR ? b >> 8
            : Delta == cCasilla.NORTE_ESTE ? (b & ~H) << 9 : Delta == cCasilla.SUR_ESTE ? (b & ~H) >> 7
            : Delta == cCasilla.NORTE_OESTE ? (b & ~A) << 7 : Delta == cCasilla.SUR_OESTE ? (b & ~A) >> 9
            : 0;
    }

    //-----------------------------------------------------------------------------------------------  
    public static bitbrd GetFila(fila r)
    {
      return m_Fila[r];
    }

    //-----------------------------------------------------------------------------------------------  
    public static bitbrd GetFilaS1(sq s)
    {
      return m_Fila[cTypes.Fila(s)];
    }

    //-----------------------------------------------------------------------------------------------  
    public static bitbrd GetColumna(colum f)
    {
      return m_Colum[f];
    }

    //-----------------------------------------------------------------------------------------------  
    public static bitbrd GetColumnaSq(sq s)
    {
      return m_Colum[cTypes.Columna(s)];
    }

    //-----------------------------------------------------------------------------------------------  
    public static bitbrd ColumnaAdjacente(colum f)
    {
      return m_ColAdyacentes[f];
    }

    //-----------------------------------------------------------------------------------------------  
    public static bitbrd EnFrente(color c, fila r)
    {
      return m_Frente[c][r];
    }

    //-----------------------------------------------------------------------------------------------  
    public static bitbrd Entre(sq s1, sq s2)
    {
      return m_Entre[s1][s2];
    }

    //-----------------------------------------------------------------------------------------------  
    public static bitbrd Atras(color c, sq s)
    {
      return m_Atras[c][s];
    }

    //-----------------------------------------------------------------------------------------------  
    public static bitbrd AtaquePeon(color c, sq s)
    {
      return m_MascPeones[c][s];
    }

    //-----------------------------------------------------------------------------------------------  
    public static bitbrd PeonPasado(color c, sq s)
    {
      return m_MascPeonesPasados[c][s];
    }

    //-----------------------------------------------------------------------------------------------  
    public static bitbrd GetColor(sq s)
    {
      return (DarkSquares & m_nCasillas[s]) != 0 ? DarkSquares : ~DarkSquares;
    }

    //-----------------------------------------------------------------------------------------------  
    public static bitbrd Alineado(sq s1, sq s2, sq s3)
    {
      return m_EnLinea[s1][s2] & m_nCasillas[s3];
    }

    //-----------------------------------------------------------------------------------------------  
    public static uint Magic(sq s, UInt64 occ, type Pt)
    {
      bitbrd[] Masks = Pt == cPieza.TORRE ? m_MascTorre : m_MascAlfil;
      bitbrd[] Magics = Pt == cPieza.TORRE ? m_MagicTorre : m_MagicAlfil;
      uint[] Shifts = Pt == cPieza.TORRE ? m_DesplTorre : m_DesplAlfil;

      uint lo = (uint)(occ) & (uint)Masks[s];
      uint hi = (uint)(occ >> 32) & (uint)(Masks[s] >> 32);
      return (lo * (uint)(Magics[s]) ^ hi * (uint)(Magics[s] >> 32)) >> (int)Shifts[s];

    }

    //-----------------------------------------------------------------------------------------------  
    public static bitbrd AtaquesPieza(sq s, bitbrd occ, type Pt)
    {
      return (Pt == cPieza.TORRE ? m_AtaquesTorre : m_AtaquesAlfil)[s][Magic(s, occ, Pt)];
    }

    //-----------------------------------------------------------------------------------------------  
    public static bitbrd Ataques(pieza pc, sq s, bitbrd occ)
    {
      switch (cTypes.TipoPieza(pc))
      {
        case cPieza.ALFIL: return AtaquesPieza(s, occ, cPieza.ALFIL);
        case cPieza.TORRE: return AtaquesPieza(s, occ, cPieza.TORRE);
        case cPieza.DAMA: return AtaquesPieza(s, occ, cPieza.ALFIL) | AtaquesPieza(s, occ, cPieza.TORRE);
        default: return m_Ataques[pc][s];
      }
    }

    //-----------------------------------------------------------------------------------------------  
    public static sq Adelantado(color c, bitbrd b) { return c == cColor.BLANCO ? MSB(b) : LSB(b); }


    //-----------------------------------------------------------------------------------------------  
    public static sq backmost_sq(color c, bitbrd b) { return c == cColor.BLANCO ? LSB(b) : MSB(b); }


    //-----------------------------------------------------------------------------------------------  
    public static uint IndexBsf(bitbrd b)
    {
      b ^= (b - 1);
      return (((uint)(b) ^ (uint)(b >> 32)) * DeBruijn_32) >> 26;
    }

    //-----------------------------------------------------------------------------------------------  
    public static sq LSB(bitbrd b)
    {
      return BSFTable[IndexBsf(b)];
    }

    //-----------------------------------------------------------------------------------------------  
    public static int GetLSB(ref bitbrd b)
    {
      bitbrd bb = b;
      b = bb & (bb - 1);
      return BSFTable[IndexBsf(bb)];
    }

    //-----------------------------------------------------------------------------------------------  
    public static sq MSB(UInt64 b)
    {
      uint b32;
      int result = 0;

      if (b > 0xFFFFFFFF)
      {
        b >>= 32;
        result = 32;
      }

      b32 = (UInt32)(b);

      if (b32 > 0xFFFF)
      {
        b32 >>= 16;
        result += 16;
      }

      if (b32 > 0xFF)
      {
        b32 >>= 8;
        result += 8;
      }

      return (sq)(result + MS1BTable[b32]);
    }

    //-----------------------------------------------------------------------------------------------  
    public static void Init()
    {
      for (sq s = cCasilla.A1; s <= cCasilla.H8; ++s)
        BSFTable[IndexBsf(m_nCasillas[s] = 1UL << s)] = s;

      for (bitbrd b = 1; b < 256; ++b)
        MS1BTable[b] = MayorQue(b) ? MS1BTable[b - 1] : LSB(b);

      for (colum f = COLUMNA.A; f <= COLUMNA.H; ++f)
        m_Colum[f] = f > COLUMNA.A ? m_Colum[f - 1] << 1 : A;

      for (fila r = FILA.F1; r <= FILA.F8; ++r)
        m_Fila[r] = r > FILA.F1 ? m_Fila[r - 1] << 8 : F1;

      for (colum f = COLUMNA.A; f <= COLUMNA.H; ++f)
        m_ColAdyacentes[f] = (f > COLUMNA.A ? m_Colum[f - 1] : 0) | (f < COLUMNA.H ? m_Colum[f + 1] : 0);

      for (int c = cColor.BLANCO; c <= cColor.NEGRO; c++)
        m_Frente[c] = new bitbrd[FILA.OUT];

      for (fila r = FILA.F1; r < FILA.F8; ++r)
        m_Frente[cColor.BLANCO][r] = ~(m_Frente[cColor.NEGRO][r + 1] = m_Frente[cColor.NEGRO][r] | m_Fila[r]);

      for (int c = cColor.BLANCO; c <= cColor.NEGRO; c++)
      {
        m_Atras[c] = new bitbrd[cCasilla.ESCAQUES];
        m_MascPeones[c] = new bitbrd[cCasilla.ESCAQUES];
        m_MascPeonesPasados[c] = new bitbrd[cCasilla.ESCAQUES];
      }

      for (color c = cColor.BLANCO; c <= cColor.NEGRO; ++c)
        for (sq s = cCasilla.A1; s <= cCasilla.H8; ++s)
        {
          m_Atras[c][s] = m_Frente[c][cTypes.Fila(s)] & m_Colum[cTypes.Columna(s)];
          m_MascPeones[c][s] = m_Frente[c][cTypes.Fila(s)] & m_ColAdyacentes[cTypes.Columna(s)];
          m_MascPeonesPasados[c][s] = m_Atras[c][s] | m_MascPeones[c][s];
        }

      for (sq c = 0; c < cCasilla.ESCAQUES; c++)
      {
        SquareDistance[c] = new int[cCasilla.ESCAQUES];
        m_Distancia[c] = new bitbrd[8];
      }

      for (sq s1 = cCasilla.A1; s1 <= cCasilla.H8; ++s1)
        for (sq s2 = cCasilla.A1; s2 <= cCasilla.H8; ++s2)
          if (s1 != s2)
          {
            SquareDistance[s1][s2] = Math.Max(ColumDistancia(s1, s2), FilaDistancia(s1, s2));
            m_Distancia[s1][SquareDistance[s1][s2] - 1] |= m_nCasillas[s2];
          }

      SByte[][] steps = new SByte[7][];
      steps[0] = new SByte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
      steps[1] = new SByte[] { 7, 9, 0, 0, 0, 0, 0, 0, 0 };
      steps[2] = new SByte[] { 17, 15, 10, 6, -6, -10, -15, -17, 0 };
      steps[3] = new SByte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
      steps[4] = new SByte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
      steps[5] = new SByte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
      steps[6] = new SByte[] { 9, 7, -7, -9, 8, 1, -1, -8, 0 };

      for (pieza p = cPieza.NAN; p < cPieza.COLORES; p++)
        m_Ataques[p] = new bitbrd[cCasilla.ESCAQUES];

      for (color c = cColor.BLANCO; c <= cColor.NEGRO; ++c)
        for (type pt = cPieza.PEON; pt <= cPieza.MAXSIZE; ++pt)
          for (sq s = cCasilla.A1; s <= cCasilla.H8; ++s)
            for (int i = 0; steps[pt][i] != 0; ++i)
            {
              sq to = s + (sq)(c == cColor.BLANCO ? steps[pt][i] : -steps[pt][i]);

              if (cTypes.IsCasillaOcupable(to) && cBitBoard.Distancia(s, to) < 3)
                m_Ataques[cTypes.CreaPieza(c, pt)][s] |= m_nCasillas[to];
            }

       
      sq[] RDeltas = new sq[] { cCasilla.NORTE, cCasilla.ESTE, cCasilla.SUR, cCasilla.OESTE };
      sq[] BDeltas = new sq[] { cCasilla.NORTE_ESTE, cCasilla.SUR_ESTE, cCasilla.SUR_OESTE, cCasilla.NORTE_OESTE };

      InitMagic(cPieza.TORRE, m_AtaquesTorre, m_MagicTorre, m_MascTorre, m_DesplTorre, RDeltas, Magic);
      InitMagic(cPieza.ALFIL, m_AtaquesAlfil, m_MagicAlfil, m_MascAlfil, m_DesplAlfil, BDeltas, Magic);

      for (type pt = cPieza.NAN; pt < cPieza.SIZE; pt++)
        m_PseudoAtaques[pt] = new bitbrd[cCasilla.ESCAQUES];

      for (sq s = cCasilla.A1; s <= cCasilla.H8; s++)
      {
        m_Entre[s] = new bitbrd[cCasilla.ESCAQUES];
        m_EnLinea[s] = new bitbrd[cCasilla.ESCAQUES];
      }

      for (sq s1 = cCasilla.A1; s1 <= cCasilla.H8; ++s1)
      {
        m_PseudoAtaques[cPieza.DAMA][s1] = m_PseudoAtaques[cPieza.ALFIL][s1] = AtaquesPieza(s1, 0, cPieza.ALFIL);
        m_PseudoAtaques[cPieza.DAMA][s1] |= m_PseudoAtaques[cPieza.TORRE][s1] = AtaquesPieza(s1, 0, cPieza.TORRE);

        for (sq s2 = cCasilla.A1; s2 <= cCasilla.H8; ++s2)
        {
          pieza pc = (m_PseudoAtaques[cPieza.ALFIL][s1] & m_nCasillas[s2]) != 0 ? cPieza.ALFIL_BLANCO :
                     (m_PseudoAtaques[cPieza.TORRE][s1] & m_nCasillas[s2]) != 0 ? cPieza.TORRE_BLANCO : cPieza.NAN;

          if (pc == cPieza.NAN)
            continue;

          m_EnLinea[s1][s2] = (Ataques(pc, s1, 0) & Ataques(pc, s2, 0)) | m_nCasillas[s1] | m_nCasillas[s2];
          m_Entre[s1][s2] = Ataques(pc, s1, m_nCasillas[s2]) & Ataques(pc, s2, m_nCasillas[s1]);
        }
      }
    }

    //-----------------------------------------------------------------------------------------------  
    public static bitbrd AtaqueSlide(sq[] deltas, sq sq, bitbrd occupied)
    {
      bitbrd attack = 0;

      for (int i = 0; i < 4; ++i)
        for (sq s = sq + deltas[i];
             cTypes.IsCasillaOcupable(s) && cBitBoard.Distancia(s, s - deltas[i]) == 1;
             s += deltas[i])
        {
          attack |= m_nCasillas[s];

          if ((occupied & m_nCasillas[s]) != 0)
            break;
        }

      return attack;
    }

    //-----------------------------------------------------------------------------------------------  
    public static void InitMagic(type pt, bitbrd[][] attacks, bitbrd[] magics, bitbrd[] masks, uint[] shifts, sq[] deltas, Funcion index)
    {
      int[][] MagicBoosters = new int[2][]{ 
   				new int[] { 969, 1976, 2850,  542, 2069, 2852, 1708,  164 },
		   		new int[] { 3101,  552, 3555,  926,  834,   26, 2131, 1117 } 
			};

      cAleatorio rk = new cAleatorio();
      bitbrd[] occupancy = new UInt64[4096], reference = new UInt64[4096];
      bitbrd edges, b;
      int i, size, booster;

      for (sq s = cCasilla.A1; s <= cCasilla.H8; s++)
      {
        edges = ((cBitBoard.F1 | cBitBoard.F8) & ~cBitBoard.GetFilaS1(s)) | ((cBitBoard.A | cBitBoard.H) & ~cBitBoard.GetColumnaSq(s));

        masks[s] = AtaqueSlide(deltas, s, 0) & ~edges;
        shifts[s] = 32 - (uint)cBitBoard.CountMax15(masks[s]);

        b = 0;
        size = 0;
        do
        {
          occupancy[size] = b;
          reference[size] = AtaqueSlide(deltas, s, b);
          size++;
          b = (b - masks[s]) & masks[s];
        } while (b != 0);

        attacks[s] = new bitbrd[size];
        booster = MagicBoosters[0][cTypes.Fila(s)];

        do
        {
          do magics[s] = rk.NumeroMagico(booster);
          while (cBitBoard.CountMax15((magics[s] * masks[s]) >> 56) < 6);

          Array.Clear(attacks[s], 0, size);

          for (i = 0; i < size; i++)
          {
            bitbrd attack = attacks[s][index(s, occupancy[i], pt)];

            if (attack != 0 && attack != reference[i])
              break;

            attacks[s][index(s, occupancy[i], pt)] = reference[i];
          }
        } while (i != size);
      }
    }
  }
}
