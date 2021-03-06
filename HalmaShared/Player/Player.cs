namespace HalmaShared.Player
{
    public abstract class Player
    {
        protected Player(int playerNumber)
        {
            PlayerIndex = playerNumber;
        }

        /// <summary>
        /// Signals the player that it's turn has started.
        /// The player may trigger the OnTurnReady event immediately, or delayed (due to threading or waiting for userinput).
        /// </summary
        /// <param name="currentBoard">Current state of the game board.</param>
        public abstract void OnTurnStarted(GameBoard currentBoard);

        /// <summary>
        /// Delegate for OnTurnReady event.
        /// </summary>
        /// <param name="turn">Turn the player came up with. Needs to be validated!</param>
        public delegate void TurnSuggestionHandler(Turn turn);

        /// <summary>
        /// Fired when the player came up with a turn.
        /// </summary>
        public event TurnSuggestionHandler TurnReady;

        protected void OnTurnReady(Turn turn)
        {
            TurnSuggestionHandler handler = TurnReady;
            if (handler != null)
            {
                handler(turn);
            }
        }

        /// <summary>
        /// Signals the player that it's turn has ended.
        /// This usually means that a turn has been accepted.
        /// <param name="cancel">True if the turn was canceled since the user wants to undo the previous turn.</param>
        /// </summary>
        public abstract void OnTurnEnded(bool cancel);

        /// <summary>
        /// The index of the player in the current game.
        /// </summary>
        public readonly int PlayerIndex;
    }
}