using Types;

using bitbrd = System.UInt64;
using sq = System.Int32;
using color = System.Int32;

namespace Motor
{
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
      if (cTypes.IsCasillaOcupable(m_SqRey))
      {
        m_Clavadas = pos.PiezasClavadas(pos.ColorMueve());
        m_Candidatas = pos.CandidatosJaque();
        m_Jaque[cPieza.PEON] = pos.AtaquesDePeon(m_SqRey, colorVS);
        m_Jaque[cPieza.CABALLO] = pos.AtaquesDesdeTipoDePieza(m_SqRey, cPieza.CABALLO);
        m_Jaque[cPieza.ALFIL] = pos.AtaquesDesdeTipoDePieza(m_SqRey, cPieza.ALFIL);
        m_Jaque[cPieza.TORRE] = pos.AtaquesDesdeTipoDePieza(m_SqRey, cPieza.TORRE);
        m_Jaque[cPieza.DAMA] = m_Jaque[cPieza.ALFIL] | m_Jaque[cPieza.TORRE];
        m_Jaque[cPieza.REY] = 0;
      }
    }
  }
}
