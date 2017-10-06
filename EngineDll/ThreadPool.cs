using System;
using System.Collections.Generic;
using InOut;
using Types;

using ply = System.Int32;
using mov = System.Int32;

namespace Motor
{

  //------------------------------------------------------------------------------------------
  //------------------------------------------------------------------------------------------
  public class cThreadPool : List<cThread>
  {
    //--  Máximo de 16 procesadores
    public static int MAX_THREADS = 16;// Environment.ProcessorCount;

    public ply minimumSplitDepth = 0;
    public cExclusionMutua mutex = new cExclusionMutua();
    public cExclusionMutua sleepCondition = new cExclusionMutua();
    public cRelojThread m_RelojThread;

    //------------------------------------------------------------------------------------------
    public cMainThread Principal() { return (cMainThread)this[0]; }

    //------------------------------------------------------------------------------------------
    public static void Rutina(Object th)
    {
      ((cThreadBase)th).OnIdle();

    }

    //------------------------------------------------------------------------------------------
    public static cThread NewThread()
    {
      cThread th = new cThread();
      cThreadUtils.Create(out th.handle, cThreadPool.Rutina, th);

      return th;
    }

    //------------------------------------------------------------------------------------------
    public static cRelojThread NewThreadReloj()
    {
      cRelojThread th = new cRelojThread();
      cThreadUtils.Create(out th.handle, cThreadPool.Rutina, th);
      th.handle.IsBackground = true;

      return th;
    }

    //------------------------------------------------------------------------------------------
    public static cMainThread NewMainThread()
    {
      cMainThread th = new cMainThread();
      cThreadUtils.Create(out th.handle, cThreadPool.Rutina, th);
      return th;
    }

    //------------------------------------------------------------------------------------------
    public void delete_thread(cThreadBase th)
    {
      th.exit = true;
      th.Signal();
      th.handle.Join();
    }

    //------------------------------------------------------------------------------------------
    public void Init()
    {
      m_RelojThread = NewThreadReloj();
      Add(NewMainThread());
      ReadUCIThreads();
    }

    //------------------------------------------------------------------------------------------
    public void Abort()
    {
      m_RelojThread.handle.Abort();

      foreach (cThread it in this)
      {
        it.handle.Abort();
      }
    }

    //------------------------------------------------------------------------------------------
    public void Close()
    {
      delete_thread(m_RelojThread);

      foreach (cThread it in this)
      {
        delete_thread(it);
      }
    }

    //------------------------------------------------------------------------------------------
    public void ReadUCIThreads()
    {
      int nThreads = cMotor.m_mapConfig["Threads"].Get();

      if (minimumSplitDepth == 0)
        minimumSplitDepth = nThreads < cThread.MAX_SPLITPOINTS_PER_THREAD ? (cThread.MAX_SPLITPOINTS_PER_THREAD / 2) * cPly.ONE_PLY : (cThread.MAX_SPLITPOINTS_PER_THREAD - 1) * cPly.ONE_PLY;

      while (Count < nThreads)
      {
        Add(NewThread());
      }

      while (Count > nThreads)
      {
        delete_thread(this[Count - 1]);
        RemoveAt(Count - 1);
      }
    }

    //------------------------------------------------------------------------------------------
    public cThread SlaveDisponible(cThread master)
    {
      foreach (cThread it in this)
        if (it.Disponible(master))
          return it;

      return null;
    }

    //------------------------------------------------------------------------------------------
    public void WaitAnalizando()
    {
      cMainThread t = (cMainThread)Principal();
      t.mutex.Bloquear();
      while (t.m_bBuscando) sleepCondition.Wait(t.mutex);
      t.mutex.Liberar();
    }

    //------------------------------------------------------------------------------------------
    public static bool IsSearching(List<mov> moves, mov m)
    {
      int moveLength = moves.Count;
      if (moveLength == 0)
        return false;
      for (int i = 0; i < moveLength; i++)
      {
        if (moves[i] == m)
          return true;
      }
      return false;
    }

    //------------------------------------------------------------------------------------------
    public void Analizando(cPosicion pos, cControlReloj limits, Stack<cPosInfo> states)
    {
      WaitAnalizando();

      cSearch.SearchTime = cReloj.Now();

      cSearch.m_Sennales.STOP_ON_PONDER = cSearch.m_Sennales.FIRST_MOVE = false;
      cSearch.m_Sennales.STOP = cSearch.m_Sennales.FAILED = false;

      cSearch.RootMoves.Clear();
      cSearch.RootPos = pos;
      cSearch.m_Limites = limits;
      

      if (states.Count > 0)
      {
        cSearch.m_PilaEstados = states;

      }

      for (cReglas it = new cReglas(pos, cMovType.LEGAL); it.GetActualMov() != 0; ++it)
        if (limits.searchmoves.Count == 0
            || IsSearching(limits.searchmoves, it.GetActualMov()))
          cSearch.RootMoves.Add(new cRaizMov(it.GetActualMov()));

      Principal().m_bBuscando = true;
      Principal().Signal();
    }
  }
}
