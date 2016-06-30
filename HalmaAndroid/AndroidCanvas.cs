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
using Android.Graphics;
using HalmaShared;
using Xamarin.Forms;

using Color = Xamarin.Forms.Color;

namespace HalmaAndroid
{
    class AndroidCanvas : HalmaShared.ICrossPlatformCanvas
    {
        public Canvas Canvas { private get; set; }

        private Typeface typeface;
        private Bitmap gradient0;
        private Paint paintNoAA;
        private Paint paintFillAA;
        private Paint paintPlayerText;
        private Paint paintLine;

        public AndroidCanvas(Context context)
        {
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
            };
            paintPlayerText.SetTypeface(typeface);
            paintLine = new Paint
            {
                AntiAlias = true,
            };
        }

        #region Transform

        void ICrossPlatformCanvas.ResetTransform()
        {
            Canvas.Matrix = new Matrix();
        }

        void ICrossPlatformCanvas.Translate(Vec2 offset)
        {
            Canvas.Translate((float)offset.X, (float)offset.Y);
        }

        void ICrossPlatformCanvas.Scale(float scale)
        {
            Canvas.Scale(scale, scale);
        }

        #endregion

        #region Drawing

        void ICrossPlatformCanvas.FillColor(Color color)
        {
            paintNoAA.Color = color.ToAndroid();
            Canvas.DrawPaint(paintNoAA);
        }

        void ICrossPlatformCanvas.DrawCircle(Color color, float radius, Vec2 center)
        {
            paintFillAA.Color = color.ToAndroid();
            Canvas.DrawCircle((float)center.X, (float)center.Y, (float)radius, paintFillAA);
        }

        void ICrossPlatformCanvas.DrawLine(Color color, Vec2 start, Vec2 end, float thickness)
        {
            paintLine.Color = color.ToAndroid();
            paintLine.StrokeWidth = thickness;
            Canvas.DrawLine((float)start.X, (float)start.Y, (float)end.X, (float)end.Y, paintLine);
        }

        void ICrossPlatformCanvas.DrawRect(Color color, Rectangle rect)
        {
            paintNoAA.Color = color.ToAndroid();
            Canvas.DrawRect((float)rect.Left, (float)rect.Top, (float)rect.Right, (float)rect.Bottom, paintNoAA);
        }

        void ICrossPlatformCanvas.DrawText(Color color, string text, Vec2 center, float textSize)
        {
            paintPlayerText.Color = color.ToAndroid();
            paintPlayerText.TextSize = textSize;

            var metrics = paintPlayerText.GetFontMetrics();
            float x = (float)(center.X - paintPlayerText.MeasureText(text) / 2.0f);
            float y = (float)(center.Y + textSize / 2.0f);
            Canvas.DrawText(text, x, y, paintPlayerText);
        }

        void ICrossPlatformCanvas.DrawTopDownAlphaGradient(Color color, Rectangle rect)
        {
            paintNoAA.Color = color.ToAndroid();
            Canvas.DrawBitmap(gradient0, new Rect(0, 0, gradient0.Width, gradient0.Height),
                                         new Rect((int)rect.Left, (int)rect.Top, (int)rect.Right, (int)rect.Bottom), paintNoAA);
        }

        #endregion

        void IDisposable.Dispose()
        {
            typeface.Dispose();
            gradient0.Dispose();

            paintNoAA.Dispose();
            paintFillAA.Dispose();
            paintPlayerText.Dispose();
            paintLine.Dispose();
        }
    }
}