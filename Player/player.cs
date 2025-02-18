using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public static class PlayerVariables
{
    public static float MaximumSpeed = 700;
    public static float MaximumBoostedSpeed = 1200;

    public static float LaserDistance = 2000f;
    public static float TrainingLaserDistance = 1500f;
    public static float MissileDistance = 1800f;
    public static float TrainingMissileDistance = 1500f;
    public static float PulseDistance = 250f;
    public static float Artifact1Distance = 1200f;
    public static float Artifact2Distance = 900f;

    public static float LaserSpeed = 1300f;
    public static float TrainingLaserSpeed = 900f;
    public static float MissileSpeed = 1000f;
    public static float TrainingMissileSpeed = 800f;
    public static float SprialLaserSpeed = 600f;
    public static float CrabLaserSpeed = 700f;
    public static float PulseSpeed = 250f;
    public static float ArtifactSpeed = 750f;
    public static float Artifact2Speed = 350f;

    public static float LaserReloadTime = 0.3f;
    public static float MissileReloadTime = 0.5f;
    public static float Artifact1ReloadTime = 0.2f;
    public static float Artifact2ReloadTime = 0.1f;

    public static float LaserReloadTimeArc = 0.24f;
    public static float MissileReloadTimeArc = 0.40f;

    public static float PowerupMissileTime = 20f;
    public static float PowerupLaserTime = 20f;
    public static float PowerupShieldTime = 20f;
    public static float PowerupJellyTime = 30f;
    public static float PowerupLandCrabTime = 30f;
}

public class Player
{
    public int Width { get; set; }
    public int Height { get; set; }
    public float Scale = 1.0f;
    public float rotation;
    // Input for controlling the player
    Controllers inputMethod;
    int playerIndex = 0;


    public Vector2 Position { get; set; } // Position of the player
    public Vector2 velocity;          // Velocity of the player
    public Vector2 direction;          // Direction of the player
    public static float maximumSpeed = PlayerVariables.MaximumSpeed; // Maximum possible speed for the player
    private float speed = Player.maximumSpeed; // Speed multiplier for movement
    private float maxVelocity = Player.maximumSpeed; // Maximum velocity for the player
    private float decelerationRate = Player.maximumSpeed / 2; // Deceleration rate when the player stops moving
    
    // Health 
    public int health;                // Player's current health
    public int maxHealth = 100;        // Maximum player health

    public bool IsDead { get; set; }

    // Animation
    private int currentFrame;          // Current frame for player's animation
    private double totalElapsedTime;   // For player frame animation
    private float animationSpeed = 0.06f; // Speed of player animation (time per frame)

    // Graphics
    public Texture2D[] shipFrames;    // Frames for player ship animation
    public Camera2D camera;

    // Tile pointed to
    public int tilePointedToX, tilePointedToY; // Player's current tile position in the grid
    public int previousPointedToX = -1, previousPointedToY = -1; // Player's previous tile position in the grid
    public TileTypes tilePointedTo;       // The type of tile the player is currently pointing to
    public int overlayPointedTo;          // The overlay the player is currently over

    public Player(Controllers controlMethod, Texture2D[] frames,Vector2 position, Camera2D playerCamera, int playerIndex)
    {
        this.shipFrames = frames;
        this.IsDead = false;
        this.playerIndex = playerIndex - 1;
        inputMethod = controlMethod;

        this.velocity = Vector2.Zero;  // Initial velocity of the player
        this.direction = Vector2.UnitX; // Initial direction of the player
        this.currentFrame = 10;        // Start with the neutral animation frame
        this.health = maxHealth;
        this.camera = playerCamera;
        this.Position = position;
        this.Width = frames[0].Width;
        this.Height = frames[0].Height;
        InitializeCollisionData();
    }

    public void InitializeCollisionData()
    {
    }

    private void HandleZoom()
    {
        // Zoom in and out using keyboard
        if (inputMethod.IsActionPressed(ControlAction.ZoomIn))
        {
            camera.Zoom = Math.Min(camera.Zoom + Camera2D.ZoomIncrement, Camera2D.MaximumCameraZoom); // Adjust max zoom as needed
        }
        if (inputMethod.IsActionPressed(ControlAction.ZoomOut))
        {
            camera.Zoom = Math.Max(camera.Zoom - Camera2D.ZoomIncrement, Camera2D.MinimumCameraZoom); // Adjust min zoom as needed
        }
    }

    public void CommonUpdate(Vector2 movementInput, float rotationInput, bool isFiringMissile, bool isChaffReleased, bool isFlipping, bool isFiringLaser)
    {
        HandleZoom();
        HandlePlayerMovement(movementInput, rotationInput);
    }
    private void HandlePlayerMovement(Vector2 movementInput, float rotationInput)
    {
        // Handle rotation
        if (rotationInput != 0)
        {
            // Rotate the direction vector by the rotation input
            direction = Vector2.Transform(direction, Matrix.CreateRotationZ(rotationInput));
            direction.Normalize(); // Keep direction normalized
        }

        float currentSpeed = velocity.Length();

        // Apply forward impulse if there's movement input
        if (movementInput.Length() > 0)
        {

            velocity += direction * speed * movementInput.Length() * Game1.ElapsedTime;

            if (currentSpeed > maxVelocity)
            {
                float targetSpeed = MathHelper.Clamp(currentSpeed, maxVelocity, PlayerVariables.MaximumBoostedSpeed); // Ensure speed never exceeds MaxBoostedVelocity
                float slowDownFactor = Game1.ElapsedTime; // Adjust deceleration rate dynamically

                velocity *= 1f - MathHelper.Clamp(slowDownFactor, 0f, 1f);

                // Ensure it doesn't overshoot below maxVelocity
                if (currentSpeed < maxVelocity)
                {
                    velocity = Vector2.Normalize(velocity) * maxVelocity;
                }
                else if (currentSpeed > PlayerVariables.MaximumBoostedSpeed)
                {
                    velocity = Vector2.Normalize(velocity) * PlayerVariables.MaximumBoostedSpeed;
                }
            }
        }
        else
        {
            // Decelerate when no movement input
            if (currentSpeed > 0)
            {
                float decelerationAmount = decelerationRate * Game1.ElapsedTime;
                velocity -= Vector2.Normalize(velocity) * decelerationAmount;

                // If velocity gets too low, set it to zero to prevent jittering
                if (currentSpeed < 1f)
                {
                    velocity = Vector2.Zero;
                }
            }
        }
        // Update the position based on velocity
        Position += velocity * Game1.ElapsedTime;
    }


    public void Update()
    {
        Vector2 movementInput = Vector2.Zero;
        float rotationInput = 0f;
        bool isFiringMissile = false;
        bool isChaffReleased = false;
        bool isFlipping = false;
        bool isFiringLaser = false;
        bool isAttemptingActivate = false;

        if (inputMethod is KeyboardControls)
        {
            if (inputMethod.IsActionPressed(ControlAction.Accelerate)) movementInput = new Vector2(1, 0);
            if (inputMethod.IsActionPressed(ControlAction.Left)) rotationInput = -0.05f;
            if (inputMethod.IsActionPressed(ControlAction.Right)) rotationInput = 0.05f;

            isFiringMissile = inputMethod.IsActionPressed(ControlAction.Missile);
            isChaffReleased = inputMethod.IsActionPressed(ControlAction.Chaff);
            isFlipping = inputMethod.IsActionPressed(ControlAction.Flip);
            isFiringLaser = inputMethod.IsActionPressed(ControlAction.Laser);
            isAttemptingActivate = inputMethod.IsActionPressed(ControlAction.Activate);
            AnimatePlayer(rotationInput);
        }
        else if (inputMethod is GamepadControls)
        {
            GamePadState gamepadState = GamePad.GetState(((GamepadControls)inputMethod).GamepadIndex);

            // Rotate the player based on left stick horizontal axis
            float horizontalInput = gamepadState.ThumbSticks.Left.X;
            if (Math.Abs(horizontalInput) > 0.1f) // Add dead zone to prevent jitter
            {
                rotationInput = horizontalInput * 0.05f; // Adjust rotation speed here (similar to keys)
            }

            // Accelerate the player when a button is pressed
            if (gamepadState.IsButtonDown(Buttons.A))
            {
                movementInput = new Vector2(1, 0); // Forward movement similar to keyboard acceleration
            }

            // Button actions for missile, laser, chaff, flipping, etc.
            isFiringMissile = gamepadState.IsButtonDown(Buttons.RightTrigger);
            isChaffReleased = gamepadState.IsButtonDown(Buttons.B);
            isFlipping = gamepadState.IsButtonDown(Buttons.X);
            isFiringLaser = gamepadState.IsButtonDown(Buttons.LeftTrigger);
            isAttemptingActivate = gamepadState.IsButtonDown(Buttons.Y);
            AnimatePlayer(rotationInput);
        }
        // Call the common update logic
        CommonUpdate(movementInput, rotationInput, isFiringMissile, isChaffReleased, isFlipping, isFiringLaser);

    }

    public void SetPosition(Vector2 newPosition)
    {
        this.Position = newPosition;
    }

    private void AnimatePlayer(float rotationInput)
    {
        // Animate player based on rotation input
        totalElapsedTime += Game1.ElapsedTime;

        if (rotationInput < 0)
        {
            // Animate towards frame 0 when rotating left
            if (currentFrame > 1 && totalElapsedTime >= animationSpeed)
            {
                currentFrame--;
                totalElapsedTime = 0;
            }
        }
        else if (rotationInput > 0)
        {
            // Animate towards frame 20 when rotating right
            if (currentFrame < 19 && totalElapsedTime >= animationSpeed)
            {
                currentFrame++;
                totalElapsedTime = 0;
            }
        }
        else
        {
            // Animate back to frame 10 when no rotation input
            if (currentFrame < 10 && totalElapsedTime >= animationSpeed)
            {
                currentFrame++;
                totalElapsedTime = 0;
            }
            else if (currentFrame > 10 && totalElapsedTime >= animationSpeed)
            {
                currentFrame--;
                totalElapsedTime = 0;
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (IsDead)
        {
            return;
        }

        // Draw the engine and its smoke particles
        float rotation = (float)Math.Atan2(direction.Y, direction.X);

        // Draw the player, checking if flipping
        Texture2D playerTexture;

        playerTexture = shipFrames[currentFrame];


        // Draw the player
        spriteBatch.Draw(
            playerTexture,
            Position,
            null,
            Color.White,
            rotation,
            new Vector2(AssetManager.Player1[currentFrame].Width / 2, AssetManager.Player1[currentFrame].Height / 2),
            1,
            SpriteEffects.None,
            0f);

    }

}
