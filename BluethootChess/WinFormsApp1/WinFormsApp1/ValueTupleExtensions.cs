using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsApp1
{
    public static class ValueTupleExtensions
    {
        public static int GetX(this (int, int) tuple) => tuple.Item1;
        public static int GetY(this (int, int) tuple) => tuple.Item2;
    }
}
