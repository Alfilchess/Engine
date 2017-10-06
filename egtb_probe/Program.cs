using System;
using System.Collections.Generic;
using System.Text;
using Gaviota;
using Pwned.Chess;
using System.IO;
using Pwned.Chess.Common;

namespace egtb_probe
{
    public class WindowsStreamCreator : IStreamCreator
    {
        public Stream OpenStream(string path)
        {
            return new FileStream(path, FileMode.Open);
        }
    }

    class Program
    {
        static bool running = true;

        static bool DidSetupSquaresFromFen(string fen)
        {
            byte index = 0;
            byte spc = 0;
            byte spacers = 0;

            whosTurn = 0;
            enPassantSquare = 0;
            whitePieceSquares.Clear();
            whiteTypesSquares.Clear();
            blackPieceSquares.Clear();
            blackTypesSquares.Clear();

            if (fen.Contains("a3"))
                enPassantSquare = EGTB.a8toa1[40];
            else if (fen.Contains("b3"))
                enPassantSquare = EGTB.a8toa1[41];
            else if (fen.Contains("c3"))
                enPassantSquare = EGTB.a8toa1[42];
            else if (fen.Contains("d3"))
                enPassantSquare = EGTB.a8toa1[43];
            else if (fen.Contains("e3"))
                enPassantSquare = EGTB.a8toa1[44];
            else if (fen.Contains("f3"))
                enPassantSquare = EGTB.a8toa1[45];
            else if (fen.Contains("g3"))
                enPassantSquare = EGTB.a8toa1[46];
            else if (fen.Contains("h3"))
                enPassantSquare = EGTB.a8toa1[47];

            if (fen.Contains("a6"))
                enPassantSquare = EGTB.a8toa1[16];
            else if (fen.Contains("b6"))
                enPassantSquare = EGTB.a8toa1[17];
            else if (fen.Contains("c6"))
                enPassantSquare = EGTB.a8toa1[18];
            else if (fen.Contains("d6"))
                enPassantSquare = EGTB.a8toa1[19];
            else if (fen.Contains("e6"))
                enPassantSquare = EGTB.a8toa1[20];
            else if (fen.Contains("f6"))
                enPassantSquare = EGTB.a8toa1[21];
            else if (fen.Contains("g6"))
                enPassantSquare = EGTB.a8toa1[22];
            else if (fen.Contains("h6"))
                enPassantSquare = EGTB.a8toa1[23];

            foreach (char c in fen)
            {
                if (index < 64 && spc == 0)
                {
                    if (c == '1' && index < 63)
                    {
                        index++;
                    }
                    else if (c == '2' && index < 62)
                    {
                        index += 2;
                    }
                    else if (c == '3' && index < 61)
                    {
                        index += 3;
                    }
                    else if (c == '4' && index < 60)
                    {
                        index += 4;
                    }
                    else if (c == '5' && index < 59)
                    {
                        index += 5;
                    }
                    else if (c == '6' && index < 58)
                    {
                        index += 6;
                    }
                    else if (c == '7' && index < 57)
                    {
                        index += 7;
                    }
                    else if (c == '8' && index < 56)
                    {
                        index += 8;
                    }
                    else if (c == 'P')
                    {
                        whitePieceSquares.Add(EGTB.a8toa1[index]);
                        whiteTypesSquares.Add(EGTB.PAWN);
                        index++;
                    }
                    else if (c == 'N')
                    {
                        whitePieceSquares.Add(EGTB.a8toa1[index]);
                        whiteTypesSquares.Add(EGTB.KNIGHT);
                        index++;
                    }
                    else if (c == 'B')
                    {
                        whitePieceSquares.Add(EGTB.a8toa1[index]);
                        whiteTypesSquares.Add(EGTB.BISHOP);
                        index++;
                    }
                    else if (c == 'R')
                    {
                        whitePieceSquares.Add(EGTB.a8toa1[index]);
                        whiteTypesSquares.Add(EGTB.ROOK);
                        index++;
                    }
                    else if (c == 'Q')
                    {
                        whitePieceSquares.Add(EGTB.a8toa1[index]);
                        whiteTypesSquares.Add(EGTB.QUEEN);
                        index++;
                    }
                    else if (c == 'K')
                    {
                        whitePieceSquares.Add(EGTB.a8toa1[index]);
                        whiteTypesSquares.Add(EGTB.KING);
                        index++;
                    }
                    else if (c == 'p')
                    {
                        blackPieceSquares.Add(EGTB.a8toa1[index]);
                        blackTypesSquares.Add(EGTB.PAWN);
                        index++;
                    }
                    else if (c == 'n')
                    {
                        blackPieceSquares.Add(EGTB.a8toa1[index]);
                        blackTypesSquares.Add(EGTB.KNIGHT);
                        index++;
                    }
                    else if (c == 'b')
                    {
                        blackPieceSquares.Add(EGTB.a8toa1[index]);
                        blackTypesSquares.Add(EGTB.BISHOP);
                        index++;
                    }
                    else if (c == 'r')
                    {
                        blackPieceSquares.Add(EGTB.a8toa1[index]);
                        blackTypesSquares.Add(EGTB.ROOK);
                        index++;
                    }
                    else if (c == 'q')
                    {
                        blackPieceSquares.Add(EGTB.a8toa1[index]);
                        blackTypesSquares.Add(EGTB.QUEEN);
                        index++;
                    }
                    else if (c == 'k')
                    {
                        blackPieceSquares.Add(EGTB.a8toa1[index]);
                        blackTypesSquares.Add(EGTB.KING);
                        index++;
                    }
                    else if (c == '/')
                    {
                        continue;
                    }
                    else if (c == ' ')
                    {
                        spc++;
                    }
                }
                else
                {
                    if ((c == 'w') && (spacers < 2))
                    {
                        whosTurn = (int)PlayerColor.White;
                    }
                    else if ((c == 'b') && (spacers < 2))
                    {
                        whosTurn = (int)PlayerColor.Black;
                    }
                    else if (c == ' ')
                    {
                        spacers++;
                    }
                }
            }
            return true;
        }

        static List<int> whitePieceSquares = new List<int>(whitePieceSquares);
        static List<int> blackPieceSquares = new List<int>();
        static List<int> whiteTypesSquares = new List<int>();
        static List<int> blackTypesSquares = new List<int>();

        static int whosTurn = 0;
        static int enPassantSquare = 0;

        static void ProcessInput(string s)
        {
            if ((s == "quit") || (s == "exit"))
            {
                running = false;
                return;
            }

            if (DidSetupSquaresFromFen(s))
            {
                ProbeResultType pr = EGTB.Probe(whitePieceSquares, blackPieceSquares, whiteTypesSquares, blackTypesSquares, whosTurn, enPassantSquare);
                if (pr.found)
                {
                    if (pr.stm == MateResult.BlackToMate)
                    {
                        Console.WriteLine("Black to mate in {0} plies", pr.ply);
                    }
                    else if (pr.stm == MateResult.WhiteToMate)
                    {
                        Console.WriteLine("White to mate in {0} plies", pr.ply);
                    }
                    else if (pr.stm == MateResult.Draw)
                    {
                        Console.WriteLine("Draw", pr.ply);
                    }
                    else
                    {
                        Console.WriteLine("No match in EGTB", pr.ply);
                    }
                }
                else
                {
                    Console.WriteLine("Could not find it: " + pr.error);
                }
            }
            else
            {
                Console.WriteLine("FEN string not accepted, max 5 man tables allowed for probe");
            }
        }

        static void Main(string[] args)
        {
            EGTB.Init(AppDomain.CurrentDomain.BaseDirectory, new WindowsStreamCreator());

            Console.WriteLine("Gaviota EGTB .NET prober, enter FEN string to probe and hit enter\n");

            while (running)
            {
                ProcessInput(Console.ReadLine());
            }
        }
    }
}
