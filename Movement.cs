using Microsoft.Xna.Framework;

public static  class Movement
{
    public static float LerpAngle(float from, float to, float t)
    {
        float num = (to - from) % MathHelper.TwoPi; // Difference between angles
        if (num > MathHelper.Pi) num -= MathHelper.TwoPi; // Wrap around for angle
        if (num < -MathHelper.Pi) num += MathHelper.TwoPi; // Wrap around for angle

        return from + num * t; // Interpolate between the angles
    }
}
