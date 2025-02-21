using Microsoft.Xna.Framework;
using System;

public class Camera2D
{
    public static float DefaultCameraZoom = 0.8f;
    public static float MinimumCameraZoom = 0.35f;
    public static float MaximumCameraZoom = 1.4f;
    public static float ZoomIncrement = 0.03f;
    public Vector2 Position { get; set; }
    public float Zoom { get; set; } = DefaultCameraZoom;
    public float Rotation { get; set; } = 0f;

    public int ViewportWidth;
    public int ViewportHeight;

    // Variables for shake effect
    private Vector2 shakeOffset = Vector2.Zero;
    private float shakeDuration = 0f;
    private float shakeIntensity = 0f;

    // Constructor to initialize screen dimensions
    public Camera2D(int screenWidth, int screenHeight)
    {
        this.ViewportWidth = screenWidth;
        this.ViewportHeight = screenHeight;
    }

    // Method to trigger camera shake
    public void Shake(float intensity, float duration)
    {
        shakeIntensity = intensity;
        shakeDuration = duration;
    }

    public void Update()
    {
        if (shakeDuration > 0)
        {
            // Reduce shake duration over time
            shakeDuration -= Game1.ElapsedTime;

            // Generate random shake offset within the intensity range
            float offsetX = (float)(Rand.Instance.NextDouble() * 2 - 1) * shakeIntensity;
            float offsetY = (float)(Rand.Instance.NextDouble() * 2 - 1) * shakeIntensity;
            shakeOffset = new Vector2(offsetX, offsetY);

            // Reduce intensity over time for a gradual shake
            shakeIntensity = MathHelper.Max(0, shakeIntensity - Game1.ElapsedTime);
        }
        else
        {
            shakeOffset = Vector2.Zero; // Reset shake when done
        }
    }

    // Method to get the view matrix for drawing
    public Matrix GetViewMatrix()
    {
        return Matrix.CreateTranslation(new Vector3(-Position + shakeOffset, 0.0f)) *
               Matrix.CreateRotationZ(Rotation) *
               Matrix.CreateScale(Zoom) *
               Matrix.CreateTranslation(new Vector3(ViewportWidth / 2.0f, ViewportHeight / 2.0f, 0.0f));
    }
}
