using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Waypoint = System.Collections.Generic.KeyValuePair<HalmaShared.HexCoord, HalmaShared.GameBoard.WaypointInfo>;

namespace HalmaShared
{
    public static class GameBoardEnumExtensions
    {
        public static bool IsStar(this GameBoard.Configuration config)
        {
            return true;
        }

        public static int NumPlayers(this GameBoard.Configuration config)
        {
            switch (config)
            {
                case GameBoard.Configuration.STAR_2:
                    return 2;
                case GameBoard.Configuration.STAR_3:
                    return 3;
                case GameBoard.Configuration.STAR_4:
                    return 4;
                case GameBoard.Configuration.STAR_6:
                    return 6;
                default:
                    throw new NotImplementedException();
            }
        }

        public static GameBoard.Configuration ConfigFromPlayerCount(int numPlayers)
        {
            switch (numPlayers)
            {
                case 2:
                    return GameBoard.Configuration.STAR_2;
                case 3:
                    return GameBoard.Configuration.STAR_3;
                case 4:
                    return GameBoard.Configuration.STAR_4;
                case 6:
                    return GameBoard.Configuration.STAR_6;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Returns for which player the given FieldType is a goal.
        /// </summary>
        /// <param name="fieldType"></param>
        /// <returns></returns>
        public static int GetPlayerGoal(this GameBoard.FieldType fieldType)
        {
            return (int)fieldType - (int)GameBoard.FieldType.PlayerGoal0;
        }
    }

    public class GameBoard
    {
        public enum Configuration
        {
            /// <summary>
            /// Classic chinese checkers with 2 players.
            /// </summary>
            STAR_2,

            /// <summary>
            /// Classic chinese checkers with 3 players.
            /// </summary>
            STAR_3,

            /// <summary>
            /// Classic chinese checkers with 4 players. Two players paired
            /// </summary>
            STAR_4,
            /// <summary>
            /// Classic chinese checkers with all 6 players.
            /// </summary>
            STAR_6,
        }

        /// <summary>
        /// Map configuration. May be used for specific/optimized drawing and AI.
        /// </summary>
        public Configuration Config { get; private set; }

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

        private readonly Dictionary<HexCoord, Field> fields = new Dictionary<HexCoord, Field>();

        public GameBoard(Configuration config)
        {
            Config = config;

            // Config determines fields per direction.
            int coreSize = 4;
            Field[] dirField;
            switch (Config)
            {
                case Configuration.STAR_2:
                    dirField = new Field[6]
                    {
                        new Field {Type = FieldType.Normal, PlayerPiece = -1},
                        new Field {Type = FieldType.PlayerGoal1, PlayerPiece = 0}, // Top
                        new Field {Type = FieldType.Normal, PlayerPiece = -1},
                        new Field {Type = FieldType.Normal, PlayerPiece = -1},
                        new Field {Type = FieldType.PlayerGoal0, PlayerPiece = 1}, // Bottom
                        new Field {Type = FieldType.Normal, PlayerPiece = -1}
                    };
                    break;

                case Configuration.STAR_3:
                    dirField = new Field[6]
                    {
                        new Field {Type = FieldType.Normal, PlayerPiece = 2},
                        new Field {Type = FieldType.PlayerGoal0, PlayerPiece = -1},
                        new Field {Type = FieldType.Normal, PlayerPiece = 1},
                        new Field {Type = FieldType.PlayerGoal2, PlayerPiece = -1},
                        new Field {Type = FieldType.Normal, PlayerPiece = 0},
                        new Field {Type = FieldType.PlayerGoal1, PlayerPiece = -1}
                    };
                    break;

                case Configuration.STAR_4:
                    dirField = new Field[6]
                    {
                        new Field {Type = FieldType.Normal, PlayerPiece = -1},
                        new Field {Type = FieldType.PlayerGoal0, PlayerPiece = 2},
                        new Field {Type = FieldType.PlayerGoal1, PlayerPiece = 3},
                        new Field {Type = FieldType.Normal, PlayerPiece = -1},
                        new Field {Type = FieldType.PlayerGoal2, PlayerPiece = 0},
                        new Field {Type = FieldType.PlayerGoal3, PlayerPiece = 1}
                    };
                    break;

                case Configuration.STAR_6:
                    dirField = new Field[6]
                    {
                        new Field {Type = FieldType.PlayerGoal4, PlayerPiece = 1},
                        new Field {Type = FieldType.PlayerGoal5, PlayerPiece = 2},
                        new Field {Type = FieldType.PlayerGoal0, PlayerPiece = 3},
                        new Field {Type = FieldType.PlayerGoal1, PlayerPiece = 4},
                        new Field {Type = FieldType.PlayerGoal2, PlayerPiece = 5},
                        new Field {Type = FieldType.PlayerGoal3, PlayerPiece = 0}
                    };
                    break;

                default:
                    throw new NotImplementedException();
            }

            // Mostly empty hexagon core.
            for (int r = -coreSize; r <= coreSize; ++r)
            {
                int endQ = coreSize - Math.Max(r, 0);
                int startQ = -coreSize - Math.Min(r, 0);
                for (int q = startQ; q <= endQ; ++q)
                {
                    var coord = new HexCoord(q, r);
                    var field = new Field { Type = FieldType.Normal, PlayerPiece = -1 };

                    // Extra players in the core vary.
                    // (Code not optimal, but readability is more important for this piece of init code!)
                    if (coord.Length == coreSize)
                    {
                        switch (Config)
                        {
                            case Configuration.STAR_2:
                                if (coord.Z == -coreSize)
                                    field = dirField[(int)HexCoord.Direction.NorthEast];
                                else if (coord.Z == coreSize)
                                    field = dirField[(int)HexCoord.Direction.SouthWest];
                                break;

                            case Configuration.STAR_3:
                                if (coord.Z == coreSize)
                                    field.PlayerPiece = dirField[(int)HexCoord.Direction.SouthWest].PlayerPiece;
                                if (coord.Z == -coreSize)
                                    field.Type = dirField[(int)HexCoord.Direction.NorthEast].Type;
                                if (coord.X == coreSize)
                                    field.PlayerPiece = dirField[(int)HexCoord.Direction.East].PlayerPiece;
                                if (coord.X == -coreSize)
                                    field.Type = dirField[(int)HexCoord.Direction.West].Type;
                                if (coord.Y == coreSize)
                                    field.PlayerPiece = dirField[(int)HexCoord.Direction.NorthWest].PlayerPiece;
                                if (coord.Y == -coreSize)
                                    field.Type = dirField[(int)HexCoord.Direction.SouthEast].Type;
                                break;

                            case Configuration.STAR_4:
                                if (coord.Y != -coord.Z)
                                {
                                    if (coord.Z == -coreSize)
                                        field = dirField[(int)HexCoord.Direction.NorthEast];
                                    else if (coord.Z == coreSize)
                                        field = dirField[(int)HexCoord.Direction.SouthWest];
                                    else if (coord.Y == -coreSize)
                                        field = dirField[(int)HexCoord.Direction.SouthEast];
                                    else if (coord.Y == coreSize)
                                        field = dirField[(int)HexCoord.Direction.NorthWest];
                                }
                                break;

                            case Configuration.STAR_6:
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
                HexCoord dirB = HexCoord.Directions[(dir + 2) % 6];

                HexCoord current = dirA * (coreSize + 1);
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

            // Sanity check for board setup.
#if DEBUG
            int[] playerPieceCount = new int[6];
            int[] playerGoalCount = new int[6];
            foreach (Field f in fields.Values)
            {
                System.Diagnostics.Debug.Assert(f.PlayerPiece < 6);

                if (f.PlayerPiece >= 0)
                    ++playerPieceCount[f.PlayerPiece];
                if (f.Type.GetPlayerGoal() >= 0)
                    ++playerGoalCount[f.Type.GetPlayerGoal()];
            }
            for(int i=0; i<6; ++i)
            {
                System.Diagnostics.Debug.Assert(playerGoalCount[i] == playerPieceCount[i]);
            }
#endif
        }

        /// <summary>
        /// Check weather a given player has won.
        /// </summary>
        /// <returns>True if the current player has won.</returns>
        public bool HasPlayerWon(int currentPlayer)
        {
            foreach (Field field in fields.Values)
            {
                if (field.PlayerPiece == currentPlayer)
                {
                    int playerGoal = field.Type.GetPlayerGoal();
                    if (playerGoal < 0 || playerGoal != currentPlayer)
                    {
                        return false;
                    }
                }
            }

            return true;
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

        public IEnumerable<KeyValuePair<HexCoord, Field>> GetGoalFields()
        {
            return fields;
        }

        #region Path Finding

        public class WaypointInfo
        {
            public WaypointInfo(HexCoord position, WaypointInfo predecessor, int distanceToStart)
            {
                Position = position;
                Predecessor = predecessor;
                DistanceToStart = distanceToStart;
            }

            public void TryUpdatePredecessor(WaypointInfo predecessorCandidate)
            {
                if(predecessorCandidate.DistanceToStart < (Predecessor == null ? Predecessor.DistanceToStart : 0))
                {
                    Predecessor = predecessorCandidate;
                    DistanceToStart = predecessorCandidate.DistanceToStart + 1;
                }
            }

            public HexCoord Position { get; set; }
            public WaypointInfo Predecessor { get; private set; }
            public int DistanceToStart { get; private set; }
        }

        /// <summary>
        /// Lists all fields that would be reachable for a piece starting from given coord.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<HexCoord> GetReachableFields(HexCoord from)
        {
            return GetPossiblePaths(from).Select(x => x.Key);
        }

        public Dictionary<HexCoord, WaypointInfo> GetPossiblePaths(HexCoord from)
        {
            var visited = new Dictionary<HexCoord, WaypointInfo>();

            // Technically not reachable, but we don't want to walk in circles during the search that comes next.
            var startWaypoint = new Waypoint(from, new WaypointInfo(from, null, 0));
            visited.Add(startWaypoint.Key, startWaypoint.Value);

            // Add direct neighbors.
            for (int i = 0; i < 6; ++i)
            {
                HexCoord targetCoord = from + HexCoord.Directions[i];
                Field targetField = this[targetCoord];

                if (targetField.Type != FieldType.Invalid && targetField.PlayerPiece < 0)
                    visited.Add(targetCoord, new WaypointInfo(targetCoord, startWaypoint.Value, 1));
            }

            // Breath first search.
            var searchQueue = new Queue<Waypoint>();
            searchQueue.Enqueue(startWaypoint);
            
            while (searchQueue.Count > 0)
            {
                Waypoint current = searchQueue.Dequeue();

                // Check neighborhood.
                for (int i = 0; i < 6; ++i)
                {
                    HexCoord targetCoord = current.Key + HexCoord.Directions[i];
                    Field targetField = this[targetCoord];

                    // Existing field with piece?
                    if (targetField.Type != FieldType.Invalid && targetField.PlayerPiece >= 0)
                    {
                        targetCoord += HexCoord.Directions[i]; // Jump!
                        targetField = this[targetCoord];
                        if (targetField.Type != FieldType.Invalid && targetField.PlayerPiece < 0)
                        {
                            WaypointInfo oldWaypoint = null;
                            if (visited.TryGetValue(targetCoord, out oldWaypoint))
                                oldWaypoint.TryUpdatePredecessor(current.Value);
                            else
                            {
                                Waypoint newWaypoint = new Waypoint(targetCoord, new WaypointInfo(targetCoord, current.Value, current.Value.DistanceToStart + 1));
                                visited.Add(newWaypoint.Key, newWaypoint.Value);
                                searchQueue.Enqueue(newWaypoint);
                            }
                        }
                    }
                }
            }

            return visited;
        }

        #endregion
    }
}