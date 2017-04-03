using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Resources;

using Types;
using Motor;

using hash = System.UInt64;
using bitbrd = System.UInt64;
using mov = System.Int32;
using sq = System.Int32;
using pieza = System.Int32;

namespace Book
{
  //--  http://hgm.nubati.net/book_format.html
  public class cLibro
  {
    private const int nCasillaPeon = 0;
    private const int m_nEnroque = 768; //-- Piezas * escaques
    private const int m_nEnPaso = 772; //-- Flag de enroque
    private const int m_nTurno = 780; //-- Número de filas
    private cAleatorio m_Rand;
    
    //------------------------------------------------------------------------------------
    public cLibro()
    {
      m_Rand = new cAleatorio((int)(InOut.cReloj.Now() % 1000));
    }
    
    //------------------------------------------------------------------------------------
    public static hash PolyglotKey(cPosicion posicion)
    {
      hash key = 0;
      bitbrd bitBoard = posicion.Piezas();

      while (bitBoard != 0)
      {
        sq s = cBitBoard.GetLSB(ref bitBoard);
        pieza pc = posicion.GetPieza(s);
        int pieceOfs = 2 * (cTypes.TipoPieza(pc) - 1) + ((cTypes.GetColor(pc) == cColor.BLANCO) ? 1 : 0);
        key ^= cPolyglotBook.PG[nCasillaPeon + (64 * pieceOfs + s)];
      }

      bitBoard = (ulong)posicion.PosibleEnrocar(cEnroque.CUALQUIER_ENROQUE);

      while (bitBoard != 0)
        key ^= cPolyglotBook.PG[m_nEnroque + cBitBoard.GetLSB(ref bitBoard)];

      if (posicion.CasillaEnPaso() != cCasilla.NONE)
        key ^= cPolyglotBook.PG[m_nEnPaso + cTypes.Columna(posicion.CasillaEnPaso())];

      if (posicion.ColorMueve() == cColor.BLANCO)
        key ^= cPolyglotBook.PG[m_nTurno + 0];

      return key;
    }

    //------------------------------------------------------------------------------------
    public mov Buscar(cPosicion posicion)
    {
      mov move = cMovType.MOV_NAN;
      cPolyglotBook book = new cPolyglotBook();
      if (book.Open())
      {
        cPolyglotBook.stLineaLibro lineaLibro = new cPolyglotBook.stLineaLibro();
        UInt16 bMejorJugada = 0;
        uint sum = 0;

        hash key = PolyglotKey(posicion);

        book.m_Stream.Seek(book.FindFirst(key) * cPolyglotBook.SIZE_OF_BOOKENTRY, SeekOrigin.Begin);
        while (book.Read(ref lineaLibro) && lineaLibro.key == key)
        {
          bMejorJugada = Math.Max(bMejorJugada, lineaLibro.count);
          sum += lineaLibro.count;

          if ((sum != 0 && ((((uint)m_Rand.GetRand()) % sum) < lineaLibro.count)) || (lineaLibro.count == bMejorJugada))
            move = (lineaLibro.move);
        }

        if (move != 0)
        {
          int pt = (move >> 12) & 7;

          if(pt != 0)
          {
            if(posicion.IsObstaculo(cTypes.GetFromCasilla(move)) == false && posicion.IsObstaculo(cTypes.GetToCasilla(move)) == false)
            {
              move = cTypes.CreaMov(cTypes.GetFromCasilla(move), cTypes.GetToCasilla(move), cMovType.PROMOCION, (pt + 1));

              for(cReglas listaMovimientos = new cReglas(posicion, cMovType.LEGAL); listaMovimientos.GetActualMov() != cMovType.MOV_NAN; ++listaMovimientos)
                if(move == (listaMovimientos.GetActualMov() ^ cTypes.TipoMovimiento(listaMovimientos.GetActualMov())))
                {
                  move = listaMovimientos.GetActualMov();
                  break;
                }
            }
          }
        }
      }
      return move;
    }
  }
}
