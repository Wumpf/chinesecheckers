using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HalmaAndroid
{
    class GameBoard
    {
        public enum Field
        {
            Invalid = -1,
            Normal = 0,
            Player0,
            Player1,
            Player2,
            Player3,
            Player4,
            Player5,
        }

        public static int GetPlayerNumber(Field field)
        {
            return (int) field - (int) GameBoard.Field.Player0;
        }

        private readonly Dictionary<HexCoord, Field> fields = new Dictionary<HexCoord, Field>();

        public GameBoard()
        {
            // Basic star config.
            // Empty core.
            const int coreSize = 4;
            for(int r=-coreSize; r<= coreSize; ++r)
            {
                int endQ = coreSize - Math.Max(r, 0);
                int startQ = -coreSize - Math.Min(r, 0);
                for (int q = startQ; q <= endQ; ++q)
                {
                    fields.Add(new HexCoord(q, r), Field.Normal);
                }
            }
            // Pikes
            //Field[] dirFieldType = new Field[6] {Field.Normal, Field.Player1, Field.Normal, Field.Normal, Field.Player0, Field.Normal }; // two players
            Field[] dirFieldType = new Field[6] {Field.Player2, Field.Player3, Field.Player4, Field.Player5, Field.Player0, Field.Player1 }; // all players
            for (int dir = 0; dir < 6; ++dir)
            {
                HexCoord dirA = HexCoord.Directions[dir];
                HexCoord dirB = HexCoord.Directions[(dir+2) % 6];

                HexCoord current = dirA*(coreSize+1);
                for (int a = 0; a <= coreSize; ++a)
                {
                    HexCoord backup = current;
                    for (int b = 0; b < coreSize - a; ++b)
                    {
                        current += dirB;
                        fields.Add(current, dirFieldType[dir]);
                    }
                    current = backup + dirA + dirB;
                }
            }

        }

        /// <summary>
        /// Field access.
        /// </summary>
        /// <returns>Get: If coord does not exist, Field.Invalid is returned.</returns>
        public Field this[HexCoord hexcoord]
        {
            get
            {
                Field outField = Field.Invalid;
                fields.TryGetValue(hexcoord, out outField);
                return outField;
            }
            private set
            {
                System.Diagnostics.Debug.Assert(fields.ContainsKey(hexcoord), "There is no field at given coordinate!");
                fields[hexcoord] = value;
            }
        }

        public IEnumerable<KeyValuePair<HexCoord, Field>> GetFields()
        {
            return fields;
        }
    }
}