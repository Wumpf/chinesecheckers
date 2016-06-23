using Android.App;
using Android.OS;
using Android.Views;
using HalmaShared;
using System;

namespace HalmaAndroid
{
    [Activity(Label = "HalmaAndroid", //MainLauncher = true, 
                Icon = "@drawable/icon",
                Theme = "@android:style/Theme.Light.NoTitleBar",
                ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class GameActivity : Activity
    {
        private HalmaShared.Match match;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Decode bundle
            GameBoard.Configuration config;
            Type[] playerTypes;
            MenuActivity.DecodeStartGameInfo(Intent, out config, out playerTypes);

            // Setup main modules.
            GameBoard gameBoard = new GameBoard(config);
            MatchView view = new HalmaAndroid.MatchView(this, gameBoard);
            MatchInput input = new HalmaAndroid.MatchInput(view);
            match = new Match(playerTypes, gameBoard, view, input);

            // Set our view from the "main" layout resource
            SetContentView(view.View);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                match.Dispose();
            }
            base.Dispose(disposing);
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            return ((HalmaAndroid.MatchInput)match.Input).OnTouchEvent(e);
        }
    }
}

