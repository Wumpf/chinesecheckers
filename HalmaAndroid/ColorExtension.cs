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
    static class ColorExtension
    {
        public static Android.Graphics.Color ToAndroid(this Xamarin.Forms.Color color)
        {
            return new Android.Graphics.Color((byte)(255 * color.R), (byte)(255 * color.G), (byte)(255 * color.B), (byte)(255 * color.A));
        }
    }
}