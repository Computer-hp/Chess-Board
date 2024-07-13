using ChessLogic;
using WindowHelper;

namespace Events
{
    public class GameFinishEventArgs : EventArgs
    {
        public readonly int LastTickingTimerIdx;
        public readonly string Winner;


        public GameFinishEventArgs(int lastTickingTimerIdx, string winner)
        {
            LastTickingTimerIdx = lastTickingTimerIdx;
            Winner = winner;
        }
    }


    public class CastleEventArgs : EventArgs
    {
        public readonly (int x, int y) KingDestination;
        public readonly (int x, int y) RookDestination;
        public readonly PieceColor Color; 


        public CastleEventArgs((int x, int y) kingDestination, (int x, int y) rookDestination, PieceColor color)
        {
            KingDestination = kingDestination;
            RookDestination = rookDestination;
            Color = color;
        }
    }


    public class UIPieceMovementEventArgs : EventArgs
    {
        public readonly (int x, int y) DestButtonTag;
        public readonly Bitmap ButtonImage;


        public UIPieceMovementEventArgs((int x, int y) originSquare, PieceColor color, char pieceName)
        {
            DestButtonTag = originSquare;
            ButtonImage = PieceImages.GetPieceImage(color, pieceName);
        }
    }


    public class UIClockTickEventArgs : EventArgs
    {
        public readonly int BlackOrWhiteClock;

        public UIClockTickEventArgs(int blackOrWhiteClock)
        {
            BlackOrWhiteClock = blackOrWhiteClock;
        }
    }

    public delegate char ManageUIPromotion(int turn, (int x, int y) buttonPosition);
}
