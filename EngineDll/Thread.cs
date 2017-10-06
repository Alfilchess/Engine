using System;
using Finales;

using ply = System.Int32;
using val = System.Int32;
using mov = System.Int32;

//------------------------------------------------------------------------------------------
namespace Motor
{
  //------------------------------------------------------------------------------------------
  public struct cSignal
  {
    public bool STOP, STOP_ON_PONDER, FIRST_MOVE, FAILED;
  };
  

  //------------------------------------------------------------------------------------------
  //------------------------------------------------------------------------------------------
  public class cExclusionMutua
  {
    public Object l = new object();
    public Object c = new object();

    public void Bloquear() { cThreadUtils.Bloquear(l); }
    public void Liberar() { cThreadUtils.Liberar(l); }
    public void Wait(cExclusionMutua m) { cThreadUtils.Wait(c, m.l); }
    public void Wait(cExclusionMutua m, int ms) { cThreadUtils.Wait(c, m.l, ms); }
    public void Signal() { cThreadUtils.Signal(c); }
  }

  //------------------------------------------------------------------------------------------
  //------------------------------------------------------------------------------------------
  public class cSplitPoint
  {
    public cPosicion pos;
    public cPilaMov[] ss;
    public int ssPos;
    public cThread masterThread;
    public ply depth;
    public val beta;
    public int nodeType;
    public bool cutNode;

    public cListaOrdenadaMov movePicker;
    public cSplitPoint parentSplitPoint;

    public cExclusionMutua mutex = new cExclusionMutua();
    public BitSet slavesMask = new BitSet(cThreadPool.MAX_THREADS);
    public bool allSlavesSearching;
    public UInt32 nodes;
    public val alpha;
    public val bestValue;
    public mov bestMove;
    public int moveCount;
    public bool m_bCorte;
  }

  //------------------------------------------------------------------------------------------
  //------------------------------------------------------------------------------------------
  public partial class cThreadBase
  {
    public static int MAX_SPLITPOINTS_PER_THREAD = 16;// Environment.ProcessorCount;

    public cExclusionMutua mutex;
    public cExclusionMutua sleepCondition;
    public System.Threading.Thread handle;
    public bool exit;

    public cThreadBase()
    {
      mutex = new cExclusionMutua();
      sleepCondition = new cExclusionMutua();
      exit = false;
    }

    public virtual void OnIdle() { }

    public void Signal()
    {
      mutex.Bloquear();
      sleepCondition.Signal();
      mutex.Liberar();
    }

    public void Wait(ref bool b)
    {
      mutex.Bloquear();
      while (!b) sleepCondition.Wait(mutex);
      mutex.Liberar();
    }
  }

  //------------------------------------------------------------------------------------------
  //------------------------------------------------------------------------------------------
  public partial class cThread : cThreadBase
  {
    public cSplitPoint[] m_SplitPoints = new cSplitPoint[MAX_SPLITPOINTS_PER_THREAD];
    public cMaterial.cTablaHashMaterial m_Material = new cMaterial.cTablaHashMaterial();
    public cListaDeFinales m_Finales = null;//new cListaDeFinales();
    public cBonusPeones.cHashPeones m_TablaPeones = new cBonusPeones.cHashPeones();
    public cPosicion m_posActiva;
    public int idx;
    public int m_nMaxPly;
    public cSplitPoint m_SplitPointActivo;
    public int m_nSplitPointSize;
    public bool m_bBuscando;

    //------------------------------------------------------------------------------------------
    public cThread() : base()
    {
      m_bBuscando = exit = false;
      m_nMaxPly = m_nSplitPointSize = 0;
      m_SplitPointActivo = null;
      m_posActiva = null;
      idx = cMotor.m_Threads.Count;

      for (int i = 0; i < MAX_SPLITPOINTS_PER_THREAD; i++)
        m_SplitPoints[i] = new cSplitPoint();
    }

    //------------------------------------------------------------------------------------------
    public bool IsCorte()
    {
      for (cSplitPoint sp = m_SplitPointActivo; sp != null; sp = sp.parentSplitPoint)
        if (sp.m_bCorte)
          return true;

      return false;
    }

    //------------------------------------------------------------------------------------------
    public bool Disponible(cThread master)
    {
      if (m_bBuscando)
        return false;

      int size = m_nSplitPointSize;

      return size == 0 || m_SplitPoints[size - 1].slavesMask[master.idx];
    }

    //------------------------------------------------------------------------------------------
    public void Split(cPosicion pos, cPilaMov[] ss, int ssPos, val alpha, val beta, ref val bestValue, ref mov bestMove, ply depth, int moveCount,
                               cListaOrdenadaMov movePicker, int nodeType, bool cutNode)
    {
      cSplitPoint sp = m_SplitPoints[m_nSplitPointSize];

      sp.masterThread = this;
      sp.parentSplitPoint = m_SplitPointActivo;
      sp.slavesMask.SetAll(false); sp.slavesMask[idx] = true;
      sp.depth = depth;
      sp.bestValue = bestValue;
      sp.bestMove = bestMove;
      sp.alpha = alpha;
      sp.beta = beta;
      sp.nodeType = nodeType;
      sp.cutNode = cutNode;
      sp.movePicker = movePicker;
      sp.moveCount = moveCount;
      sp.pos = pos;
      sp.nodes = 0;
      sp.m_bCorte = false;
      sp.ss = ss;
      sp.ssPos = ssPos;

      cMotor.m_Threads.mutex.Bloquear();
      sp.mutex.Bloquear();

      sp.allSlavesSearching = true;
      ++m_nSplitPointSize;
      m_SplitPointActivo = sp;
      m_posActiva = null;

      for (cThread slave; (slave = cMotor.m_Threads.SlaveDisponible(this)) != null; )
      {
        sp.slavesMask[slave.idx] = true;
        slave.m_SplitPointActivo = sp;
        slave.m_bBuscando = true;
        slave.Signal();
      }

      sp.mutex.Liberar();
      cMotor.m_Threads.mutex.Liberar();

      Bucle();

      cMotor.m_Threads.mutex.Bloquear();
      sp.mutex.Bloquear();


      m_bBuscando = true;
      --m_nSplitPointSize;
      m_SplitPointActivo = sp.parentSplitPoint;
      m_posActiva = pos;
      pos.SetNodos(pos.GetNodos() + sp.nodes);
      bestMove = sp.bestMove;
      bestValue = sp.bestValue;

      sp.mutex.Liberar();
      cMotor.m_Threads.mutex.Liberar();
    }

    //------------------------------------------------------------------------------------------
    public override void OnIdle()
    {

      cSplitPoint this_sp = m_nSplitPointSize != 0 ? m_SplitPointActivo : null;
      while (true)
      {


        while (!m_bBuscando || exit)
        {
          if (exit)
          {
            return;
          }

          mutex.Bloquear();

          if (this_sp != null && this_sp.slavesMask.none())
          {
            mutex.Liberar();
            break;
          }




          if (!m_bBuscando && !exit)
            sleepCondition.Wait(mutex);
          mutex.Liberar();
        }

        if (m_bBuscando)
        {
          cMotor.m_Threads.mutex.Bloquear();
          cSplitPoint sp = m_SplitPointActivo;
          cMotor.m_Threads.mutex.Liberar();
          cPilaMov[] stack = new cPilaMov[cSearch.MAX_PLY_PLUS_6];
          int ss = 2;
          for (int i = 0; i < cSearch.MAX_PLY_PLUS_6; i++)
            stack[i] = new cPilaMov();
          cPosicion pos = new cPosicion(sp.pos, this);
          for (int i = sp.ssPos - 2, n = 0; n < 5; n++, i++)
            stack[ss - 2 + n].From(sp.ss[i]);
          stack[ss].splitPoint = sp;
          sp.mutex.Bloquear();
          m_posActiva = pos;
          if (sp.nodeType == cTipoNodo.NO_PV)
            cSearch.Buscar(pos, stack, ss, sp.alpha, sp.beta, sp.depth, sp.cutNode, cTipoNodo.NO_PV, true);
          else if (sp.nodeType == cTipoNodo.PV)
            cSearch.Buscar(pos, stack, ss, sp.alpha, sp.beta, sp.depth, sp.cutNode, cTipoNodo.PV, true);
          else if (sp.nodeType == cTipoNodo.RAIZ)
            cSearch.Buscar(pos, stack, ss, sp.alpha, sp.beta, sp.depth, sp.cutNode, cTipoNodo.RAIZ, true);
          m_bBuscando = false;
          m_posActiva = null;
          sp.slavesMask[idx] = false;
          sp.allSlavesSearching = false;
          sp.nodes += (UInt32)pos.GetNodos();


          if (this != sp.masterThread
            && sp.slavesMask.none())
          {
            sp.masterThread.Signal();
          }



          sp.mutex.Liberar();
          if (cMotor.m_Threads.Count > 2)
            for (int i = 0; i < cMotor.m_Threads.Count; ++i)
            {
              int size = cMotor.m_Threads[i].m_nSplitPointSize;
              sp = size != 0 ? cMotor.m_Threads[i].m_SplitPoints[size - 1] : null;
              if (sp != null
                && sp.allSlavesSearching
                && Disponible(cMotor.m_Threads[i]))
              {

                cMotor.m_Threads.mutex.Bloquear();
                sp.mutex.Bloquear();
                if (sp.allSlavesSearching
                  && Disponible(cMotor.m_Threads[i]))
                {
                  sp.slavesMask[idx] = true;
                  m_SplitPointActivo = sp;
                  m_bBuscando = true;
                }
                sp.mutex.Liberar();
                cMotor.m_Threads.mutex.Liberar();
                break;
              }
            }
        }


        if (this_sp != null && this_sp.slavesMask.none())
        {
          this_sp.mutex.Bloquear();
          bool finished = this_sp.slavesMask.none();
          this_sp.mutex.Liberar();
          if (finished)
            return;
        }
      }
    }

    //------------------------------------------------------------------------------------------
    public virtual void Bucle()
    {
      OnIdle();
    }
  }

  //------------------------------------------------------------------------------------------
  public class cMainThread : cThread
  {
    new public bool m_bBuscando;

    //------------------------------------------------------------------------------------------
    public cMainThread()
      : base()
    {
      m_bBuscando = true;
    }

    //------------------------------------------------------------------------------------------
    public override void OnIdle()
    {
      while (true)
      {
        mutex.Bloquear();

        m_bBuscando = false;

        while (!m_bBuscando && !exit)
        {
          cMotor.m_Threads.sleepCondition.Signal();
          sleepCondition.Wait(mutex);
        }

        mutex.Liberar();

        if (exit)
          return;

        m_bBuscando = true;

        cSearch.Busqueda();

        m_bBuscando = false;
      }
    }

    //------------------------------------------------------------------------------------------
    public override void Bucle()
    {
      base.OnIdle();
    }

    //------------------------------------------------------------------------------------------
    public void Abort()
    {
      cSearch.m_Sennales.STOP = true;
      Signal();
    }
  }

}