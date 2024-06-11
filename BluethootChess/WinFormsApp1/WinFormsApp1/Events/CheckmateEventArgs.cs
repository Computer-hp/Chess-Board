using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Events
{
    public class CheckmateEventArgs : EventArgs
    {
        public string Winner { get; set; }

        public CheckmateEventArgs(string winner)
        {
            Winner = winner;
        }
    }


    public class CastleEventArgs : EventArgs
    {
        public (int x, int y) KingDestination { get; set; }
        public (int x, int y) RookDestination { get; set; }


        public CastleEventArgs((int x, int y) kingDestination, (int x, int y) rookDestination)
        {
            KingDestination = kingDestination;
            RookDestination = rookDestination;
        }
    }
}
