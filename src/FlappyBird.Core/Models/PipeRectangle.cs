namespace FlappyBird.Core.Models;

/// <summary>A logical obstacle rectangle, independent of drawing APIs.</summary>
public readonly record struct PipeRectangle(double X, double Y, double Width, double Height)
{
    public double Right => X + Width;
    public double Bottom => Y + Height;
}
