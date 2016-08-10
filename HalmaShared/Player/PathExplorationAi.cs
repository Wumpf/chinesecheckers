using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HalmaShared.Player
{
    public class PathExplorationAi : Player
    {
        private HexCoord mostDistantGoal;
        private Turn lastTurn;

        public PathExplorationAi(int playerNumber, GameBoard board) : base(playerNumber)
        {
            // Assuming config where distant are automatically farther away from the center.
            int mostDistant = -1;
            foreach (KeyValuePair<HexCoord, GameBoard.Field> field in board.GetFields())
            {
                if (field.Value.Type.GetPlayerGoal() != playerNumber)
                    continue;
                int distance = field.Key.Length;
                if (distance > mostDistant)
                {
                    mostDistant = distance;
                    mostDistantGoal = field.Key;
                }
            }
        }

        public override void OnTurnStarted(GameBoard currentBoard)
        {
#if CONCURRENT_AI
            System.Threading.Tasks.Task.Run(() => Think(currentBoard));
#else
            Think(currentBoard);
#endif
        }

        private int ComputeScore(HexCoord start, HexCoord end)
        {
            if (start == end)
                return -2;

            // Principle: "Distance reduction to target field with highest distance"
            int distanceBefore = start.Distance(mostDistantGoal);
            int distanceAfter = end.Distance(mostDistantGoal);
            int distanceReduction = distanceBefore - distanceAfter;

            // Prefer pieces that are far away from the goal to start with.
            int score = distanceReduction * 3 + distanceBefore;

            if (lastTurn.To == start && lastTurn.From == end)
                return -1;

            return score;
        }

        private void Think(GameBoard board)
        {
            // Greedy search for best possible move.
            HexCoord movingPiecePos = HexCoord.Zero;
            HexCoord targetPos = HexCoord.Zero;
            int bestScore = int.MinValue;
            foreach (KeyValuePair<HexCoord, GameBoard.Field> startField in board.GetFields())
            {
                if (startField.Value.PlayerPiece != PlayerIndex)
                    continue;

                IEnumerable<HexCoord> reachableFields = board.GetReachableFields(startField.Key);
                foreach(HexCoord targetCoord in reachableFields)
                {
                    int newScore = ComputeScore(startField.Key, targetCoord);
                    if(newScore > bestScore)
                    {
                        movingPiecePos = startField.Key;
                        targetPos = targetCoord;
                        bestScore = newScore;
                    }
                }
            }

            lastTurn = new Turn() { From = movingPiecePos, To = targetPos };

            if (!lastTurn.ValidateAndUpdateTurnSequence(board, PlayerIndex))
                return;

#if CONCURRENT_AI
            Xamarin.Forms.Device.BeginInvokeOnMainThread(() => OnTurnReady(lastTurn));
#else
            OnTurnReady(lastTurn);
#endif
        }

        public override void OnTurnEnded(bool cancel)
        {

        }
    }
}