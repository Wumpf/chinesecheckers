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

namespace HalmaAndroid
{
    class GameView : View, GestureDetector.IOnGestureListener, ScaleGestureDetector.IOnScaleGestureListener
    {
        private GameActivity game;

        private Typeface typeface;
        private Bitmap gradient0;

        private Paint paintNoAA;
        private Paint paintPlayerText;

        /// <summary>
        /// The entire draw area.
        /// </summary>
        private Rect visibleRect = new Rect();
        /// <summary>
        /// Area for drawing the gameboard.
        /// </summary>
        private Rect gameBoardRect = new Rect();
        private const int playerInfoBarHeight = 150;
        private const int playerTextHeight = 80;

        /// <summary>
        /// Translation offsets for the canvas when drawing with game coordinates.
        /// </summary>
        private float gameDrawOffsetX;
        private float gameDrawOffsetY;
        /// <summary>
        /// Scale factor for the canvas when drawing with game coordinates.
        /// </summary>
        private float gameDrawScale;
        private float minGameDrawScale;


        private int winningPlayer = -1;


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
        private bool hasHighlighted = false;
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
        private HexCoord highlightedPos;

        private void DrawToGameSpace(float drawX, float drawY, out float gameX, out float gameY)
        {
            gameX = drawX / gameDrawScale - gameDrawOffsetX;
            gameY = drawY / gameDrawScale - gameDrawOffsetY;
        }

        private void GameToDrawSpace(float gameX, float gameY, out float drawX, out float drawY)
        {
            drawX = (gameX + gameDrawOffsetX) * gameDrawScale;
            drawY = (gameY + gameDrawOffsetY) * gameDrawScale;
        }
        public GameView(Context context, GameActivity game) : base(context)
        {
            this.SetPadding(0, 0, 0, 0);
            this.game = game;

            scrollListener = new GestureDetector(context, this);
            scaleDetector = new ScaleGestureDetector(context, this);

            typeface = Typeface.Create(Typeface.SansSerif, TypefaceStyle.Normal);
            gradient0 = BitmapFactory.DecodeResource(Context.Resources, Resource.Drawable.gradient0);

            // paints
            paintNoAA = new Paint();
            paintPlayerText = new Paint
            {
                AntiAlias = true,
                TextSize = playerTextHeight
            };
            paintPlayerText.SetTypeface(typeface);
        }

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);
            GetWindowVisibleDisplayFrame(visibleRect);
            gameBoardRect.Top = visibleRect.Top + playerInfoBarHeight;
            gameBoardRect.Bottom = visibleRect.Bottom;
            gameBoardRect.Left = visibleRect.Left;
            gameBoardRect.Right = visibleRect.Right;

            ResetBoardScale(game.GameBoard.GetFields());
        }

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
                System.Drawing.PointF point = new System.Drawing.PointF(x, y);

                minX = System.Math.Min(minX, x);
                maxX = System.Math.Max(maxX, x);
                minY = System.Math.Min(minY, y);
                maxY = System.Math.Max(maxY, y);
            }
            float extentX = maxX - minX;
            float extentY = maxY - minY;

            // Transform to fit canvas' clipping space. It is important not to use canvas.Width/Height directly, since the system's buttom bar bar clips!
            int drawAreaWidth = gameBoardRect.Width();
            int drawAreaHeight = gameBoardRect.Height();
            gameDrawScale = System.Math.Min((drawAreaWidth - drawAreaWidth * offsetPercent * 2) / extentX,
                                          (drawAreaHeight - drawAreaHeight * offsetPercent * 2) / extentY);
            minGameDrawScale = gameDrawScale;
            gameDrawOffsetX = (drawAreaWidth / gameDrawScale - (maxX - minX)) / 2 - minX + gameBoardRect.Left / gameDrawScale;
            gameDrawOffsetY = (drawAreaHeight / gameDrawScale - (maxY - minY)) / 2 - minY + gameBoardRect.Top / gameDrawScale;
        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);

            // Clear background.
            canvas.Matrix = new Matrix();

            paintNoAA.Color = Color.White;
            canvas.DrawPaint(paintNoAA);

            DrawFields(game.GameBoard.GetFields(), canvas);
            canvas.Matrix = new Matrix();
            DrawPlayerInfo(canvas);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if(disposing)
            {
                typeface.Dispose();
                gradient0.Dispose();

                paintNoAA.Dispose();
                paintPlayerText.Dispose();

                visibleRect.Dispose();
                gameBoardRect.Dispose();

                scrollListener.Dispose();
                scaleDetector.Dispose();
            }
        }

        public void ShowWinningScreen(uint winningPlayer)
        {
            this.winningPlayer = (int)winningPlayer;
        }

        private static readonly Color[] playerColors = new Color[6]
        {
            Color.Red, Color.Blue, Color.Green, Color.Yellow, Color.Black, Color.Violet
        };

        private readonly Color hightlightColor = Color.MediumTurquoise;

        private const float offsetPercent = 0.05f;
        private const float fieldRadius = 0.2f;
        private const float playerRadius = 0.4f;
        private const float highlightRadius = 0.7f;

        private void DrawPlayerInfo(Canvas canvas)
        {
            Paint backgroundPaint = paintNoAA;

            string text;
            if (winningPlayer >= 0)
            {
                text = "Player " + (winningPlayer + 1).ToString() + " Won!";
                paintPlayerText.Color = Color.White;
                backgroundPaint.Color = playerColors[winningPlayer];
            }
            else
            {
                text = "Player " + (game.CurrentPlayer + 1).ToString();
                paintPlayerText.Color = playerColors[game.CurrentPlayer];
                backgroundPaint.Color = Color.White;
            }

            canvas.DrawRect(visibleRect.Left, visibleRect.Top, visibleRect.Right, visibleRect.Top + playerInfoBarHeight, backgroundPaint);

            var metrics = paintPlayerText.GetFontMetrics();
            float x = visibleRect.Left + (visibleRect.Width() - paintPlayerText.MeasureText(text)) / 2.0f;
            float y = visibleRect.Top + (playerInfoBarHeight + playerTextHeight) / 2.0f;
            canvas.DrawText(text, x, y, paintPlayerText);

            // Debug!
            #if DEBUG
               highlightedPos.ToCartesian(out x, out y);
               GameToDrawSpace(x, y, out x, out y);
               canvas.DrawText(string.Format("{0} {1} {2}", highlightedPos.X, highlightedPos.Y, highlightedPos.Z), 0, 200, paintPlayerText);
            #endif

            paintNoAA.Color = Color.Argb(70, 70, 70, 70);
            canvas.DrawBitmap(gradient0, new Rect(0, 0, gradient0.Width, gradient0.Height), 
                                         new Rect(visibleRect.Left, gameBoardRect.Top, visibleRect.Right, gameBoardRect.Top + 24), paintNoAA);
        }

        private void DrawFields(IEnumerable<KeyValuePair<HexCoord, GameBoard.Field>> fields, Canvas canvas)
        {

            canvas.DrawCircle(0, 0, canvas.ClipBounds.Top + 10, new Paint
            {
                AntiAlias = true,
                Color = Color.Black,
            });

            canvas.Scale(gameDrawScale, gameDrawScale);
            canvas.Translate(gameDrawOffsetX, gameDrawOffsetY);

            // Draw highlight if any.
            if (hasHighlighted)
            {
                float highlightedX, highlightedY;
                HighlightedPos.ToCartesian(out highlightedX, out highlightedY);

                Paint highlightPaint = new Paint
                {
                    AntiAlias = true,
                    Color = hightlightColor,
                };
                highlightPaint.SetStyle(Paint.Style.Fill);

                canvas.DrawCircle(highlightedX, highlightedY, highlightRadius, highlightPaint);
            }

            // Draw points
            Paint fieldPaint = new Paint
            {
                AntiAlias = true,
            };
            fieldPaint.SetStyle(Paint.Style.Fill);

            Paint playerPaint = new Paint
            {
                AntiAlias = true,
            };
            fieldPaint.SetStyle(Paint.Style.Fill);


            foreach (var field in fields)
            {
                int player = field.Value.PlayerPiece;

                float x, y;
                field.Key.ToCartesian(out x, out y);

                if (player >= 0)
                {
                    playerPaint.Color = playerColors[player];
                    canvas.DrawCircle(x, y, playerRadius, playerPaint);
                }
                else
                {
                    int playerGoal = GameBoard.GetPlayerGoal(field.Value.Type);
                    if (playerGoal >= 0)
                        fieldPaint.Color = playerColors[playerGoal];
                    else
                        fieldPaint.Color = Color.Black;
                    canvas.DrawCircle(x, y, fieldRadius, fieldPaint);
                }
            }
        }

        #region Input

        private GestureDetector scrollListener;
        private ScaleGestureDetector scaleDetector;

        public delegate void FieldTouchedHandler(HexCoord hexcoord);
        public event FieldTouchedHandler FieldTouched;

        public override bool OnTouchEvent(MotionEvent e)
        {
            scrollListener.OnTouchEvent(e);
            scaleDetector.OnTouchEvent(e);

            return true;
        }

        public bool OnScale(ScaleGestureDetector detector)
        {
            float scaleFactor = detector.ScaleFactor;
            float newGameScale = gameDrawScale * scaleFactor;
            newGameScale = System.Math.Max(newGameScale, minGameDrawScale);
            float maxGameDrawScale = System.Math.Min(Width, Height) / (playerRadius * 10.0f);
            newGameScale = System.Math.Min(newGameScale, maxGameDrawScale);
            scaleFactor = newGameScale / gameDrawScale;

            // Scaling means also that we need to move towards the focus point.
            float focusDiffX = detector.FocusX - gameDrawOffsetX / gameDrawScale;
            float focusDiffY = detector.FocusY - gameDrawOffsetY / gameDrawScale;

            gameDrawOffsetX -= (focusDiffX * scaleFactor - focusDiffX) / gameDrawScale;
            gameDrawOffsetY -= (focusDiffY * scaleFactor - focusDiffY) / gameDrawScale;

            // Apply actual scale.
            gameDrawScale = newGameScale;

            Invalidate();

            return true;
        }

        public bool OnScaleBegin(ScaleGestureDetector detector)
        {
            return true;
        }

        public void OnScaleEnd(ScaleGestureDetector detector)
        {
        }


        public bool OnDown(MotionEvent e)
        {
            if (FieldTouched != null)
            {
                float pointerViewX = e.GetX() + visibleRect.Left;
                float pointerViewY = e.GetY() + visibleRect.Top;
                float pointerCoordGameX, pointerCoordGameY;
                DrawToGameSpace(pointerViewX, pointerViewY, out pointerCoordGameX, out pointerCoordGameY);

                foreach (var field in game.GameBoard.GetFields())
                {
                    float fieldX, fieldY;
                    field.Key.ToCartesian(out fieldX, out fieldY);

                    float dx = fieldX - pointerCoordGameX;
                    float dy = fieldY - pointerCoordGameY;
                    float distanceSq = dx * dx + dy * dy;

                    // Assume you can only press a single field.
                    if (distanceSq < highlightRadius * highlightRadius)
                    {
                        FieldTouched(field.Key);
                        break;
                    }
                }
            }

            // Need to return false, otherwise the event is consumed and we no longer get scroll or scale!
            return false;
        }

        public bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
        {
            return true;
        }

        public void OnLongPress(MotionEvent e)
        {
        }

        public bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
        {
            gameDrawOffsetX -= distanceX / gameDrawScale;
            gameDrawOffsetY -= distanceY / gameDrawScale;
            Invalidate();
            return true;
        }

        public void OnShowPress(MotionEvent e)
        {
        }

        public bool OnSingleTapUp(MotionEvent e)
        {
            return true;
        }

        #endregion
    }
}