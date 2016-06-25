using Android.App;
using Android.Content;
using Android.OS;
using HalmaShared;
using System;

namespace HalmaAndroid
{
    [Activity(Label = "@string/menuActivityLabel", MainLauncher = true)]
    public class MenuActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
    {
        private HalmaShared.Forms.MainMenuPage menuPage;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            LoadApplication(new HalmaShared.Forms.App());

            menuPage = (HalmaShared.Forms.MainMenuPage)HalmaShared.Forms.App.Current.MainPage;
            menuPage.GameStarted += OnStartGame;
        }

        private void OnStartGame()
        {
            var intent = new Intent(this, typeof(GameActivity));
            EncodeStartGameInfo(intent);
            StartActivity(intent);
        }

        private void EncodeStartGameInfo(Intent intent)
        {
            int numPlayers = menuPage.BoardConfig.NumPlayers();

            intent.PutExtra("playerCount", numPlayers);
            for (int i = 0; i < numPlayers; ++i)
            {
                intent.PutExtra("playerType" + i.ToString(), menuPage.SelectedPlayerTypeIndices[i]);
            }
        }

        static internal void DecodeStartGameInfo(Intent intent, out GameBoard.Configuration config, out Type[] players)
        {
            int playerCount = intent.GetIntExtra("playerCount", 2);
            config = GameBoardEnumExtensions.ConfigFromPlayerCount(playerCount);

            players = new Type[playerCount];
            for (int i = 0; i < players.Length; ++i)
            {
                int typeId = intent.GetIntExtra("playerType" + i.ToString(), 0);
                players[i] = HalmaShared.Forms.MainMenuPage.PlayerTypes[typeId];
            }
        }
    }
}