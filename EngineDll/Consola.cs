using System.IO;
using Types;
using System;
using Motor;

namespace InOut
{
  public enum AccionConsola
  {
    NADA,
    GET,
    RELEASE,
    ATOMIC
  }

  //-----------------------------------------------------------------------------------
  //-----------------------------------------------------------------------------------
  public class cConsola
  {
    private System.Threading.Mutex m_Mutex;
    private TextReader m_In;
    private TextWriter m_Out;
    private Action<string> m_CallBackFunction = null;
    private string m_strCallBackString = "";

    public void SetDelegateFunction(Action<string> callBackFunction)
    {
      m_CallBackFunction = callBackFunction;
    }

    //-----------------------------------------------------------------------------------
    public cConsola(TextReader inputReader, TextWriter outputWriter)
    {
      m_Mutex = new System.Threading.Mutex();

      if(m_CallBackFunction != null)
      {
      }
      else
      {
        m_In = inputReader;
        m_Out = outputWriter;
      }
      
      m_CallBackFunction = null;
    }

    //-----------------------------------------------------------------------------------
    public void LogRead(string logMessage, TextWriter w)
    {
      w.WriteLine("{0} {1} -> {2} ", DateTime.Now.ToLongTimeString(), DateTime.Now.ToShortDateString(), logMessage);
    }

    //-----------------------------------------------------------------------------------
    public void LogWrite(string logMessage, TextWriter w)
    {
      w.WriteLine("{0} {1} <- {2} ", DateTime.Now.ToLongTimeString(), DateTime.Now.ToShortDateString(), logMessage);
    }
      
    //-----------------------------------------------------------------------------------
    public string ReadLine(AccionConsola accion)
    {
      string cad = "";
      if (accion == AccionConsola.GET || accion == AccionConsola.ATOMIC)
        m_Mutex.WaitOne();
      if(m_CallBackFunction != null)
      {
      }
      else
      {
        cad = m_In.ReadLine();

        if(cMotor.m_mapConfig["Log"].Get() != 0)
        {
          using(StreamWriter w = File.AppendText("log.txt"))
          {
            LogRead(cad, w);
          }
        }
      }

      if(accion == AccionConsola.RELEASE || accion == AccionConsola.ATOMIC)
        m_Mutex.ReleaseMutex();

      return cad;
    }

    //-----------------------------------------------------------------------------------
    public void Print(string cad, AccionConsola accion)
    {
      if (accion == AccionConsola.GET || accion == AccionConsola.ATOMIC)
        m_Mutex.WaitOne();

      if(m_CallBackFunction != null)
        m_strCallBackString += cad;
      else
      {
        m_Out.Write(cad);
      }
      
      foreach(var line in cad.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
      {
        if (cMotor.m_mapConfig["Log"].Get() != 0)
        {
          using (StreamWriter w = File.AppendText("log.txt"))
          {
            LogWrite(line, w);
          }
        }
      }

      if(accion == AccionConsola.RELEASE || accion == AccionConsola.ATOMIC)
      {
        if(m_CallBackFunction != null)
        {
          m_CallBackFunction(m_strCallBackString);
          m_strCallBackString = "";
        }
        m_Mutex.ReleaseMutex();
      }
    }

    //-----------------------------------------------------------------------------------
    public void PrintLine(string cad, AccionConsola accion)
    {
      if(m_CallBackFunction != null)
        Print(cad, accion);
      else
        Print(cad + cTypes.LF, accion);
    }
  }
}
