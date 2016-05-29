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

            // Can move to it directly?
            if (From.Distance(To) == 1)
                return true;

            // Check whether there is a path via breadth first search.
            // Todo: This is most likley an important AI helper func and might also be needed for visualization.
            var searchQueue = new Queue<HexCoord>();
            var visited = new HashSet<HexCoord>();
            searchQueue.Enqueue(From);
            visited.Add(From);
            while (searchQueue.Count > 0)
            {
                HexCoord current = searchQueue.Dequeue();
                if (current == To)
                    return true;

                // Check neighborhood.
                for (int i = 0; i < 6; ++i)
                {
                    HexCoord targetCoord = current + HexCoord.Directions[i];
                    GameBoard.Field targetField = currentBoard[targetCoord];

                    // Existing field with piece?
                    if (targetField.Type != GameBoard.FieldType.Invalid && targetField.PlayerPiece >= 0)
                    {
                        targetCoord += HexCoord.Directions[i];
                        targetField = currentBoard[targetCoord];
                        if (targetField.Type != GameBoard.FieldType.Invalid && targetField.PlayerPiece < 0)
                        {
                            if (visited.Add(targetCoord))
                                searchQueue.Enqueue(targetCoord);
                        }
                    }
                }
            }

            return false;
        }
    }
}