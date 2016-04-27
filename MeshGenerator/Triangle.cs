using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MeshHelpers
{
    struct Triangle
    {
        public int a;
        public int b;
        public int c;

        public Triangle(int a, int b, int c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }

        public bool Contains(int vertex)
        {
            return (vertex == a) || (vertex == b) || (vertex == c);
        }

        public int this[int i]
        {
            get
            {
                if (i == 0)
                    return a;
                else if (i == 1)
                    return b;
                else if (i == 2)
                    return c;
                else
                    throw new System.ArgumentOutOfRangeException();
            }
        }

        public int GetThirdPoint(int indexOne, int indexTwo)
        {
            for (int i = 0; i < 3; i++)
            {
                if (this[i] != indexOne && this[i] != indexTwo)
                {
                    return this[i];
                }
            }
            return -1;
        }
    }
}
