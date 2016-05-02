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

        private readonly Dictionary<HexCoord, Field> fields = new Dictionary<HexCoord, Field>();

        public GameBoard()
        {
            for (int q = -5; q < 8; q++)
            {
                for (int r = 0; r < 10; r++)
                {
                    this[new HexCoord(q, r)] = Field.Normal;
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
                if (fields.ContainsKey(hexcoord))
                {
                    fields.Add(hexcoord, value);
                }
                else
                {
                    fields[hexcoord] = value;
                }
            }
        }

        public IEnumerable<KeyValuePair<HexCoord, Field>> GetFields()
        {
            return fields;
        }
    }
}