using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsApp1
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
        FirstFile,
        SecondFile,
        ThirdFile,
        FourthFile,
        FifthFile,
        SixthFile,
        SeventhFile,
        EighthFile
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
    }
}
