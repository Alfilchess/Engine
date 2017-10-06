using System;
using System.Collections.Generic;
using Motor;

namespace InOut
{
  //-- Derivado de INIFile (C)2009-2013 S.T.A.
  //------------------------------------------------------------------------------------------------------
  public delegate void OnModify(cOptConfig opt);

  //------------------------------------------------------------------------------------------------------
  //------------------------------------------------------------------------------------------------------
  public class cOptConfig
  {
    public string m_strDefecto;
    private string m_strActual;
    public string m_strTipo;
    public int m_nMin = 0, m_nMax = 0;
    public int m_nIndex;
    public string m_nNombre;
    public bool m_bUCI = true;

    public OnModify onModify = null;

    //------------------------------------------------------------------------------------------------------
    public class OptionComparer : IComparer<cOptConfig>
    {
      int IComparer<cOptConfig>.Compare(cOptConfig x, cOptConfig y)
      {
        return x.m_nIndex.CompareTo(y.m_nIndex);
      }
    }

    //---------------------------------------------------------------------------------
    public static void OnEval(cOptConfig o)
    {
      cEval.Init();
    }

    //---------------------------------------------------------------------------------
    public static void OnThread(cOptConfig o)
    {
      cMotor.m_Threads.ReadUCIThreads();
    }

    //---------------------------------------------------------------------------------
    public static void OnHash(cOptConfig o)
    {
      cMotor.m_TablaHash.Init((UInt32)o.Get());
    }

    //---------------------------------------------------------------------------------

    public static void OnClearHash(cOptConfig o)
    {
      cMotor.m_TablaHash.Clear();
    }
    
    public static void OnLevel(cOptConfig o)
    {
      cMotor.m_ConfigFile.SetNivel((int)o.Get());
      cMotor.m_ConfigFile.m_bLevelELO = false;
      //cMotor.m_UCI.Init(cMotor.m_mapConfig);
    }

    public static void OnLevelELO(cOptConfig o)
    {
      cMotor.m_ConfigFile.SetNivel((int)o.Get());
      cMotor.m_ConfigFile.m_bLevelELO = true;
      //cMotor.m_UCI.Init(cMotor.m_mapConfig);
    }

    public static int[] ELO_SCORES = { 0, 200, 400, 600, 800, 1000, 1200, 1400, 1600, 1800, 2000, 2200, 2400, 2600, 2800, 3000 };
    public static void OnElo(cOptConfig o)
    {
      int nNivel = 1;
      foreach (int n in ELO_SCORES)
      {
        if (nNivel + 1 < ELO_SCORES.Length)
          if ((int)o.Get() >= ELO_SCORES[nNivel] && (int)o.Get() < ELO_SCORES[nNivel + 1])
            break;

        nNivel++;
      }

      cMotor.m_ConfigFile.SetNivel(nNivel);
    }


    public static void OnCPU(cOptConfig o)
    {
      cMotor.m_Threads.ReadUCIThreads();
    }

    //------------------------------------------------------------------------------------------------------
    public cOptConfig(string sName, int indice, OnModify f, bool bUci)
    {
      m_nNombre = sName;
      m_bUCI = bUci;
      m_strTipo = "button";
      m_nMin = 0;
      m_nMax = 0;
      m_nIndex = indice;
      onModify = f;
    }

    //------------------------------------------------------------------------------------------------------
    public cOptConfig(string sName, int indice, bool v, OnModify f, bool bUci)
    {
      m_nNombre = sName;
      m_strTipo = "check";
      m_bUCI = bUci;
      m_nMin = 0;
      m_nMax = 0;
      m_nIndex = indice;
      onModify = f;
      m_strDefecto = m_strActual = cMotor.m_ConfigFile.Get(sName, v) ? "true" : "false";
    }

    //------------------------------------------------------------------------------------------------------
    public cOptConfig(string sName, int indice, string v, OnModify f, bool bUci)
    {
      m_nNombre = sName;
      m_strTipo = "string";
      m_bUCI = bUci;
      m_nMin = 0;
      m_nMax = 0;
      m_nIndex = indice;
      onModify = f;
      m_strDefecto = m_strActual = cMotor.m_ConfigFile.Get(sName, v).ToString();
    }

    //------------------------------------------------------------------------------------------------------
    public cOptConfig(string sName, int indice, int v, int minv, int maxv, OnModify f, bool bUci, bool bPerc)
    {
      m_nNombre = sName;
      m_strTipo = "spin";
      m_bUCI = bUci;
      m_nMin = minv;
      m_nMax = maxv;
      m_nIndex = indice;
      onModify = f;

      v = cMotor.m_ConfigFile.Get(sName, v);

      if (bPerc == true)
        v = (v * (maxv) + v * (minv)) / 100;

      m_strDefecto = m_strActual = v.ToString();
    }
    
    //------------------------------------------------------------------------------------------------------
    public int Get()
    {
      return (m_strTipo == "spin" ? Convert.ToInt32(m_strActual) : ((m_strActual == "true") ? 1 : 0));
    }

    //------------------------------------------------------------------------------------------------------
    public string getString()
    {
      return m_strActual;
    }

    //------------------------------------------------------------------------------------------------------
    public cOptConfig setCurrentValue(string v)
    {
      if (((m_strTipo != "button") && (v == null || String.IsNullOrEmpty(v))) || 
        ((m_strTipo == "check") && v.CompareTo("true") == 0 && v.CompareTo("v") == 0) || 
        ((m_strTipo == "spin") && (int.Parse(v) < m_nMin || int.Parse(v) > m_nMax)))
        return this;

      if (m_strTipo != "button")
        m_strActual = v;

      if (onModify != null)
        onModify(this);

      return this;
    }
  }
}
