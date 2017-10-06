using System.Diagnostics;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using sq = System.Int32;
using color = System.Int32;
using pieza = System.Int32;
using type = System.Int32;
using pnt = System.Int32;
using mov = System.Int32;

using Types;
using InOut;
using Motor;
#if CHESSARIA
#else
namespace Chessaria
{
  public partial class cChessariaUci
  {
    //---------------------------------------------------------------------------------
    public static void SetMission(cPosicion pos, color col, string strMission)
    {
    }
  }
}

namespace Motor
{ 
  public partial class cMotor
  {
    public static void ChessariaTest()
    {

    }
  }

  //---------------------------------------------------------------------------------
  public partial class cPosicion
  {
    public void SetFENChessaria(string fenStr, bool bChess960, cThread th)
    {

    }

    public pnt EvalMissionPuntosEval(color col)
    {
      return 0;
    }

    public pnt EvalMission(color col)
    {
      pnt nPuntos = 0;
      return nPuntos;
    }

    //--------------------------------------------------------------------------------------------------------------------------------
    public bool IsObstaculo(sq m, bool to)
    {
      bool bRet = false;
      return bRet;
    }

    //--------------------------------------------------------------------------------------------------------------------------------
    public bool IsWall(sq m)
    {
      bool bRet = false;
      return bRet;
    }

    //--------------------------------------------------------------------------------------------------------------------------------
    public bool IsHole(sq m)
    {
      bool bRet = false;

      return bRet;
    }

    //--------------------------------------------------------------------------------------------------------------------------------
    public bool IsTesoro(sq m, color c)
    {
      bool bRet = false;
      return bRet;
    }

    //--------------------------------------------------------------------------------------------------------------------------------
    public void SetWall(sq m)
    {
    }

    //--------------------------------------------------------------------------------------------------------------------------------
    public void SetTesoro(sq m, color c)
    {
    }

    //--------------------------------------------------------------------------------------------------------------------------------
    public void SetObstaculo(sq q)
    {
    }

    //------------------------------------------------------------------------------------------------------------------------------------
    public bool HasObstacles()
    {
      bool bRet = false;
      
      return bRet;
    }
  }

  //---------------------------------------------------------------------------------
  public class cMission
  {
    public const int CHECKMATE = 0, CAPTURE = 1, DOMINATION = 2;
    public int m_nMissionType = CHECKMATE;
    public int m_nTotalCapture = 0;
    public int m_nTotalDomination = 0;
    public bool m_bReglasCambiadas = false;
    public List<cMissionCapture> m_lstMissionCapture = new List<cMissionCapture>();
    public List<cMissionDomination> m_lstMissionDomination = new List<cMissionDomination>();

    color m_Color = cColor.NAN;
    
    //--------------------------------------------------------------------------------------
    public cMission(color c)
    {
      m_Color = c;
    }

    //--------------------------------------------------------------------------------------
    public int EvalActualMission(cPosicion pos)
    {
      int nRet = 0;
      
      return nRet;
    }

    //--------------------------------------------------------------------------------------
    public pnt EvalMission(ref int nPuntoEval)
    {
      pnt nPuntos = 0;
     
      return nPuntos;
    }

    //--------------------------------------------------------------------------------------
    public void BorraPiezaDomination(sq s, color c, type pt, bool bSetPos = false)
    {
      
    }

    //--------------------------------------------------------------------------------------
    public void SetPiezaDomination(sq s, color c, type pt, bool bSetPos = false)
    {
      
    }
    //--------------------------------------------------------------------------------------
    public void BorraPiezaCapture(sq s, color c, type pt, bool bSetPos = false)
    {
      
    }

    //--------------------------------------------------------------------------------------
    public void SetPiezaCapture(sq s, color c, type pt, bool bSetPos = false)
    {
     
    }

    //--------------------------------------------------------------------------------------
    public string GetText()
    {
      string strRet = "";
      
      return strRet;
    }

    //--------------------------------------------------------------------------------------
    public void Clear()
    {
      m_lstMissionCapture.Clear();
      m_lstMissionDomination.Clear();
    }

    //--------------------------------------------------------------------------------------
    public class cMissionCapture
    {
      public int[] m_lstPiezas = new int[cPieza.MAX];
      public int[] m_lstPiezasAux = new int[cPieza.MAX];
      public int m_nlstPiezaNum = 0;
      public int m_nlstPiezaNumAux = 0;

      public cMissionCapture()
      {
        Array.Clear(m_lstPiezas, 0, cPieza.MAX);
        Array.Clear(m_lstPiezasAux, 0, cPieza.MAX);
      }

      public int CountPiezas()
      {
        int nRet = 0;
        foreach(int i in m_lstPiezas)
        {
          nRet += i;
        }
        return nRet;
      }
      
      //--------------------------------------------------------------------------------------
      public void AddPieza(pieza pt)
      {
        
      }

      //--------------------------------------------------------------------------------------
      public bool BorraPieza(sq s, color c, type pt, bool bSetPos = false)
      {
        bool bDeleted = false;
        return bDeleted;
      }

      //--------------------------------------------------------------------------------------
      public bool SetPieza(sq s, color c, type pt, bool bSetPos = false)
      {
        bool bRet = false;
        return bRet;
      }
    }
    
    //--------------------------------------------------------------------------------------
    public class cMissionDomination
    {
      public sq[] m_lstPos = new sq[cCasilla.ESCAQUES];
      public sq[] m_lstPosAux = new sq[cCasilla.ESCAQUES];
      public cMissionCapture m_capture;

      public cMissionDomination()
      {
        Array.Clear(m_lstPos, 0, cCasilla.ESCAQUES);
        Array.Clear(m_lstPosAux, 0, cCasilla.ESCAQUES);
      }

      //--------------------------------------------------------------------------------------
      public int CountPosAux()
      {
        int nRet = 0;
        
        return nRet;
      }

      //--------------------------------------------------------------------------------------
      public int CountPos()
      {
        int nRet = 0;
        
        return nRet;
      }

      //--------------------------------------------------------------------------------------
      public void AddPos(sq pos)
      {
        m_lstPos[pos]++;
        m_lstPosAux[pos]++;
      }

      //--------------------------------------------------------------------------------------
      public void SumaPos(sq s, color c, type pt, bool bSetPos = false)
      {
      }

      //--------------------------------------------------------------------------------------
      public void BorraPos(sq s, color c, type pt, bool bSetPos = false)
      {
      }
    }
  }
}
#endif // CHESSARIA