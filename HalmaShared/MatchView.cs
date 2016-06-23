using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace HalmaShared
{
    public abstract class MatchView : IDisposable
    {
        protected GameBoard Board { get; private set; }
        protected int CurrentPlayer { get; private set; }

        protected abstract float Width { get; }
        protected abstract float Height { get; }

        /// <summary>
        /// The entire draw area.
        /// </summary>
        protected Rectangle visibleRect = new Rectangle();
        /// <summary>
        /// Area for drawing the gameboard.
        /// </summary>
        protected Rectangle gameBoardRect = new Rectangle();
        protected const int playerInfoBarHeight = 150;
        protected const int playerTextHeight = 80;

        /// <summary>
        /// Translation offsets for the canvas when drawing with game coordinates.
        /// </summary>
        public float GameDrawOffsetX { get; set; }
        public float GameDrawOffsetY { get; set; }
        /// <summary>
        /// Scale factor for the canvas when drawing with game coordinates.
        /// </summary>
        public float GameDrawScale
        {
            get { return gameDrawScale; }
            set
            {
                gameDrawScale = value;
                gameDrawScale = System.Math.Max(gameDrawScale, minGameDrawScale);
                float maxGameDrawScale = System.Math.Min(Width, Height) / (playerRadius * 10.0f);
                gameDrawScale = System.Math.Min(gameDrawScale, maxGameDrawScale);
            }
        }
        float gameDrawScale;
        float minGameDrawScale;

        protected int winningPlayer = -1;


        public bool HasHighlighted
        {
            get { return hasHighlighted; }
            set
            {
                if (hasHighlighted != value)
                {
                    hasHighlighted = value;
                    Invalidate();
                }
            }
        }
        bool hasHighlighted = false;
        public HexCoord HighlightedPos
        {
            get { return highlightedPos; }
            set
            {
                if (highlightedPos != value)
                {
                    highlightedPos = value;
                    Invalidate();
                }
            }
        }
        HexCoord highlightedPos;


        public MatchView(GameBoard board)
        {
            this.Board = board;
        }

        public void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            GetWindowVisibleDisplayFrame(out visibleRect);
            gameBoardRect.Top = visibleRect.Top + playerInfoBarHeight;
            gameBoardRect.Bottom = visibleRect.Bottom;
            gameBoardRect.Left = visibleRect.Left;
            gameBoardRect.Right = visibleRect.Right;

            ResetBoardScale(Board.GetFields());
        }

        protected abstract void GetWindowVisibleDisplayFrame(out Rectangle rect);

        private void ResetBoardScale(IEnumerable<KeyValuePair<HexCoord, GameBoard.Field>> fields)
        {
            // Gather points in cartesian coordinates.
            // All spatial properties here might be precomputed
            float minX = int.MaxValue;
            float minY = int.MaxValue;
            float maxX = int.MinValue;
            float maxY = int.MinValue;
            foreach (KeyValuePair<HexCoord, GameBoard.Field> field in fields)
            {
                if (field.Value.Type == GameBoard.FieldType.Invalid)
                    continue;

                float x, y;
                field.Key.ToCartesian(out x, out y);

                minX = System.Math.Min(minX, x);
                maxX = System.Math.Max(maxX, x);
                minY = System.Math.Min(minY, y);
                maxY = System.Math.Max(maxY, y);
            }
            float extentX = maxX - minX;
            float extentY = maxY - minY;

            // Transform to fit canvas' clipping space. It is important not to use canvas.Width/Height diRectanglely, since the system's buttom bar bar clips!
            double drawAreaWidth = gameBoardRect.Size.Width;
            double drawAreaHeight = gameBoardRect.Size.Height;
            GameDrawScale = (float)System.Math.Min((drawAreaWidth - drawAreaWidth * offsetPercent * 2) / extentX,
                                                   (drawAreaHeight - drawAreaHeight * offsetPercent * 2) / extentY);
            minGameDrawScale = GameDrawScale;
            GameDrawOffsetX = (float)((drawAreaWidth / GameDrawScale - (maxX - minX)) / 2 - minX + gameBoardRect.Left / GameDrawScale);
            GameDrawOffsetY = (float)((drawAreaHeight / GameDrawScale - (maxY - minY)) / 2 - minY + gameBoardRect.Top / GameDrawScale);
        }
        public void ShowWinningScreen(int winningPlayer)
        {
            this.winningPlayer = winningPlayer;
        }
        protected void DrawToGameSpace(float drawX, float drawY, out float gameX, out float gameY)
        {
            gameX = drawX / GameDrawScale - GameDrawOffsetX;
            gameY = drawY / GameDrawScale - GameDrawOffsetY;
        }

        protected void GameToDrawSpace(float gameX, float gameY, out float drawX, out float drawY)
        {
            drawX = (gameX + GameDrawOffsetX) * GameDrawScale;
            drawY = (gameY + GameDrawOffsetY) * GameDrawScale;
        }

        // TODO: Platform independent input.
        //public HexCoord? GetTouchResult(MotionEvent activityMotionEvent)
        //{
        //    // Assuming the motion event comes from the activity, not the view.
        //    float pointerViewX = activityMotionEvent.GetX(); //+ visibleRect.Left;
        //    float pointerViewY = activityMotionEvent.GetY(); //+ visibleRect.Top;
        //    float pointerCoordGameX, pointerCoordGameY;
        //    DrawToGameSpace(pointerViewX, pointerViewY, out pointerCoordGameX, out pointerCoordGameY);

        //    foreach (var field in game.GameBoard.GetFields())
        //    {
        //        float fieldX, fieldY;
        //        field.Key.ToCartesian(out fieldX, out fieldY);

        //        float dx = fieldX - pointerCoordGameX;
        //        float dy = fieldY - pointerCoordGameY;
        //        float distanceSq = dx * dx + dy * dy;

        //        // Assume you can only press a single field.
        //        if (distanceSq < AndroidGameView.highlightRadius * AndroidGameView.highlightRadius)
        //        {
        //            return field.Key;
        //        }
        //    }

        //    return null;
        //}

        #region Drawing

        // TODO: Move more code into this class.

        //protected abstract void OnDraw();

        public abstract void Invalidate();

        public static readonly Color[] playerColors = new Color[6]
        {
            new Color(1.0, 0.0, 0.0), new Color(65/255.0, 105/255.0, 225/255.0),
            new Color(0.0, 0.8, 0.0), new Color(1.0, 165/255.0, 0.0),
            new Color(238/255.0, 130/255.0, 238/255.0) , new Color(25/255.0, 25/255.0, 112/255.0)
        };

        protected readonly Color hightlightColor = new Color(0.1290, 0.4516, 0.4194);

        protected const float offsetPercent = 0.05f;
        protected const float fieldRadius = 0.2f;
        protected const float playerRadius = 0.4f;
        public const float highlightRadius = 0.7f;

        //protected abstract void DrawPlayerInfo();

        //protected abstract void DrawBackground(IEnumerable<HexCoord> fieldPositions, GameBoard.Configuration boardConfig);

        //protected abstract void DrawFields(IEnumerable<KeyValuePair<HexCoord, GameBoard.Field>> fields);


        protected TurnAnimation animation = new TurnAnimation();
        public void AnimateTurn(Turn turn, int player)
        {
            animation.AnimateTurn(turn, player);
            Invalidate();
        }

        public delegate void AnimationFinishedHandler();
        public event AnimationFinishedHandler TurnAnimationFinished
        {
            add { animation.AnimationFinished += value; }
            remove { animation.AnimationFinished -= value; }
        }

        public void TransitionToPlayer(int currentPlayer)
        {
            // todo: actual transition.
            CurrentPlayer = currentPlayer;
        }

        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            disposedValue = true;
        }

        ~MatchView()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}