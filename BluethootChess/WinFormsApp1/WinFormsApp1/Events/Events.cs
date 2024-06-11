using ChessLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Events
{
    public class CheckmateEventArgs : EventArgs
    {
        public readonly string Winner;// { get; }


        public CheckmateEventArgs(string winner)
        {
            Winner = winner;
        }
    }


    public class CastleEventArgs : EventArgs
    {
        public readonly (int x, int y) KingDestination;
        public readonly PieceColor Color; 


        public CastleEventArgs((int x, int y) kingDestination, PieceColor color)
        {
            KingDestination = kingDestination;
            Color = color;
        }
    }


    public class UIPieceMovementEventArgs : EventArgs
    {
        public readonly (int x, int y) DestButtonTag;


        public UIPieceMovementEventArgs((int x, int y) originSquare)
        {
            DestButtonTag = originSquare;
        }
    }


    public class UIClockTickEventArgs : EventArgs
    {
        // public bool IsFirstMovePlayed { get; set; }
        public readonly int BlackOrWhiteClock;


        public UIClockTickEventArgs(int blackOrWhiteClock)
        {
            BlackOrWhiteClock = blackOrWhiteClock;
        }
    }
}
