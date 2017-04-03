using System;
using System.Collections.Generic;
using Types;

using val = System.Int32;
using borde = System.Int32;
using ply = System.Int32;
using mov = System.Int32;
using hash = System.UInt64;

namespace Motor
{
  //----------------------------------------------------------------------------------------
  //----------------------------------------------------------------------------------------
  public class cHashZobrist
  {
    public static hash[][][] m_sqPeon = new hash[cColor.SIZE][][];
    public static hash[] m_EnPaso = new hash[COLUMNA.OUT];
    public static hash[] m_Enroque = new hash[cEnroque.ENROQUE_DERECHA];
    public static hash m_Bando;
    public static hash m_Exclusion;
  }

  //----------------------------------------------------------------------------------------
  //----------------------------------------------------------------------------------------
  public class cHashTable<Entry>
  {
    protected List<Entry> m_TablaHash;
    protected int Size;

    //----------------------------------------------------------------------------------------
    public cHashTable(int nSize)
    {
      m_TablaHash = new List<Entry>(nSize);
      Size = nSize;
    }

    //----------------------------------------------------------------------------------------
    public Entry this[hash k]
    {
      get
      {
        return m_TablaHash[(int)((UInt32)k & (UInt32)(Size - 1))];
      }
      set
      {
        m_TablaHash[(int)((UInt32)k & (UInt32)(Size - 1))] = value;
      }
    }
  }

  //----------------------------------------------------------------------------------------
  //----------------------------------------------------------------------------------------
  public class cTablaHashStruct
  {
    private UInt32 m_nClave;
    private UInt16 m_nMove;
    public Byte m_nBound, m_nIteraccion;
    private Int16 m_nValue, m_nDepth, m_nValueEval;

    //----------------------------------------------------------------------------------------
    public void Save(UInt32 clave, val valor, borde b, ply depth, mov m, Byte iteraccion, val valorEval)
    {
      m_nClave = (UInt32)clave;
      m_nMove = (UInt16)m;
      m_nBound = (Byte)b;
      m_nIteraccion = (Byte)iteraccion;
      m_nValue = (Int16)valor;
      m_nDepth = (Int16)depth;
      m_nValueEval = (Int16)valorEval;
    }

    //----------------------------------------------------------------------------------------
    public void Clear()
    {
      m_nClave = 0;
      m_nMove = 0;
      m_nBound = 0;
      m_nIteraccion = 0;
      m_nValue = 0;
      m_nDepth = 0;
      m_nValueEval = 0;
    }

    //----------------------------------------------------------------------------------------
    public mov GetIteraccion()
    {
      return (mov)m_nIteraccion;
    }

    //----------------------------------------------------------------------------------------
    public UInt32 GetClave()
    {
      return (UInt32)m_nClave;
    }

    //----------------------------------------------------------------------------------------
    public mov GetMove()
    {
      return (mov)m_nMove;
    }

    //----------------------------------------------------------------------------------------
    public borde GetBound()
    {
      return (borde)m_nBound;
    }

    //----------------------------------------------------------------------------------------
    public val GetValue()
    {
      return (val)m_nValue;
    }

    //----------------------------------------------------------------------------------------
    public ply GetDepth()
    {
      return (ply)m_nDepth;
    }

    //----------------------------------------------------------------------------------------
    public val GetValueEval()
    {
      return (val)m_nValueEval;
    }
  }

  //----------------------------------------------------------------------------------------
  //----------------------------------------------------------------------------------------
  public class cTablaHash
  {
    //public List<cTablaHashStruct> m_lstTabla;
    public cTablaHashStruct[] m_lstTabla;

    public const uint CLUSTER_SIZE = 4;
    private UInt32 m_nMask;
    public uint m_nHashFull = 0;
    private byte m_nIteraccion;
    public uint m_nHashSize = 0;

    //----------------------------------------------------------------------------------------
    public void NewIteraccion()
    {
      ++m_nIteraccion;
    }



    //-- Inline
    //----------------------------------------------------------------------------------------
    public int First(hash clave)
    {
      return (int)((UInt32)clave & m_nMask);
    }

    //----------------------------------------------------------------------------------------
    public void Init(UInt64 nSize)
    {
      uint nEntradaSize = sizeof(UInt32) + sizeof(UInt16) + sizeof(Byte) + sizeof(Byte) + sizeof(Int16) + sizeof(Int16) + sizeof(Int16);

      m_nHashSize = (CLUSTER_SIZE << cBitBoard.MSB((nSize << 20) / (nEntradaSize * 8)));

      if (m_nMask == m_nHashSize - CLUSTER_SIZE)
        return;

      m_nMask = m_nHashSize - CLUSTER_SIZE;

      //m_lstTabla = null;
      //m_lstTabla = new List<cTablaHashStruct>((int)m_nHashSize);
      m_lstTabla = new cTablaHashStruct[m_nHashSize];
      for (int i = 0; i < m_nHashSize; i++)
        //m_lstTabla.Add(new cTablaHashStruct());
        m_lstTabla[i] = new cTablaHashStruct();
    }

    //----------------------------------------------------------------------------------------
    public void Clear()
    {
      if (m_lstTabla != null)
      {
        m_nHashFull = 0;
        for (int i = 0; i < m_lstTabla.Length; i++)
        {
          m_lstTabla[i].Clear();
        }
      }
    }

    //-----------------------------------------------------------------------------------
    public cTablaHashStruct Buscar(hash clave)
    {
      int tte = First(clave);
      UInt32 nClave = (UInt32)(clave >> 32);
      if (m_lstTabla != null)
      {
        uint i = 0;
        while (i < CLUSTER_SIZE)
        {
          if (m_lstTabla[tte].GetClave() == nClave)
          {
            m_lstTabla[tte].m_nIteraccion = m_nIteraccion;
            return m_lstTabla[tte];
          }
          ++i;
          ++tte;
        }
      }
      return null;
    }

    //-----------------------------------------------------------------------------------
    public void Save(hash clave, val valor, borde b, ply d, mov m, val statV)
    {
      int nIndice, nIndiceReemplazar;
      cTablaHashStruct claveExistente, claveAReemplazar;
      UInt32 nClave = (UInt32)(clave >> 32);

      if (m_lstTabla != null)
      {
        nIndice = nIndiceReemplazar = First(clave);
        claveAReemplazar = m_lstTabla[nIndiceReemplazar];

        for (uint i = 0; i < CLUSTER_SIZE; ++i, ++nIndice)
        {
          claveExistente = m_lstTabla[nIndice];
          if (claveExistente.GetClave() == 0 || claveExistente.GetClave() == nClave)
          {
            if (m == 0)
              m = claveExistente.GetMove();

            claveAReemplazar = claveExistente;
            break;
          }

          //-- Casos de reemplazo de la clave Hash
          if (claveAReemplazar != claveExistente && (claveExistente.GetIteraccion() == m_nIteraccion || (claveExistente.GetBound() == cBordes.BOUND_EXACT &&
            (claveAReemplazar.GetIteraccion() == m_nIteraccion || claveExistente.GetDepth() < claveAReemplazar.GetDepth()))))
            claveAReemplazar = claveExistente;
        }

        m_nHashFull++;
        claveAReemplazar.Save(nClave, valor, b, d, m, m_nIteraccion, statV);
      }
    }

    //-----------------------------------------------------------------------------------
    public uint HashFullPercent()
    {
      if (m_nHashFull <= m_nHashSize)
        return (uint)((m_nHashFull / (float)m_nHashSize) * 1000.0);
      else
        return 1000;
    }
  }
}
