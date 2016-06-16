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
    [Activity(Label = "@string/menuActivityLabel", MainLauncher = true)]
    public class MenuActivity : Activity
    {
        private Spinner[] playerTypeSpinner;
        private TextView[] playerTypeText;
        private int numPlayers = 2;

        private static readonly int[] playerCountSpinnerToCount = new[] { 2, 3, 4, 6 };
        private static readonly Type[] playerTypeSpinnerToType = new[] { typeof(Player.HumanPlayer), typeof(Player.PathExplorationAi) };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Menu);

            {
                var playerCountArrayAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerItem, playerCountSpinnerToCount);
                playerCountArrayAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);

                Spinner spinner = FindViewById<Spinner>(Resource.Id.numberOfPlayers);
                spinner.Adapter = playerCountArrayAdapter;
                spinner.ItemSelected += NumPlayersSelected;
            }
            {
                playerTypeSpinner = new Spinner[]
                {
                    FindViewById<Spinner>(Resource.Id.playerType0),
                    FindViewById<Spinner>(Resource.Id.playerType1),
                    FindViewById<Spinner>(Resource.Id.playerType2),
                    FindViewById<Spinner>(Resource.Id.playerType3),
                    FindViewById<Spinner>(Resource.Id.playerType4),
                    FindViewById<Spinner>(Resource.Id.playerType5),
                };
                playerTypeText = new TextView[]
                {
                    FindViewById<TextView>(Resource.Id.playerType0text),
                    FindViewById<TextView>(Resource.Id.playerType1text),
                    FindViewById<TextView>(Resource.Id.playerType2text),
                    FindViewById<TextView>(Resource.Id.playerType3text),
                    FindViewById<TextView>(Resource.Id.playerType4text),
                    FindViewById<TextView>(Resource.Id.playerType5text),
                };

                var playerTypeArrayAdapter = ArrayAdapter.CreateFromResource(this, Resource.Array.playerTypes, Android.Resource.Layout.SimpleSpinnerItem);
                playerTypeArrayAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);

                System.Diagnostics.Debug.Assert(playerTypeArrayAdapter.Count == playerTypeSpinnerToType.Length);

                foreach (Spinner spinner in playerTypeSpinner)
                    spinner.Adapter = playerTypeArrayAdapter;
            }
            {
                FindViewById<Button>(Resource.Id.startGameButton).Click += OnStartGame; ;
            }
        }

        private void NumPlayersSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            numPlayers = playerCountSpinnerToCount[e.Position];

            for (int i = 0; i < numPlayers; ++i)
            {
                playerTypeSpinner[i].Visibility = ViewStates.Visible;
                playerTypeText[i].Visibility = ViewStates.Visible;
            }
            for (int i = numPlayers; i < playerTypeSpinner.Length; ++i)
            {
                playerTypeSpinner[i].Visibility = ViewStates.Gone;
                playerTypeText[i].Visibility = ViewStates.Gone;
            }
        }

        private void OnStartGame(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(GameActivity));
            EncodeStartGameInfo(intent);
            StartActivity(intent);
        }

        private void EncodeStartGameInfo(Intent intent)
        {
            intent.PutExtra("playerCount", numPlayers);
            for (int i = 0; i < numPlayers; ++i)
            {
                intent.PutExtra("playerType" + i.ToString(), playerTypeSpinner[i].SelectedItemPosition);
            }
        }

        static internal void DecodeStartGameInfo(Intent intent, out GameBoard.Configuration config, out Type[] players)
        {
            int playerCount = intent.GetIntExtra("playerCount", 2);
            switch(playerCount)
            {
                case 2:
                    config = GameBoard.Configuration.STAR_2;
                    break;
                case 3:
                    config = GameBoard.Configuration.STAR_3;
                    break;
                case 4:
                    config = GameBoard.Configuration.STAR_4;
                    break;
                case 6:
                    config = GameBoard.Configuration.STAR_6;
                    break;
                default:
                    throw new NotImplementedException();
            }

            players = new Type[playerCount];
            for (int i = 0; i < players.Length; ++i)
            {
                int typeId = intent.GetIntExtra("playerType" + i.ToString(), 0);
                players[i] = playerTypeSpinnerToType[typeId];
            }
        }
    }
}