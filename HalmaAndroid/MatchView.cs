using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using HalmaShared;

using Color = Xamarin.Forms.Color;
using Xamarin.Forms;

namespace HalmaAndroid
{
    class MatchView : HalmaShared.MatchView2D
    {
        class AndroidView : Android.Views.View
        {
            private MatchView matchView;
            private AndroidCanvas canvasWrapper;

            public AndroidView(Context context, MatchView matchView, AndroidCanvas canvasWrapper) : base(context)
            {
                this.SetPadding(0, 0, 0, 0);
                this.matchView = matchView;
                this.canvasWrapper = canvasWrapper;
            }

            protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
            {
                matchView.OnSizeChanged(w, h, oldw, oldh);
            }

            protected override void OnDraw(Canvas canvas)
            {
                canvasWrapper.Canvas = canvas;
                base.OnDraw(canvas);
                matchView.OnDraw();
            }
        }

        public Android.Views.View View { get; private set; }

        protected override float Width => View.Width;
        protected override float Height => View.Height;


        public MatchView(Context context, HalmaShared.GameBoard board) : 
            base(board, new AndroidCanvas(context))
        {
            View = new AndroidView(context, this, (AndroidCanvas)Canvas);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                View.Dispose();
            }
        }

        protected override void GetWindowVisibleDisplayFrame(out Xamarin.Forms.Rectangle rect)
        {
            using (Rect androidRect = new Rect())
            {
                View.GetWindowVisibleDisplayFrame(androidRect);
                rect = new Xamarin.Forms.Rectangle(androidRect.Left, androidRect.Top, androidRect.Width(), androidRect.Height());
            }
        }

        public override void Invalidate()
        {
            View.Invalidate();
        }
    }
}