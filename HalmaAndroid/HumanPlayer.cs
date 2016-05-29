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
        private GameBoard gameBoard;

        private HexCoord movingPiecePos;
        private bool pieceSelected;

        public HumanPlayer(uint playerNumber, GameView view) : base(playerNumber)
        {
            gameView = view;
        }

        public override void OnTurnStarted(GameBoard currentBoard)
        {
            gameBoard = currentBoard;
            gameView.FieldTouched += OnFieldTouched;
        }

        private void OnFieldTouched(HexCoord hexcoord)
        {
            int selectedPiece = gameBoard[hexcoord].PlayerPiece;

            // Selected piece to move.
            if (selectedPiece == PlayerIndex)
            {
                pieceSelected = true;
                gameView.HasHighlighted = true;
                movingPiecePos = hexcoord;
                gameView.HighlightedPos = hexcoord;
            }
            // Target selected.
            else if (pieceSelected && selectedPiece < 0)
            {
                OnTurnReady(new Turn() { From = movingPiecePos, To = hexcoord });
            }
            // Invalid selection
            else
            {
                // todo: Some kind of feedback?
            }
        }

        public override void OnTurnEnded()
        {
            gameBoard = null;
            gameView.FieldTouched -= OnFieldTouched;
            gameView.HasHighlighted = false;
        }
    }
}