using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

    // Adjust noise parameters for better effect
    private float noiseIntensity = 2f; // Constant small intensity
    private float noiseFrequency = 0.5f; // Slower frequency for smoother movement
    private float noiseTime = 0f;
    private Vector2 noiseOffset = Vector2.Zero;

    // Add these new fields
    private Texture2D noiseTexture;
    private Color[] noiseData;
    private Random random = new Random();
    private const float MIN_NOISE_OPACITY = 0.02f;
    private const float MAX_NOISE_OPACITY = 0.15f;
    private const int NOISE_HEIGHT = 1;
    
    // New fields for pulsing effect
    private float currentNoiseOpacity = MIN_NOISE_OPACITY;
    private float noiseTimer = 0f;
    private const float NOISE_PULSE_SPEED = 0.8f; // Speed of opacity change
    private bool increasingNoise = true;

    // Constructor to initialize screen dimensions
    public Camera2D(int screenWidth, int screenHeight)
    {
        this.ViewportWidth = screenWidth;
        this.ViewportHeight = screenHeight;
        
        // Initialize noise texture
        InitializeNoiseTexture();
    }

    private void InitializeNoiseTexture()
    {
        noiseTexture = new Texture2D(Game1.graphicsDevice, ViewportWidth, ViewportHeight);
        noiseData = new Color[ViewportWidth * ViewportHeight];
        
        // Initialize with transparent pixels
        for (int i = 0; i < noiseData.Length; i++)
        {
            noiseData[i] = Color.Transparent;
        }
    }

    public void UpdateNoiseEffect()
    {
        // Update noise intensity
        noiseTimer += Game1.ElapsedTime * NOISE_PULSE_SPEED;
        
        // Calculate noise opacity using a sine wave for smooth transitions
        float pulseValue = (float)Math.Sin(noiseTimer);
        currentNoiseOpacity = MathHelper.Lerp(MIN_NOISE_OPACITY, MAX_NOISE_OPACITY, (pulseValue + 1f) / 2f);

        // Randomly decide to switch between high and low noise states
        if (random.NextDouble() < 0.01) // 1% chance each frame to switch states
        {
            currentNoiseOpacity = random.NextDouble() < 0.5 ? MAX_NOISE_OPACITY : MIN_NOISE_OPACITY;
        }

        // Update scanlines with current opacity
        for (int y = 0; y < ViewportHeight; y += NOISE_HEIGHT)
        {
            if (random.NextDouble() < 0.8) // 80% chance for each line to show noise
            {
                float noiseValue = (float)random.NextDouble() * currentNoiseOpacity;
                Color noiseColor = new Color(1f, 1f, 1f, noiseValue);

                for (int x = 0; x < ViewportWidth; x++)
                {
                    for (int dy = 0; dy < NOISE_HEIGHT && (y + dy) < ViewportHeight; dy++)
                    {
                        noiseData[x + (y + dy) * ViewportWidth] = noiseColor;
                    }
                }
            }
            else
            {
                // Clear unused lines
                for (int x = 0; x < ViewportWidth; x++)
                {
                    for (int dy = 0; dy < NOISE_HEIGHT && (y + dy) < ViewportHeight; dy++)
                    {
                        noiseData[x + (y + dy) * ViewportWidth] = Color.Transparent;
                    }
                }
            }
        }
        
        noiseTexture.SetData(noiseData);
    }

    public void DrawNoiseEffect(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
        spriteBatch.Draw(noiseTexture, new Rectangle(0, 0, ViewportWidth, ViewportHeight), Color.White);
        spriteBatch.End();
    }

    // Method to trigger camera shake
    public void Shake(float intensity, float duration)
    {
        shakeIntensity = intensity;
        shakeDuration = duration;
    }

    // Method to trigger camera noise
    public void AddNoise(float intensity, float frequency)
    {
        noiseIntensity = intensity;
        noiseFrequency = frequency;
    }

    public void Update()
    {
        // Update existing shake effect
        if (shakeDuration > 0)
        {
            shakeDuration -= Game1.ElapsedTime;
            float offsetX = (float)(Rand.Instance.NextDouble() * 2 - 1) * shakeIntensity;
            float offsetY = (float)(Rand.Instance.NextDouble() * 2 - 1) * shakeIntensity;
            shakeOffset = new Vector2(offsetX, offsetY);
            shakeIntensity = MathHelper.Max(0, shakeIntensity - Game1.ElapsedTime);
        }
        else
        {
            shakeOffset = Vector2.Zero;
        }

        // Update constant noise effect
        noiseTime += Game1.ElapsedTime * noiseFrequency;
        
        // Use different time offsets for X and Y to create more organic movement
        float noiseX = PerlinNoise.Noise(noiseTime, 0) * noiseIntensity;
        float noiseY = PerlinNoise.Noise(0, noiseTime + 100) * noiseIntensity; // Offset for Y to create different pattern
        noiseOffset = new Vector2(noiseX, noiseY);
    }

    // Method to get the view matrix for drawing
    public Matrix GetViewMatrix()
    {
        return Matrix.CreateTranslation(new Vector3(-Position + shakeOffset + noiseOffset, 0.0f)) *
               Matrix.CreateRotationZ(Rotation) *
               Matrix.CreateScale(Zoom) *
               Matrix.CreateTranslation(new Vector3(ViewportWidth / 2.0f, ViewportHeight / 2.0f, 0.0f));
    }
}
