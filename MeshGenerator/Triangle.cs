namespace MeshHelpers
{
    /// <summary>
    /// A struct to hold three indices corresponding to points in space. The Triangle does not keep track of what points
    /// these indices refer to.
    /// </summary>
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

        /// <summary>
        /// Does the triangle contain this index?
        /// </summary>
        /// <param name="vertex">The index corresponding to the point. Not the point itself.</param>
        /// <returns>Returns whether this triangle contains the index.</returns>
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

        /// <summary>
        /// Gets the first point not equal to the arguments. Does not check that the indices passed in are actually in the 
        /// triangle.
        /// </summary>
        /// <param name="indexOne">An index already in the triangle.</param>
        /// <param name="indexTwo">Another index already in the triangle.</param>
        /// <returns>Returns the first index not equal to either of the arguments.</returns>
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
