using System;

using System.Threading;
using System.Runtime.InteropServices;
using ComTypes = System.Runtime.InteropServices.ComTypes;
using System.Diagnostics;

namespace Motor
{
  //------------------------------------------------------------------------------
  //------------------------------------------------------------------------------
  public class cCpu
  {
    public static cCpu m_CPU = new cCpu();

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool GetSystemTimes(out ComTypes.FILETIME lpIdleTime, out ComTypes.FILETIME lpKernelTime, out ComTypes.FILETIME lpUserTime);

    ComTypes.FILETIME m_nKernel;
    ComTypes.FILETIME m_nUser;
    TimeSpan m_nTime;

    Int16 m_nUsoCPU;
    DateTime m_nLastTime;
    public static long m_nContador;

    //------------------------------------------------------------------------------
    public cCpu()
    {
      m_nUsoCPU = -1;
      m_nLastTime = DateTime.MinValue;
      m_nUser.dwHighDateTime = m_nUser.dwLowDateTime = 0;
      m_nKernel.dwHighDateTime = m_nKernel.dwLowDateTime = 0;
      m_nTime = TimeSpan.MinValue;
      m_nContador = 0;
    }

    //------------------------------------------------------------------------------
    public short Get()
    {
      short cpuCopy = m_nUsoCPU;
      if(Interlocked.Increment(ref m_nContador) == 1)
      {
        if(!EnoughTimePassed)
        {
          Interlocked.Decrement(ref m_nContador);
          return cpuCopy;
        }

        ComTypes.FILETIME sysIdle, sysKernel, sysUser;
        TimeSpan procTiempo;

        Process process = Process.GetCurrentProcess();
        procTiempo = process.TotalProcessorTime;

        if(!GetSystemTimes(out sysIdle, out sysKernel, out sysUser))
        {
          Interlocked.Decrement(ref m_nContador);
          return cpuCopy;
        }

        if(!bPrimeraVez)
        {
          UInt64 sysKernelDiff = SubtractTimes(sysKernel, m_nKernel);
          UInt64 sysUserDiff = SubtractTimes(sysUser, m_nUser);

          UInt64 sysTotal = sysKernelDiff + sysUserDiff;

          Int64 procTotal = procTiempo.Ticks - m_nTime.Ticks;

          if(sysTotal > 0)
          {
            m_nUsoCPU = (short)((100.0 * procTotal) / sysTotal);
          }
        }

        m_nTime = procTiempo;
        m_nKernel = sysKernel;
        m_nUser = sysUser;

        m_nLastTime = DateTime.Now;

        cpuCopy = m_nUsoCPU;
      }
      Interlocked.Decrement(ref m_nContador);

      return cpuCopy;

    }

    //------------------------------------------------------------------------------
    private UInt64 SubtractTimes(ComTypes.FILETIME a, ComTypes.FILETIME b)
    {
      UInt64 aInt = ((UInt64)(a.dwHighDateTime << 32)) | (UInt64)a.dwLowDateTime;
      UInt64 bInt = ((UInt64)(b.dwHighDateTime << 32)) | (UInt64)b.dwLowDateTime;

      return aInt - bInt;
    }

    //------------------------------------------------------------------------------
    private bool EnoughTimePassed
    {
      get
      {
        const int minimumElapsedMS = 250;
        TimeSpan sinceLast = DateTime.Now - m_nLastTime;
        return sinceLast.TotalMilliseconds > minimumElapsedMS;
      }
    }

    //------------------------------------------------------------------------------
    private bool bPrimeraVez
    {
      get
      {
        return (m_nLastTime == DateTime.MinValue);
      }
    }
  }
}
