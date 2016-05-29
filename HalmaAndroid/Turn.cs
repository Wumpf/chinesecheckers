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
    struct Turn
    {
        public HexCoord From;
        public HexCoord To;

        /// <summary>
        /// Checks whether the given turn is valid.
        /// </summary>
        /// <param name="currentBoard">Game board state on which the turn is checked</param>
        /// <param name="player">Player performing the turn.</param>
        /// <returns></returns>
        public bool IsValidTurn(GameBoard currentBoard, uint player)
        {
            // todo
            return true;
        }
    }
}