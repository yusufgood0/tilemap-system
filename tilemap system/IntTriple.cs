using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tilemap_system
{
    internal class IntTriple
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public IntTriple(int a, int b, int c)
        {
            X = a;
            Y = b;
            Z = c;
        }
    }
}
