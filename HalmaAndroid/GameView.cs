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
    class GameView : View
    {
        private Level level;

        public GameView(Context context, Level level) : base(context)
        {
            this.level = level;
        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);

            DrawFields(level.GetFields(), canvas);
        }

        const float offsetPercent = 0.01f;
        const float fieldRadius = 0.2f;

        private static void DrawFields(IEnumerable<KeyValuePair<HexCoord, Level.Field>> fields, Canvas canvas)
        {
            // Gather points in cartesian coordinates.
            // All spatial properties here might be precomputed
            float minX = int.MaxValue;
            float minY = int.MaxValue;
            float maxX = int.MinValue;
            float maxY = int.MinValue;
            var fieldsCartesian = new List<KeyValuePair<System.Drawing.PointF, Level.Field>>();
            foreach (KeyValuePair<HexCoord, Level.Field> field in fields)
            {
                if (field.Value == Level.Field.Invalid)
                    continue;

                float x, y;
                field.Key.ToCartesian(out x, out y);
                System.Drawing.PointF point = new System.Drawing.PointF(x, y);

                minX = System.Math.Min(minX, x);
                maxX = System.Math.Max(maxX, x);
                minY = System.Math.Min(minY, y);
                maxY = System.Math.Max(maxY, y);

                fieldsCartesian.Add(new KeyValuePair<System.Drawing.PointF, Level.Field>(point, field.Value));
            }
            float extentX = maxX - minX;
            float extentY = maxY - minY;

            // Transform to fit canvas' clipping space. It is important not to use canvas.Width/Height directly, since the system's buttom bar bar clips!
            Rect clipBounds = new Rect(0, 0, canvas.Width, canvas.Height);
            canvas.GetClipBounds(clipBounds);
            int drawAreaWidth = clipBounds.Width();
            int drawAreaHeight = clipBounds.Height();

            float scale = System.Math.Min((drawAreaWidth - drawAreaWidth * offsetPercent * 2) / extentX,
                                          (drawAreaHeight - drawAreaHeight * offsetPercent * 2) / extentY);
            float offsetX = (drawAreaWidth / scale - (maxX - minX)) / 2 - minX + drawAreaWidth * offsetPercent / scale;
            float offsetY = (drawAreaHeight / scale - (maxY - minY)) / 2 - minY + drawAreaHeight * offsetPercent / scale;

            canvas.Scale(scale, scale);
            canvas.Translate(offsetX, offsetY);

            // Draw points
            Paint background = new Paint
            {
                Color = Color.White
            };
            canvas.DrawPaint(background);

            Paint fieldPaint = new Paint
            {
                AntiAlias = true,
                Color = Color.Black,
            };
            fieldPaint.SetStyle(Paint.Style.Fill);

            foreach (var field in fieldsCartesian)
            {
                canvas.DrawCircle(field.Key.X, field.Key.Y, fieldRadius, fieldPaint);
            }
        }
    }
}