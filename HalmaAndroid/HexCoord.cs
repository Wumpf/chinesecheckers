namespace HalmaAndroid
{
    // Big thanks to Amit Patel's Hexagon page! http://www.redblobgames.com/grids/hexagons/

    struct HexCoord
    {
        public HexCoord(HexCoord coord)
        {
            this.X = coord.X;
            this.Y = coord.Y;
            this.Z = coord.Z;
        }

        /// <summary>
        /// From existing cube coordinates.
        /// x+y+z needs to be equal 0!
        /// </summary>
        public HexCoord(int x, int y, int z)
        {
            System.Diagnostics.Debug.Assert(x + y + z == 0, "Invalid hex coordinate!");

            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        /// <summary>
        /// From axial coordinates.
        /// </summary>
        /// <param name="q">Axis in direction of Direction.East</param>
        /// <param name="r">Axis in direction of Direction.SouthEast</param>
        public HexCoord(int q, int r)
        {
            this.X = q;
            this.Y = -q - r;
            this.Z = r;
        }

        public int X { get; private set; }
        public int Y { get; private set; }
        public int Z { get; private set; }

        #region Math

        public void ToCartesian(out float x, out float y)
        {
            x = (float)System.Math.Sqrt(3) * (this.X + this.Z/2.0f);
            y = 3.0f/2.0f*this.Z;
        }

        public int Distance(HexCoord b)
        {
            return Distance(this, b);
        }

        public static int Distance(HexCoord a, HexCoord b)
        {
            // It's a manhattan distance!
            return (System.Math.Abs(a.X - b.X) + System.Math.Abs(a.Y - b.Y) + System.Math.Abs(a.Z - b.Z)) / 2;
        }

        #endregion

        #region Constants

        /// <summary>
        /// Directions, corresponding to the static Directions array.
        /// Naming assumes "pointy topped" hexagons and has postive rotation.
        /// </summary>
        enum Direction
        {
            East,
            NorthEast,
            NorthWest,
            West,
            SouthWest,
            SouthEast,
        };

        public static readonly HexCoord[] Directions = new HexCoord[]
        {
            new HexCoord(+1, -1, 0),
            new HexCoord(+1, 0, -1),
            new HexCoord(0, +1, -1),
            new HexCoord(-1, +1, 0),
            new HexCoord(-1, 0, +1),
            new HexCoord(0, -1, +1)
        };

        public static readonly HexCoord Zero = new HexCoord(0, 0, 0);

        #endregion

        public override int GetHashCode()
        {
            // Axial coordinates are sufficient.
            return X ^ Z;
        }
    }
}