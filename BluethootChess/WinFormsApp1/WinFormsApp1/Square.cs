using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsApp1
{
    public struct Square
    {
        public int x { get; set; }
        public int y { get; set; }

        public Square(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
