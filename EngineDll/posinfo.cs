using Types;

using bitbrd = System.UInt64;
using hash = System.UInt64;
using sq = System.Int32;
using type = System.Int32;
using val = System.Int32;
using pnt = System.Int32;

namespace Motor
{
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
