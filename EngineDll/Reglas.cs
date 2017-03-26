using Types;

using bitbrd = System.UInt64;
using type = System.Int32;
using mov = System.Int32;
using color = System.Int32;
using enroque = System.Int32;
using sq = System.Int32;
using pieza = System.Int32;
using val = System.Int32;

namespace Motor
{
  //----------------------------------------------------------------------------------------
  public struct cMov
  {
    public mov m;
    public val v;
  };

  //----------------------------------------------------------------------------------------
  //----------------------------------------------------------------------------------------
  public class cReglas
  {
    public const int MAX_MOVES = 256;
    public cMov[] m_ListMoves = new cMov[MAX_MOVES];
    private int m_Current = 0, m_Last = 0;

    //----------------------------------------------------------------------------------------
    public cReglas(cPosicion pos, type tipo)
    {
      m_Current=0;
      m_Last=cReglas.Generar(pos, m_ListMoves, 0, tipo);
      m_ListMoves[m_Last].m=cMovType.MOV_NAN;
    }

    //----------------------------------------------------------------------------------------
    public static cReglas operator ++(cReglas moveList)
    {
      ++moveList.m_Current;
      return moveList;
    }

    //----------------------------------------------------------------------------------------
    public mov GetActualMov()
    {
      return m_ListMoves[m_Current].m;
    }

    //----------------------------------------------------------------------------------------
    public int Size()
    {
      return m_Last;
    }

    //----------------------------------------------------------------------------------------
    public bool Exist(mov m)
    {
      for(int it = 0; it!=m_Last; ++it)
        if(m_ListMoves[it].m==m)
          return true;
      return false;
    }

    //----------------------------------------------------------------------------------------
    public static int Enroques(cPosicion pos, cMov[] mlist, int mPos, color us, cInfoJaque ci, enroque Cr, bool Checks, bool Chess960)
    {
      bool KingSide = (Cr==cEnroque.OO_BLANCAS||Cr==cEnroque.OO_NEGRAS);

      if(pos.CanNotEnroque(Cr)||0==pos.PosibleEnrocar(Cr))
        return mPos;

      sq kfrom = pos.GetRey(us);
      sq rfrom = pos.CasillaTorreEnroque(Cr);
      sq kto = cTypes.CasillaProxima(us, KingSide ? cCasilla.G1 : cCasilla.C1);
      bitbrd enemies = pos.PiezasColor(cTypes.Contrario(us));

      sq K = Chess960 ? kto>kfrom ? cCasilla.OESTE : cCasilla.ESTE : KingSide ? cCasilla.OESTE : cCasilla.ESTE;

      for(sq s = kto; s!=kfrom; s+=K)
        if((pos.AtaquesA(s)&enemies)!=0)
          return mPos;

      if(Chess960&&(cBitBoard.AtaquesPieza(kto, pos.Piezas()^cBitBoard.m_nCasillas[rfrom], cPieza.TORRE)&pos.PiezasColor(cTypes.Contrario(us), cPieza.TORRE, cPieza.DAMA))!=0)
        return mPos;

      if(pos.IsObstaculo(rfrom) == false)
      {
        mov m = cTypes.CreaMov(kfrom, rfrom, cMovType.ENROQUE, cPieza.CABALLO);

        if(Checks && !pos.IsJaque(m, ci))
          return mPos;

        mlist[mPos++].m = m;
      }

      return mPos;
    }

    //----------------------------------------------------------------------------------------
    public static int Promocion(cPosicion pos, cMov[] mlist, int mPos, bitbrd pawnsOn7, bitbrd target, cInfoJaque ci, type Type, sq Delta)
    {
      bitbrd b = cBitBoard.Desplazamiento(pawnsOn7, Delta)&target;

      while(b!=0)
      {
        sq to = cBitBoard.GetLSB(ref b);
        if(pos.IsObstaculo(to) == false)
        {

          if(Type == cMovType.CAPTURES || Type == cMovType.EVASIONS || Type == cMovType.NON_EVASIONS)
            mlist[mPos++].m = cTypes.CreaMov(to - Delta, to, cMovType.PROMOCION, cPieza.DAMA);

          if(Type == cMovType.QUIETS || Type == cMovType.EVASIONS || Type == cMovType.NON_EVASIONS)
          {
            mlist[mPos++].m = cTypes.CreaMov(to - Delta, to, cMovType.PROMOCION, cPieza.TORRE);
            mlist[mPos++].m = cTypes.CreaMov(to - Delta, to, cMovType.PROMOCION, cPieza.ALFIL);
            mlist[mPos++].m = cTypes.CreaMov(to - Delta, to, cMovType.PROMOCION, cPieza.CABALLO);
          }

          if(Type == cMovType.QUIET_CHECKS && (cBitBoard.m_Ataques[cPieza.CABALLO_BLANCO][to] & cBitBoard.m_nCasillas[ci.m_SqRey]) != 0)
            mlist[mPos++].m = cTypes.CreaMov(to - Delta, to, cMovType.PROMOCION, cPieza.CABALLO);
        }
      }

      return mPos;
    }

    //----------------------------------------------------------------------------------------
    public static int Peones(cPosicion pos, cMov[] mlist, int mPos, bitbrd target, cInfoJaque ci, color colr, type Type)
    {
      color colorVS = (colr==cColor.BLANCO ? cColor.NEGRO : cColor.BLANCO);
      bitbrd TRank8BB = (colr==cColor.BLANCO ? cBitBoard.F8 : cBitBoard.F1);
      bitbrd TRank7BB = (colr==cColor.BLANCO ? cBitBoard.F7 : cBitBoard.F2);
      bitbrd TRank3BB = (colr==cColor.BLANCO ? cBitBoard.F3 : cBitBoard.F6);
      sq Up = (colr==cColor.BLANCO ? cCasilla.NORTE : cCasilla.SUR);
      sq Right = (colr==cColor.BLANCO ? cCasilla.NORTE_ESTE : cCasilla.SUR_OESTE);
      sq Left = (colr==cColor.BLANCO ? cCasilla.NORTE_OESTE : cCasilla.SUR_ESTE);

      bitbrd b1, b2, dc1, dc2, emptySquares = 0;

      bitbrd pawnsOn7 = pos.PiezasColor(colr, cPieza.PEON)&TRank7BB;
      bitbrd pawnsNotOn7 = pos.PiezasColor(colr, cPieza.PEON)&~TRank7BB;

      bitbrd enemies = (Type==cMovType.EVASIONS ? pos.PiezasColor(colorVS)&target :
                          Type==cMovType.CAPTURES ? target : pos.PiezasColor(colorVS));

      if(Type!=cMovType.CAPTURES)
      {
        emptySquares=(Type==cMovType.QUIETS||Type==cMovType.QUIET_CHECKS ? target : ~pos.Piezas());

        b1=cBitBoard.Desplazamiento(pawnsNotOn7, Up)&emptySquares;
        b2=cBitBoard.Desplazamiento(b1&TRank3BB, Up)&emptySquares;

        if(Type==cMovType.EVASIONS)
        {
          b1&=target;
          b2&=target;
        }

        if(Type==cMovType.QUIET_CHECKS)
        {
          b1&=pos.AtaquesDePeon(ci.m_SqRey, colorVS);
          b2&=pos.AtaquesDePeon(ci.m_SqRey, colorVS);

          if((pawnsNotOn7&ci.m_Candidatas)!=0)
          {
            dc1=cBitBoard.Desplazamiento(pawnsNotOn7&ci.m_Candidatas, Up)&emptySquares&~cBitBoard.GetColumnaSq(ci.m_SqRey);
            dc2=cBitBoard.Desplazamiento(dc1&TRank3BB, Up)&emptySquares;

            b1|=dc1;
            b2|=dc2;
          }
        }

        while(b1!=0)
        {
          sq to = cBitBoard.GetLSB(ref b1);
          if(pos.IsObstaculo(to) == false)
            mlist[mPos++].m=cTypes.CreaMov(to-Up, to);
        }

        while(b2!=0)
        {
          sq to = cBitBoard.GetLSB(ref b2);
          if(pos.IsObstaculo(to) == false)
            mlist[mPos++].m=cTypes.CreaMov(to-Up-Up, to);
        }
      }

      if(pawnsOn7!=0&&(Type!=cMovType.EVASIONS||(target&TRank8BB)!=0))
      {
        if(Type==cMovType.CAPTURES)
          emptySquares=~pos.Piezas();

        if(Type==cMovType.EVASIONS)
          emptySquares&=target;

        mPos=Promocion(pos, mlist, mPos, pawnsOn7, enemies, ci, Type, Right);
        mPos=Promocion(pos, mlist, mPos, pawnsOn7, enemies, ci, Type, Left);
        mPos=Promocion(pos, mlist, mPos, pawnsOn7, emptySquares, ci, Type, Up);
      }

      if(Type==cMovType.CAPTURES||Type==cMovType.EVASIONS||Type==cMovType.NON_EVASIONS)
      {
        b1=cBitBoard.Desplazamiento(pawnsNotOn7, Right)&enemies;
        b2=cBitBoard.Desplazamiento(pawnsNotOn7, Left)&enemies;

        while(b1!=0)
        {
          sq to = cBitBoard.GetLSB(ref b1);
          if(pos.IsObstaculo(to) == false)
            mlist[mPos++].m=cTypes.CreaMov(to-Right, to);
        }

        while(b2!=0)
        {
          sq to = cBitBoard.GetLSB(ref b2);
          if(pos.IsObstaculo(to) == false)
            mlist[mPos++].m=cTypes.CreaMov(to-Left, to);
        }

        if(pos.CasillaEnPaso()!=cCasilla.NONE)
        {
          if(Type==cMovType.EVASIONS&&(target&cBitBoard.m_nCasillas[(pos.CasillaEnPaso()-Up)])==0)
            return mPos;

          b1=pawnsNotOn7&pos.AtaquesDePeon(pos.CasillaEnPaso(), colorVS);

          while(b1 != 0)
          {
            type btb = cBitBoard.GetLSB(ref b1);
            if(pos.IsObstaculo(pos.CasillaEnPaso()) == false)
              mlist[mPos++].m = cTypes.CreaMov(btb, pos.CasillaEnPaso(), cMovType.ENPASO, cPieza.CABALLO);
          }
        }
      }

      return mPos;
    }

    //----------------------------------------------------------------------------------------
    public static int Piezas(cPosicion pos, cMov[] mlist, int mPos, color us, bitbrd target, cInfoJaque ci, type Pt, bool Checks)
    {
      sq[] pieceList = pos.GetList(us, Pt);
      int pl = 0;

      for(sq from = pieceList[pl]; from!=cCasilla.NONE; from=pieceList[++pl])
      {
        if(Checks)
        {
          if((Pt==cPieza.ALFIL||Pt==cPieza.TORRE||Pt==cPieza.DAMA)
              &&0==(cBitBoard.m_PseudoAtaques[Pt][from]&target&ci.m_Jaque[Pt]))
            continue;

          if(ci.m_Candidatas!=0&&(ci.m_Candidatas&cBitBoard.m_nCasillas[from])!=0)
            continue;
        }
        if(pos.IsObstaculo(from) == true)
          continue;

        bitbrd b = pos.AtaquesDesdeTipoDePieza(from, Pt)&target;

        if(Checks)
          b&=ci.m_Jaque[Pt];

        while(b != 0)
        {
          type to = cBitBoard.GetLSB(ref b);
          if(pos.IsObstaculo(to) == false)
            mlist[mPos++].m = cTypes.CreaMov(from, to);
        }
      }

      return mPos;
    }

    //----------------------------------------------------------------------------------------
    public static int ToDO(cPosicion pos, cMov[] mlist, int mPos, bitbrd target, color us, type Type, cInfoJaque ci)
    {
      bool Checks = Type==cMovType.QUIET_CHECKS;

      mPos=Peones(pos, mlist, mPos, target, ci, us, Type);
      mPos=Piezas(pos, mlist, mPos, us, target, ci, cPieza.CABALLO, Checks);
      mPos=Piezas(pos, mlist, mPos, us, target, ci, cPieza.ALFIL, Checks);
      mPos=Piezas(pos, mlist, mPos, us, target, ci, cPieza.TORRE, Checks);
      mPos=Piezas(pos, mlist, mPos, us, target, ci, cPieza.DAMA, Checks);

      if(Type!=cMovType.QUIET_CHECKS&&Type!=cMovType.EVASIONS)
      {
        sq ksq = pos.GetRey(us);
        bitbrd b = pos.AtaquesDesdeTipoDePieza(ksq, cPieza.REY)&target;
        while(b != 0)
        {
          type to = cBitBoard.GetLSB(ref b);
          if(pos.IsObstaculo(to) == false)
            mlist[mPos++].m = cTypes.CreaMov(ksq, to);
        }
      }

      if(Type!=cMovType.CAPTURES&&Type!=cMovType.EVASIONS&&pos.CanEnroque(us)!=0)
      {
        if (pos.IsChess960() != false)
        {
          mPos=Enroques(pos, mlist, mPos, us, ci, (new cEnroque(us, cEnroque.LADO_REY)).m_Tipo, Checks, true);
          mPos=Enroques(pos, mlist, mPos, us, ci, (new cEnroque(us, cEnroque.LADO_DAMA)).m_Tipo, Checks, true);
        }
        else
        {
          mPos=Enroques(pos, mlist, mPos, us, ci, (new cEnroque(us, cEnroque.LADO_REY)).m_Tipo, Checks, false);
          mPos=Enroques(pos, mlist, mPos, us, ci, (new cEnroque(us, cEnroque.LADO_DAMA)).m_Tipo, Checks, false);
        }
      }

      return mPos;
    }

    //----------------------------------------------------------------------------------------
    public static int Capturas(cPosicion pos, cMov[] mlist, int mPos, type Type)
    {

      color us = pos.ColorMueve();

      bitbrd target = Type==cMovType.CAPTURES ? pos.PiezasColor(cTypes.Contrario(us))
            : Type==cMovType.QUIETS ? ~pos.Piezas()
            : Type==cMovType.NON_EVASIONS ? ~pos.PiezasColor(us) : 0;

      return us==cColor.BLANCO ? ToDO(pos, mlist, mPos, target, cColor.BLANCO, Type, null)
                                : ToDO(pos, mlist, mPos, target, cColor.NEGRO, Type, null);
    }

    //----------------------------------------------------------------------------------------
    public static int Evasiones(cPosicion pos, cMov[] mlist, int mPos)
    {
      color us = pos.ColorMueve();
      sq ksq = pos.GetRey(us);
      bitbrd sliderAttacks = 0;
      bitbrd sliders = pos.Jaques()&~pos.GetNumPiezas(cPieza.CABALLO, cPieza.PEON);

      while(sliders!=0)
      {
        sq checksq = cBitBoard.GetLSB(ref sliders);
        sliderAttacks|=cBitBoard.m_EnLinea[checksq][ksq]^cBitBoard.m_nCasillas[checksq];
      }

      bitbrd b = pos.AtaquesDesdeTipoDePieza(ksq, cPieza.REY)&~pos.PiezasColor(us)&~sliderAttacks;
      while(b != 0)
      {
        type to = cBitBoard.GetLSB(ref b);
        if(pos.IsObstaculo(to) == false)
          mlist[mPos++].m = cTypes.CreaMov(ksq, to);
      }

      if(cBitBoard.MayorQue(pos.Jaques()))
        return mPos;

      sq checksq2 = cBitBoard.LSB(pos.Jaques());
      bitbrd target = cBitBoard.Entre(checksq2, ksq)|cBitBoard.m_nCasillas[checksq2];

      return us==cColor.BLANCO ? ToDO(pos, mlist, mPos, target, cColor.BLANCO, cMovType.EVASIONS, null) :
                                  ToDO(pos, mlist, mPos, target, cColor.NEGRO, cMovType.EVASIONS, null);
    }

    //----------------------------------------------------------------------------------------
    public static int JugadasLegales(cPosicion pos, cMov[] mlist, int mPos)
    {
      int end, cur = mPos;
      bitbrd pinned = pos.PiezasClavadas(pos.ColorMueve());
      sq ksq = pos.GetRey(pos.ColorMueve());

      //-- Si es jaque evasiones, sino generar capturas. El final es una captura
      end=pos.Jaques()!=0 ? Evasiones(pos, mlist, mPos) : Generar(pos, mlist, mPos, cMovType.NON_EVASIONS);

      while(cur != end)
      {
        if(pos.IsObstaculo(end) == true || pos.IsObstaculo(cur))
          mlist[cur].m = mlist[--end].m;
        else if((pinned != 0 || cTypes.GetFromCasilla(mlist[cur].m) == ksq || cTypes.TipoMovimiento(mlist[cur].m) == cMovType.ENPASO)
            && pos.IsLegalMov(mlist[cur].m, pinned) == false)
          mlist[cur].m = mlist[--end].m;
        else
          ++cur;

      }

      return end;
    }

    //----------------------------------------------------------------------------------------
    public static int Jaques(cPosicion pos, cMov[] mlist, int mPos)
    {
      color us = pos.ColorMueve();
      cInfoJaque ci = new cInfoJaque(pos);
      bitbrd dc = ci.m_Candidatas;

      while(dc!=0)
      {
        sq from = cBitBoard.GetLSB(ref dc);
        type pt = cTypes.TipoPieza(pos.GetPieza(from));

        if(pt==cPieza.PEON)
          continue;


        bitbrd b = pos.AtaquesDePieza((pieza)pt, from)&~pos.Piezas();

        if(pt==cPieza.REY)
          b&=~cBitBoard.m_PseudoAtaques[cPieza.DAMA][ci.m_SqRey];

        while(b != 0)
        {
          type to = cBitBoard.GetLSB(ref b);
          if(pos.IsObstaculo(to) == false)
            mlist[mPos++].m = cTypes.CreaMov(from, to);
        }
      }

      return us==cColor.BLANCO ? ToDO(pos, mlist, mPos, ~pos.Piezas(), cColor.BLANCO, cMovType.QUIET_CHECKS, ci) :
                                  ToDO(pos, mlist, mPos, ~pos.Piezas(), cColor.NEGRO, cMovType.QUIET_CHECKS, ci);
    }

    //----------------------------------------------------------------------------------------
    public static int Generar(cPosicion pos, cMov[] mlist, int mPos, type Type)
    {
      switch(Type)
      {
        case cMovType.LEGAL:
          return JugadasLegales(pos, mlist, mPos);
        case cMovType.CAPTURES:
          return Capturas(pos, mlist, mPos, Type);
        case cMovType.QUIETS:
          return Capturas(pos, mlist, mPos, Type);
        case cMovType.NON_EVASIONS:
          return Capturas(pos, mlist, mPos, Type);
        case cMovType.EVASIONS:
          return Evasiones(pos, mlist, mPos);
        case cMovType.QUIET_CHECKS:
          return Jaques(pos, mlist, mPos);
      }

      return 0;
    }
  }
}
