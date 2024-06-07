using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessGame
{
    public enum PieceColor
    {
        White,
        Black
    };


    public enum Ranks
    {
        EighthRank,
        SeventhRank,
        SixthRank,
        FifthRank,
        FourthRank,
        ThirdRank,
        SecondRank,
        FirstRank
    };


    public enum Files
    {
        aFile,
        bFile,
        cFile,
        dFile,
        eFile,
        fFile,
        gFile,
        hFile
    };


    public enum Directions
    {
        Left,
        Right,
        Up,
        Down,
        LeftDown,
        LeftUp,
        RightDown,
        RightUp,
    };
}
