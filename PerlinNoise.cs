using System;

public class PerlinNoise
{
    private static int[] permutation = new int[512];

    static PerlinNoise()
    {
        for (int i = 0; i < 256; i++)
        {
            permutation[i] = i;
        }

        // Shuffle the permutation array
        for (int i = 255; i > 0; i--)
        {
            int j = Rand.Instance.Next(i + 1);
            int temp = permutation[i];
            permutation[i] = permutation[j];
            permutation[j] = temp;
        }

        for (int i = 0; i < 256; i++)
        {
            permutation[i + 256] = permutation[i];
        }
    }

    public static float Noise(float x, float y)
    {
        int xi = (int)Math.Floor(x) & 255;
        int yi = (int)Math.Floor(y) & 255;

        float xf = x - (float)Math.Floor(x);
        float yf = y - (float)Math.Floor(y);

        float u = Fade(xf);
        float v = Fade(yf);

        int aa = permutation[permutation[xi] + yi];
        int ab = permutation[permutation[xi] + yi + 1];
        int ba = permutation[permutation[xi + 1] + yi];
        int bb = permutation[permutation[xi + 1] + yi + 1];

        float x1 = Lerp(Grad(aa, xf, yf), Grad(ab, xf, yf - 1), v);
        float x2 = Lerp(Grad(ba, xf - 1, yf), Grad(bb, xf - 1, yf - 1), v);
        return Lerp(x1, x2, u);
    }

    private static float Fade(float t) => t * t * t * (t * (t * 6 - 15) + 10);

    private static float Lerp(float a, float b, float t) => a + t * (b - a);

    private static float Grad(int hash, float x, float y)
    {
        int h = hash & 3; // convert low 2 bits of hash code
        float u = h < 2 ? x : y; // for 2D gradients
        float v = h < 2 ? y : x; // and a negative gradient
        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
    }
}
