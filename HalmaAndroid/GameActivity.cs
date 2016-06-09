using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace HalmaAndroid
{
    [Activity(Label = "HalmaAndroid", //MainLauncher = true, 
                Icon = "@drawable/icon",
                Theme = "@android:style/Theme.Light.NoTitleBar",
                ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class GameActivity : Activity
    {
        internal GameBoard GameBoard { get; private set; }
        private GameView view;
        private GameInput input;
        private Player[] players;
        public uint CurrentPlayer { get; private set; } = 0;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Decode bundle
            GameBoard.Configuration config;
            Type[] playerTypes;
            MenuActivity.DecodeStartGameInfo(Intent, out config, out playerTypes);

            // Setup main modules.
            GameBoard = new GameBoard(config);
            System.Diagnostics.Debug.Assert(playerTypes.Length == GameBoard.NumPlayers);
            view = new GameView(this, this);
            input = new GameInput(view);
            RequestWindowFeature(WindowFeatures.NoTitle);

            // Set our view from the "main" layout resource
            SetContentView(view);

            // Setup players
            players = new Player[playerTypes.Length];
            for (int i = 0; i < playerTypes.Length; ++i)
            {
                if (playerTypes[i] == typeof(HumanPlayer))
                    players[i] = new HumanPlayer((uint)i, input, view);
                else
                    throw new NotImplementedException();
            }
            StartTurn(0);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                input.Dispose();
                view.Dispose();
            }
            base.Dispose(disposing);
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            return input.OnTouchEvent(e);
        }

        private void StartTurn(uint newCurrentPlayer)
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

            view.HasHighlighted = false;
        }

        private void ExecuteTurn(Turn turn)
        {
            GameBoard.ExecuteTurn(turn);
            players[CurrentPlayer].OnTurnEnded();

            if (GameBoard.HasPlayerWon(CurrentPlayer))
            {
                view.ShowWinningScreen(CurrentPlayer);
            }
            else
            {
                StartTurn((CurrentPlayer + 1) % (uint)players.Length);
            }

            view.Invalidate();
        }
    }
}

