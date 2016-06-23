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
        /// Constructs a new match.
        /// Will intantiate players with the given types.
        /// Takes ownership of gameBoard, view and input.
        /// </summary>
        public Match(Type[] playerTypes, GameBoard gameBoard, MatchView view, MatchInput input)
        {
            System.Diagnostics.Debug.Assert(gameBoard != null && view != null && input != null);
            System.Diagnostics.Debug.Assert(gameBoard.NumPlayers == playerTypes.Length, "There are not enought players for the current game board configuration.");

            this.GameBoard = gameBoard;
            this.View = view;
            this.Input = input;

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


        //public override bool OnTouchEvent(MotionEvent e)
        //{
        //    return input.OnTouchEvent(e);
        //}

        private void StartTurn(int newCurrentPlayer)
        {
            // todo: Check for the super rare possibility, that the player cannot move at all

            CurrentPlayer = newCurrentPlayer;
            players[CurrentPlayer].TurnReady += OnPlayerTurnReady;
            players[CurrentPlayer].OnTurnStarted(GameBoard);

            View.TransitionToPlayer(CurrentPlayer);

            // todo: trigger visualization/feedback
        }

        private void OnPlayerTurnReady(Turn turn)
        {
            if (turn.ValidateAndUpdateTurnSequence(GameBoard, CurrentPlayer))
            {
                players[CurrentPlayer].TurnReady -= OnPlayerTurnReady;

                GameBoard.ExecuteTurn(turn);

                View.TurnAnimationFinished += OnTurnAnimationFinished;
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
            View.TurnAnimationFinished -= OnTurnAnimationFinished;

            players[CurrentPlayer].OnTurnEnded();

            if (GameBoard.HasPlayerWon(CurrentPlayer))
            {
                View.ShowWinningScreen(CurrentPlayer);
            }
            else
            {
                StartTurn((CurrentPlayer + 1) % players.Length);
            }
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

