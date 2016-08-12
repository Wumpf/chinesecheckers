using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace HalmaShared
{
    public abstract class MatchView2D : MatchView
    {
        public ICrossPlatformCanvas Canvas { get; private set; }

        public MatchView2D(GameBoard board, ICrossPlatformCanvas canvas) : base(board)
        {
            this.Canvas = canvas;
        }

        #region Size, Offset and Scaling

        /// <summary>
        /// The entire draw area.
        /// </summary>
        protected Rectangle visibleRect;
        /// <summary>
        /// Area for drawing the gameboard.
        /// </summary>
        protected Rectangle gameBoardRect;
        protected const int playerInfoBarHeight = 150;
        protected const int playerTextHeight = 80;

        private Rectangle undoButtonRect;
        private Rectangle redoButtonRect;
        private const double arrowButtonHorizontalMargin = 20.0;
        private const double arrowButtonHeight = 40.0;
        private const double arrowButtonWidth = 40.0;
        private const float arrowButtomLineThickness = 9.0f;

        /// <summary>
        /// Translation offsets for the canvas when drawing with game coordinates.
        /// </summary>
        public Vec2 GameDrawOffset
        {
            get { return gameDrawOffset; }
            set
            {
                double middleOffsetX = gameBoardRect.Size.Width / GameDrawScale / 2 + gameBoardRect.Left / GameDrawScale;
                double middleOffsetY = gameBoardRect.Size.Height / GameDrawScale / 2 + gameBoardRect.Top / GameDrawScale;
                gameDrawOffset = new Vec2
                    (MathUtils.Clamp(value.X, middleOffsetX - gameboardExtentX * 0.6, middleOffsetX + gameboardExtentX * 0.6),
                     MathUtils.Clamp(value.Y, middleOffsetY - gameboardExtentY * 0.6, middleOffsetX + gameboardExtentY * 0.6));
            }
        }
        private Vec2 gameDrawOffset= new Vec2();

        float gameboardExtentX;
        float gameboardExtentY;

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


        private void ResetBoardScale(IEnumerable<KeyValuePair<HexCoord, GameBoard.Field>> fields)
        {
            // Gather points in cartesian coordinates.
            // All spatial properties here might be precomputed
            float minGameboardX = int.MaxValue;
            float minGameboardY = int.MaxValue;
            float maxGameboardX = int.MinValue;
            float maxGameboardY = int.MinValue;
            foreach (KeyValuePair<HexCoord, GameBoard.Field> field in fields)
            {
                if (field.Value.Type == GameBoard.FieldType.Invalid)
                    continue;

                float x, y;
                field.Key.ToCartesian(out x, out y);

                minGameboardX = System.Math.Min(minGameboardX, x);
                maxGameboardX = System.Math.Max(maxGameboardX, x);
                minGameboardY = System.Math.Min(minGameboardY, y);
                maxGameboardY = System.Math.Max(maxGameboardY, y);
            }
            gameboardExtentX = maxGameboardX - minGameboardX;
            gameboardExtentY = maxGameboardY - minGameboardY;

            // Transform to fit canvas' clipping space. It is important not to use canvas.Width/Height diRectanglely, since the system's buttom bar bar clips!
            double drawAreaWidth = gameBoardRect.Size.Width;
            double drawAreaHeight = gameBoardRect.Size.Height;
            GameDrawScale = (float)System.Math.Min((drawAreaWidth - drawAreaWidth * offsetPercent * 2) / gameboardExtentX,
                                                   (drawAreaHeight - drawAreaHeight * offsetPercent * 2) / gameboardExtentY);
            minGameDrawScale = GameDrawScale;
            GameDrawOffset = new Vec2(
                ((drawAreaWidth / GameDrawScale - gameboardExtentX) / 2 - minGameboardX + gameBoardRect.Left / GameDrawScale),
                ((drawAreaHeight / GameDrawScale - gameboardExtentY) / 2 - minGameboardY + gameBoardRect.Top / GameDrawScale));
        }

        public override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            GetWindowVisibleDisplayFrame(out visibleRect);
            gameBoardRect.Top = visibleRect.Top + playerInfoBarHeight;
            gameBoardRect.Bottom = visibleRect.Bottom;
            gameBoardRect.Left = visibleRect.Left;
            gameBoardRect.Right = visibleRect.Right;

            double midHeight = visibleRect.Top + playerInfoBarHeight / 2.0;

            undoButtonRect.Left = visibleRect.Left + arrowButtonHorizontalMargin;
            undoButtonRect.Right = visibleRect.Left + arrowButtonHorizontalMargin + arrowButtonWidth;
            undoButtonRect.Top = midHeight - arrowButtonHeight;
            undoButtonRect.Bottom = midHeight + arrowButtonHeight;

            redoButtonRect.Left = visibleRect.Right - arrowButtonHorizontalMargin - arrowButtonWidth;
            redoButtonRect.Right = visibleRect.Right - arrowButtonHorizontalMargin;
            redoButtonRect.Top = midHeight - arrowButtonHeight;
            redoButtonRect.Bottom = midHeight + arrowButtonHeight;

            ResetBoardScale(Board.GetFields());
        }

        protected void DrawToGameSpace(Vec2 draw, out Vec2 game)
        {
            game = new Vec2(draw.X / GameDrawScale - GameDrawOffset.X,
                            draw.Y / GameDrawScale - GameDrawOffset.Y);
        }

        protected void GameToDrawSpace(double gameX, double gameY, out double drawX, out double drawY)
        {
            drawX = (gameX + GameDrawOffset.X) * GameDrawScale;
            drawY = (gameY + GameDrawOffset.Y) * GameDrawScale;
        }

        public MatchInput.TouchResultType GetTouchResult(Vec2 touchPosition, out HexCoord touchedCoord)
        {
            touchedCoord = HexCoord.Zero;

            if (winningPlayer >= 0)
                return MatchInput.TouchResultType.None;

            // Assuming the motion event comes from the activity, not the view.
            Vec2 pointerCoordGame;
            DrawToGameSpace(touchPosition, out pointerCoordGame);

            foreach (var field in Board.GetFields())
            {
                float fieldX, fieldY;
                field.Key.ToCartesian(out fieldX, out fieldY);

                double dx = fieldX - pointerCoordGame.X;
                double dy = fieldY - pointerCoordGame.Y;
                double distanceSq = dx * dx + dy * dy;

                // Assume you can only press a single field.
                if (distanceSq < highlightTouchSize * highlightTouchSize)
                {
                    touchedCoord = field.Key;
                    return MatchInput.TouchResultType.Field;
                }
            }

            if (Board.TurnHistory.CanUndo() && undoButtonRect.Contains(new Point(touchPosition.X, touchPosition.Y)))
                return MatchInput.TouchResultType.Undo;
            if (Board.TurnHistory.CanRedo() && redoButtonRect.Contains(new Point(touchPosition.X, touchPosition.Y)))
                return MatchInput.TouchResultType.Redo;

            return MatchInput.TouchResultType.None;
        }

        #endregion


        #region Drawing

        private const float offsetPercent = 0.05f;
        private const float fieldRadius = 0.2f;
        private const float playerRadius = 0.4f;
        public const float highlightTouchSize = 0.7f;
        public const float highlightSize = 2.0f;

        private readonly Color lineColor = new Color(240 / 255.0, 240 / 255.0, 240 / 255.0);

        protected override void OnDraw()
        {
            // Clear background.
            Canvas.ResetTransform();

            Canvas.FillColor(Xamarin.Forms.Color.White);

            Canvas.Scale(GameDrawScale);
            Canvas.Translate(GameDrawOffset);
            DrawBackground(Board.GetFields().Select(x => x.Key), Board.Config);

            animation.Update();
            if (animation.Active)
                Invalidate();
            DrawFields(Board.GetFields());

            Canvas.ResetTransform();
            DrawTopBar(CurrentPlayer);
        }

        private void DrawTopBar(int currentPlayer)
        {
            Color backgroundColor;
            Color playerTextColor;

            string text;
            if (winningPlayer >= 0)
            {
                text = "Player " + (winningPlayer + 1).ToString() + " Won!";
                playerTextColor = Color.White;
                backgroundColor = playerColors[winningPlayer];
            }
            else
            {
                text = "Player " + (currentPlayer + 1).ToString();
                playerTextColor = playerColors[currentPlayer];
                backgroundColor = Color.White;
            }

            Canvas.DrawRect(backgroundColor, new Rectangle(visibleRect.Left, visibleRect.Top, visibleRect.Width, playerInfoBarHeight));

            double midHeight = visibleRect.Top + playerInfoBarHeight / 2.0;

            Vec2 textPosition = new Vec2(visibleRect.Left + visibleRect.Width / 2, midHeight);
            Canvas.DrawText(playerTextColor, text, textPosition, playerTextHeight);

            if (winningPlayer < 0)
            {
                double startOffset = arrowButtomLineThickness * 0.33333;

                // Undo button.
                Color lineColor = Board.TurnHistory.CanUndo() ? Color.Black : new Color(0.9f);
                Canvas.DrawLine(lineColor, new Vec2(undoButtonRect.Left + startOffset, midHeight + startOffset), new Vec2(undoButtonRect.Right, undoButtonRect.Top), arrowButtomLineThickness);
                Canvas.DrawLine(lineColor, new Vec2(undoButtonRect.Left + startOffset, midHeight - startOffset), new Vec2(undoButtonRect.Right, undoButtonRect.Bottom), arrowButtomLineThickness);

                // Redo button.
                lineColor = Board.TurnHistory.CanRedo() ? Color.Black : new Color(0.9f);
                Canvas.DrawLine(lineColor, new Vec2(redoButtonRect.Right - startOffset, midHeight + startOffset), new Vec2(redoButtonRect.Left, redoButtonRect.Top), arrowButtomLineThickness);
                Canvas.DrawLine(lineColor, new Vec2(redoButtonRect.Right - startOffset, midHeight - startOffset), new Vec2(redoButtonRect.Left, redoButtonRect.Bottom), arrowButtomLineThickness);
            }

            // Gradient.
            Canvas.DrawTopDownAlphaGradient(new Color(1.0f, 1.0f, 1.0f, 70 / 255.0),
                                            new Rectangle(visibleRect.Left, gameBoardRect.Top, visibleRect.Width, 24));
        }

        private void DrawBackground(IEnumerable<HexCoord> fieldPositions, GameBoard.Configuration boardConfig)
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

                    Canvas.DrawLine(lineColor, new Vec2(startX, startY), new Vec2(endX, endY), fieldRadius);
                }
            }
        }

        private void DrawFields(IEnumerable<KeyValuePair<HexCoord, GameBoard.Field>> fields)
        {
            // Draw highlight if any.
            if (HasHighlighted)
            {
                float highlightedX, highlightedY;
                HighlightedPos.ToCartesian(out highlightedX, out highlightedY);
                Canvas.DrawGlow(playerColors[CurrentPlayer], new Rectangle(highlightedX - highlightSize/2, highlightedY - highlightSize/2, highlightSize, highlightSize));
            }

            // Draw points
            Color fillColor;
            foreach (var field in fields)
            {
                int player = field.Value.PlayerPiece;

                float x, y;
                field.Key.ToCartesian(out x, out y);

                if (player >= 0 &&
                    !(animation.Active && animation.Turn.To == field.Key))
                {
                    fillColor = playerColors[player];
                    Canvas.DrawCircle(fillColor, playerRadius, new Vec2(x, y));
                }
                else
                {
                    int playerGoal = field.Value.Type.GetPlayerGoal();
                    if (playerGoal >= 0)
                        fillColor = playerColors[playerGoal];
                    else
                        fillColor = Color.Black;
                    Canvas.DrawCircle(fillColor, fieldRadius, new Vec2(x, y));
                }
            }

            // Animated turn.
            if (animation.Active)
            {
                float x, y;
                animation.GetCurrentCartesian(out x, out y);
                fillColor = playerColors[animation.Player];
                Canvas.DrawCircle(fillColor, playerRadius, new Vec2(x, y));
            }
        }

        #endregion


        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if(disposing)
            {
                Canvas.Dispose();
            }
        }
    }
}
