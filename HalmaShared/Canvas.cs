using Xamarin.Forms;

namespace HalmaShared
{
    public interface ICrossPlatformCanvas : System.IDisposable
    {
        void ResetTransform();
        void Translate(Vec2 offset);
        void Scale(float scale);

        void FillColor(Color color);
        void DrawCircle(Color color, float radius, Vec2 center);
        void DrawLine(Color color, Vec2 start, Vec2 end, float thickness);
        void DrawRect(Color color, Rectangle rect);
        void DrawText(Color color, string text, Vec2 center, float textSize);

        /// <summary>
        /// Would be nice to have a better abstraction on this.
        /// </summary>
        void DrawTopDownAlphaGradient(Color color, Rectangle rect);
    }
}
