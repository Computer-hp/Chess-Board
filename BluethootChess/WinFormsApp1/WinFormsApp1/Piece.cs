using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsApp1
{
    public class Piece
    {
        public int x {  get; set; }
        public int y { get; set; }
        public char Name { get; }
        public PieceColor Color { get; }


        public Piece(int x, int y, char Name, PieceColor Color)
        {
            this.x = x;
            this.y = y;
            this.Name = Name;
            this.Color = Color;
        }
    }
}
