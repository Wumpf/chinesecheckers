using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace HalmaAndroid
{
    abstract class Player
    {
        protected Player(uint playerNumber)
        {
            PlayerNumber = playerNumber;
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
        public delegate void SuggestTurn(Turn turn);

        /// <summary>
        /// Fired when the player came up with a turn.
        /// </summary>
        public event SuggestTurn OnTurnReady;

        /// <summary>
        /// Signals the player that it's turn has ended.
        /// This usually means that a turn has been accepted.
        /// </summary>
        public abstract void OnTurnEnded();

        /// <summary>
        /// The index of the player in the current game.
        /// </summary>
        public readonly uint PlayerNumber;
    }
}