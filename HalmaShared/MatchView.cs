using System;

using Xamarin.Forms;

namespace HalmaShared
{
    public abstract class MatchView : IDisposable
    {
        protected GameBoard Board { get; private set; }
        protected int CurrentPlayer { get; private set; }


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

        protected abstract float Width { get; }
        protected abstract float Height { get; }

        public abstract void OnSizeChanged(int w, int h, int oldw, int oldh);

        protected abstract void GetWindowVisibleDisplayFrame(out Rectangle rect);

        public virtual void ShowWinningScreen(int winningPlayer)
        {
            this.winningPlayer = winningPlayer;
        }


        #region Drawing

        public static readonly Color[] playerColors = new Color[6]
        {
            new Color(1.0, 0.0, 0.0), new Color(65/255.0, 105/255.0, 225/255.0),
            new Color(0.0, 0.8, 0.0), new Color(1.0, 165/255.0, 0.0),
            new Color(238/255.0, 130/255.0, 238/255.0) , new Color(25/255.0, 25/255.0, 112/255.0)
        };


        protected abstract void OnDraw();

        public abstract void Invalidate();

        protected TurnAnimation animation = new TurnAnimation();
        public void AnimateTurn(Turn turn, int player)
        {
            animation.AnimateTurn(turn, player);
            Invalidate();
        }

        public bool IsAnimating 
        {
            get { return animation.Active; }
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