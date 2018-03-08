using InOut;
using Motor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Types;

namespace EngineConsole
{
  class Program
  {
    //------------------------------------------------------------------------------
    public static void FromEngine(string str)
    {
      Debug.WriteLine(str);
      Console.WriteLine(str);
    }

    //------------------------------------------------------------------------------
    static void Main(string[] args)
    {
#if CHESSARIA
      cMotor.Init();
      
      cPosicion posStart = new cPosicion("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", false, cMotor.m_Threads.Principal());
      
      cMotor.m_Consola.SetDelegateFunction(FromEngine);
      
      cMotor.m_UCI.Command("uci", ref posStart);
      cMotor.m_UCI.Command("setoption name Hash value 128", ref posStart);
      cMotor.m_UCI.Command("setoption name Ponder value false", ref posStart);
      cMotor.m_UCI.Command("setoption name Threads value 1", ref posStart); //-- From 1 to 16
      cMotor.m_UCI.Command("setoption name MultiPV value 1", ref posStart); 
      cMotor.m_UCI.Command("setoption name OwnBook value false", ref posStart);

      if (args.Length > 3)
      {
        cMotor.m_UCI.Command("setoption name Level value " + args[0], ref posStart);
        cMotor.m_UCI.Command("setoption name LevelFilterK value " + float.Parse(args[2].ToString()).ToString(), ref posStart);
        cMotor.m_UCI.Command("setoption name LevelFilterC value " + float.Parse(args[3].ToString()).ToString(), ref posStart);
        cMotor.m_UCI.Command("position fen " + args[1], ref posStart);
      }
      else
      {
        cMotor.m_UCI.Command("setoption name Level value 16", ref posStart);
        cMotor.m_UCI.Command("setoption name LevelFilterK value -80,000", ref posStart);
        cMotor.m_UCI.Command("setoption name LevelFilterC value 0,010", ref posStart);

        //cMotor.m_UCI.Command("position fen ---k[skin=FireElement]k[skin=FireElement]---/k[skin=FireElement]-k[skin=FireElement]--k[skin=FireElement]-k[skin=FireElement]/p[skin=MiniDragon]p[skin=MiniDragon]p[skin=MiniDragon]O[pass=false]O[pass=false]p[skin=MiniDragon]p[skin=MiniDragon]p[skin=MiniDragon]/p[skin=MiniDragon]p[skin=MiniDragon]p[skin=MiniDragon]O[pass=false]O[pass=false]p[skin=MiniDragon]p[skin=MiniDragon]p[skin=MiniDragon]/PPPO[pass=false]O[pass=false]PPP/N-NO[pass=false]O[pass=false]N-N/---BB---/-RRQKRR- w - - 0 1 wmission domination,qty_1[P,N,R,B,Q,K],pos[a8,b8,c8,d8,e8,f8,g8,h8] bmission capture,qty_1[K] wenpassant false benpassant false w2sqpawn 0 b2sqpawn 0 wpromotionline 0 bpromotionline 0", ref posStart);
        cMotor.m_UCI.Command("position fen --------/--k[skin=FireElement]--k[skin=FireElement]--/--------/p[skin=MiniDragon]k[skin=FireElement]-k[skin=FireElement]-k[skin=FireElement]p[skin=MiniDragon]k[skin=FireElement]/p[skin=MiniDragon]p[skin=MiniDragon]p[skin=MiniDragon]p[skin=MiniDragon]p[skin=MiniDragon]p[skin=MiniDragon]p[skin=MiniDragon]p[skin=MiniDragon]/--------/PPPPPPPP/RNBQKBNR w - - 0 1 wmission capture,qty_16[p,n,b,r,q,k] bmission checkmate wenpassant true benpassant true w2sqpawn 2 b2sqpawn 7 wpromotionline 8 bpromotionline 1", ref posStart);


      }

      cMotor.m_UCI.Command("ucinewgame", ref posStart);
      cMotor.m_UCI.Command("isready", ref posStart);

      
      cMotor.m_UCI.Command("setoption name Clear Hash value true", ref posStart);

      cMotor.m_UCI.Command("go infinite", ref posStart);
      //cMotor.m_UCI.Command("go nodes 300000", ref posStart);
    
      //-- Waiting and stop when user needs
      System.Threading.Thread.Sleep(5000);
      
      cMotor.m_UCI.Command("stop", ref posStart);
       
      System.Threading.Thread.Sleep(10);

      Debug.Write(posStart.DibujaTablero());
      Console.Write(posStart.DibujaTablero());
#else
      cMotor motor = new cMotor(args);
#endif
    }
  }
}

