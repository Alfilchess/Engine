using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using TableBases.Properties;
using System.Diagnostics;

namespace TableBases
{
  public enum MateResult
  {
    MATE_BLANCAS, MATE_NEGRAS, TABLAS, NAN
  };

  public struct ProbeResultType
  {
 
    public bool found;
    public MateResult stm;
    public string error;
    public int ply;
    public int dtm;
  }

  public static class cTablaFinales
  {
    static int entries_per_block = 16*1024;
    static Stream currentStream;
    static string currentFilename;
    static List<string> validTables = new List<string>(15);
    public static Dictionary<int, int> a8toa1 = new Dictionary<int, int>();

    const int A1 = 0;
    const int B1 = 1;
    const int C1 = 2;
    const int D1 = 3;
    const int E1 = 4;
    const int F1 = 5;
    const int G1 = 6;
    const int H1 = 7;
    const int A2 = 8;
    const int B2 = 9;
    const int C2 = 10;
    const int D2 = 11;
    const int E2 = 12;
    const int F2 = 13;
    const int G2 = 14;
    const int H2 = 15;
    const int A3 = 16;
    const int B3 = 17;
    const int C3 = 18;
    const int D3 = 19;
    const int E3 = 20;
    const int F3 = 21;
    const int G3 = 22;
    const int H3 = 23;
    const int A4 = 24;
    const int B4 = 25;
    const int C4 = 26;
    const int D4 = 27;
    const int E4 = 28;
    const int F4 = 29;
    const int G4 = 30;
    const int H4 = 31;
    const int A5 = 32;
    const int B5 = 33;
    const int C5 = 34;
    const int D5 = 35;
    const int E5 = 36;
    const int F5 = 37;
    const int G5 = 38;
    const int H5 = 39;
    const int A6 = 40;
    const int B6 = 41;
    const int C6 = 42;
    const int D6 = 43;
    const int E6 = 44;
    const int F6 = 45;
    const int G6 = 46;
    const int H6 = 47;
    const int A7 = 48;
    const int B7 = 49;
    const int C7 = 50;
    const int D7 = 51;
    const int E7 = 52;
    const int F7 = 53;
    const int G7 = 54;
    const int H7 = 55;
    const int A8 = 56;
    const int B8 = 57;
    const int C8 = 58;
    const int D8 = 59;
    const int E8 = 60;
    const int F8 = 61;
    const int G8 = 62;
    const int H8 = 63;
    const int NOINDEX = -1;

    static List<int> whitePieceSquares = new List<int>();
    static List<int> blackPieceSquares = new List<int>();
    static int MAX_KKINDEX = 462;
    static int MAX_PPINDEX = 576;
    static int MAX_PpINDEX = (24*48);
    static int MAX_AAINDEX = ((63-62)+(62*(127-62)/2)-1+1);
    static int MAX_AAAINDEX = (64*21*31);
    static int MAX_PPP48_INDEX = 8648;
    static int MAX_PP48_INDEX = (1128);

    static int MAX_KXK = MAX_KKINDEX*64;
    static int MAX_kabk = MAX_KKINDEX*64*64;
    static int MAX_kakb = MAX_KKINDEX*64*64;
    static int MAX_kpk = 24*64*64;
    static int MAX_kakp = 24*64*64*64;
    static int MAX_kapk = 24*64*64*64;
    static int MAX_kppk = MAX_PPINDEX*64*64;
    static int MAX_kpkp = MAX_PpINDEX*64*64;
    static int MAX_kaak = MAX_KKINDEX*MAX_AAINDEX;
    static int MAX_kabkc = MAX_KKINDEX*64*64*64;
    static int MAX_kabck = MAX_KKINDEX*64*64*64;
    static int MAX_kaakb = MAX_KKINDEX*MAX_AAINDEX*64;
    static int MAX_kaabk = MAX_KKINDEX*MAX_AAINDEX*64;
    static int MAX_kabbk = MAX_KKINDEX*MAX_AAINDEX*64;
    static int MAX_kaaak = MAX_KKINDEX*MAX_AAAINDEX;
    static int MAX_kapkb = 24*64*64*64*64;
    static int MAX_kabkp = 24*64*64*64*64;
    static int MAX_kabpk = 24*64*64*64*64;
    static int MAX_kppka = MAX_PPINDEX*64*64*64;
    static int MAX_kappk = MAX_PPINDEX*64*64*64;
    static int MAX_kapkp = MAX_PPINDEX*64*64*64;
    static int MAX_kaapk = 24*MAX_AAINDEX*64*64;
    static int MAX_kaakp = 24*MAX_AAINDEX*64*64;
    static int MAX_kppkp = 24*MAX_PP48_INDEX*64*64;
    static int MAX_kpppk = MAX_PPP48_INDEX*64*64;

    static int PLYSHIFT = 3;
    static int INFOMASK = 7;

    internal static Func<int> currentPctoi;

    class endgamekey
    {
      internal int maxindex;
      internal int slice_n;
      internal Func<int> pctoi;

      public endgamekey(int a_maxindex, int a_slice_n, Func<int> a_pctoi)
      {
        this.maxindex=a_maxindex;
        this.slice_n=a_slice_n;
        this.pctoi=a_pctoi;
      }
    };

    static cTablaFinales()
    {
      //-- stub, needed to initialize variables
    }

    static Dictionary<string, endgamekey> egkey = new Dictionary<string, endgamekey>()
          {
              { "kqk",  new endgamekey(MAX_KXK, 1, kxk_pctoindex)},
              { "krk",  new endgamekey(MAX_KXK, 1, kxk_pctoindex)},
              { "kbk",  new endgamekey(MAX_KXK, 1, kxk_pctoindex)},
              { "knk",  new endgamekey(MAX_KXK, 1, kxk_pctoindex)},
              { "kpk",  new endgamekey(MAX_kpk, 24, kpk_pctoindex)},

              { "kqkq", new endgamekey(MAX_kakb, 1, kakb_pctoindex) },
              { "kqkr", new endgamekey(MAX_kakb, 1, kakb_pctoindex)},
              { "kqkb", new endgamekey(MAX_kakb, 1, kakb_pctoindex)},
              { "kqkn", new endgamekey(MAX_kakb, 1, kakb_pctoindex)},

              { "krkr", new endgamekey(MAX_kakb, 1,  kakb_pctoindex)},
              {"krkb", new endgamekey(MAX_kakb, 1, kakb_pctoindex)},
              {"krkn", new endgamekey(MAX_kakb, 1, kakb_pctoindex)},

              {"kbkb", new endgamekey(MAX_kakb, 1,  kakb_pctoindex)},
              {"kbkn", new endgamekey(MAX_kakb, 1, kakb_pctoindex)},

              {"knkn", new endgamekey(MAX_kakb, 1, kakb_pctoindex)},
      /**/
              {"kqqk", new endgamekey(MAX_kaak, 1, kaak_pctoindex)},
              {"kqrk", new endgamekey(MAX_kabk, 1,  kabk_pctoindex)},
              {"kqbk", new endgamekey(MAX_kabk, 1,  kabk_pctoindex)},
              {"kqnk", new endgamekey(MAX_kabk, 1,  kabk_pctoindex)},

              {"krrk", new endgamekey(MAX_kaak, 1,  kaak_pctoindex)},
              {"krbk", new endgamekey(MAX_kabk, 1, kabk_pctoindex)},
              {"krnk",new endgamekey( MAX_kabk, 1,  kabk_pctoindex)},

              {"kbbk", new endgamekey(MAX_kaak, 1,  kaak_pctoindex)},
              {"kbnk", new endgamekey(MAX_kabk, 1,  kabk_pctoindex)},

              {"knnk", new endgamekey(MAX_kaak, 1,  kaak_pctoindex)},
      /**/
      /**/
              {"kqkp", new endgamekey(MAX_kakp, 24, kakp_pctoindex)},
              {"krkp", new endgamekey(MAX_kakp, 24, kakp_pctoindex)},
              {"kbkp", new endgamekey(MAX_kakp, 24, kakp_pctoindex)},
              {"knkp", new endgamekey(MAX_kakp, 24, kakp_pctoindex)},
      /**/
              {"kqpk", new endgamekey(MAX_kapk, 24, kapk_pctoindex)},
              {"krpk", new endgamekey(MAX_kapk, 24, kapk_pctoindex)},
              {"kbpk", new endgamekey(MAX_kapk, 24, kapk_pctoindex)},
              {"knpk", new endgamekey(MAX_kapk, 24, kapk_pctoindex)},
      /**/
              {"kppk", new endgamekey(MAX_kppk, MAX_PPINDEX , kppk_pctoindex)},
      /**/
              {"kpkp", new endgamekey(MAX_kpkp, MAX_PpINDEX , kpkp_pctoindex)},

              {"kppkp", new endgamekey(MAX_kppkp, 24*MAX_PP48_INDEX, kppkp_pctoindex) },

              {"kbbkr", new endgamekey(MAX_kaakb, 1, kaakb_pctoindex) },
              {"kbbkb", new endgamekey(MAX_kaakb, 1, kaakb_pctoindex) },
              {"knnkb", new endgamekey(MAX_kaakb, 1, kaakb_pctoindex) },
              {"knnkn", new endgamekey(MAX_kaakb, 1, kaakb_pctoindex) },

      //-- pwned does use anything below here
      { "kqqqk", new endgamekey(MAX_kaaak, 1, kaaak_pctoindex)},
              { "kqqrk", new endgamekey(MAX_kaabk, 1, kaabk_pctoindex)},
              { "kqqbk", new endgamekey(MAX_kaabk, 1, kaabk_pctoindex)},
              { "kqqnk", new endgamekey(MAX_kaabk, 1, kaabk_pctoindex)},
              { "kqrrk", new endgamekey(MAX_kabbk, 1, kabbk_pctoindex)},
              { "kqrbk", new endgamekey(MAX_kabck, 1, kabck_pctoindex)},
              { "kqrnk", new endgamekey(MAX_kabck, 1, kabck_pctoindex)},
              { "kqbbk", new endgamekey(MAX_kabbk, 1, kabbk_pctoindex)},
              { "kqbnk", new endgamekey(MAX_kabck, 1, kabck_pctoindex)},
              { "kqnnk", new endgamekey(MAX_kabbk, 1, kabbk_pctoindex)},
              { "krrrk", new endgamekey(MAX_kaaak, 1, kaaak_pctoindex)},
              { "krrbk", new endgamekey(MAX_kaabk, 1, kaabk_pctoindex)},
              { "krrnk", new endgamekey(MAX_kaabk, 1, kaabk_pctoindex)},
              { "krbbk", new endgamekey(MAX_kabbk, 1, kabbk_pctoindex)},
              { "krbnk", new endgamekey(MAX_kabck, 1, kabck_pctoindex)},
              { "krnnk", new endgamekey(MAX_kabbk, 1, kabbk_pctoindex)},
              { "kbbbk", new endgamekey(MAX_kaaak, 1, kaaak_pctoindex)},
              { "kbbnk", new endgamekey(MAX_kaabk, 1, kaabk_pctoindex)},
              { "kbnnk", new endgamekey(MAX_kabbk, 1, kabbk_pctoindex)},
              { "knnnk", new endgamekey(MAX_kaaak, 1, kaaak_pctoindex)},
              { "kqqkq", new endgamekey(MAX_kaakb, 1, kaakb_pctoindex)},
              { "kqqkr", new endgamekey(MAX_kaakb, 1, kaakb_pctoindex)},
              { "kqqkb", new endgamekey(MAX_kaakb, 1, kaakb_pctoindex)},
              { "kqqkn", new endgamekey(MAX_kaakb, 1, kaakb_pctoindex)},
              { "kqrkq", new endgamekey(MAX_kabkc, 1, kabkc_pctoindex)},
              { "kqrkr", new endgamekey(MAX_kabkc, 1, kabkc_pctoindex)},
              { "kqrkb", new endgamekey(MAX_kabkc, 1, kabkc_pctoindex)},
              { "kqrkn", new endgamekey(MAX_kabkc, 1, kabkc_pctoindex)},
              { "kqbkq", new endgamekey(MAX_kabkc, 1, kabkc_pctoindex)},
              { "kqbkr", new endgamekey(MAX_kabkc, 1, kabkc_pctoindex)},
              { "kqbkb", new endgamekey(MAX_kabkc, 1, kabkc_pctoindex)},
              { "kqbkn", new endgamekey(MAX_kabkc, 1, kabkc_pctoindex)},
              { "kqnkq", new endgamekey(MAX_kabkc, 1, kabkc_pctoindex)},
              { "kqnkr", new endgamekey(MAX_kabkc, 1, kabkc_pctoindex)},
              { "kqnkb", new endgamekey(MAX_kabkc, 1, kabkc_pctoindex)},
              { "kqnkn", new endgamekey(MAX_kabkc, 1, kabkc_pctoindex)},
              { "krrkq", new endgamekey(MAX_kaakb, 1, kaakb_pctoindex)},
              { "krrkr", new endgamekey(MAX_kaakb, 1, kaakb_pctoindex)},
              { "krrkb", new endgamekey(MAX_kaakb, 1, kaakb_pctoindex)},
              { "krrkn", new endgamekey(MAX_kaakb, 1, kaakb_pctoindex)},
              { "krbkq", new endgamekey(MAX_kabkc, 1, kabkc_pctoindex)},
              { "krbkr", new endgamekey(MAX_kabkc, 1, kabkc_pctoindex)},
              { "krbkb", new endgamekey(MAX_kabkc, 1, kabkc_pctoindex)},
              { "krbkn", new endgamekey(MAX_kabkc, 1, kabkc_pctoindex)},
              { "krnkq", new endgamekey(MAX_kabkc, 1, kabkc_pctoindex)},
              { "krnkr", new endgamekey(MAX_kabkc, 1, kabkc_pctoindex)},
              { "krnkb", new endgamekey(MAX_kabkc, 1, kabkc_pctoindex)},
              { "krnkn", new endgamekey(MAX_kabkc, 1, kabkc_pctoindex)},
              { "kbbkq", new endgamekey(MAX_kaakb, 1, kaakb_pctoindex)},
      //{ 84,"kbbkr", MAX_kaakb, 1, kaakb_indextopc, kaakb_pctoindex, NULL ,  NULL   ,NULL ,0, 0 },
      //{ 85,"kbbkb", MAX_kaakb, 1, kaakb_indextopc, kaakb_pctoindex, NULL ,  NULL   ,NULL ,0, 0 },
      { "kbbkn", new endgamekey(MAX_kaakb, 1, kaakb_pctoindex)},
              { "kbnkq", new endgamekey(MAX_kabkc, 1, kabkc_pctoindex)},
              { "kbnkr", new endgamekey(MAX_kabkc, 1, kabkc_pctoindex)},
              { "kbnkb", new endgamekey(MAX_kabkc, 1, kabkc_pctoindex)},
              { "kbnkn", new endgamekey(MAX_kabkc, 1, kabkc_pctoindex)},
              { "knnkq", new endgamekey(MAX_kaakb, 1, kaakb_pctoindex)},
              { "knnkr", new endgamekey(MAX_kaakb, 1, kaakb_pctoindex)},
      //{ 93,"knnkb", MAX_kaakb, 1, kaakb_indextopc, kaakb_pctoindex, NULL ,  NULL   ,NULL ,0, 0 },
      //{ 94,"knnkn", MAX_kaakb, 1, kaakb_indextopc, kaakb_pctoindex, NULL ,  NULL   ,NULL ,0, 0 },

      { "kqqpk", new endgamekey(MAX_kaapk, 24, kaapk_pctoindex)},
              { "kqrpk", new endgamekey(MAX_kabpk, 24, kabpk_pctoindex)},
              { "kqbpk", new endgamekey(MAX_kabpk, 24, kabpk_pctoindex)},
              { "kqnpk", new endgamekey(MAX_kabpk, 24, kabpk_pctoindex)},
              { "krrpk", new endgamekey(MAX_kaapk, 24, kaapk_pctoindex)},
              { "krbpk", new endgamekey(MAX_kabpk, 24, kabpk_pctoindex)},
              { "krnpk", new endgamekey(MAX_kabpk, 24, kabpk_pctoindex)},
              { "kbbpk", new endgamekey(MAX_kaapk, 24, kaapk_pctoindex)},
              { "kbnpk", new endgamekey(MAX_kabpk, 24, kabpk_pctoindex)},
              { "knnpk", new endgamekey(MAX_kaapk, 24, kaapk_pctoindex)},

              { "kqppk", new endgamekey(MAX_kappk, MAX_PPINDEX, kappk_pctoindex)},
              { "krppk", new endgamekey(MAX_kappk, MAX_PPINDEX, kappk_pctoindex)},
              { "kbppk", new endgamekey(MAX_kappk, MAX_PPINDEX, kappk_pctoindex)},
              { "knppk", new endgamekey(MAX_kappk, MAX_PPINDEX, kappk_pctoindex)},

              { "kqpkq", new endgamekey(MAX_kapkb, 24, kapkb_pctoindex)},
              { "kqpkr", new endgamekey(MAX_kapkb, 24, kapkb_pctoindex)},
              { "kqpkb", new endgamekey(MAX_kapkb, 24, kapkb_pctoindex)},
              { "kqpkn", new endgamekey(MAX_kapkb, 24, kapkb_pctoindex)},
              { "krpkq", new endgamekey(MAX_kapkb, 24, kapkb_pctoindex)},
              { "krpkr", new endgamekey(MAX_kapkb, 24, kapkb_pctoindex)},
              { "krpkb", new endgamekey(MAX_kapkb, 24, kapkb_pctoindex)},
              { "krpkn", new endgamekey(MAX_kapkb, 24, kapkb_pctoindex)},
              { "kbpkq", new endgamekey(MAX_kapkb, 24, kapkb_pctoindex)},
              { "kbpkr", new endgamekey(MAX_kapkb, 24, kapkb_pctoindex)},
              { "kbpkb", new endgamekey(MAX_kapkb, 24, kapkb_pctoindex)},
              { "kbpkn", new endgamekey(MAX_kapkb, 24, kapkb_pctoindex)},
              { "knpkq", new endgamekey(MAX_kapkb, 24, kapkb_pctoindex)},
              { "knpkr", new endgamekey(MAX_kapkb, 24, kapkb_pctoindex)},
              { "knpkb", new endgamekey(MAX_kapkb, 24, kapkb_pctoindex)},
              { "knpkn", new endgamekey(MAX_kapkb, 24, kapkb_pctoindex)},
              { "kppkq", new endgamekey(MAX_kppka, MAX_PPINDEX, kppka_pctoindex)},
              { "kppkr", new endgamekey(MAX_kppka, MAX_PPINDEX, kppka_pctoindex)},
              { "kppkb", new endgamekey(MAX_kppka, MAX_PPINDEX, kppka_pctoindex)},
              { "kppkn", new endgamekey(MAX_kppka, MAX_PPINDEX, kppka_pctoindex)},

              { "kqqkp", new endgamekey(MAX_kaakp, 24, kaakp_pctoindex)},
              { "kqrkp", new endgamekey(MAX_kabkp, 24, kabkp_pctoindex)},
              { "kqbkp", new endgamekey(MAX_kabkp, 24, kabkp_pctoindex)},
              { "kqnkp", new endgamekey(MAX_kabkp, 24, kabkp_pctoindex)},
              { "krrkp", new endgamekey(MAX_kaakp, 24, kaakp_pctoindex)},
              { "krbkp", new endgamekey(MAX_kabkp, 24, kabkp_pctoindex)},
              { "krnkp", new endgamekey(MAX_kabkp, 24, kabkp_pctoindex)},
              { "kbbkp", new endgamekey(MAX_kaakp, 24, kaakp_pctoindex)},
              { "kbnkp", new endgamekey(MAX_kabkp, 24, kabkp_pctoindex)},
              { "knnkp", new endgamekey(MAX_kaakp, 24, kaakp_pctoindex)},

              { "kqpkp", new endgamekey(MAX_kapkp, MAX_PpINDEX, kapkp_pctoindex)},
              { "krpkp", new endgamekey(MAX_kapkp, MAX_PpINDEX, kapkp_pctoindex)},
              { "kbpkp", new endgamekey(MAX_kapkp, MAX_PpINDEX, kapkp_pctoindex)},
              { "knpkp", new endgamekey(MAX_kapkp, MAX_PpINDEX, kapkp_pctoindex)},

      //{143,"kppkp", MAX_kppkp, 24*MAX_PP48_INDEX, kppkp_indextopc, kppkp_pctoindex, NULL ,  NULL   ,NULL ,0, 0 },
      { "kpppk", new endgamekey(MAX_kpppk, MAX_PPP48_INDEX, kpppk_pctoindex)},
          };

    static SevenZip.Compression.LZMA.Decoder decoder = new SevenZip.Compression.LZMA.Decoder();
    static int[][] flipt = new int[64][];

    static int WE_FLAG = 1;
    static int NS_FLAG = 2;
    static int NW_SE_FLAG = 4;

    static int map24_b(int s)
    {
      s-=8;
      return ((s&3)+s)>>1;
    }

    static int[][] pp48_idx = new int[48][];
    static int[] pp48_sq_x = new int[MAX_PP48_INDEX];
    static int[] pp48_sq_y = new int[MAX_PP48_INDEX];
    static int[][][] ppp48_idx = new int[48][][];

    static int[] ppp48_sq_x = new int[MAX_PPP48_INDEX];
    static int[] ppp48_sq_y = new int[MAX_PPP48_INDEX];
    static int[] ppp48_sq_z = new int[MAX_PPP48_INDEX];

    static int[] itosq = new int[48]
    {
              H7,G7,F7,E7,
              H6,G6,F6,E6,
              H5,G5,F5,E5,
              H4,G4,F4,E4,
              H3,G3,F3,E3,
              H2,G2,F2,E2,
              D7,C7,B7,A7,
              D6,C6,B6,A6,
              D5,C5,B5,A5,
              D4,C4,B4,A4,
              D3,C3,B3,A3,
              D2,C2,B2,A2
    };

    static bool in_queenside(int x)
    {
      return 0==(x&(1<<2));
    }

    static void init_ppp48_idx()
    {
      int MAX_I = 48;
      int MAX_J = 48;
      int MAX_K = 48;

      int i, j, k;
      int idx = 0;
      int a, b, c;
      int x, y, z;

      /* default is noindex */
      for(i=0; i<MAX_I; i++)
      {
        ppp48_idx[i]=new int[48][];
        for(j=0; j<MAX_J; j++)
        {
          ppp48_idx[i][j]=new int[48];
          for(k=0; k<MAX_K; k++)
          {
            ppp48_idx[i][j][k]=-1;
          }
        }
      }

      for(idx=0; idx<MAX_PPP48_INDEX; idx++)
      {
        ppp48_sq_x[idx]=(int)NOSQUARE;
        ppp48_sq_y[idx]=(int)NOSQUARE;
        ppp48_sq_z[idx]=(int)NOSQUARE;
      }

      idx=0;
      for(x=0; x<48; x++)
      {
        for(y=x+1; y<48; y++)
        {
          for(z=y+1; z<48; z++)
          {
            a=itosq[x];
            b=itosq[y];
            c=itosq[z];

            if(!in_queenside(b)||!in_queenside(c))
              continue;

            i=a-8;
            j=b-8;
            k=c-8;

            if(IDX_is_empty(ppp48_idx[i][j][k]))
            {

              ppp48_idx[i][j][k]=idx;
              ppp48_idx[i][k][j]=idx;
              ppp48_idx[j][i][k]=idx;
              ppp48_idx[j][k][i]=idx;
              ppp48_idx[k][i][j]=idx;
              ppp48_idx[k][j][i]=idx;
              ppp48_sq_x[idx]=(int)i;
              ppp48_sq_y[idx]=(int)j;
              ppp48_sq_z[idx]=(int)k;
              idx++;
            }
          }
        }
      }
    }


    static int kpppk_pctoindex()
    {
      int BLOCK_A = 64*64;
      int BLOCK_B = 64;

      int ppp48_slice;

      int wk = whitePieceSquares[0];
      int pawn_a = whitePieceSquares[1];
      int pawn_b = whitePieceSquares[2];
      int pawn_c = whitePieceSquares[3];

      int bk = blackPieceSquares[0];

      int i, j, k;

      i=pawn_a-8;
      j=pawn_b-8;
      k=pawn_c-8;

      ppp48_slice=ppp48_idx[i][j][k];

      if(IDX_is_empty(ppp48_slice))
      {
        wk=flipWE(wk);
        pawn_a=flipWE(pawn_a);
        pawn_b=flipWE(pawn_b);
        pawn_c=flipWE(pawn_c);
        bk=flipWE(bk);
      }

      i=pawn_a-8;
      j=pawn_b-8;
      k=pawn_c-8;

      ppp48_slice=ppp48_idx[i][j][k];

      if(IDX_is_empty(ppp48_slice))
      {
        return NOINDEX;
      }

      return (int)ppp48_slice*BLOCK_A+(int)wk*BLOCK_B+(int)bk;
    }

    static int kapkb_pctoindex()
    {
      int BLOCK_A = 64*64*64*64;
      int BLOCK_B = 64*64*64;
      int BLOCK_C = 64*64;
      int BLOCK_D = 64;

      int pslice;
      int sq;
      int pawn = whitePieceSquares[2];
      int wa = whitePieceSquares[1];
      int wk = whitePieceSquares[0];
      int bk = blackPieceSquares[0];
      int ba = blackPieceSquares[1];

      if(!(A2<=pawn&&pawn<A8))
      {
        return NOINDEX;
      }

      if((pawn&7)>3)
      { /* column is more than 3. e.g. = e,f,g, or h */
        pawn=flipWE(pawn);
        wk=flipWE(wk);
        bk=flipWE(bk);
        wa=flipWE(wa);
        ba=flipWE(ba);
      }

      sq=pawn;
      sq^=56; /* flipNS*/
      sq-=8;   /* down one row*/
      pslice=(int)((sq+(sq&3))>>1);

      return pslice*BLOCK_A+(int)wk*BLOCK_B+(int)bk*BLOCK_C+(int)wa*BLOCK_D+(int)ba;
    }

    static int kabpk_pctoindex()
    {
      int BLOCK_A = 64*64*64*64;
      int BLOCK_B = 64*64*64;
      int BLOCK_C = 64*64, BLOCK_D = 64;
      int pslice;

      int wk = whitePieceSquares[0];
      int wa = whitePieceSquares[1];
      int wb = whitePieceSquares[2];
      int pawn = whitePieceSquares[3];
      int bk = blackPieceSquares[0];

      if((pawn&7)>3)
      { /* column is more than 3. e.g. = e,f,g, or h */
        pawn=flipWE(pawn);
        wk=flipWE(wk);
        bk=flipWE(bk);
        wa=flipWE(wa);
        wb=flipWE(wb);
      }

      pslice=wsq_to_pidx24(pawn);

      return pslice*(int)BLOCK_A+(int)wk*(int)BLOCK_B+(int)bk*(int)BLOCK_C+(int)wa*(int)BLOCK_D+(int)wb;

    }


    static int kabkp_pctoindex()
    {
      int BLOCK_A = 64*64*64*64;
      int BLOCK_B = 64*64*64;
      int BLOCK_C = 64*64;
      int BLOCK_D = 64;

      int pslice;
      int sq;
      int pawn = blackPieceSquares[1];
      int wa = whitePieceSquares[1];
      int wk = whitePieceSquares[0];
      int bk = blackPieceSquares[0];
      int wb = whitePieceSquares[2];

      if(!(A2<=pawn&&pawn<A8))
      {
        return NOINDEX;
      }

      if((pawn&7)>3)
      { /* column is more than 3. e.g. = e,f,g, or h */
        pawn=flipWE(pawn);
        wk=flipWE(wk);
        bk=flipWE(bk);
        wa=flipWE(wa);
        wb=flipWE(wb);
      }

      sq=pawn;
      /*sq ^= 070;*/
      /* do not flipNS*/
      sq-=8;   /* down one row*/
      pslice=(int)((sq+(sq&3))>>1);

      return pslice*BLOCK_A+(int)wk*BLOCK_B+(int)bk*BLOCK_C+(int)wa*BLOCK_D+(int)wb;
    }

    static int kaapk_pctoindex()
    {
      int BLOCK_C = MAX_AAINDEX;
      int BLOCK_B = 64*BLOCK_C;
      int BLOCK_A = 64*BLOCK_B;

      int aa_combo, pslice;

      int wk = whitePieceSquares[0];
      int wa = whitePieceSquares[1];
      int wa2 = whitePieceSquares[2];
      int pawn = whitePieceSquares[3];
      int bk = blackPieceSquares[0];

      if((pawn&7)>3)
      { /* column is more than 3. e.g. = e,f,g, or h */
        pawn=flipWE(pawn);
        wk=flipWE(wk);
        bk=flipWE(bk);
        wa=flipWE(wa);
        wa2=flipWE(wa2);
      }

      pslice=wsq_to_pidx24(pawn);

      aa_combo=(int)aaidx[wa][wa2];

      if(IDX_is_empty(aa_combo))
      {
        return NOINDEX;
      }

      return pslice*(int)BLOCK_A+(int)wk*(int)BLOCK_B+(int)bk*(int)BLOCK_C+aa_combo;
    }


    static int kaakp_pctoindex()
    {
      int BLOCK_C = MAX_AAINDEX;
      int BLOCK_B = 64*BLOCK_C;
      int BLOCK_A = 64*BLOCK_B;

      int aa_combo, pslice;

      int wk = whitePieceSquares[0];
      int wa = whitePieceSquares[1];
      int wa2 = whitePieceSquares[2];
      int bk = blackPieceSquares[0];
      int pawn = blackPieceSquares[1];

      if((pawn&7)>3)
      { /* column is more than 3. e.g. = e,f,g, or h */
        pawn=flipWE(pawn);
        wk=flipWE(wk);
        bk=flipWE(bk);
        wa=flipWE(wa);
        wa2=flipWE(wa2);
      }

      pawn=flipNS(pawn);
      pslice=wsq_to_pidx24(pawn);

      aa_combo=(int)aaidx[wa][wa2];

      if(IDX_is_empty(aa_combo))
      {
        return NOINDEX;
      }

      return pslice*(int)BLOCK_A+(int)wk*(int)BLOCK_B+(int)bk*(int)BLOCK_C+aa_combo;
    }

    static int kapkp_pctoindex()
    {
      int BLOCK_A = 64*64*64;
      int BLOCK_B = 64*64;
      int BLOCK_C = 64;

      int pp_slice;
      int anchor = 0;
      int loosen = 0;

      int wk = whitePieceSquares[0];
      int wa = whitePieceSquares[1];
      int pawn_a = whitePieceSquares[2];
      int bk = blackPieceSquares[0];
      int pawn_b = blackPieceSquares[1];
      int m, n;

      anchor=pawn_a;
      loosen=pawn_b;

      if((anchor&7)>3)
      { /* column is more than 3. e.g. = e,f,g, or h */
        anchor=flipWE(anchor);
        loosen=flipWE(loosen);
        wk=flipWE(wk);
        bk=flipWE(bk);
        wa=flipWE(wa);
      }

      m=wsq_to_pidx24(anchor);
      n=(int)loosen-8;

      pp_slice=m*48+n;

      if(IDX_is_empty(pp_slice))
      {
        return NOINDEX;
      }

      return pp_slice*(int)BLOCK_A+(int)wk*(int)BLOCK_B+(int)bk*(int)BLOCK_C+(int)wa;
    }

    static int kappk_pctoindex()
    {
      int BLOCK_A = 64*64*64;
      int BLOCK_B = 64*64;
      int BLOCK_C = 64;

      int pp_slice;
      int anchor = 0;
      int loosen = 0;

      int wk = whitePieceSquares[0];
      int wa = whitePieceSquares[1];
      int pawn_a = whitePieceSquares[2];
      int pawn_b = whitePieceSquares[3];
      int bk = blackPieceSquares[0];

      int i, j;

      pp_putanchorfirst(pawn_a, pawn_b, ref anchor, ref loosen);

      if((anchor&7)>3)
      { /* column is more than 3. e.g. = e,f,g, or h */
        anchor=flipWE(anchor);
        loosen=flipWE(loosen);
        wk=flipWE(wk);
        bk=flipWE(bk);
        wa=flipWE(wa);
      }

      i=wsq_to_pidx24(anchor);
      j=wsq_to_pidx48(loosen);

      pp_slice=ppidx[i][j];

      if(IDX_is_empty(pp_slice))
      {
        return NOINDEX;
      }

      return pp_slice*(int)BLOCK_A+(int)wk*(int)BLOCK_B+(int)bk*(int)BLOCK_C+(int)wa;
    }

    static int kppka_pctoindex()
    {
      int BLOCK_A = 64*64*64;
      int BLOCK_B = 64*64;
      int BLOCK_C = 64;

      int pp_slice;
      int i, j;

      int anchor = 0;
      int loosen = 0;

      int wk = whitePieceSquares[0];
      int pawn_a = whitePieceSquares[1];
      int pawn_b = whitePieceSquares[2];
      int bk = blackPieceSquares[0];
      int ba = blackPieceSquares[1];

      pp_putanchorfirst(pawn_a, pawn_b, ref anchor, ref loosen);

      if((anchor&7)>3)
      { /* column is more than 3. e.g. = e,f,g, or h */
        anchor=flipWE(anchor);
        loosen=flipWE(loosen);
        wk=flipWE(wk);
        bk=flipWE(bk);
        ba=flipWE(ba);
      }

      i=wsq_to_pidx24(anchor);
      j=wsq_to_pidx48(loosen);

      pp_slice=ppidx[i][j];

      if(IDX_is_empty(pp_slice))
      {
        return NOINDEX;
      }

      return pp_slice*(int)BLOCK_A+(int)wk*(int)BLOCK_B+(int)bk*(int)BLOCK_C+(int)ba;
    }

    static int kabck_pctoindex()
    {
      int N_WHITE = 4;
      int N_BLACK = 1;
      int BLOCK_A = 64*64*64;
      int BLOCK_B = 64*64;
      int BLOCK_C = 64;

      int ki;
      int i;
      int ft;

      ft=flipt[blackPieceSquares[0]][whitePieceSquares[0]];

      List<int> ws = new List<int>(whitePieceSquares);
      List<int> bs = new List<int>(blackPieceSquares);

      if((ft&WE_FLAG)!=0)
      {
        for(i=0; i<N_WHITE; i++)
          ws[i]=flipWE(ws[i]);
        for(i=0; i<N_BLACK; i++)
          bs[i]=flipWE(bs[i]);
      }

      if((ft&NS_FLAG)!=0)
      {
        for(i=0; i<N_WHITE; i++)
          ws[i]=flipNS(ws[i]);
        for(i=0; i<N_BLACK; i++)
          bs[i]=flipNS(bs[i]);
      }

      if((ft&NW_SE_FLAG)!=0)
      {
        for(i=0; i<N_WHITE; i++)
          ws[i]=flipNW_SE(ws[i]);
        for(i=0; i<N_BLACK; i++)
          bs[i]=flipNW_SE(bs[i]);
      }


      ki=kkidx[bs[0]][ws[0]]; /* kkidx [black king] [white king] */

      if(IDX_is_empty(ki))
      {
        return NOINDEX;
      }
      return ki*BLOCK_A+(int)ws[1]*BLOCK_B+(int)ws[2]*BLOCK_C+(int)ws[3];

    }

    static int kabbk_pctoindex()
    {
      int N_WHITE = 4;
      int N_BLACK = 1;

      int BLOCK_Bx = 64;
      int BLOCK_Ax = BLOCK_Bx*MAX_AAINDEX;

      int ki, ai;
      int ft;
      int i;

      ft=flipt[blackPieceSquares[0]][whitePieceSquares[0]];

      List<int> ws = new List<int>(whitePieceSquares);
      List<int> bs = new List<int>(blackPieceSquares);

      if((ft&WE_FLAG)!=0)
      {
        for(i=0; i<N_WHITE; i++)
          ws[i]=flipWE(ws[i]);
        for(i=0; i<N_BLACK; i++)
          bs[i]=flipWE(bs[i]);
      }

      if((ft&NS_FLAG)!=0)
      {
        for(i=0; i<N_WHITE; i++)
          ws[i]=flipNS(ws[i]);
        for(i=0; i<N_BLACK; i++)
          bs[i]=flipNS(bs[i]);
      }

      if((ft&NW_SE_FLAG)!=0)
      {
        for(i=0; i<N_WHITE; i++)
          ws[i]=flipNW_SE(ws[i]);
        for(i=0; i<N_BLACK; i++)
          bs[i]=flipNW_SE(bs[i]);
      }

      ki=kkidx[bs[0]][ws[0]]; /* kkidx [black king] [white king] */
      ai=aaidx[ws[2]][ws[3]];

      if(IDX_is_empty(ki)||IDX_is_empty(ai))
      {
        return NOINDEX;
      }
      return ki*BLOCK_Ax+ai*BLOCK_Bx+(int)ws[1];
    }

    static int kaabk_pctoindex()
    {
      int N_WHITE = 4;
      int N_BLACK = 1;
      int BLOCK_Bx = 64;
      int BLOCK_Ax = BLOCK_Bx*MAX_AAINDEX;


      int ki, ai;
      int ft;
      int i;

      ft=flipt[blackPieceSquares[0]][whitePieceSquares[0]];

      List<int> ws = new List<int>(whitePieceSquares);
      List<int> bs = new List<int>(blackPieceSquares);

      if((ft&WE_FLAG)!=0)
      {
        for(i=0; i<N_WHITE; i++)
          ws[i]=flipWE(ws[i]);
        for(i=0; i<N_BLACK; i++)
          bs[i]=flipWE(bs[i]);
      }

      if((ft&NS_FLAG)!=0)
      {
        for(i=0; i<N_WHITE; i++)
          ws[i]=flipNS(ws[i]);
        for(i=0; i<N_BLACK; i++)
          bs[i]=flipNS(bs[i]);
      }

      if((ft&NW_SE_FLAG)!=0)
      {
        for(i=0; i<N_WHITE; i++)
          ws[i]=flipNW_SE(ws[i]);
        for(i=0; i<N_BLACK; i++)
          bs[i]=flipNW_SE(bs[i]);
      }

      ki=kkidx[bs[0]][ws[0]]; /* kkidx [black king] [white king] */
      ai=aaidx[ws[1]][ws[2]];

      if(IDX_is_empty(ki)||IDX_is_empty(ai))
      {
        return NOINDEX;
      }
      return ki*BLOCK_Ax+ai*BLOCK_Bx+(int)ws[3];
    }

    static int init_pp48_idx()
    {
      /* modifies pp48_idx[][], pp48_sq_x[], pp48_sq_y[] */

      int MAX_I = 48;
      int MAX_J = 48;
      int i, j;
      int idx = 0;
      int a, b;

      /* default is noindex */
      for(i=0; i<MAX_I; i++)
      {
        pp48_idx[i]=new int[48];
        for(j=0; j<MAX_J; j++)
        {
          pp48_idx[i][j]=-1;
        }
      }

      for(idx=0; idx<MAX_PP48_INDEX; idx++)
      {
        pp48_sq_x[idx]=NOSQUARE;
        pp48_sq_y[idx]=NOSQUARE;
      }

      idx=0;
      for(a=H7; a>=A2; a--)
      {

        for(b=a-1; b>=A2; b--)
        {

          i=flipWE(flipNS(a))-8;
          j=flipWE(flipNS(b))-8;

          if(IDX_is_empty(pp48_idx[i][j]))
          {

            pp48_idx[i][j]=idx;
            pp48_idx[j][i]=idx;
            pp48_sq_x[idx]=i;
            pp48_sq_y[idx]=j;
            idx++;
          }
        }
      }
      return idx;
    }

    static int kaaak_pctoindex()
    {
      int N_WHITE = 4;
      int N_BLACK = 1;
      int BLOCK_Ax = MAX_AAAINDEX;

      int ki, ai;
      int ft;
      int i;

      List<int> ws = new List<int>(whitePieceSquares);
      List<int> bs = new List<int>(blackPieceSquares);

      ft=flipt[blackPieceSquares[0]][whitePieceSquares[0]];


      if((ft&WE_FLAG)!=0)
      {
        for(i=0; i<N_WHITE; i++)
          ws[i]=flipWE(ws[i]);
        for(i=0; i<N_BLACK; i++)
          bs[i]=flipWE(bs[i]);
      }

      if((ft&NS_FLAG)!=0)
      {
        for(i=0; i<N_WHITE; i++)
          ws[i]=flipNS(ws[i]);
        for(i=0; i<N_BLACK; i++)
          bs[i]=flipNS(bs[i]);
      }

      if((ft&NW_SE_FLAG)!=0)
      {
        for(i=0; i<N_WHITE; i++)
          ws[i]=flipNW_SE(ws[i]);
        for(i=0; i<N_BLACK; i++)
          bs[i]=flipNW_SE(bs[i]);
      }


      {
        int tmp;
        if(ws[2]<ws[1])
        {
          tmp=ws[1];
          ws[1]=ws[2];
          ws[2]=tmp;
        }
        if(ws[3]<ws[2])
        {
          tmp=ws[2];
          ws[2]=ws[3];
          ws[3]=tmp;
        }
        if(ws[2]<ws[1])
        {
          tmp=ws[1];
          ws[1]=ws[2];
          ws[2]=tmp;
        }
      }

      ki=kkidx[bs[0]][ws[0]];

      if(ws[1]==ws[2]||ws[1]==ws[3]||ws[2]==ws[3])
      {
        return NOINDEX;
      }

      ai=aaa_getsubi(ws[1], ws[2], ws[3]);

      if(IDX_is_empty(ki)||IDX_is_empty(ai))
      {
        return NOINDEX;
      }
      return ki*BLOCK_Ax+ai;
    }

    static int[] aaa_base = new int[64];

    static int aaa_getsubi(int x, int y, int z)
    {
      int calc_idx, bse;

      bse=aaa_base[z];
      calc_idx=(int)x+((int)y-1)*(int)y/2+bse;

      return calc_idx;
    }

    static int[][] aaa_xyz = new int[MAX_AAAINDEX][];

    static void init_aaa()
    {
      int[] comb = new int[64];
      int accum;
      int a;

      int idx;
      int x, y, z;

      /* getting aaa_base */
      comb[0]=0;
      for(a=1; a<64; a++)
      {
        comb[a]=a*(a-1)/2;
      }

      accum=0;
      aaa_base[0]=accum;
      for(a=0; a<(64-1); a++)
      {
        accum+=comb[a];
        aaa_base[a+1]=accum;
      }

      /* end getting aaa_base */


      /* initialize aaa_xyz [][] */
      for(idx=0; idx<MAX_AAAINDEX; idx++)
      {
        aaa_xyz[idx]=new int[3];
        aaa_xyz[idx][0]=-1;
        aaa_xyz[idx][1]=-1;
        aaa_xyz[idx][2]=-1;
      }

      idx=0;
      for(z=0; z<64; z++)
      {
        for(y=0; y<z; y++)
        {
          for(x=0; x<y; x++)
          {


            aaa_xyz[idx][0]=x;
            aaa_xyz[idx][1]=y;
            aaa_xyz[idx][2]=z;

            idx++;
          }
        }
      }

    }


    static int kppkp_pctoindex()
    {
      int BLOCK_Ax = MAX_PP48_INDEX*64*64;
      int BLOCK_Bx = 64*64;
      int BLOCK_Cx = 64;
      int pp48_slice;

      int wk = whitePieceSquares[0];
      int pawn_a = whitePieceSquares[1];
      int pawn_b = whitePieceSquares[2];
      int bk = blackPieceSquares[0];
      int pawn_c = blackPieceSquares[1];
      int i, j, k;

      if((pawn_c&7)>3)
      { /* column is more than 3. e.g. = e,f,g, or h */
        wk=flipWE(wk);
        pawn_a=flipWE(pawn_a);
        pawn_b=flipWE(pawn_b);
        bk=flipWE(bk);
        pawn_c=flipWE(pawn_c);
      }

      i=flipWE(flipNS(pawn_a))-8;
      j=flipWE(flipNS(pawn_b))-8;
      k=map24_b(pawn_c); /* black pawn, so low indexes mean more advanced 0 == A2 */

      pp48_slice=pp48_idx[i][j];

      if(IDX_is_empty(pp48_slice))
      {
        return NOINDEX;
      }

      return (int)k*(int)BLOCK_Ax+pp48_slice*(int)BLOCK_Bx+(int)wk*(int)BLOCK_Cx+(int)bk;
    }


    static int kaakb_pctoindex()
    {
      int N_WHITE = 3;
      int N_BLACK = 2;
      int BLOCK_Bx = 64;
      int BLOCK_Ax = BLOCK_Bx*MAX_AAINDEX;

      int ki, ai;
      int ft;
      int i;

      ft=flipt[blackPieceSquares[0]][whitePieceSquares[0]];

      List<int> ws = new List<int>(whitePieceSquares);
      List<int> bs = new List<int>(blackPieceSquares);

      if((ft&WE_FLAG)!=0)
      {
        for(i=0; i<N_WHITE; i++)
          ws[i]=flipWE(ws[i]);
        for(i=0; i<N_BLACK; i++)
          bs[i]=flipWE(bs[i]);
      }

      if((ft&NS_FLAG)!=0)
      {
        for(i=0; i<N_WHITE; i++)
          ws[i]=flipNS(ws[i]);
        for(i=0; i<N_BLACK; i++)
          bs[i]=flipNS(bs[i]);
      }

      if((ft&NW_SE_FLAG)!=0)
      {
        for(i=0; i<N_WHITE; i++)
          ws[i]=flipNW_SE(ws[i]);
        for(i=0; i<N_BLACK; i++)
          bs[i]=flipNW_SE(bs[i]);
      }

      ki=kkidx[bs[0]][ws[0]]; /* kkidx [black king] [white king] */
      ai=aaidx[ws[1]][ws[2]];

      if(IDX_is_empty(ki)||IDX_is_empty(ai))
      {
        return NOINDEX;
      }
      return ki*BLOCK_Ax+ai*BLOCK_Bx+(int)bs[1];
    }

    static int kabkc_pctoindex()
    {
      int N_WHITE = 3;
      int N_BLACK = 2;

      int BLOCK_Ax = 64*64*64;
      int BLOCK_Bx = 64*64;
      int BLOCK_Cx = 64;

      int ki;
      int i;
      int ft;

      ft=flipt[blackPieceSquares[0]][whitePieceSquares[0]];

      List<int> ws = new List<int>(whitePieceSquares);
      List<int> bs = new List<int>(blackPieceSquares);


      if((ft&WE_FLAG)!=0)
      {
        for(i=0; i<N_WHITE; i++)
          ws[i]=flipWE(ws[i]);
        for(i=0; i<N_BLACK; i++)
          bs[i]=flipWE(bs[i]);
      }

      if((ft&NS_FLAG)!=0)
      {
        for(i=0; i<N_WHITE; i++)
          ws[i]=flipNS(ws[i]);
        for(i=0; i<N_BLACK; i++)
          bs[i]=flipNS(bs[i]);
      }

      if((ft&NW_SE_FLAG)!=0)
      {
        for(i=0; i<N_WHITE; i++)
          ws[i]=flipNW_SE(ws[i]);
        for(i=0; i<N_BLACK; i++)
          bs[i]=flipNW_SE(bs[i]);
      }


      ki=kkidx[bs[0]][ws[0]]; /* kkidx [black king] [white king] */

      if(IDX_is_empty(ki))
      {
        return NOINDEX;
      }
      return ki*BLOCK_Ax+(int)ws[1]*BLOCK_Bx+(int)ws[2]*BLOCK_Cx+(int)bs[1];

    }


    static int[][] aaidx = new int[64][];
    static byte[] aabase = new byte[MAX_AAINDEX];

    static int kpkp_pctoindex()
    {
      int BLOCK_Ax = 64*64;
      int BLOCK_Bx = 64;
      int pp_slice;
      int anchor = 0;
      int loosen = 0;

      int wk = whitePieceSquares[0];
      int bk = blackPieceSquares[0];
      int pawn_a = whitePieceSquares[1];
      int pawn_b = blackPieceSquares[1];

      int m, n;

      /*pp_putanchorfirst (pawn_a, pawn_b, &anchor, &loosen);*/
      anchor=pawn_a;
      loosen=pawn_b;

      if((anchor&7)>3)
      { /* column is more than 3. e.g. = e,f,g, or h */
        anchor=flipWE(anchor);
        loosen=flipWE(loosen);
        wk=flipWE(wk);
        bk=flipWE(bk);
      }

      m=(int)wsq_to_pidx24(anchor);
      n=loosen-8;

      pp_slice=m*48+n;

      if(IDX_is_empty(pp_slice))
      {
        return NOINDEX;
      }

      return (int)(pp_slice*BLOCK_Ax+wk*BLOCK_Bx+bk);
    }


    static void pp_putanchorfirst(int a, int b, ref int out_anchor, ref int out_loosen)
    {
      int anchor, loosen;

      int row_b, row_a;
      row_b=b&56;
      row_a=a&56;

      /* default */
      anchor=a;
      loosen=b;
      if(row_b>row_a)
      {
        anchor=b;
        loosen=a;
      }
      else
          if(row_b==row_a)
      {
        int x, col, inv, hi_a, hi_b;
        x=a;
        col=x&7;
        inv=col^7;
        x=(1<<col)|(1<<inv);
        x&=(x-1);
        hi_a=x;

        x=b;
        col=x&7;
        inv=col^7;
        x=(1<<col)|(1<<inv);
        x&=(x-1);
        hi_b=x;

        if(hi_b>hi_a)
        {
          anchor=b;
          loosen=a;
        }

        if(hi_b<hi_a)
        {
          anchor=a;
          loosen=b;
        }

        if(hi_b==hi_a)
        {
          if(a<b)
          {
            anchor=a;
            loosen=b;
          }
          else
          {
            anchor=b;
            loosen=a;
          }
        }
      }

      out_anchor=anchor;
      out_loosen=loosen;
      return;
    }

    static int wsq_to_pidx24(int pawn)
    {
      int idx24;
      int sq = pawn;

      /* input can be only queen side, pawn valid */

      sq^=56; /* flipNS*/
      sq-=8;   /* down one row*/
      idx24=(sq+(sq&3))>>1;
      return (int)idx24;
    }

    static int wsq_to_pidx48(int pawn)
    {
      int idx48;
      int sq = pawn;

      sq^=56; /* flipNS*/
      sq-=8;   /* down one row*/
      idx48=sq;
      return (int)idx48;
    }

    static int[][] ppidx = new int[24][];


    static int kppk_pctoindex()
    {
      int BLOCK_Ax = 64*64;
      int BLOCK_Bx = 64;
      int pp_slice;
      int anchor = 0;
      int loosen = 0;

      int wk = whitePieceSquares[0];
      int pawn_a = whitePieceSquares[1];
      int pawn_b = whitePieceSquares[2];
      int bk = blackPieceSquares[0];
      int i, j;

      pp_putanchorfirst(pawn_a, pawn_b, ref anchor, ref loosen);

      if((anchor&7)>3)
      { /* column is more than 3. e.g. = e,f,g, or h */
        anchor=flipWE(anchor);
        loosen=flipWE(loosen);
        wk=flipWE(wk);
        bk=flipWE(bk);
      }

      i=wsq_to_pidx24(anchor);
      j=wsq_to_pidx48(loosen);

      pp_slice=ppidx[i][j];

      if(IDX_is_empty(pp_slice))
      {
        return NOINDEX;
      }

      return pp_slice*BLOCK_Ax+(int)wk*BLOCK_Bx+(int)bk;
    }

    static int kapk_pctoindex()
    {
      int BLOCK_Ax = 64*64*64;
      int BLOCK_Bx = 64*64;
      int BLOCK_Cx = 64;

      int pslice;
      int sq;
      int pawn = whitePieceSquares[2];
      int wa = whitePieceSquares[1];
      int wk = whitePieceSquares[0];
      int bk = blackPieceSquares[0];

      if(!(A2<=pawn&&pawn<A8))
      {
        return NOINDEX;
      }

      if((pawn&7)>3)
      { /* column is more than 3. e.g. = e,f,g, or h */
        pawn=flipWE(pawn);
        wk=flipWE(wk);
        bk=flipWE(bk);
        wa=flipWE(wa);
      }

      sq=pawn;
      sq^=56; /* flipNS*/
      sq-=8;   /* down one row*/
      pslice=(int)((sq+(sq&3))>>1);

      return pslice*BLOCK_Ax+(int)wk*BLOCK_Bx+(int)bk*BLOCK_Cx+(int)wa;
    }

    static int kabk_pctoindex()
    {
      int BLOCK_Ax = 64*64;
      int BLOCK_Bx = 64;
      //int p;
      int ki;
      //int i;

      int ft;

      ft=flip_type(blackPieceSquares[0], whitePieceSquares[0]);

      List<int> ws = new List<int>(whitePieceSquares);
      List<int> bs = new List<int>(blackPieceSquares);

      if((ft&1)!=0)
      {
        for(int b = 0; b<ws.Count; b++)
          ws[b]=flipWE(ws[b]);
        for(int b = 0; b<bs.Count; b++)
          bs[b]=flipWE(bs[b]);
      }

      if((ft&2)!=0)
      {
        for(int b = 0; b<ws.Count; b++)
          ws[b]=flipNS(ws[b]);
        for(int b = 0; b<bs.Count; b++)
          bs[b]=flipNS(bs[b]);
      }

      if((ft&4)!=0)
      {
        for(int b = 0; b<ws.Count; b++)
          ws[b]=flipNW_SE(ws[b]);
        for(int b = 0; b<bs.Count; b++)
          bs[b]=flipNW_SE(bs[b]);

      }

      ki=kkidx[bs[0]][ws[0]]; /* kkidx [black king] [white king] */

      if(IDX_is_empty(ki))
      {
        return NOINDEX;
      }
      return ki*BLOCK_Ax+(int)ws[1]*BLOCK_Bx+(int)ws[2];

    }

    static int kakp_pctoindex()
    {
      int BLOCK_Ax = 64*64*64;
      int BLOCK_Bx = 64*64;
      int BLOCK_Cx = 64;

      int pslice;
      int sq;
      int pawn = blackPieceSquares[1];
      int wa = whitePieceSquares[1];
      int wk = whitePieceSquares[0];
      int bk = blackPieceSquares[0];

      if(!(A2<=pawn&&pawn<A8))
      {
        return NOINDEX;
      }

      if((pawn&7)>3)
      { /* column is more than 3. e.g. = e,f,g, or h */
        pawn=flipWE(pawn);
        wk=flipWE(wk);
        bk=flipWE(bk);
        wa=flipWE(wa);
      }

      sq=pawn;
      /*sq ^= 070;*/
      /* flipNS*/
      sq-=8;   /* down one row*/
      pslice=(int)((sq+(sq&3))>>1);

      return pslice*BLOCK_Ax+(int)wk*BLOCK_Bx+(int)bk*BLOCK_Cx+(int)wa;
    }

    static void init_aaidx()
    {
      /* modifies aabase[], aaidx[][] */

      int idx;
      int x, y;

      /* default is noindex */
      for(x=0; x<64; x++)
      {
        aaidx[x]=new int[64];
        for(y=0; y<64; y++)
        {
          aaidx[x][y]=-1;
        }
      }

      for(idx=0; idx<MAX_AAINDEX; idx++)
        aabase[idx]=0;

      idx=0;
      for(x=0; x<64; x++)
      {
        for(y=x+1; y<64; y++)
        {

          if(IDX_is_empty(aaidx[x][y]))
          { /* still empty */
            aaidx[x][y]=idx;
            aaidx[y][x]=idx;
            aabase[idx]=(byte)x;
            idx++;
          }
        }
      }
    }

    static int kaak_pctoindex()
    {
      int N_WHITE = 3;
      int N_BLACK = 1;
      int BLOCK_Ax = MAX_AAINDEX;

      int ki, ai;
      int ft;
      int i;

      ft=flipt[blackPieceSquares[0]][whitePieceSquares[0]];

      List<int> ws = new List<int>(whitePieceSquares);
      List<int> bs = new List<int>(blackPieceSquares);

      if((ft&WE_FLAG)!=0)
      {
        for(i=0; i<N_WHITE; i++)
          ws[i]=flipWE(ws[i]);
        for(i=0; i<N_BLACK; i++)
          bs[i]=flipWE(bs[i]);
      }

      if((ft&NS_FLAG)!=0)
      {
        for(i=0; i<N_WHITE; i++)
          ws[i]=flipNS(ws[i]);
        for(i=0; i<N_BLACK; i++)
          bs[i]=flipNS(bs[i]);
      }

      if((ft&NW_SE_FLAG)!=0)
      {
        for(i=0; i<N_WHITE; i++)
          ws[i]=flipNW_SE(ws[i]);
        for(i=0; i<N_BLACK; i++)
          bs[i]=flipNW_SE(bs[i]);
      }

      ki=kkidx[bs[0]][ws[0]]; /* kkidx [black king] [white king] */
      ai=(int)aaidx[ws[1]][ws[2]];

      if(IDX_is_empty(ki)||IDX_is_empty(ai))
      {
        return NOINDEX;
      }
      return ki*BLOCK_Ax+ai;
    }


    static int kakb_pctoindex()
    {
      int BLOCK_Ax = 64*64;
      int BLOCK_Bx = 64;

      int ki;
      int ft;

      ft=flipt[blackPieceSquares[0]][whitePieceSquares[0]];

      List<int> ws = new List<int>(whitePieceSquares);
      List<int> bs = new List<int>(blackPieceSquares);

      if((ft&1)!=0)
      {
        ws[0]=flipWE(ws[0]);
        ws[1]=flipWE(ws[1]);
        bs[0]=flipWE(bs[0]);
        bs[1]=flipWE(bs[1]);
      }

      if((ft&2)!=0)
      {
        ws[0]=flipNS(ws[0]);
        ws[1]=flipNS(ws[1]);
        bs[0]=flipNS(bs[0]);
        bs[1]=flipNS(bs[1]);
      }

      if((ft&4)!=0)
      {
        ws[0]=flipNW_SE(ws[0]);
        ws[1]=flipNW_SE(ws[1]);
        bs[0]=flipNW_SE(bs[0]);
        bs[1]=flipNW_SE(bs[1]);
      }

      ki=kkidx[bs[0]][ws[0]]; /* kkidx [black king] [white king] */

      if(IDX_is_empty(ki))
      {
        return NOINDEX;
      }

      return ki*BLOCK_Ax+(int)ws[1]*BLOCK_Bx+(int)bs[1];
    }

    static void init_flipt()
    {
      int i, j;
      for(i=0; i<64; i++)
      {
        flipt[i]=new int[64];
        for(j=0; j<64; j++)
        {
          flipt[i][j]=flip_type(i, j);
        }
      }
    }

    static int[] pp_hi24 = new int[MAX_PPINDEX]; /* was unsigned int */
    static int[] pp_lo48 = new int[MAX_PPINDEX];

    static int init_ppidx()
    {
      /* modifies ppidx[][], pp_hi24[], pp_lo48[] */
      int i, j;
      int idx = 0;
      int a, b;

      /* default is noindex */
      for(i=0; i<24; i++)
      {
        ppidx[i]=new int[48];
        for(j=0; j<48; j++)
        {
          ppidx[i][j]=-1;
        }
      }

      for(idx=0; idx<MAX_PPINDEX; idx++)
      {
        pp_hi24[idx]=-1;
        pp_lo48[idx]=-1;
      }

      idx=0;
      for(a=H7; a>=A2; a--)
      {

        if((a&7)<4) /* square in the queen side */
          continue;

        for(b=a-1; b>=A2; b--)
        {

          int anchor = 0;
          int loosen = 0;

          pp_putanchorfirst(a, b, ref anchor, ref loosen);

          if((anchor&7)>3)
          { /* square in the king side */
            anchor=flipWE(anchor);
            loosen=flipWE(loosen);
          }

          i=wsq_to_pidx24(anchor);
          j=wsq_to_pidx48(loosen);

          if(IDX_is_empty(ppidx[i][j]))
          {

            ppidx[i][j]=idx;
            pp_hi24[idx]=i;
            pp_lo48[idx]=j;
            idx++;
          }

        }
      }
      return idx;
    }

    static string egtb_root_path;

    public static void Create()
    {
      //-- so that all the other statics get initialized before we call init
      //-- nothing should really go here, and this function should be called before doing anything with the EGTB's, including initializing :)
    }

    public static void Init(string egtb_path)
    {
      //streamCreator = sc;
      egtb_root_path=egtb_path;
      reach_init();
      attack_maps_init();
      init_flipt();

      init_kkidx();
      init_ppidx();
      init_aaidx();
      init_aaa();
      init_pp48_idx();
      init_ppp48_idx();

      LoadTableDescriptions();

      for(int i = 0; i<64; i++)
      {
        int strow = (i%8);
        int stcol = 7-(i/8);
        int newId = (stcol*8)+strow;
        a8toa1.Add(i, newId);
      }

      int _dictionarySize = 4096;
      int _posStateBits = 2;
      int _numLiteralPosStateBits = 0;
      int _numLiteralContextBits = 3;
      byte[] properties = new byte[5];
      properties[0]=(byte)((_posStateBits*5+_numLiteralPosStateBits)*9+_numLiteralContextBits);
      for(int i = 0; i<4; i++)
        properties[1+i]=(byte)((_dictionarySize>>(8*i))&0xFF);
      decoder.SetDecoderProperties(properties);
    }

    static bool Reversed = false;

    //static void SetupPieceArrayFromBoard(Board b)
    //{
    //--    currentFilename = "";

    //--    for (int i = 0; i < 7; i++)
    //--    {
    //--        pieceCount[0, i] = 0;
    //--        pieceCount[1, i] = 0;
    //--        pieceSquares[0, i].Clear();
    //--        pieceSquares[1, i].Clear();
    //--    }

    //--    int cnt = 0;
    //--    for (int i = 0; i < 64; i++)
    //--    {
    //--        PieceType pieceType = b.pieceKind[i];

    //--        if (pieceType == PieceType.All)
    //--            continue;
    //--        cnt++;
    //--        //-- most I do is 5 man eg's in Pwned
    //--        if (cnt > 5)
    //--            return;

    //--        PlayerColor thisColor = b.pieceColor[i];

    //--        pieceCount[(int)thisColor, (int)pieceType]++;
    //--        pieceSquares[(int)thisColor, (int)pieceType].Add(a8toa1[i]);
    //--    }

    //--    string res = "";
    //--    char[] fenType = { 'k', 'q', 'r', 'b', 'n', 'p' };
    //--    string res2 = "";
    //--    string res3 = "";
    //--    for (int i = 0; i < 6; i++)
    //--    {
    //--        res += new String(fenType[i], pieceCount[0, i]);
    //--        res2 += new String(fenType[i], pieceCount[0, i]);
    //--    }

    //--    for (int i = 0; i < 6; i++)
    //--    {
    //--        res += new String(fenType[i], pieceCount[1, i]);
    //--        res3 += new String(fenType[i], pieceCount[1, i]);
    //--    }

    //--    bool ok = false;
    //--    Reversed = false;
    //--    string newFile = res;
    //--    if (!validTables.Contains(res))
    //--    {
    //--        if (validTables.Contains(res3 + res2))
    //--        {
    //--            newFile = res3 + res2;
    //--            Reversed = true;
    //--            ok = true;
    //--        }
    //--        if (res == "kk")
    //--        {
    //--            ok = true;
    //--        }
    //--    }
    //--    else
    //--    {
    //--        ok = true;
    //--    }

    //--    if (!ok)
    //--        return;

    //--    if (currentFilename != res)
    //--    {
    //--        OpenEndgameTableBase(newFile);
    //--    }

    //--    int whiteId = 0;
    //--    int blackId = 1;
    //--    if (Reversed)
    //--    {
    //--        whiteId = 1;
    //--        blackId = 0;

    //--        //if (epsq != NOSQUARE) epsq ^= 070;                              /* added */
    //--        //{ SQ_CONTENT* tempp = wp; wp = bp; bp = tempp; }  /* added */
    //--    }

    //--    whitePieceSquares.Clear();
    //--    whiteTypesSquares.Clear();
    //--    for (int i = 0; i < 7; i++)
    //--    {
    //--        List<int> s = pieceSquares[whiteId, i];
    //--        foreach (int x in s)
    //--        {
    //--            whitePieceSquares.Add(x);
    //--            whiteTypesSquares.Add(b.pieceKind[x]);
    //--        }
    //--    }

    //--    blackPieceSquares.Clear();
    //--    blackTypesSquares.Clear();
    //--    for (int i = 0; i < 7; i++)
    //--    {
    //--        List<int> s = pieceSquares[blackId, i];
    //--        foreach (int x in s)
    //--        {
    //--            blackPieceSquares.Add(x);
    //--            blackTypesSquares.Add(b.pieceKind[x]);
    //--        }
    //--    }

    //--    if (Reversed)
    //--    {
    //--        list_sq_flipNS(whitePieceSquares);
    //--        list_sq_flipNS(blackPieceSquares);
    //--    }
    //}

    static void list_sq_flipNS(List<int> s)
    {
      for(int i = 0; i<s.Count; i++)
      {
        s[i]=s[i]^56;
      }
    }

    static List<int> whitePieceTypes = new List<int>();
    static List<int> blackPieceTypes = new List<int>();

    static void sortlists(List<int> ws, List<int> wp)
    {
      int i, j;
      int ts;
      int tp;
      /* input is sorted */

      int wpl = wp.Count;

      for(i=0; i<wpl; i++)
      {
        for(j=(i+1); j<wpl; j++)
        {
          if(wp[j]>wp[i])
          {
            tp=wp[i];
            wp[i]=wp[j];
            wp[j]=tp;
            ts=ws[i];
            ws[i]=ws[j];
            ws[j]=ts;
          }
        }
      }
    }
    static string newFile;
    static bool SetupEGTB(List<int> whiteSquares, List<int> whiteTypes, List<int> blackSquares, List<int> blackTypes, ref int side, ref int epsq)
    {
      sortlists(whiteSquares, whiteTypes);
      sortlists(blackSquares, blackTypes);

      char[] fenType = { ' ', 'p', 'n', 'b', 'r', 'q', 'k' };
      string blackLetters = "";
      string whiteLetters = "";

      int whiteScore = 0;
      foreach(int i in whiteTypes)
      {
        whiteScore+=i;
        whiteLetters+=new String(fenType[i], 1);
      }

      int blackScore = 0;
      foreach(int i in blackTypes)
      {
        blackScore+=i;
        blackLetters+=new String(fenType[i], 1);
      }

      newFile="";

      if(validTables.Contains(whiteLetters+blackLetters))
      {
        whitePieceSquares=whiteSquares;
        whitePieceTypes=whiteTypes;
        blackPieceSquares=blackSquares;
        blackPieceTypes=blackTypes;
        newFile=whiteLetters+blackLetters;
        Reversed=false;
      }
      else if(validTables.Contains(blackLetters+whiteLetters))
      {
        whitePieceSquares=blackSquares;
        whitePieceTypes=blackTypes;
        blackPieceSquares=whiteSquares;
        blackPieceTypes=whiteTypes;
        newFile=blackLetters+whiteLetters;
        Reversed=true;

        list_sq_flipNS(whitePieceSquares);
        list_sq_flipNS(blackPieceSquares);

        side=Opp(side);
        if(epsq!=NOSQUARE)
          epsq^=56;
      }
      else
      {
        newFile=whiteLetters+blackLetters;
        return false;
      }

      OpenEndgameTableBase(newFile);

      return true;
    }

    public static ProbeResultType Probe(List<int> whiteSquares, List<int> blackSquares, List<int> whiteTypes, List<int> blackTypes, int realside, int epsq)
    {
      ProbeResultType probeResult = new ProbeResultType();

      try
      {
        if((blackSquares.Count == 1) && (whiteSquares.Count == 1))
        {
          probeResult.found = true;
          probeResult.stm = MateResult.TABLAS;
          probeResult.error = "With only 2 kings, Draw is assumed";
          return probeResult;
        }

        if((blackSquares.Count + whiteSquares.Count) > 5)
        {
          probeResult.error = "Max 5 man tables are supported";
          probeResult.found = false;
          return probeResult;
        }

        int side = realside;

        if(!SetupEGTB(whiteSquares, whiteTypes, blackSquares, blackTypes, ref side, ref epsq))
        {
          probeResult.found = false;
          probeResult.error = "Could not find EGTB file: " + newFile;
          return probeResult;
        }

        int dtm = 0;

        if(egtb_get_dtm(side, epsq, ref dtm))
        {
          probeResult.found = true;
          probeResult.stm = MateResult.NAN;
          int res = 0;
          int ply = 0;
          unpackdist(dtm, ref res, ref ply);

          int ret = 0;

          if(res == iWMATE)
            if(Reversed)
              probeResult.stm = MateResult.MATE_NEGRAS;
            else
              probeResult.stm = MateResult.MATE_BLANCAS;
          else if(res == iBMATE)
            if(Reversed)
              probeResult.stm = MateResult.MATE_BLANCAS;
            else
              probeResult.stm = MateResult.MATE_NEGRAS;
          else if(res == iDRAW)
            probeResult.stm = MateResult.TABLAS;

          if(realside == 0)
          {
            if(probeResult.stm == MateResult.MATE_NEGRAS)
            {
              ret = -ply;
            }
            else if(probeResult.stm == MateResult.MATE_BLANCAS)
            {
              ret = ply;
            }
          }
          else
          {
            if(probeResult.stm == MateResult.MATE_BLANCAS)
            {
              ret = -ply;
            }
            else if(probeResult.stm == MateResult.MATE_NEGRAS)
            {
              ret = ply;
            }
          }

          probeResult.dtm = ret;
          probeResult.ply = ply;
        }
        else
        {
          probeResult.found = false;
        }
      }
      catch(Exception ex)
      {
#if DEBUG
        Debug.Write(ex.Message);
#endif
      }


      return probeResult;
    }

    static void LoadTableDescriptions()
    {
      /*string[] files = Directory.GetFiles(egtb_root_path);
      int n = files.Length;

      for(int i = 0; i<n; i++)
      {
        string[] tokes = files[i].Split('\\');

        if(tokes.Length>0)
        {
          string fn = tokes[tokes.Length-1];

          if(fn.Contains(".gtb.cp4"))
          {
            validTables.Add(fn.Replace(".gtb.cp4", ""));
          }
        }
      }*/

      validTables.Add("kpk");
      validTables.Add("kbk");
      validTables.Add("kbkb");

      validTables.Add("kbkn");

      validTables.Add("kbnk");

      validTables.Add("knk");
      validTables.Add("knkn");

      validTables.Add("knnk");

      validTables.Add("kqk");

      validTables.Add("kqkr");
      validTables.Add("krk");
      validTables.Add("krkb");
      validTables.Add("kpkp");

    }

    //static string BoardToPieces(Board board)
    //{
    //--    int[,] pieceCount = new int[2, 7];

    //--    int cnt = 0;
    //--    for (int i = 0; i < 64; i++)
    //--    {
    //--        PieceType pieceType = board.pieceKind[i];
    //--        if (pieceType == PieceType.All)
    //--            continue;
    //--        cnt++;
    //--        //-- most I do is 5 man eg's in Pwned
    //--        if (cnt > 5)
    //--            return "";
    //--        PlayerColor thisColor = board.pieceColor[i];

    //--        pieceCount[(int)thisColor, (int)pieceType]++;
    //--    }

    //--    string res = "";
    //--    char[] fenType = { 'k', 'q', 'r', 'b', 'n', 'p' };

    //--    for (int i = 0; i < 6; i++)
    //--    {
    //--        res += new String(fenType[i], pieceCount[0, i]);
    //--    }

    //--    for (int i = 0; i < 6; i++)
    //--    {
    //--        res += new String(fenType[i], pieceCount[1, i]);
    //--    }

    //--    //-- don't return a key if the table isn't loaded
    //--    //-- make sure you call LoadTableDescriptions when the game loads
    //--    if (validTables.Contains(res))
    //--        return res;

    //--    return "";
    //}

    //static FileStream streamCreator;

    static bool OpenEndgameTableBase(string pieces)
    {
      if(currentFilename==pieces)
        return true;
      string tablePath = string.Format("{0}\\{1}.gtb.cp4", egtb_root_path, pieces);
      if(currentStream!=null)
      {
        currentStream.Close();
        currentStream.Dispose();
        currentStream=null;
      }
     // if(File.Exists(tablePath))
      {
        currentFilename=pieces;
        //currentStream = TitleContainer.OpenStream(tablePath);
        //currentStream = streamCreator.OpenStream(tablePath);
        switch(pieces)
        {
          case "kpk": currentStream=new MemoryStream(Resources.kpk_gtb); break;
          case "kbk": currentStream=new MemoryStream(Resources.kbk_gtb); break;
          case "kbkb":
            currentStream=new MemoryStream(Resources.kbkb_gtb);
            break;
          case "kbkn":
            currentStream=new MemoryStream(Resources.kbkn_gtb);
            break;
          case "kbnk":
            currentStream=new MemoryStream(Resources.kbnk_gtb);
            break;
         
          case "knkn":
            currentStream=new MemoryStream(Resources.knkn_gtb);
            break;
         
          case "kqk":
            currentStream=new MemoryStream(Resources.kqk_gtb);
            break;
          case "kqkr":
            currentStream=new MemoryStream(Resources.kqkr_gtb);
            break;
          case "krk":
            currentStream=new MemoryStream(Resources.krk_gtb);
            break;
          case "krkb":
            currentStream=new MemoryStream(Resources.krkb_gtb);
            break;
          case "kpkp":
            currentStream=new MemoryStream(Resources.kpkp_gtb);
            break;
        }
        
        if (currentStream != null)
        {
          currentStream.Seek(0, SeekOrigin.Begin);
          egtb_loadindexes();

          currentPctoi=egkey[currentFilename].pctoi;
        }
         
      }

      return (currentStream!=null);
    }

    static int egtb_block_getnumber(int side, int idx)
    {
      int blocks_per_side;
      int block_in_side;
      int max = egkey[currentFilename].maxindex;

      blocks_per_side=1+(max-1)/entries_per_block;
      block_in_side=idx/entries_per_block;

      return side*blocks_per_side+block_in_side; /* block */
    }

    static int egtb_block_getsize(int idx)
    {
      int blocksz = entries_per_block;
      int maxindex = egkey[currentFilename].maxindex;
      int block, offset, x;

      block=idx/blocksz;
      offset=block*blocksz;

      if((offset+blocksz)>maxindex)
        x=maxindex-offset; /* last block size */
      else
        x=blocksz; /* size of a normal block */

      return x;
    }

    class ZIPINFO
    {
      internal int extraoffset;
      internal int totalblocks;
      internal int[] blockindex;
    };

    static Dictionary<string, ZIPINFO> Zipinfo = new Dictionary<string, ZIPINFO>();

    const int MAX_EGKEYS = 145;
    const int SZ = 4;

    static bool fread32(ref ulong y)
    {
      ulong x = 0;
      byte[] p = new byte[SZ];

      currentStream.Read(p, 0, SZ);

      for(int i = 0; i<SZ; i++)
      {
        x|=(ulong)p[i]<<(i*8);
      }
      y=x;

      return true;
    }

    static bool egtb_loadindexes()
    {
      ulong blocksize = 1;
      ulong tailblocksize1 = 0;
      ulong tailblocksize2 = 0;
      ulong offset = 0;
      ulong dummy = 0;
      ulong i;
      ulong blocks;
      ulong n_idx;
      ulong idx = 0;
      int[] p = new int[MAX_AAAINDEX];

      /* Get Reserved bytes, blocksize, offset */
      currentStream.Seek(0, SeekOrigin.Begin);

      fread32(ref dummy);
      fread32(ref dummy);
      fread32(ref blocksize);
      fread32(ref dummy);
      fread32(ref tailblocksize1);
      fread32(ref dummy);
      fread32(ref tailblocksize2);
      fread32(ref dummy);
      fread32(ref offset);
      fread32(ref dummy);

      blocks=(offset-40)/4-1;
      n_idx=blocks+1;

      /* Input of Indexes */
      for(i=0; i<n_idx; i++)
      {
        fread32(ref idx);
        p[i]=(int)idx; /* reads a 32 bit int, and converts it to index_t */
      }

      if(!Zipinfo.ContainsKey(currentFilename))
        Zipinfo.Add(currentFilename, new ZIPINFO());

      Zipinfo[currentFilename].extraoffset=0;
      Zipinfo[currentFilename].totalblocks=(int)n_idx;
      Zipinfo[currentFilename].blockindex=p;

      return true;
    }

    static int egtb_block_getsize_zipped(int block)
    {
      int i, j;
      i=Zipinfo[currentFilename].blockindex[block];
      j=Zipinfo[currentFilename].blockindex[block+1];

      return j-i;
    }

    static bool egtb_block_park(int block)
    {
      int i;
      long fseek_i;

      i=Zipinfo[currentFilename].blockindex[block];
      i+=Zipinfo[currentFilename].extraoffset;

      fseek_i=(long)i;
      return 0==currentStream.Seek(fseek_i, SeekOrigin.Begin);
    }

    static int EGTB_MAXBLOCKSIZE = 65536;
    static byte[] Buffer_zipped = new byte[EGTB_MAXBLOCKSIZE];
    static byte[] Buffer_packed = new byte[EGTB_MAXBLOCKSIZE];

    /* bp:buffer packed to out:distance to mate buffer */
    static bool egtb_block_unpack(int side, int n, byte[] bp, ref int[] dtm)
    {
      for(int i = 0; i<n; i++)
      {
        dtm[i]=dtm_unpack(side, bp[i]);
      }
      return true;
    }

    //public static byte[] Decompress(byte[] inputBytes)
    //{
    //--    MemoryStream newInStream = new MemoryStream(inputBytes);

    //--    SevenZip.Compression.LZMA.Decoder decoder = new SevenZip.Compression.LZMA.Decoder();

    //--    newInStream.Seek(0, 0);
    //--    MemoryStream newOutStream = new MemoryStream();

    //--    byte[] properties2 = new byte[5];
    //--    if (newInStream.Read(properties2, 0, 5) != 5)
    //--        throw (new Exception("input .lzma is too short"));
    //--    long outSize = 0;
    //--    for (int i = 0; i < 8; i++)
    //--    {
    //--        int v = newInStream.ReadByte();
    //--        if (v < 0)
    //--            throw (new Exception("Can't Read 1"));
    //--        outSize |= ((long)(byte)v) << (8 * i);
    //--    }
    //--    decoder.SetDecoderProperties(properties2);

    //--    long compressedSize = newInStream.Length - newInStream.Position;
    //--    decoder.Code(newInStream, newOutStream, compressedSize, outSize, null);

    //--    byte[] b = newOutStream.ToArray();

    //--    return b;



    //}

    static void split_index(int entries_per_block, int i, ref int o, ref int r)
    {
      int n;
      n=i/entries_per_block;
      o=n*entries_per_block;
      r=i-o;
      return;
    }


    //static bool get_dtm_from_cache(int side, int idx, ref int dtm)
    //{
    //--    int offset = 0;
    //--    int remainder = 0;

    //--    split_index(entries_per_block, idx, ref offset, ref remainder);

    //--    int tableIndex = blockCache.FindIndex(delegate(TableBlock tb)
    //--    {
    //--        return (tb.key == currentFilename) && (tb.offset == offset) && (tb.side == (int)side);
    //--    });

    //--    dtm = blockCache[tableIndex].pcache[remainder];

    //--    return true;
    //}

    struct TableBlock
    {
      public string key;
      public int side;
      public int offset;
      public int[] pcache;

      public TableBlock(string currentFilename, int stm, int o)
      {
        key=currentFilename;
        side=stm;
        offset=o;
        pcache=new int[16384];
      }

      public override int GetHashCode()
      {
        unchecked //-- Overflow is fine, just wrap
        {
          int hash = 17;
          //-- Suitable nullity checks etc, of course :)
          hash=hash*23+key.GetHashCode();
          hash=hash*23+side.GetHashCode();
          hash=hash*23+offset.GetHashCode();
          return hash;
        }
      }

      public override bool Equals(object other)
      {
        return other is TableBlock&&Equals((TableBlock)other);
      }

      private bool Equals(TableBlock other)
      {
        return (key==other.key)&&(side==other.side)&&(offset==other.offset);
      }
    }

    //static void removepiece(List<int> ys, List<PieceType> yp, int j)
    //{
    //--    ys.RemoveAt(j);
    //--    yp.RemoveAt(j);
    //}

    static int NOSQUARE = 0;

    static List<TableBlock> blockCache = new List<TableBlock>();

    static Stream zipStream = new MemoryStream(Buffer_zipped);
    static Stream packStream = new MemoryStream(Buffer_packed);

    static int map88(int x)
    {
      return x+(x&56);
    }

    static int unmap88(int x)
    {
      return x+(x&7)>>1;
    }

    static int FindBlockIndex(int offset, int side)
    {
      int f = -1;
      for(int i = 0; i<blockCache.Count; i++)
      {
        if(blockCache[i].offset==offset)
          if(blockCache[i].side==side)
            if(blockCache[i].key==currentFilename)
            {
              f=i;
              break;
            }
      }
      return f;
    }

    static void removepiece(ref List<int> ys, ref List<int> yp, int j)
    {
      ys.RemoveAt(j);
      yp.RemoveAt(j);
    }


    static bool tb_probe_(int side, int epsq, ref int dtm)
    {
      int idx = currentPctoi();

      int offset = 0;
      int remainder = 0;
      split_index(entries_per_block, idx, ref offset, ref remainder);

      int tableIndex = FindBlockIndex(offset, side);

      if(tableIndex==-1)
      {
        TableBlock t = new TableBlock(currentFilename, side, offset);

        zipStream.Seek(0, SeekOrigin.Begin);
        packStream.Seek(0, SeekOrigin.Begin);

        int block = egtb_block_getnumber(side, idx);
        int n = egtb_block_getsize(idx);
        int z = egtb_block_getsize_zipped(block);

        egtb_block_park(block);

        currentStream.Read(Buffer_zipped, 0, z);

        z=z-(LZMA86_HEADER_SIZE+1);
        Array.Copy(Buffer_zipped, LZMA86_HEADER_SIZE+1, Buffer_zipped, 0, z);

        decoder.Code(zipStream, packStream, (long)z, (long)n, null);

        egtb_block_unpack(side, n, Buffer_packed, ref t.pcache);

        blockCache.Add(t);
        if(blockCache.Count>256)
          blockCache.RemoveAt(0);

        dtm=t.pcache[remainder];
      }
      else
      {
        dtm=blockCache[tableIndex].pcache[remainder];
      }

      return true;
    }

    static int Opp(int side)
    {
      if(side==0)
        return 1;

      return 0;
    }
    //static uint INFOMASK = 7;
    //static int PLYSHIFT = 3;

    static int adjust_up(int dist)
    {
      uint udist = (uint)dist;
      uint sw = (uint)(udist&INFOMASK);

      if((sw==iWMATE)||(sw==iWMATEt)||(sw==iBMATE)||(sw==iBMATEt))
      {
        uint x = 1;
        udist+=(x<<PLYSHIFT);
      }

      return (int)udist;
    }


    static int bestx(int side, int a, int b)
    {
      int key;
      int[][] comparison = new int[4][]  {
        /*draw, wmate, bmate, forbid*/
        /* draw  */
                      new int[4] {0, 3, 0, 0},
        /* wmate */
                      new int[4] {0, 1, 0, 0},
        /* bmate */
                      new int[4] {3, 3, 2, 0},
        /* forbid*/
                      new int[4] {3, 3, 3, 0}

                      /* 0 = selectfirst   */
                      /* 1 = selectlowest  */
                      /* 2 = selecthighest */
                      /* 3 = selectsecond  */
          };

      int[] xorkey = new int[2] { 0, 3 };
      int[] retu = new int[4];
      int ret = iFORBID;

      if(a==iFORBID)
        return b;
      if(b==iFORBID)
        return a;

      retu[0]=a; /* first parameter */
      retu[1]=a; /* the lowest by default */
      retu[2]=b; /* highest by default */
      retu[3]=b; /* second parameter */
      if(b<a)
      {
        retu[1]=b;
        retu[2]=a;
      }

      key=comparison[a&3][b&3]^xorkey[side];
      ret=retu[key];

      return ret;
    }

    static bool egtb_get_dtm(int side, int epsq, ref int dtm)
    {
      bool ok = tb_probe_(side, epsq, ref dtm);

      if(ok)
      {
        if(epsq!=NOSQUARE)
        {
          int capturer_a = 0;
          int capturer_b = 0;
          int xed = 0;
          bool okcall = true;

          List<int> old_whitePieceSquares = new List<int>(whitePieceSquares);
          List<int> old_blackPieceSquares = new List<int>(blackPieceSquares);
          List<int> old_whitePieceType = new List<int>(whitePieceTypes);
          List<int> old_blackPieceType = new List<int>(blackPieceTypes);
          string oldFileName = currentFilename;
          bool oldReversed = Reversed;

          List<int> xs;
          List<int> xp;
          List<int> ys;
          List<int> yp;

          if(side==0)
          {
            xs=new List<int>(whitePieceSquares);
            xp=new List<int>(whitePieceTypes);
            ys=new List<int>(blackPieceSquares);
            yp=new List<int>(blackPieceTypes);
          }
          else
          {
            xs=new List<int>(blackPieceSquares);
            xp=new List<int>(blackPieceTypes);
            ys=new List<int>(whitePieceSquares);
            yp=new List<int>(whitePieceTypes);
          }

          /* captured pawn, trick: from epsquare to captured */
          xed=epsq^(1<<3);

          /* find index captured (j) */
          int j = ys.IndexOf(xed);

          /* try first possible ep capture */
          if(0==(136&(map88(xed)+1)))
            capturer_a=xed+1;
          /* try second possible ep capture */
          if(0==(136&(map88(xed)-1)))
            capturer_b=xed-1;

          if((j>-1)&&(ys[j]==xed))
          {
            int xsl = xs.Count;
            /* find capturers (i) */
            for(int i = 0; (i<xsl)&&okcall; i++)
            {
              if(xp[i]==PAWN&&(xs[i]==capturer_a||xs[i]==capturer_b))
              {
                int epscore = iFORBID;

                /* execute capture */
                xs[i]=epsq;
                removepiece(ref ys, ref yp, j);

                int newdtm = 0;

                whitePieceSquares=ys;
                whitePieceTypes=yp;
                blackPieceSquares=xs;
                blackPieceTypes=xp;

                //-- changing currentFile to kpk for ex. we lost a piece so new file is regq
                //-- make sure to change back when done
                int noep = NOSQUARE;
                if(!SetupEGTB(whitePieceSquares, whitePieceTypes, blackPieceSquares, blackPieceTypes, ref side, ref noep))
                {
                  okcall=false;
                }
                else
                {
                  okcall=tb_probe_(Opp(side), NOSQUARE, ref newdtm);
                }

                whitePieceSquares=old_whitePieceSquares;
                whitePieceTypes=old_whitePieceType;
                blackPieceSquares=old_blackPieceSquares;
                blackPieceTypes=old_blackPieceType;

                if(okcall)
                {
                  epscore=newdtm;
                  epscore=adjust_up(epscore);

                  /* chooses to ep or not */
                  dtm=bestx(side, epscore, dtm);
                }

              }
            }
          }
        } /* ep */
      }

      return ok;
    }

    static void unpackdist(int d, ref int res, ref int ply)
    {
      ply=d>>PLYSHIFT;
      res=d&INFOMASK;
    }

    static int LZMA_PROPS_SIZE = 5;
    static int LZMA86_SIZE_OFFSET = (1+LZMA_PROPS_SIZE);
    static int LZMA86_HEADER_SIZE = (LZMA86_SIZE_OFFSET+8);

    static bool egtb_filepeek(int side, int idx, ref int dtm)
    {
      bool ok;
      int x = 0;
      int maxindex = egkey[currentFilename].maxindex;

      ok=fpark_entry_packed(side, idx, maxindex);
      ok=ok&&fread_entry_packed(side, ref x);

      if(ok)
        dtm=x;
      else
        dtm=iFORBID;

      return ok;
    }

    static bool fread_entry_packed(int side, ref int px)
    {
      int p = currentStream.ReadByte();
      px=dtm_unpack(side, p);

      return true;
    }

    static bool fpark_entry_packed(int side, int idx, int max)
    {
      long fseek_i = ((int)side*max+idx);
      currentStream.Seek(fseek_i, SeekOrigin.Begin);
      return true;
    }

    const int tb_DRAW = 0;
    const int tb_WMATE = 1;
    const int tb_BMATE = 2;
    const int tb_FORBID = 3;
    const int tb_UNKNOWN = 7;
    const int iDRAW = tb_DRAW;
    const int iWMATE = tb_WMATE;
    const int iBMATE = tb_BMATE;
    const int iFORBID = tb_FORBID;

    const int iDRAWt = tb_DRAW|4;
    const int iWMATEt = tb_WMATE|4;
    const int iBMATEt = tb_BMATE|4;
    const int iUNKNOWN = tb_UNKNOWN;

    const int iUNKNBIT = (1<<2);

    static int dtm_unpack(int stm, int packed)
    {
      int info, plies, prefx, store, moves;
      int ret;
      int p = packed;

      if(iDRAW==p||iFORBID==p)
      {
        return p;
      }

      info=p&3;
      store=p>>2;

      if(stm==0)
      {
        if(info==iWMATE)
        {
          moves=store+1;
          plies=moves*2-1;
          prefx=info;
        }
        else if(info==iBMATE)
        {
          moves=store;
          plies=moves*2;
          prefx=info;
        }
        else if(info==iDRAW)
        {
          moves=store+1+63;
          plies=moves*2-1;
          prefx=iWMATE;
        }
        else if(info==iFORBID)
        {

          moves=store+63;
          plies=moves*2;
          prefx=iBMATE;
        }
        else
        {
          plies=0;
          prefx=0;
        }

        ret=(prefx|(plies<<3));
      }
      else
      {
        if(info==iBMATE)
        {
          moves=store+1;
          plies=moves*2-1;
          prefx=info;
        }
        else if(info==iWMATE)
        {
          moves=store;
          plies=moves*2;
          prefx=info;
        }
        else if(info==iDRAW)
        {
          if(store==63)
          {
            /* 	exception: no position in the 5-man 
                TBs needs to store 63 for iBMATE 
                it is then used to indicate iWMATE 
                when just overflows */
            store++;

            moves=store+63;
            plies=moves*2;
            prefx=iWMATE;

          }
          else
          {

            moves=store+1+63;
            plies=moves*2-1;
            prefx=iBMATE;
          }
        }
        else if(info==iFORBID)
        {

          moves=store+63;
          plies=moves*2;
          prefx=iWMATE;
        }
        else
        {
          plies=0;
          prefx=0;

        }

        ret=(prefx|(plies<<3));
      }
      return ret;
    }

    static int flipWE(int x)
    {
      return x^7;
    }
    static int flipNS(int x)
    {
      return x^56;
    }
    static int flipNW_SE(int x)
    {
      return ((x&7)<<3)|(x>>3);
    }


    static int BLOCK_Ax = 64;
    static int getcol(int x)
    {
      return x&7;
    }
    static int getrow(int x)
    {
      return x>>3;
    }

    static int flip_type(int x, int y)
    {
      int rowx, rowy, colx, coly;
      int ret = 0;

      if(getcol(x)>3)
      {
        x=flipWE(x); /* x = x ^ 07  */
        y=flipWE(y);
        ret|=1;
      }
      if(getrow(x)>3)
      {
        x=flipNS(x); /* x = x ^ 070  */
        y=flipNS(y);
        ret|=2;
      }
      rowx=getrow(x);
      colx=getcol(x);
      if(rowx>colx)
      {
        x=flipNW_SE(x); /* x = ((x&7)<<3) | (x>>3) */
        y=flipNW_SE(y);
        ret|=4;
      }
      rowy=getrow(y);
      coly=getcol(y);
      if(rowx==colx&&rowy>coly)
      {
        x=flipNW_SE(x);
        y=flipNW_SE(y);
        ret|=4;
      }
      return ret;
    }

    static bool IDX_is_empty(int x)
    {
      return (0==(1+(x)));
    }

    static int[][] kkidx = new int[64][];

    static int WHITES = (1<<6);
    static int BLACKS = (1<<7);

    public static int NOPIECE = 0;
    public static int PAWN = 1;
    public static int KNIGHT = 2;
    public static int BISHOP = 3;
    public static int ROOK = 4;
    public static int QUEEN = 5;
    public static int KING = 6;

    static int wK = (KING|WHITES);
    static int wP = (PAWN|WHITES);
    static int wN = (KNIGHT|WHITES);
    static int wB = (BISHOP|WHITES);
    static int wR = (ROOK|WHITES);
    static int wQ = (QUEEN|WHITES);

    /*Blacks*/
    static int bK = (KING|BLACKS);
    static int bP = (PAWN|BLACKS);
    static int bN = (KNIGHT|BLACKS);
    static int bB = (BISHOP|BLACKS);
    static int bR = (ROOK|BLACKS);
    static int bQ = (QUEEN|BLACKS);

    static void norm_kkindex(int x, int y, ref int pi, ref int pj)
    {
      int rowx, rowy, colx, coly;

      if(getcol(x)>3)
      {
        x=flipWE(x); /* x = x ^ 07  */
        y=flipWE(y);
      }
      if(getrow(x)>3)
      {
        x=flipNS(x); /* x = x ^ 070  */
        y=flipNS(y);
      }
      rowx=getrow(x);
      colx=getcol(x);
      if(rowx>colx)
      {
        x=flipNW_SE(x); /* x = ((x&7)<<3) | (x>>3) */
        y=flipNW_SE(y);
      }
      rowy=getrow(y);
      coly=getcol(y);
      if(rowx==colx&&rowy>coly)
      {
        x=flipNW_SE(x);
        y=flipNW_SE(y);
      }

      pi=x;
      pj=y;
    }

    static int[] wksq = new int[MAX_KKINDEX];
    static int[] bksq = new int[MAX_KKINDEX];
    static byte[][] attmap = new byte[64][];
    static int[] attmsk = new int[256];

    static bool possible_attack(int from, int to, int piece)
    {
      return (0!=(attmap[to][from]&attmsk[piece]));
    }
    static ulong[][] Reach = new ulong[7][];

    static int[] bstep = { 17, 15, -15, -17, 0 };
    static int[] rstep = { 1, 16, -1, -16, 0 };
    static int[] nstep = { 18, 33, 31, 14, -18, -33, -31, -14, 0 };
    static int[] kstep = { 1, 17, 16, 15, -1, -17, -16, -15, 0 };

    static int[][] psteparr = {null, null, /* NOPIECE & PAWN */
                     nstep, bstep, rstep, kstep, kstep /* same for Q & K*/
                    };

    static bool[] pslider = {false, false,
                     false,  true,  true,  true, false
                    };

    static void tolist_rev(ulong occ, int input_piece, int sq, ref List<int> list)
    {
      /* reversible moves from pieces. Output is a list of squares */
      int direction;
      int pc;
      int s;
      int from;
      int step;
      int[] steparr;
      bool slider;
      int us;

      from=(int)map88(sq);

      pc=input_piece&(PAWN|KNIGHT|BISHOP|ROOK|QUEEN|KING);

      steparr=psteparr[pc];
      slider=pslider[pc];

      if(slider)
      {

        for(direction=0; steparr[direction]!=0; direction++)
        {
          step=steparr[direction];
          s=from+step;
          while(0==(s&0x88))
          {
            us=(int)unmap88(s);
            if(0!=(0x1u&(int)(occ>>us)))
              break;
            list.Add(us);
            s+=step;
          }
        }

      }
      else
      {
        for(direction=0; steparr[direction]!=0; direction++)
        {
          step=steparr[direction];
          s=from+step;
          if(0==(s&0x88))
          {
            us=(int)unmap88(s);
            if(0==(0x1u&(int)(occ>>us)))
            {
              list.Add(us);
            }
          }
        }
      }

      return;
    }

    static void reach_init()
    {
      List<int> buflist = new List<int>();
      List<int> list = new List<int>();
      int pc;
      int[] stp_a = { 15, -15 };
      int[] stp_b = { 17, -17 };
      int STEP_A, STEP_B;
      int side;
      int index;
      int sq, us;

      int s;

      for(int q = 0; q<7; q++)
      {
        Reach[q]=new ulong[64];
      }

      for(pc=KNIGHT; pc<(KING+1); pc++)
      {
        for(sq=0; sq<64; sq++)
        {
          ulong bb = (ulong)0;
          buflist.Clear();
          tolist_rev((ulong)0, pc, sq, ref buflist);
          list=new List<int>(buflist);
          foreach(int li in list)
          {
            bb|=(ulong)1<<li;
          }
          Reach[pc][sq]=bb;
        }
      }

      for(side=0; side<2; side++)
      {
        index=1^side;
        STEP_A=stp_a[side];
        STEP_B=stp_b[side];
        for(sq=0; sq<64; sq++)
        {

          int sq88 = (int)map88(sq);
          ulong bb = (ulong)0;

          list=new List<int>();

          s=sq88+STEP_A;
          if(0==(s&0x88))
          {
            us=(int)unmap88(s);
            list.Add(us);
          }
          s=sq88+STEP_B;
          if(0==(s&0x88))
          {
            us=(int)unmap88(s);
            list.Add(us);
          }

          foreach(int li in list)
          {
            bb|=(ulong)1<<li;
          }
          Reach[index][sq]=bb;
        }
      }
    }

    static bool BB_ISBITON(ulong bb, int bit)
    {
      return (0!=(((bb)>>(bit))&(ulong)(1)));
    }

    static int mapx88(int x)
    {
      return ((x&56)<<1)|(x&7);
    }


    static void attack_maps_init()
    {
      int i;
      int m, from, to;
      int to88, fr88;
      int diff;

      ulong rook, bishop, queen, knight, king;

      for(i=0; i<256; ++i)
      {
        attmsk[i]=0;
      }
      attmsk[wP]=1<<0;
      attmsk[bP]=1<<1;

      attmsk[KNIGHT]=1<<2;
      attmsk[wN]=1<<2;
      attmsk[bN]=1<<2;

      attmsk[BISHOP]=1<<3;
      attmsk[wB]=1<<3;
      attmsk[bB]=1<<3;

      attmsk[ROOK]=1<<4;
      attmsk[wR]=1<<4;
      attmsk[bR]=1<<4;

      attmsk[QUEEN]=1<<5;
      attmsk[wQ]=1<<5;
      attmsk[bQ]=1<<5;

      attmsk[KING]=1<<6;
      attmsk[wK]=1<<6;
      attmsk[bK]=1<<6;

      for(to=0; to<64; ++to)
      {
        attmap[to]=new byte[64];
        for(from=0; from<64; ++from)
        {
          m=0;
          rook=Reach[ROOK][from];
          bishop=Reach[BISHOP][from];
          queen=Reach[QUEEN][from];
          knight=Reach[KNIGHT][from];
          king=Reach[KING][from];

          if(BB_ISBITON(knight, to))
          {
            m|=attmsk[wN];
          }
          if(BB_ISBITON(king, to))
          {
            m|=attmsk[wK];
          }
          if(BB_ISBITON(rook, to))
          {
            m|=attmsk[wR];
          }
          if(BB_ISBITON(bishop, to))
          {
            m|=attmsk[wB];
          }
          if(BB_ISBITON(queen, to))
          {
            m|=attmsk[wQ];
          }

          to88=mapx88(to);
          fr88=mapx88(from);
          diff=(int)to88-(int)fr88;

          if(diff==17||diff==15)
          {
            m|=attmsk[wP];
          }
          if(diff==-17||diff==-15)
          {
            m|=attmsk[bP];
          }

          attmap[to][from]=(byte)m;
        }
      }
    }

    static int init_kkidx()
    {
      /* modifies kkidx[][], wksq[], bksq[] */
      int idx;
      int x, y;
      int i = 0;
      int j = 0;

      /* default is noindex */
      for(x=0; x<64; x++)
      {
        kkidx[x]=new int[64];
        for(y=0; y<64; y++)
        {
          kkidx[x][y]=-1;
        }
      }

      idx=0;
      for(x=0; x<64; x++)
      {
        for(y=0; y<64; y++)
        {
          /* is x,y illegal? continue */
          if(possible_attack(x, y, wK)||x==y)
            continue;

          /* normalize */
          /*i <-- x; j <-- y */
          norm_kkindex(x, y, ref i, ref j);

          if(kkidx[i][j]==-1)
          { /* still empty */
            kkidx[i][j]=idx;
            kkidx[x][y]=idx;
            bksq[idx]=i;
            wksq[idx]=j;
            idx++;
          }
        }
      }

      return idx;
    }

    static int kxk_pctoindex()
    {
      int ki;
      //int i;
      List<int> ws = new List<int>();
      List<int> bs = new List<int>();
      int ft;

      ft=flip_type(blackPieceSquares[0], whitePieceSquares[0]);

      ws=new List<int>(whitePieceSquares);
      bs=new List<int>(blackPieceSquares);

      if((ft&1)!=0)
      {
        for(int b = 0; b<ws.Count; b++)
          ws[b]=flipWE(ws[b]);
        for(int b = 0; b<bs.Count; b++)
          bs[b]=flipWE(bs[b]);
      }

      if((ft&2)!=0)
      {
        for(int b = 0; b<ws.Count; b++)
        {
          ws[b]=flipNS(ws[b]);
        }
        for(int b = 0; b<bs.Count; b++)
        {
          bs[b]=flipNS(bs[b]);
        }
      }

      if((ft&4)!=0)
      {
        for(int b = 0; b<ws.Count; b++)
        {
          ws[b]=flipNW_SE(ws[b]);
        }
        for(int b = 0; b<bs.Count; b++)
        {
          bs[b]=flipNW_SE(bs[b]);
        }
      }

      ki=kkidx[bs[0]][ws[0]]; /* kkidx [black king] [white king] */

      if(ki==-1)
      {
        return NOINDEX;
      }

      return ki*BLOCK_Ax+(int)ws[1];
    }

    static int kpk_pctoindex()
    {
      int BLOCK_A = 64*64;
      int BLOCK_B = 64;

      int pslice;
      int sq;
      int pawn = whitePieceSquares[1];
      int wk = whitePieceSquares[0];
      int bk = blackPieceSquares[0];

      if(!(A2<=pawn&&pawn<A8))
      {
        return NOINDEX;
      }

      if((pawn&7)>3)
      { /* column is more than 3. e.g. = e,f,g, or h */
        pawn=flipWE(pawn);
        wk=flipWE(wk);
        bk=flipWE(bk);
      }

      sq=pawn;
      sq^=56; /* flipNS*/
      sq-=8;   /* down one row*/
      pslice=((sq+(sq&3))>>1);

      int res = pslice*BLOCK_A+wk*BLOCK_B+bk;

      return res;
    }
  }
}
