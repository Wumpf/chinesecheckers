using System;
using System.Collections.Generic;
using System.Linq;

using Xamarin.Forms;

namespace HalmaShared.Forms
{
    public partial class MainMenuPage : ContentPage
    {
        /// <summary>
        /// Automatically converting this to GameBoard.Configuration is a lot of work.
        /// Also it seems to be impossible to reference nested types from within XAML.
        /// (see http://stackoverflow.com/questions/13368099/reference-nested-enum-type-from-xaml)
        /// </summary>
        public static readonly string[] NumPlayerOptions = new[] { "2", "3", "4", "6" };

        public string NumPlayers
        {
            get { return numPlayers; }
            set
            {
                numPlayers = value;
                OnPropertyChanged("NumPlayers");
            }
        }
        string numPlayers = "2";
        public GameBoard.Configuration BoardConfig
        {
            get { return GameBoardEnumExtensions.ConfigFromPlayerCount(int.Parse(NumPlayers)); }
        }


        public static readonly string[] PlayerTypeDescriptions =
        {
            "Human", "Path Exploration Ai"
        };
        public static readonly System.Type[] PlayerTypes =
        {
            typeof(Player.HumanPlayer), typeof(Player.PathExplorationAi),
        };

        public int[] SelectedPlayerTypeIndices { get; private set; } = new int[6];

        public IEnumerable<System.Type> SelectedPlayerTypes
        {
            get
            {
                return SelectedPlayerTypeIndices.Take(BoardConfig.NumPlayers()).Select(x => PlayerTypes[x]);
            }
        }

        public delegate void GameStartedHandler();
        public event GameStartedHandler GameStarted;

        public MainMenuPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        private void OnNumPlayersChanges(object sender, EventArgs e)
        {
            int numPlayers = BoardConfig.NumPlayers();
            BindablePicker[] pickers = new[] { playerType0, playerType1, playerType2, playerType3, playerType4, playerType5 };
            Label[] labels = new[] { playerLabel0, playerLabel1, playerLabel2, playerLabel3, playerLabel4, playerLabel5 };

            for (int i = 0; i < numPlayers; ++i)
            {
                pickers[i].IsVisible = true;
                labels[i].IsVisible = true;
            }
            for (int i = numPlayers; i < pickers.Length; ++i)
            {
                pickers[i].IsVisible = false;
                labels[i].IsVisible = false;
            }
        }

        private void OnGameStarted(object sender, EventArgs e)
        {
            GameStarted?.Invoke();
        }
    }
}
