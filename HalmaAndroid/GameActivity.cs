﻿using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace HalmaAndroid
{
    [Activity(Label = "HalmaAndroid", MainLauncher = true, Icon = "@drawable/icon")]
    public class GameActivity : Activity
    {
        private GameBoard gameBoard;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            gameBoard = new GameBoard();
            var view = new GameView(this, gameBoard);

            // Set our view from the "main" layout resource
            SetContentView(view);
        }
    }
}
