using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HalmaShared
{
    static class MathUtils
    {
        public static float Lerp(float a, float b, float amount)
        {
            return (b - a) * amount + a;
        }

        public static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static float Smoothstep(float edge0, float edge1, float x)
        {
            // Scale, bias and saturate x to 0..1 range
            x = Clamp((x - edge0) / (edge1 - edge0), 0.0f, 1.0f);
            // Evaluate polynomial
            return x * x * (3 - 2 * x);
        }

        public static float Smootherstep(float edge0, float edge1, float x)
        {
            // Scale, and clamp x to 0..1 range
            x = Clamp((x - edge0) / (edge1 - edge0), 0.0f, 1.0f);
            // Evaluate polynomial
            return x * x * x * (x * (x * 6 - 15) + 10);
        }
    }
}