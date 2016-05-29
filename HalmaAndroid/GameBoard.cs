using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HalmaAndroid
{
    class GameBoard
    {
        public enum Configuration
        {
            /// <summary>
            /// Classic chineese checkers with 2 players.
            /// </summary>
            STAR_2,
        }
        /// <summary>
        /// Map configuration. May be used for specific/optimized drawing and AI.
        /// </summary>
        public Configuration Config { get; private set; }

        public uint NumPlayers
        {
            get
            {
                switch (Config)
                {
                    case Configuration.STAR_2:
                        return 2;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public enum FieldType
        {
            Invalid = -2,
            Normal = -1,
            PlayerGoal0,
            PlayerGoal1,
            PlayerGoal2,
            PlayerGoal3,
            PlayerGoal4,
            PlayerGoal5,
        }

        public struct Field
        {
            /// <summary>
            /// Index of the player who has a piece on the field.
            /// </summary>
            public int PlayerPiece;

            /// <summary>
            /// Type of the field.
            /// </summary>
            public FieldType Type;
        }

        /// <summary>
        /// Returns for which player the given FieldType is a goal.
        /// </summary>
        /// <param name="fieldType"></param>
        /// <returns></returns>
        public static int GetPlayerGoal(FieldType fieldType)
        {
            return (int) fieldType - (int) GameBoard.FieldType.PlayerGoal0;
        }

        private readonly Dictionary<HexCoord, Field> fields = new Dictionary<HexCoord, Field>();

        public GameBoard()
        {
            Config = Configuration.STAR_2;

            // Config
            int coreSize = 4;
            Field[] dirField;
            bool largePikes = true;
            switch (Config)
            {
                case Configuration.STAR_2:
                    dirField = new Field[6]
                    {
                        new Field {Type = FieldType.Normal, PlayerPiece = -1},
                        new Field {Type = FieldType.PlayerGoal1, PlayerPiece = 0},
                        new Field {Type = FieldType.Normal, PlayerPiece = -1},
                        new Field {Type = FieldType.Normal, PlayerPiece = -1},
                        new Field {Type = FieldType.PlayerGoal0, PlayerPiece = 1},
                        new Field {Type = FieldType.Normal, PlayerPiece = -1}
                    };
                    break;

                // all players
                //const int coreSize = 4;
                //FieldType[] dirFieldTypeType = new FieldType[6] {FieldType.PlayerGoal2, FieldType.PlayerGoal3, FieldType.PlayerGoal4, FieldType.PlayerGoal5, FieldType.PlayerGoal0, FieldType.PlayerGoal1 };

                default:
                    throw new NotImplementedException();
            }

            // Basic star config.
            // Empty core.
            for (int r=-coreSize; r<= coreSize; ++r)
            {
                int endQ = coreSize - Math.Max(r, 0);
                int startQ = -coreSize - Math.Min(r, 0);
                for (int q = startQ; q <= endQ; ++q)
                {
                    var coord = new HexCoord(q, r);
                    var field = new Field {Type = FieldType.Normal, PlayerPiece = -1};

                    // Extra players in the core vary.
                    // (Code not optimal, but readability is more important for this piece of init code!)
                    if (coord.Length() == coreSize)
                    {
                        switch (Config)
                        {
                            case Configuration.STAR_2:
                                if (coord.Z == -coreSize)
                                    field = dirField[(int) HexCoord.Direction.NorthEast];
                                else if (coord.Z == coreSize)
                                    field = dirField[(int) HexCoord.Direction.SouthWest];
                                break;

                            default:
                                throw new NotImplementedException();
                        }
                    }

                    fields.Add(coord, field);
                }
            }
            // Pikes
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
                        fields.Add(current, dirField[dir]);
                    }
                    current = backup + dirA + dirB;
                }
            }
        }

        /// <summary>
        /// Executes a given turn. Attention: Will execute even if the turn is illegal or meaningless.1
        /// </summary>
        public void ExecuteTurn(Turn turn)
        {
            var fromBefore = this[turn.From];
            var toBefore = this[turn.To];

            this[turn.From] = new Field() { Type = fromBefore.Type, PlayerPiece = toBefore.PlayerPiece };
            this[turn.To] = new Field() { Type = toBefore.Type, PlayerPiece = fromBefore.PlayerPiece };
        }

        /// <summary>
        /// FieldType access.
        /// </summary>
        /// <returns>Get: If coord does not exist, FieldType.Invalid is returned.</returns>
        public Field this[HexCoord hexcoord]
        {
            get
            {
                Field outField = new Field() {Type = FieldType.Invalid, PlayerPiece = -1};
                fields.TryGetValue(hexcoord, out outField);
                return outField;
            }
            private set
            {
                System.Diagnostics.Debug.Assert(fields.ContainsKey(hexcoord), "There is no FieldType at given coordinate!");
                fields[hexcoord] = value;
            }
        }

        public IEnumerable<KeyValuePair<HexCoord, Field>> GetFields()
        {
            return fields;
        }
    }
}