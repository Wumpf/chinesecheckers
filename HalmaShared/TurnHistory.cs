using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalmaShared
{
    public class TurnHistory
    {
        private List<Turn> recordedTurns = new List<Turn>();
        private int gameBoardState = -1;
        private bool undoingOrRedoing = false;

        private GameBoard gameBoard;

        public TurnHistory(GameBoard gameBoard)
        {
            this.gameBoard = gameBoard;
        }


        public bool CanUndo()
        {
            return gameBoardState >= 0;
        }

        /// <summary>
        /// Undos last turn is possible
        /// </summary>
        /// <returns>Turn that was used to perform the undoing on the board.</returns>
        public Turn Undo()
        {
            if (!CanUndo())
                throw new Exception("Cannot undo at the moment!");

            undoingOrRedoing = true;
            var undoTurn = recordedTurns[gameBoardState].CreateReverseTurn();
            undoTurn.ValidateAndUpdateTurnSequence(gameBoard);
            gameBoard.ExecuteTurn(undoTurn);
            --gameBoardState;
            undoingOrRedoing = false;

            return undoTurn;
        }

        public bool CanRedo()
        {
            return gameBoardState < recordedTurns.Count - 1;
        }

        /// <summary>
        /// Redos last turn is possible
        /// </summary>
        /// <returns>Turn that was used to perform the redoing on the board.</returns>
        public Turn Redo()
        {
            if (!CanRedo())
                throw new Exception("Cannot redo at the moment!");

            undoingOrRedoing = true;
            ++gameBoardState;
            var redoTurn = recordedTurns[gameBoardState];
            gameBoard.ExecuteTurn(redoTurn);
            undoingOrRedoing = false;

            return redoTurn;
        }

        public void RecordTurn(Turn turn)
        {
            if (undoingOrRedoing)
                return;

            // History cut.
            if(CanRedo())
                recordedTurns.RemoveRange(gameBoardState + 1, recordedTurns.Count - gameBoardState - 1);
            recordedTurns.Add(turn);
            ++gameBoardState;
        }
    }
}
