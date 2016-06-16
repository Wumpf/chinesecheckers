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

        static public HexCoord FromXY(int x, int y)
        {
            return new HexCoord(x, y, -x - y);
        }
        static public HexCoord FromXZ(int x, int z)
        {
            return new HexCoord(x, -x - z, z);
        }
        static public HexCoord FromYZ(int y, int z)
        {
            return new HexCoord(-y - z, y, z);
        }

        public int X { get; private set; }
        public int Y { get; private set; }
        public int Z { get; private set; }

        #region Math & Operators

        public int this[int component]
        {
            get
            {
                switch (component)
                {
                    case 0:
                        return X;
                    case 1:
                        return Y;
                    case 2:
                        return Z;
                    default:
                        throw new System.ArgumentException("Hexcoord component must be between 0 and 2");
                }
            }
            set
            {
                switch (component)
                {
                    case 0:
                        X = value;
                        break;
                    case 1:
                        Y = value;
                        break;
                    case 2:
                        Z = value;
                        break;
                    default:
                        throw new System.ArgumentException("Hexcoord component must be between 0 and 2");
                }
            }
        }


        public static HexCoord operator *(HexCoord coord, int factor)
        {
            return new HexCoord(coord.X * factor, coord.Y * factor, coord.Z * factor);
        }

        public static HexCoord operator +(HexCoord a, HexCoord b)
        {
            return new HexCoord(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static HexCoord operator -(HexCoord a, HexCoord b)
        {
            return new HexCoord(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static bool operator ==(HexCoord a, HexCoord b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        }

        public static bool operator !=(HexCoord a, HexCoord b)
        {
            return a.X != b.X || a.Y != b.Y || a.Z != b.Z;
        }

        public override bool Equals(object obj)
        {
            HexCoord? b = obj as HexCoord?;
            return b.HasValue ? b.Value == this : false;
        }

        public void ToCartesian(out float x, out float y)
        {
            x = (float)System.Math.Sqrt(3) * (this.X + this.Z / 2.0f);
            y = 3.0f / 2.0f * this.Z;
        }

        public int Length
        {
            get
            {
                {
                    return (System.Math.Abs(X) + System.Math.Abs(Y) + System.Math.Abs(Z)) / 2;
                }
            }
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
        public enum Direction
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