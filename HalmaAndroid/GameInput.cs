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
    class GameInput : Java.Lang.Object, GestureDetector.IOnGestureListener, ScaleGestureDetector.IOnScaleGestureListener
    {
        private GameView view;
        private GestureDetector scrollListener;
        private ScaleGestureDetector scaleDetector;

        public delegate void FieldTouchedHandler(HexCoord hexcoord);
        public event FieldTouchedHandler FieldTouched;

        public GameInput(GameView view)
        {
            this.view = view;

            scrollListener = new GestureDetector(view.Context, this);
            scaleDetector = new ScaleGestureDetector(view.Context, this);
        }

        /// <summary>
        /// Needs to be called by activity to process touch.
        /// </summary>
        public bool OnTouchEvent(MotionEvent e)
        {
            scrollListener.OnTouchEvent(e);
            scaleDetector.OnTouchEvent(e);

            return true;
        }

        public bool OnScale(ScaleGestureDetector detector)
        {
            float scaleFactor = detector.ScaleFactor;
            float oldGameDrawScale = view.GameDrawScale;
            view.GameDrawScale *= scaleFactor;
            scaleFactor = view.GameDrawScale / oldGameDrawScale; // Apply constraints.

            // Scale the distance between origin (offset) and scale center.
            float oldOffsetDrawSpaceX = view.GameDrawOffsetX * oldGameDrawScale;
            float oldOffsetDrawSpaceY = view.GameDrawOffsetY * oldGameDrawScale;
            float newOffsetDrawSpaceX = (oldOffsetDrawSpaceX - detector.FocusX) * scaleFactor + detector.FocusX;
            float newOffsetDrawSpaceY = (oldOffsetDrawSpaceY - detector.FocusY) * scaleFactor + detector.FocusY;
            view.GameDrawOffsetX = newOffsetDrawSpaceX / view.GameDrawScale;
            view.GameDrawOffsetY = newOffsetDrawSpaceY / view.GameDrawScale;

            view.Invalidate();

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
                var coord = view.GetTouchResult(e);
                if (coord != null)
                    FieldTouched(coord.Value);
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
            view.GameDrawOffsetX -= distanceX / view.GameDrawScale;
            view.GameDrawOffsetY -= distanceY / view.GameDrawScale;
            view.Invalidate();
            return true;
        }

        public void OnShowPress(MotionEvent e)
        {
        }

        public bool OnSingleTapUp(MotionEvent e)
        {
            return true;
        }

        protected override void Dispose(bool dispose)
        {
            if (dispose)
            {
                scrollListener.Dispose();
                scaleDetector.Dispose();
            }
        }
    }
}