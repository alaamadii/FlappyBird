using FlappyBird.Core.Models;

namespace FlappyBird.Core.Physics;

/// <summary>Inclusive swept AABB contact along a linear, relative-motion segment.</summary>
public static class CollisionDetector
{
    public static double? ContactFraction(PipeRectangle moving, double deltaX, double deltaY,
        PipeRectangle obstacle)
    {
        var enter = 0d;
        var exit = 1d;
        if (!IntersectAxis(moving.X, deltaX, obstacle.X - moving.Width, obstacle.Right, ref enter, ref exit)
            || !IntersectAxis(moving.Y, deltaY, obstacle.Y - moving.Height, obstacle.Bottom, ref enter, ref exit))
        {
            return null;
        }

        return enter;
    }

    private static bool IntersectAxis(double origin, double delta, double minimum, double maximum,
        ref double enter, ref double exit)
    {
        if (delta == 0)
        {
            return origin >= minimum && origin <= maximum;
        }

        var first = (minimum - origin) / delta;
        var last = (maximum - origin) / delta;
        enter = Math.Max(enter, Math.Min(first, last));
        exit = Math.Min(exit, Math.Max(first, last));
        return enter <= exit;
    }
}
