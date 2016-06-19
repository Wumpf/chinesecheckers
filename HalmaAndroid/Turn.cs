using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HalmaAndroid
{
    struct Turn
    {
        public HexCoord From
        {
            get { return from; }
            set
            {
                from = value;
                TurnSequence = null;
            }
        }
        public HexCoord from;

        public HexCoord To
        {
            get { return to; }
            set
            {
                to = value;
                TurnSequence = null;
            }
        }
        private HexCoord to;

        /// <summary>
        /// List of jumps necessary to perform this turn.
        /// </summary>
        /// <see cref="ValidateAndUpdateTurnSequence(GameBoard, uint)"/>
        public List<HexCoord> TurnSequence { get; private set; }

        /// <summary>
        /// Validates the turn and updates TurnSequence.
        /// </summary>
        /// <param name="currentBoard">Game board state on which the turn is checked</param>
        /// <param name="player">Player performing the turn.</param>
        /// <returns>False if the turn is not valid.</returns>
        public bool ValidateAndUpdateTurnSequence(GameBoard currentBoard, uint player)
        {
            // No actual move.
            if (From == To)
                return false;

            // One of the fields is invalid.
            if (currentBoard[From].Type == GameBoard.FieldType.Invalid || currentBoard[To].Type == GameBoard.FieldType.Invalid)
                return false;

            // Not moving own piece.
            if (currentBoard[From].PlayerPiece != player)
                return false;

            // Target not free.
            if (currentBoard[To].PlayerPiece >= 0)
                return false;

            // Actual check.
            Dictionary<HexCoord, GameBoard.WaypointInfo> reachableFields = currentBoard.GetPossiblePaths(From);

            GameBoard.WaypointInfo endWaypoint;
            if (reachableFields.TryGetValue(To, out endWaypoint))
            {
                TurnSequence = new List<HexCoord>();
                do
                {
                    TurnSequence.Add(endWaypoint.Position);
                    endWaypoint = endWaypoint.Predecessor;
                } while (endWaypoint != null);

                TurnSequence.Reverse();

                return true;
            }
            else
            {
                TurnSequence = null;
                return false;
            }
        }
    }
}