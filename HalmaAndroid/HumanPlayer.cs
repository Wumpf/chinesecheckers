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
    class HumanPlayer : Player
    {
        private readonly GameView gameView;

        public HumanPlayer(uint playerNumber, GameView view) : base(playerNumber)
        {
            gameView = view;
        }

        public override void OnTurnStarted(GameBoard currentBoard)
        {
            gameView.OnFieldTouched += OnFieldTouched;
        }

        private void OnFieldTouched(HexCoord hexcoord)
        {
            gameView.HasHighlighted = true;
            gameView.HighlightedPos = hexcoord;
        }

        public override void OnTurnEnded()
        {
            gameView.OnFieldTouched -= OnFieldTouched;
        }
    }
}