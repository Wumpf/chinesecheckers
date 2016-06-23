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

namespace HalmaAndroid
{
    class MatchView : HalmaShared.MatchView
    {
        class AndroidView : Android.Views.View
        {
            MatchView matchView;

            public AndroidView(Context context, MatchView matchView) : base(context)
            {
                this.SetPadding(0, 0, 0, 0);
                this.matchView = matchView;
            }

            protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
            {
                matchView.OnSizeChanged(w, h, oldw, oldh);
            }

            protected override void OnDraw(Canvas canvas)
            {
                base.OnDraw(canvas);
                matchView.OnDraw(canvas);
            }
        }

        public Android.Views.View View { get; private set; }

        protected override float Width => View.Width;
        protected override float Height => View.Height;

        private Typeface typeface;
        private Bitmap gradient0;

        private Paint paintNoAA;
        private Paint paintFillAA;
        private Paint paintPlayerText;
        private Paint paintLine;

        public MatchView(Context context, HalmaShared.GameBoard board) : base(board)
        {
            View = new AndroidView(context, this);

            typeface = Typeface.Create(Typeface.SansSerif, TypefaceStyle.Normal);
            gradient0 = BitmapFactory.DecodeResource(context.Resources, Resource.Drawable.gradient0);

            // paints
            paintNoAA = new Paint();
            paintFillAA = new Paint
            {
                AntiAlias = true,
            };
            paintFillAA.SetStyle(Paint.Style.Fill);
            paintPlayerText = new Paint
            {
                AntiAlias = true,
                TextSize = playerTextHeight
            };
            paintPlayerText.SetTypeface(typeface);
            paintLine = new Paint
            {
                AntiAlias = true,
                StrokeWidth = fieldRadius,
                Color = new Color(240, 240, 240)
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                View.Dispose();

                typeface.Dispose();
                gradient0.Dispose();

                paintNoAA.Dispose();
                paintFillAA.Dispose();
                paintPlayerText.Dispose();
                paintLine.Dispose();
            }
        }

        public HexCoord? GetTouchResult(MotionEvent activityMotionEvent)
        {
            // Assuming the motion event comes from the activity, not the view.
            float pointerViewX = activityMotionEvent.GetX(); //+ visibleRect.Left;
            float pointerViewY = activityMotionEvent.GetY(); //+ visibleRect.Top;
            float pointerCoordGameX, pointerCoordGameY;
            DrawToGameSpace(pointerViewX, pointerViewY, out pointerCoordGameX, out pointerCoordGameY);

            foreach (var field in Board.GetFields())
            {
                float fieldX, fieldY;
                field.Key.ToCartesian(out fieldX, out fieldY);

                float dx = fieldX - pointerCoordGameX;
                float dy = fieldY - pointerCoordGameY;
                float distanceSq = dx * dx + dy * dy;

                // Assume you can only press a single field.
                if (distanceSq < highlightRadius * highlightRadius)
                {
                    return field.Key;
                }
            }

            return null;
        }

        #region Drawing

        public void OnDraw(Canvas canvas)
        {
            // Clear background.
            canvas.Matrix = new Matrix();

            paintNoAA.Color = Color.White;
            canvas.DrawPaint(paintNoAA);

            canvas.Scale(GameDrawScale, GameDrawScale);
            canvas.Translate(GameDrawOffsetX, GameDrawOffsetY);
            DrawBackground(Board.GetFields().Select(x => x.Key), Board.Config, canvas);

            animation.Update();
            if (animation.Active)
                Invalidate();
            DrawFields(Board.GetFields(), canvas);

            canvas.Matrix = new Matrix();
            DrawPlayerInfo(canvas, CurrentPlayer);
        }

        private void DrawPlayerInfo(Canvas canvas, int currentPlayer)
        {
            Paint backgroundPaint = paintNoAA;

            string text;
            if (winningPlayer >= 0)
            {
                text = "Player " + (winningPlayer + 1).ToString() + " Won!";
                paintPlayerText.Color = Color.White;
                backgroundPaint.Color = playerColors[winningPlayer].ToAndroid();
            }
            else
            {
                text = "Player " + (currentPlayer + 1).ToString();
                paintPlayerText.Color = playerColors[currentPlayer].ToAndroid();
                backgroundPaint.Color = Color.White;
            }

            canvas.DrawRect((int)visibleRect.Left, (int)visibleRect.Top, (int)visibleRect.Right, (int)visibleRect.Top + playerInfoBarHeight, backgroundPaint);

            var metrics = paintPlayerText.GetFontMetrics();
            float x = (float)(visibleRect.Left + (visibleRect.Width - paintPlayerText.MeasureText(text)) / 2.0);
            float y = (float)(visibleRect.Top + (playerInfoBarHeight + playerTextHeight) / 2.0);
            canvas.DrawText(text, x, y, paintPlayerText);

            // Debug!
            #if DEBUG
                HighlightedPos.ToCartesian(out x, out y);
               GameToDrawSpace(x, y, out x, out y);
               canvas.DrawText(string.Format("{0} {1} {2}", HighlightedPos.X, HighlightedPos.Y, HighlightedPos.Z), 0, 200, paintPlayerText);
            #endif

            paintNoAA.Color = Color.Argb(70, 70, 70, 70);
            canvas.DrawBitmap(gradient0, new Rect(0, 0, gradient0.Width, gradient0.Height), 
                                         new Rect((int)visibleRect.Left, (int)gameBoardRect.Top, (int)visibleRect.Right, (int)gameBoardRect.Top + 24), paintNoAA);
        }

        private void DrawBackground(IEnumerable<HexCoord> fieldPositions, GameBoard.Configuration boardConfig, Canvas canvas)
        {
            if (!boardConfig.IsStar())
                return;

            // There are three directions of lines, one for each axis.

            // x-y lines.
            for (int comp = 0; comp < 3; ++comp)
            {
                int nextComp = (comp + 1) % 3;
                int calcComp = (comp + 2) % 3;

                int min = fieldPositions.Min(x => x[comp]);
                int max = fieldPositions.Max(x => x[comp]);
                int[] xyLineMax = Enumerable.Repeat(int.MaxValue, max - min + 1).ToArray();
                int[] xyLineMin = Enumerable.Repeat(int.MinValue, max - min + 1).ToArray();
                foreach (HexCoord pos in fieldPositions)
                {
                    int index = pos[comp] - min;
                    xyLineMin[index] = Math.Max(xyLineMin[index], pos[nextComp]);
                    xyLineMax[index] = Math.Min(xyLineMax[index], pos[nextComp]);
                }

                for (int i = 0; i < xyLineMin.Length; ++i)
                {
                    int fixCoord = i + min;
                    if (xyLineMin[i] == xyLineMax[i])
                        continue;

                    float startX, startY;
                    HexCoord start = new HexCoord();
                    start[comp] = fixCoord;
                    start[nextComp] = xyLineMin[i];
                    start[calcComp] = -fixCoord - xyLineMin[i];
                    start.ToCartesian(out startX, out startY);

                    float endX, endY;
                    HexCoord end = new HexCoord();
                    end[comp] = fixCoord;
                    end[nextComp] = xyLineMax[i];
                    end[calcComp] = -fixCoord - xyLineMax[i];
                    end.ToCartesian(out endX, out endY);
                    
                    canvas.DrawLine(startX, startY, endX, endY, paintLine);
                }
            }
        }

        private void DrawFields(IEnumerable<KeyValuePair<HexCoord, GameBoard.Field>> fields, Canvas canvas)
        {
            // Draw highlight if any.
            if (HasHighlighted)
            {
                float highlightedX, highlightedY;
                HighlightedPos.ToCartesian(out highlightedX, out highlightedY);

                Paint highlightPaint = new Paint
                {
                    AntiAlias = true,
                    Color = hightlightColor.ToAndroid(),
                };
                highlightPaint.SetStyle(Paint.Style.Fill);

                canvas.DrawCircle(highlightedX, highlightedY, highlightRadius, highlightPaint);
            }

            // Draw points
            foreach (var field in fields)
            {
                int player = field.Value.PlayerPiece;

                float x, y;
                field.Key.ToCartesian(out x, out y);

                if (player >= 0 &&
                    !(animation.Active && animation.Turn.To == field.Key))
                {
                    paintFillAA.Color = playerColors[player].ToAndroid();
                    canvas.DrawCircle(x, y, playerRadius, paintFillAA);
                }
                else
                {
                    int playerGoal = field.Value.Type.GetPlayerGoal();
                    if (playerGoal >= 0)
                        paintFillAA.Color = playerColors[playerGoal].ToAndroid();
                    else
                        paintFillAA.Color = Color.Black;
                    canvas.DrawCircle(x, y, fieldRadius, paintFillAA);
                }
            }

            // Animated turn.
            if (animation.Active)
            {
                float x, y;
                animation.GetCurrentCartesian(out x, out y);
                paintFillAA.Color = playerColors[animation.Player].ToAndroid();
                canvas.DrawCircle(x, y, playerRadius, paintFillAA);
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

        #endregion
    }
}