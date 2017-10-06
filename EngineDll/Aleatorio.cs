using System;

//--  RKISS Es un sistema de generación de números pseudoaleatorios para las claves Hash
//--  George Marsaglia inventó este sistema sobre los 90. Es ThreadSafe
namespace Types
{
  //-----------------------------------------------------------------------------------
  //-----------------------------------------------------------------------------------
  public class cAleatorio
  {
    private static UInt64 a;
    private static UInt64 b;
    private static UInt64 c;
    private static UInt64 d;

    //-----------------------------------------------------------------------------------
    UInt64 RotarIZQ(UInt64 x, int k)
    {
      return (x << k) | (x >> (64 - k));
    }

    //-----------------------------------------------------------------------------------
    public UInt64 GetRand()
    {
      UInt64 e = a - RotarIZQ(b, 7);
      a = b ^ RotarIZQ(c, 13);
      b = c + RotarIZQ(d, 37);
      c = d + e;
      return d = e + a;
    }

    //-----------------------------------------------------------------------------------
    public cAleatorio()
    {
      a = 0xF1EA5EED;
      b = c = d = 0xD4E12C77;
      for (int i = 0; i < (int)(InOut.cReloj.Now() % 100); ++i)
        GetRand();
    }

    //------------------------------------------------------------------------------------
    public UInt64 NumeroMagico(int s)
    {
      return RotarIZQ(RotarIZQ(GetRand(), (s >> 0) & 0x3F) & GetRand(), (s >> 6) & 0x3F) & GetRand();
    }
  }
}
