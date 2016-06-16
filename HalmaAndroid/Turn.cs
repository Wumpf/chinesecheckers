using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HalmaAndroid
{
    struct Turn
    {
        public HexCoord From;
        public HexCoord To;

        /// <summary>
        /// Checks whether the given turn is valid.
        /// </summary>
        /// <param name="currentBoard">Game board state on which the turn is checked</param>
        /// <param name="player">Player performing the turn.</param>
        /// <returns></returns>
        public bool IsValidTurn(GameBoard currentBoard, uint player)
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
            return currentBoard.GetReachableFields(From).Contains(To);
        }
    }
}