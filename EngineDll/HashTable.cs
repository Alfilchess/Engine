using System;
using System.Collections.Generic;
using hash = System.UInt64;

namespace Motor
{
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
}