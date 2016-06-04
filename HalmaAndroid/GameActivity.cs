using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace HalmaAndroid
{
    [Activity(Label = "HalmaAndroid", MainLauncher = true, Icon = "@drawable/icon", Theme = "@android:style/Theme.Light.NoTitleBar",
                ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class GameActivity : Activity
    {
        internal GameBoard GameBoard { get; private set; }
        private GameView gameView;
        private Player[] players;
        public uint CurrentPlayer { get; private set; } = 0;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            GameBoard = new GameBoard();
            gameView = new GameView(this, this);
            RequestWindowFeature(WindowFeatures.NoTitle);

            // Set our view from the "main" layout resource
            SetContentView(gameView);

            // Setup players.
            // For now only pescy humans.
            players = new Player[GameBoard.NumPlayers];
            for (int i = 0; i < players.Length; ++i)
                players[i] = new HumanPlayer((uint)i, gameView);
            startTurn(0);
        }

        private void startTurn(uint newCurrentPlayer)
        {
            CurrentPlayer = newCurrentPlayer;
            players[CurrentPlayer].TurnReady += OnPlayerTurnReady;
            players[CurrentPlayer].OnTurnStarted(GameBoard);

            // todo: trigger visualization/feedback
        }

        private void OnPlayerTurnReady(Turn turn)
        {
            if(turn.IsValidTurn(GameBoard, CurrentPlayer))
            {
                players[CurrentPlayer].TurnReady -= OnPlayerTurnReady;

                ExecuteTurn(turn);
            }
            else
            {
                // todo: Unallowed turn.
            }

            gameView.HasHighlighted = false;
        }

        private void ExecuteTurn(Turn turn)
        {
            GameBoard.ExecuteTurn(turn);
            players[CurrentPlayer].OnTurnEnded();

            if (GameBoard.HasPlayerWon(CurrentPlayer))
            {
                gameView.ShowWinningScreen(CurrentPlayer);
            }
            else
            {
                startTurn((CurrentPlayer + 1) % (uint)players.Length);
            }

            gameView.Invalidate();
        }
    }
}

