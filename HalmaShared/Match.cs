using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalmaShared
{
    /// <summary>
    /// Holds (but does not create) the different data components, the view and input classes for a match and wires up their interaction.
    /// </summary>
    public class Match : IDisposable
    {
        public GameBoard GameBoard { get; private set; }
        public MatchView View { get; private set; }
        public MatchInput Input { get; private set; }

        private Player.Player[] players;

        public int CurrentPlayer { get; private set; } = 0;

        /// <summary>
        /// Workaround for block Ai movements during undo/redo
        /// </summary>
        private bool undoRedoAiLock = false;

        /// <summary>
        /// Constructs a new match.
        /// Will intantiate players with the given types.
        /// Takes ownership of gameBoard, view and input.
        /// </summary>
        public Match(Type[] playerTypes, GameBoard gameBoard, MatchView view, MatchInput input)
        {
            System.Diagnostics.Debug.Assert(gameBoard != null && view != null && input != null);
            System.Diagnostics.Debug.Assert(gameBoard.Config.NumPlayers() == playerTypes.Length, "There are not enought players for the current game board configuration.");

            this.GameBoard = gameBoard;
            this.View = view;
            this.Input = input;

            Input.FieldTouched += OnFieldTouched;
            View.TurnAnimationFinished += OnTurnAnimationFinished;

            // Setup players
            players = new Player.Player[playerTypes.Length];
            for (int i = 0; i < playerTypes.Length; ++i)
            {
                if (playerTypes[i] == typeof(Player.HumanPlayer))
                    players[i] = new Player.HumanPlayer(i, input, view);
                else if (playerTypes[i] == typeof(Player.PathExplorationAi))
                    players[i] = new Player.PathExplorationAi(i, gameBoard);
                else
                    throw new NotImplementedException();
            }
            StartTurn(0);
        }

        private void OnFieldTouched(MatchInput.TouchResultType resultType, HexCoord hexcoord)
        {
            // Ignore all inputs if animating at the moment.
            if (View.IsAnimating)
                return;

            if (resultType == MatchInput.TouchResultType.Undo)
            {
                // Cancel current turn but don't start the next one until there has been an input.
                if(!undoRedoAiLock)
                {
                    undoRedoAiLock = true;
                    players[CurrentPlayer].OnTurnEnded(true);
                }

                CurrentPlayer = (CurrentPlayer == 0 ? players.Length : CurrentPlayer) - 1;
                View.TransitionToPlayer(CurrentPlayer);
                View.AnimateTurn(GameBoard.TurnHistory.Undo(), CurrentPlayer);
            }
            else if (resultType == MatchInput.TouchResultType.Redo)
            {
                // Cancel current turn but don't start the next one until there has been an input.
                if (!undoRedoAiLock)
                {
                    undoRedoAiLock = true;
                    players[CurrentPlayer].OnTurnEnded(true);
                }

                View.AnimateTurn(GameBoard.TurnHistory.Redo(), CurrentPlayer);
                CurrentPlayer = (CurrentPlayer + 1) % players.Length;
            }

            // Lift ai undo redo block if there is a touch event other than undo/redo.
            else if(undoRedoAiLock)
            {
                undoRedoAiLock = false;
                StartTurn(CurrentPlayer);
            }
        }

        private void StartTurn(int newCurrentPlayer)
        {
            // todo: Check for the super rare possibility, that the player cannot move at all

            CurrentPlayer = newCurrentPlayer;
            players[CurrentPlayer].TurnReady += OnPlayerTurnReady;
            players[CurrentPlayer].OnTurnStarted(GameBoard);

            // todo: trigger visualization/feedback
        }

        private void OnPlayerTurnReady(Turn turn)
        {
            if (turn.ValidateAndUpdateTurnSequence(GameBoard, CurrentPlayer))
            {
                players[CurrentPlayer].TurnReady -= OnPlayerTurnReady;

                GameBoard.ExecuteTurn(turn);

                View.AnimateTurn(turn, CurrentPlayer);
            }
            else
            {
                // todo: Unallowed turn.
            }

            View.HasHighlighted = false;
        }

        private void OnTurnAnimationFinished()
        {
            if (!undoRedoAiLock)
            {
                players[CurrentPlayer].OnTurnEnded(false);

                if (GameBoard.HasPlayerWon(CurrentPlayer))
                {
                    View.ShowWinningScreen(CurrentPlayer);
                }
                else
                {
                    StartTurn((CurrentPlayer + 1) % players.Length);
                }
            }

            // If the new player is human player, lift undo redo block and start the player's turn to allow input as usual.
            else if(players[CurrentPlayer].GetType() == typeof(Player.HumanPlayer))
            {
                undoRedoAiLock = false;
                StartTurn(CurrentPlayer);
            }

            View.TransitionToPlayer(CurrentPlayer);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    View.Dispose();
                    Input.Dispose();
                }

                disposedValue = true;
            }
        }

         ~Match()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}

