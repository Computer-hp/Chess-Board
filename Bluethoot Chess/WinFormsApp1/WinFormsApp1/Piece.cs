using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsApp1
{
    public class CPiece
    {
        public int x {  get; set; }
        public int y { get; set; }

        public string pieceName { get; set; }
        public string pieceType { get; set; }

        public CPiece(int x, int y, string pieceName, string pieceType)
        {
            this.x = x;
            this.y = y;
            this.pieceName = pieceName;
            this.pieceType = pieceType;
        }
    }
}
