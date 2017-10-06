using System.Threading;

//------------------------------------------------------------------------------------------
namespace Motor
{

  //------------------------------------------------------------------------------------------
  //------------------------------------------------------------------------------------------
  public static class cThreadUtils
  {
    const int TIME_WAIT = 20;

    //------------------------------------------------------------------------------------------
    public static void Bloquear(object bloqueo)
    {
      if (Monitor.TryEnter(bloqueo) == false)
        Monitor.Enter(bloqueo);
    }

    //------------------------------------------------------------------------------------------
    public static void Liberar(object bloqueo)
    {
      System.Threading.Monitor.Exit(bloqueo);
    }

    //------------------------------------------------------------------------------------------
    public static void Signal(object sleepCond)
    {
      lock (sleepCond)
      {
        Monitor.Pulse(sleepCond);
      }
    }


    //------------------------------------------------------------------------------------------
    public static void Wait(object sleepCond, object sleepLock)
    {
      Liberar(sleepLock);
      lock (sleepCond)
      {
        //--  Si se disminuye puede provocar que durante el ReadLine coja mucha cpu esperando
        Monitor.Wait(sleepCond, TIME_WAIT);
      }
      Bloquear(sleepLock);
    }

    //------------------------------------------------------------------------------------------
    public static void Wait(object sleepCond, object sleepLock, int msec)
    {
      Liberar(sleepLock);
      lock (sleepCond)
      {
        Monitor.Wait(sleepCond, msec);
      }
      Bloquear(sleepLock);
    }

    //------------------------------------------------------------------------------------------
    public static bool Create(out System.Threading.Thread handle, ParameterizedThreadStart rutina, cThreadBase thread)
    {
      handle = new System.Threading.Thread(rutina);

      handle.IsBackground = false;

      handle.Priority = ThreadPriority.Normal;

      handle.Start(thread);
      return true;
    }
  }

  //------------------------------------------------------------------------------------------
  //------------------------------------------------------------------------------------------
  public class BitSet
  {
    private bool[] bits;
    private int dim;

    //------------------------------------------------------------------------------------------
    public BitSet(int nDim)
    {
      bits = new bool[nDim];
      dim = nDim;
    }

    //------------------------------------------------------------------------------------------
    public bool this[int i]
    {
      get
      {
        return bits[i];
      }
      set
      {
        bits[i] = value;
      }
    }

    //------------------------------------------------------------------------------------------
    public bool none()
    {
      for (int i = 0; i < dim; i++)
        if (bits[i])
          return false;

      return true;
    }

    //------------------------------------------------------------------------------------------
    public void SetAll(bool val)
    {
      for (int i = 0; i < dim; i++)
        bits[i] = val;
    }
  }
  
}