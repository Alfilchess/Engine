using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Pwned.Chess.Common
{
    //public class XNAStreamCreator : IStreamCreator
    //{
    //    public Stream OpenStream(string path)
    //    {
    //        return TitleContainer.OpenStream(path);
    //    }
    //}

    //public class WindowsStreamCreator : IStreamCreator
    //{
    //    public Stream OpenStream(string path)
    //    {
    //        return new FileStream(path, FileMode.Open);
    //    }
    //}

    public interface IStreamCreator
    {
        Stream OpenStream(string path);
    }

    public enum PlayerColor
    {
        White,
        Black,
        All
    }

    public enum PieceType
    {
        King,
        Queen,
        Rook,
        Bishop,
        Knight,
        Pawn,
        All
    }
}
