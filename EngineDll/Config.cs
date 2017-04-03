using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Types;
using Motor;
using System.Diagnostics;

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
      //cMotor.m_UCI.Init(cMotor.m_mapConfig);
    }

    public static readonly int[] ELO_SCORES = {0, 200, 400, 600, 800, 1000, 1200, 1400, 1600, 1800, 2000, 2200, 2400, 2600, 2800, 3000};
    public static void OnElo(cOptConfig o)
    {
      int nNivel = 1;
      foreach(int n in ELO_SCORES)
      {
        if(nNivel + 1 < ELO_SCORES.Length)
          if((int)o.Get() >= ELO_SCORES[nNivel] && (int)o.Get() < ELO_SCORES[nNivel + 1])
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
      if (((m_strTipo != "button") && (v == null || String.IsNullOrEmpty(v))) || ((m_strTipo == "check") && v != "true" && v != "false") || ((m_strTipo == "spin") && (int.Parse(v) < m_nMin || int.Parse(v) > m_nMax)))
        return this;

      if (m_strTipo != "button")
        m_strActual = v;

      if (onModify != null)
        onModify(this);

      return this;
    }
  }

  //------------------------------------------------------------------------------------------------------
  //------------------------------------------------------------------------------------------------------
  public class cConfigFile
  {
    private object m_Lock = new object();
    private string m_FileName = null;
    internal string FileName
    {
      get
      {
        return m_FileName;
      }
    }

    private bool m_Lazy = false;
    private bool m_AutoFlush = false;
    private Dictionary<string, Dictionary<string, string>> m_Sections = new Dictionary<string, Dictionary<string, string>>();
    private Dictionary<string, Dictionary<string, string>> m_Modified = new Dictionary<string, Dictionary<string, string>>();

    public static int MAX_LEVEL = 16;
    private const string STR_NIVEL = "NIVEL ";
    public int m_nNivelJuego = MAX_LEVEL;


    //------------------------------------------------------------------------------------------------------
    public cConfigFile(string FileName)
    {
      Init(FileName, false, false);
    }

    //------------------------------------------------------------------------------------------------------
    public cConfigFile(string FileName, bool Lazy, bool AutoFlush)
    {
      Init(FileName, Lazy, AutoFlush);
    }

    //------------------------------------------------------------------------------------------------------
    private void Init(string FileName, bool Lazy, bool AutoFlush)
    {
      m_FileName = FileName;
      m_Lazy = Lazy;
      m_AutoFlush = AutoFlush;
      if (!m_Lazy)
        Refresh();
    }

    //------------------------------------------------------------------------------------------------------
    public void SetNivel(int nNivel)
    {
      m_nNivelJuego = nNivel;
    }

    //------------------------------------------------------------------------------------------------------
    private string ParseSectionName(string Line)
    {
      if (!Line.StartsWith("["))
        return null;
      if (!Line.EndsWith("]"))
        return null;
      if (Line.Length < 3)
        return null;
      return Line.Substring(1, Line.Length - 2);
    }

    //------------------------------------------------------------------------------------------------------
    private bool ParseKeyValuePair(string Line, ref string Key, ref string val)
    {
      int i;
      if ((i = Line.IndexOf('=')) <= 0)
        return false;

      int j = Line.Length - i - 1;
      Key = Line.Substring(0, i).Trim();
      if (Key.Length <= 0)
        return false;

      val = (j > 0) ? (Line.Substring(i + 1, j).Trim()) : ("");
      return true;
    }

    //------------------------------------------------------------------------------------------------------
    internal void Refresh()
    {
      lock (m_Lock)
      {
        StreamReader sr = null;
        try
        {
          m_Sections.Clear();
          m_Modified.Clear();
          try
          {
            sr = new StreamReader(m_FileName);
          }
          catch (FileNotFoundException ex)
          {
#if DEBUG
            Debug.Write(ex.Message);
#endif
            return;
          }

          Dictionary<string, string> CurrentSection = null;
          string s;
          string SectionName;
          string Key = null;
          string val = null;
          while ((s = sr.ReadLine()) != null)
          {
            s = s.Trim();
            SectionName = ParseSectionName(s);
            if (SectionName != null)
            {
              if (m_Sections.ContainsKey(SectionName))
              {
                CurrentSection = null;
              }
              else
              {
                CurrentSection = new Dictionary<string, string>();
                m_Sections.Add(SectionName, CurrentSection);
              }
            }
            else if (CurrentSection != null)
            {
              if (ParseKeyValuePair(s, ref Key, ref val))
              {
                if (!CurrentSection.ContainsKey(Key))
                {
                  CurrentSection.Add(Key, val);
                }
              }
            }
          }
        }
        finally
        {
          if (sr != null)
            sr.Close();
          sr = null;
        }
      }
    }

    //------------------------------------------------------------------------------------------------------
    internal string Get(string Key, string DefaultValue)
    {
      if (m_Lazy)
      {
        m_Lazy = false;
        Refresh();
      }

      lock (m_Lock)
      {
        string SectionName = STR_NIVEL + m_nNivelJuego.ToString();

        Dictionary<string, string> Section;
        if (!m_Sections.TryGetValue(SectionName, out Section))
          return DefaultValue;

        string val;
        if (!Section.TryGetValue(Key, out val))
          return DefaultValue;

        return val;
      }
    }

    //------------------------------------------------------------------------------------------------------
    private string EncodeByteArray(byte[] val)
    {
      if (val == null)
        return null;

      StringBuilder sb = new StringBuilder();
      foreach (byte b in val)
      {
        string hex = Convert.ToString(b, 16);
        int l = hex.Length;
        if (l > 2)
        {
          sb.Append(hex.Substring(l - 2, 2));
        }
        else
        {
          if (l < 2)
            sb.Append("0");
          sb.Append(hex);
        }
      }
      return sb.ToString();
    }

    //------------------------------------------------------------------------------------------------------
    private byte[] DecodeByteArray(string val)
    {
      if (val == null)
        return null;

      int l = val.Length;
      if (l < 2)
        return new byte[] { };

      l /= 2;
      byte[] Result = new byte[l];
      for (int i = 0; i < l; i++)
        Result[i] = Convert.ToByte(val.Substring(i * 2, 2), 16);
      return Result;
    }

    //------------------------------------------------------------------------------------------------------
    internal bool Get(string Key, bool DefaultValue)
    {
      string StringValue = Get(Key, DefaultValue.ToString(System.Globalization.CultureInfo.InvariantCulture));
      int val;
      if (int.TryParse(StringValue, out val))
        return (val != 0);
      return DefaultValue;
    }

    //------------------------------------------------------------------------------------------------------
    internal int Get(string Key, int DefaultValue)
    {
      string StringValue = Get(Key, DefaultValue.ToString(CultureInfo.InvariantCulture));
      int val;
      if (int.TryParse(StringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out val))
        return val;
      return DefaultValue;
    }

    //------------------------------------------------------------------------------------------------------
    internal long Get(string Key, long DefaultValue)
    {
      string StringValue = Get(Key, DefaultValue.ToString(CultureInfo.InvariantCulture));
      long val;
      if (long.TryParse(StringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out val))
        return val;
      return DefaultValue;
    }

    //------------------------------------------------------------------------------------------------------
    internal double Get(string Key, double DefaultValue)
    {
      string StringValue = Get(Key, DefaultValue.ToString(CultureInfo.InvariantCulture));
      double val;
      if (double.TryParse(StringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out val))
        return val;
      return DefaultValue;
    }

    //------------------------------------------------------------------------------------------------------
    internal byte[] Get(string Key, byte[] DefaultValue)
    {
      string StringValue = Get(Key, EncodeByteArray(DefaultValue));
      try
      {
        return DecodeByteArray(StringValue);
      }
      catch (FormatException ex)
      {
#if DEBUG
        Debug.Write(ex.Message);
#endif
        return DefaultValue;
      }
    }

    //------------------------------------------------------------------------------------------------------
    internal DateTime Get(string Key, DateTime DefaultValue)
    {
      string StringValue = Get(Key, DefaultValue.ToString(CultureInfo.InvariantCulture));
      DateTime val;
      if (DateTime.TryParse(StringValue, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.NoCurrentDateDefault | DateTimeStyles.AssumeLocal, out val))
        return val;
      return DefaultValue;
    }
  }
}
