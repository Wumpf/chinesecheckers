using System;
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
        private Level level;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            level = new Level();
            var view = new GameView(this, level);

            // Set our view from the "main" layout resource
            SetContentView(view);
        }
    }
}

