using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace HalmaAndroid
{
    [Activity(Label = "HalmaAndroid", MainLauncher = true, Icon = "@drawable/icon", Theme = "@android:style/Theme.Light.NoTitleBar")]
    public class GameActivity : Activity
    {
        private GameBoard gameBoard;
        private Player[] players;
        private uint currentPlayer = 0;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            gameBoard = new GameBoard();
            var view = new GameView(this, gameBoard);
            RequestWindowFeature(WindowFeatures.NoTitle);

            // Set our view from the "main" layout resource
            SetContentView(view);

            // Setup players.
            players = new Player[] { new HumanPlayer(0, view), new HumanPlayer(1, view) };
            setCurrentPlayer(0);
        }

        private void setCurrentPlayer(uint newCurrentPlayer)
        {
            currentPlayer = newCurrentPlayer;
            players[currentPlayer].OnTurnReady += OnPlayerTurnReady;
            players[currentPlayer].OnTurnStarted(gameBoard);

            // todo: trigger visualization/feedback
        }

        private void OnPlayerTurnReady(Turn turn)
        {
            if(turn.IsValidTurn(gameBoard, currentPlayer))
            {
                players[currentPlayer].OnTurnReady -= OnPlayerTurnReady;

                ExecuteTurn(turn);

                if (CheckCurrentPlayerHasWon())
                {
                    // todo: Winning.
                }
                else
                {
                    setCurrentPlayer((currentPlayer + 1) % (uint)players.Length);
                }
            }
            else
            {
                // todo: Unallowed turn.
            }
        }

        private void ExecuteTurn(Turn turn)
        {
            // todo.
        }

        /// <summary>
        /// Check weather the current player has won.
        /// </summary>
        /// <returns>True if the current player has won.</returns>
        private bool CheckCurrentPlayerHasWon()
        {
            // todo
            return false;
        }
    }
}

