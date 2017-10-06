using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Motor;
using System.Diagnostics;

namespace InOut
{
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
    public static int MAX_LEVEL_ELO = 3000;
    private const string STR_NIVEL = "NIVEL ";
    public int m_nNivelJuego = MAX_LEVEL;
    public bool m_bLevelELO = false;

    //------------------------------------------------------------------------------------------------------
    public cConfigFile()
    {
    }

    //------------------------------------------------------------------------------------------------------
    public cConfigFile(string FileName)
    {
      if (File.Exists(FileName))
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
            Debug.Write(ex.Message);
            cMotor.m_Consola.Print("Exception: " + ex.Message, AccionConsola.NADA);
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
        Debug.Write(ex.Message);
        cMotor.m_Consola.Print("Exception: " + ex.Message, AccionConsola.NADA);
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
